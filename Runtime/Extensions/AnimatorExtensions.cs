using UnityEngine;

namespace TRnK.Extensions
{
    public static class AnimatorExtensions
    {
        /// <summary>Get the length of an animation clip by name.</summary>
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

        /// <summary>Returns the clip length in seconds for the given clip-name hash.</summary>
        public static float GetAnimationLength(this Animator animator, int clipNameHash)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return 0f;

            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (Animator.StringToHash(clip.name) == clipNameHash)
                    return clip.length;
            }

            return 0f;
        }

        /// <summary>Check if the animator is currently playing a specific animation by name.</summary>
        public static bool IsPlayingAnimation(this Animator animator, string animName, int layerIndex = 0)
        {
            if (animator == null) return false;
            return animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(animName);
        }

        /// <summary>Returns true if the current state's shortNameHash matches the given hash.</summary>
        public static bool IsPlayingAnimation(this Animator animator, int stateNameHash, int layerIndex = 0)
        {
            if (animator == null) return false;
            return animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash == stateNameHash;
        }
    }
}
