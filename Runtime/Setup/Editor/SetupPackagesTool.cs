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
        /// Normalize a Git URL to a comparable form (lowercase host/path, strip scheme and .git, unify separators).
        /// </summary>
        public static string NormalizeGitUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;
            url = url.Trim();
            // Remove trailing .git and normalize scheme/host case
            if (url.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                url = url.Substring(0, url.Length - 4);
            url = url.Replace("ssh://", "")
                     .Replace("git@", "")
                     .Replace("https://", "")
                     .Replace("http://", "");
            // Replace ':' after host with '/'
            int idx = url.IndexOf(':');
            if (idx > -1 && url.IndexOf('/') == -1)
            {
                url = url.Substring(0, idx) + "/" + url.Substring(idx + 1);
            }
            return url.TrimEnd('/').ToLowerInvariant();
        }

        /// <summary>
        /// Extract the plain Git URL part from a packageId like 'name@git+https://host/repo.git#hash'.
        /// Returns null if not a git packageId.
        /// </summary>
        public static string ExtractGitUrlFromPackageId(string packageId)
        {
            if (string.IsNullOrEmpty(packageId)) return null;
            int atIdx = packageId.IndexOf('@');
            if (atIdx < 0 || atIdx + 1 >= packageId.Length) return null;
            string afterAt = packageId.Substring(atIdx + 1);
            if (!afterAt.StartsWith("git+", StringComparison.OrdinalIgnoreCase)) return null;
            afterAt = afterAt.Substring(4);
            int hashIdx = afterAt.IndexOf('#');
            if (hashIdx > -1) afterAt = afterAt.Substring(0, hashIdx);
            return afterAt;
        }

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
        /// Remove a package by its name (e.g., com.company.package). For Git packages, Unity maps to the package name declared in its package.json.
        /// Returns the RemoveRequest or null if name invalid.
        /// </summary>
        public static RemoveRequest RemovePackage(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                Debug.LogError("RemovePackage: package name is empty.");
                return null;
            }
            Debug.Log($"Removing package: {packageName}");
            return Client.Remove(packageName.Trim());
        }

        /// <summary>
        /// Get the installed package name for a given Git URL if present, otherwise null.
        /// This uses Client.List to sync with UPM.
        /// </summary>
        public static string GetInstalledPackageNameForGit(string gitUrl)
        {
            if (string.IsNullOrWhiteSpace(gitUrl)) return null;
            try
            {
                var listReq = Client.List(true, true);
                while (!listReq.IsCompleted) { }
                if (listReq.Status == StatusCode.Success && listReq.Result != null)
                {
                    // Match by package.source == Git or match repository URL when available
                    foreach (var p in listReq.Result)
                    {
                        if (p == null) continue;
                        if (p.source == PackageSource.Git)
                        {
                            // Extract the URL portion from packageId (format: name@git+https://host/repo.git#hash)
                            string afterAt = ExtractGitUrlFromPackageId(p.packageId);
                            if (!string.IsNullOrEmpty(afterAt))
                            {
                                if (NormalizeGitUrl(afterAt) == NormalizeGitUrl(gitUrl))
                                    return p.name; // canonical package name
                            }
                        }
                    }
                }
                else if (listReq.Status >= StatusCode.Failure)
                {
                    Debug.LogError($"UPM List failed: {listReq.Error?.message}");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return null;
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
