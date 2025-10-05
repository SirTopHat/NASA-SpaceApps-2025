using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple, lightweight global sound manager for playing SFX and Music from anywhere.
/// Add this to a bootstrap scene (or first loaded scene). It persists across scenes.
/// </summary>
[DefaultExecutionOrder(-100)]
public sealed class SoundManager : MonoBehaviour
{
	[Serializable]
	public sealed class NamedClip
	{
		public string id;
		public AudioClip clip;
	}

	public static SoundManager Instance { get; private set; }

	[Header("Clip Library")]
	[SerializeField] private List<NamedClip> clipLibrary = new List<NamedClip>();

	[Header("Volumes")] 
	[Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
	[Range(0f, 1f)] [SerializeField] private float musicVolume = 1f;

	[Header("SFX Pool")] 
	[SerializeField] private int sfxPoolSize = 8;
	[SerializeField] private bool sfxSpatial = false;

	private readonly Dictionary<string, AudioClip> idToClip = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);
	private readonly List<AudioSource> sfxSources = new List<AudioSource>();
	private int nextSfxIndex;
	private AudioSource musicSource;

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

		BuildClipLookup();
		EnsureAudioObjects();
	}

	private void BuildClipLookup()
	{
		idToClip.Clear();
		for (int i = 0; i < clipLibrary.Count; i++)
		{
			var entry = clipLibrary[i];
			if (entry == null || entry.clip == null || string.IsNullOrWhiteSpace(entry.id))
				continue;
			idToClip[entry.id] = entry.clip;
		}
	}

	private void EnsureAudioObjects()
	{
		// Music source
		if (musicSource == null)
		{
			var musicGo = new GameObject("MusicSource");
			musicGo.transform.SetParent(transform, false);
			musicSource = musicGo.AddComponent<AudioSource>();
			musicSource.loop = true;
			musicSource.playOnAwake = false;
			musicSource.spatialBlend = 0f;
			musicSource.volume = musicVolume;
		}

		// SFX pool
		if (sfxSources.Count != sfxPoolSize)
		{
			// Destroy extras
			for (int i = sfxSources.Count - 1; i >= sfxPoolSize; i--)
			{
				if (sfxSources[i] != null)
					Destroy(sfxSources[i].gameObject);
				sfxSources.RemoveAt(i);
			}

			// Create missing
			while (sfxSources.Count < sfxPoolSize)
			{
				var sfxGo = new GameObject($"SFXSource_{sfxSources.Count}");
				sfxGo.transform.SetParent(transform, false);
				var source = sfxGo.AddComponent<AudioSource>();
				source.playOnAwake = false;
				source.loop = false;
				source.spatialBlend = sfxSpatial ? 1f : 0f;
				source.volume = sfxVolume;
				sfxSources.Add(source);
			}
		}
	}

	private AudioSource GetNextFreeSfxSource()
	{
		// Simple round-robin; if chosen source is still playing, it will be cut off
		if (sfxSources.Count == 0)
			EnsureAudioObjects();
		var src = sfxSources[nextSfxIndex];
		nextSfxIndex = (nextSfxIndex + 1) % sfxSources.Count;
		return src;
	}

	public void PlaySfx(string id, float volume = 1f, float pitch = 1f)
	{
		if (!idToClip.TryGetValue(id, out var clip) || clip == null)
			return;
		PlaySfx(clip, volume, pitch);
	}

	public void PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f)
	{
		if (clip == null)
			return;
		var src = GetNextFreeSfxSource();
		src.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
		src.volume = Mathf.Clamp01(sfxVolume * volume);
		src.spatialBlend = sfxSpatial ? 1f : 0f;
		src.clip = clip;
		src.Play();
	}

	public void PlaySfxAt(AudioClip clip, Vector3 worldPosition, float volume = 1f, float pitch = 1f)
	{
		if (clip == null)
			return;
		var src = GetNextFreeSfxSource();
		src.transform.position = worldPosition;
		src.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
		src.volume = Mathf.Clamp01(sfxVolume * volume);
		src.spatialBlend = 1f;
		src.clip = clip;
		src.Play();
	}

	public void PlayMusic(string id, float volume = 1f, bool loop = true)
	{
		if (!idToClip.TryGetValue(id, out var clip) || clip == null)
			return;
		PlayMusic(clip, volume, loop);
	}

	public void PlayMusic(AudioClip clip, float volume = 1f, bool loop = true)
	{
		if (clip == null)
			return;
		musicSource.clip = clip;
		musicSource.loop = loop;
		musicSource.volume = Mathf.Clamp01(musicVolume * volume);
		musicSource.Play();
	}

	public void StopMusic()
	{
		musicSource.Stop();
		musicSource.clip = null;
	}

	public void SetSfxVolume(float volume)
	{
		sfxVolume = Mathf.Clamp01(volume);
		for (int i = 0; i < sfxSources.Count; i++)
		{
			var src = sfxSources[i];
			if (src != null && !src.isPlaying)
				src.volume = sfxVolume;
		}
	}

	public void SetMusicVolume(float volume)
	{
		musicVolume = Mathf.Clamp01(volume);
		musicSource.volume = musicVolume;
	}

	public bool TryGetClip(string id, out AudioClip clip)
	{
		return idToClip.TryGetValue(id, out clip);
	}
}


