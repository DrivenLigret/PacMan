using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip intro;
    [SerializeField] private AudioClip normal;
    [SerializeField] private AudioClip scared;
    [SerializeField] private AudioClip dead;

    public void PlayIntro()
    {
        Play(intro, false);
    }

    public void SetState(bool anyDead, bool anyScared)
    {
        Play(anyDead ? dead : anyScared ? scared : normal, true);
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    private void Play(AudioClip clip, bool loop)
    {
        if (audioSource.clip == clip && audioSource.isPlaying && audioSource.loop == loop) return;
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();
    }
}
