using System.Collections;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip intro;
    [SerializeField] private AudioClip normal;

    private IEnumerator Start()
    {
        audioSource.clip = intro;
        audioSource.loop = false;
        audioSource.Play();

        yield return new WaitForSeconds(Mathf.Min(3f, intro.length));

        audioSource.clip = normal;
        audioSource.loop = true;
        audioSource.Play();
    }
}
