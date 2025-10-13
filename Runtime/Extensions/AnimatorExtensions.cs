using System.Collections;
using NekoLib.Core;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Extensions
{
    public static class AnimatorExtensions
    {
        /// <summary>
        /// Get the length of an animation clip by name.
        /// </summary>
        public static float GetAnimationLength(this Animator animator, string animName)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return 0f;

            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == animName)
                    return clip.length;
            }

            return 0f;
        }

        /// <summary>
        /// Get the length of an animation clip by hash.
        /// </summary>
        public static float GetAnimationLength(this Animator animator, int animHash)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return 0f;

            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (Animator.StringToHash(clip.name) == animHash)
                    return clip.length;
            }

            return 0f;
        }

        /// <summary>
        /// Get the current progress of the animation in the specified layer (0 to 1).
        /// </summary>
        public static float GetCurrentAnimationProgress(this Animator animator, int layerIndex = 0)
        {
            if (animator == null) return 0f;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            return stateInfo.normalizedTime % 1f;
        }

        /// <summary>
        /// Get the remaining time of the current animation in the specified layer.
        /// </summary>
        public static float GetCurrentAnimationRemainingTime(this Animator animator, int layerIndex = 0)
        {
            if (animator == null) return 0f;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            float progress = stateInfo.normalizedTime % 1f;
            return stateInfo.length * (1f - progress);
        }

        /// <summary>
        /// Check if the animator is currently playing a specific animation by name.
        /// </summary>
        public static bool IsPlayingAnimation(this Animator animator, string animName, int layerIndex = 0)
        {
            if (animator == null) return false;
            return animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(animName);
        }

        /// <summary>
        /// Check if the animator is currently playing a specific animation by hash.
        /// </summary>
        public static bool IsPlayingAnimation(this Animator animator, int animHash, int layerIndex = 0)
        {
            if (animator == null) return false;
            return animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash == animHash;
        }

        /// <summary>
        /// Play an animation and wait for it to complete.
        /// </summary>
        public static IEnumerator PlayAndWait(this Animator animator, string stateName, int layerIndex = 0)
        {
            animator.Play(stateName, layerIndex);
            yield return WaitForAnimation(animator, stateName, layerIndex);
        }

        /// <summary>
        /// Play an animation and wait for it to complete.
        /// </summary>
        public static IEnumerator PlayAndWait(this Animator animator, int stateHash, int layerIndex = 0)
        {
            animator.Play(stateHash, layerIndex);
            yield return WaitForAnimation(animator, stateHash, layerIndex);
        }

        /// <summary>
        /// Cross-fade to an animation and wait for it to complete.
        /// </summary>
        public static IEnumerator CrossFadeAndWait(this Animator animator, string stateName, float transitionDuration, int layerIndex = 0)
        {
            animator.CrossFade(stateName, transitionDuration, layerIndex);
            yield return WaitForAnimation(animator, stateName, layerIndex);
        }

        /// <summary>
        /// Cross-fade to an animation and wait for it to complete.
        /// </summary>
        public static IEnumerator CrossFadeAndWait(this Animator animator, int stateHash, float transitionDuration, int layerIndex = 0)
        {
            animator.CrossFade(stateHash, transitionDuration, layerIndex);
            yield return WaitForAnimation(animator, stateHash, layerIndex);
        }

        /// <summary>
        /// Coroutine to wait for an animation to complete.
        /// </summary>
        public static IEnumerator WaitForAnimation(Animator animator, string stateName, int layerIndex = 0)
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
            while (animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(stateName) &&
                    animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime < 1f)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Coroutine to wait for an animation to complete.
        /// </summary>
        public static IEnumerator WaitForAnimation(Animator animator, int stateHash, int layerIndex = 0)
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
            while (animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash == stateHash &&
                    animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime < 1f)
            {
                yield return null;
            }
        }
    }
}
