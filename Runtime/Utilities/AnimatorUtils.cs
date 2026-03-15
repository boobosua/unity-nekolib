using System.Collections;
using NekoLib.ColorPalette;
using NekoLib.Extensions;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Utilities
{
    public static class AnimatorUtils
    {
        private const float AnimationCompleted = 1f;

        /// <summary>Coroutine to wait for an animation to complete by name.</summary>
        public static IEnumerator WaitForAnimation(this Animator animator, string stateName, int layerIndex = 0)
        {
            yield return new WaitForEndOfFrame();

            // Wait for transition to complete.
            while (animator.IsInTransition(layerIndex))
                yield return null;

            // Check if animation is looped - if so, don't wait.
            var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            if (stateInfo.loop)
            {
                Log.Warn($"Animation '{stateName.Colorize(Swatch.GA)}' is looped. Will not wait for completion.");
                yield break;
            }

            // Wait for non-looped animation to complete.
            while (true)
            {
                var info = animator.GetCurrentAnimatorStateInfo(layerIndex);
                if (!info.IsName(stateName) || info.normalizedTime >= AnimationCompleted)
                    break;
                yield return null;
            }
        }

        /// <summary>Coroutine to wait for an animation to complete by hash.</summary>
        public static IEnumerator WaitForAnimation(this Animator animator, int stateHash, int layerIndex = 0)
        {
            yield return new WaitForEndOfFrame();

            // Wait for transition to complete.
            while (animator.IsInTransition(layerIndex))
                yield return null;

            // Check if animation is looped - if so, don't wait.
            var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            if (stateInfo.loop)
            {
                Log.Warn($"Animation with hash '{stateHash.ToString().Colorize(Swatch.GA)}' is looped. Will not wait for completion.");
                yield break;
            }

            // Wait for non-looped animation to complete.
            while (true)
            {
                var info = animator.GetCurrentAnimatorStateInfo(layerIndex);
                if (info.shortNameHash != stateHash || info.normalizedTime >= AnimationCompleted)
                    break;
                yield return null;
            }
        }
    }
}
