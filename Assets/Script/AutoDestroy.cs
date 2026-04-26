using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    private void Start()
    {
        var anim = GetComponent<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            var clips = anim.runtimeAnimatorController.animationClips;
            if (clips != null && clips.Length > 0)
            {
                float maxLength = 0f;
                foreach (var clip in clips)
                    if (clip.length > maxLength) maxLength = clip.length;
                Destroy(gameObject, maxLength);
                return;
            }
        }
        Destroy(gameObject, 2f);
    }
}
