using UnityEngine;
using Random = UnityEngine.Random;

public class SoundsManager : MonoBehaviour
{
    public AudioClip[] Sounds;
    private AudioSource _audioSource => GetComponent<AudioSource>();

    protected void PlaySound(AudioClip clip, float volume = 1f, bool destroyed = false, float p1 = 0.85f, float p2 = 1.2f)
    {
        _audioSource.pitch = Random.Range(p1, p2);
        _audioSource.PlayOneShot(clip, volume);
    }
}
