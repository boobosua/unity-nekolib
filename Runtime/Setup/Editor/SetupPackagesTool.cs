#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace NekoLib
{
    public static class SetupPackagesTool
    {
        /// <summary>
        /// Add a package by identifier. Accepts either a package name (optionally with @version)
        /// or a Git URL. Returns the AddRequest or null if validation fails.
        /// </summary>
        public static AddRequest AddPackage(string identifier)
        {
            if (!ValidatePackageIdentifier(identifier, out bool isGit, out string error))
            {
                Debug.LogError($"AddPackage: invalid identifier. {error}");
                return null;
            }
            string trimmed = identifier.Trim();
            Debug.Log(isGit
                ? $"Adding package from Git URL: {trimmed}"
                : $"Adding package by name: {trimmed}");
            return Client.Add(trimmed);
        }

        /// <summary>
        /// Validate a package identifier. Determines if it's a Git URL or a package name.
        /// </summary>
        public static bool ValidatePackageIdentifier(string identifier, out bool isGit, out string error)
        {
            error = null;
            isGit = false;
            if (string.IsNullOrWhiteSpace(identifier))
            {
                error = "Identifier cannot be empty.";
                return false;
            }
            identifier = identifier.Trim();

            if (LooksLikeGitUrl(identifier))
            {
                isGit = true;
                // basic sanity: should contain a host and repo
                if (!identifier.Contains("/"))
                {
                    error = "Git URL appears malformed.";
                    return false;
                }
                return true;
            }

            // Validate package name like com.company.package or with @version suffix
            // segments: lowercase letters/numbers with hyphens allowed after first char
            // e.g., com.unity.nuget.newtonsoft-json or com.example.pkg@1.2.3
            var nameVersion = identifier.Split('@');
            var name = nameVersion[0];
            var namePattern = new Regex("^[a-z0-9]+(\\.[a-z0-9][a-z0-9-]*)+$");
            if (!namePattern.IsMatch(name))
            {
                error = "Invalid package name format.";
                return false;
            }
            // Optional version part: allow semver-ish or tags
            if (nameVersion.Length > 1)
            {
                var ver = nameVersion[1];
                if (string.IsNullOrWhiteSpace(ver))
                {
                    error = "Version suffix '@' provided without a version.";
                    return false;
                }
                // lax version check
                var verPattern = new Regex(@"^[A-Za-z0-9\.\-\+_]+$");
                if (!verPattern.IsMatch(ver))
                {
                    error = "Invalid version format.";
                    return false;
                }
            }
            return true;
        }

        private static bool LooksLikeGitUrl(string s)
        {
            s = s.Trim();
            if (s.StartsWith("git@", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.EndsWith(".git", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || s.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.StartsWith("ssh://", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
#endif
