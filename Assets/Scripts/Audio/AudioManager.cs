using UnityEngine;
using FieldFriends.Core;
using FieldFriends.Data;

namespace FieldFriends.Audio
{
    /// <summary>
    /// Audio management. 3 music tracks: Town, Path, Quiet Grove.
    /// Battle uses the Path track. Silence between loops is allowed.
    /// Creature SFX: taps, chimes, water clicks.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music Tracks")]
        [SerializeField] AudioClip townMusic;
        [SerializeField] AudioClip pathMusic;
        [SerializeField] AudioClip quietGroveMusic;

        [Header("SFX")]
        [SerializeField] AudioClip sfxTap;
        [SerializeField] AudioClip sfxChime;
        [SerializeField] AudioClip sfxWaterClick;
        [SerializeField] AudioClip sfxStep;
        [SerializeField] AudioClip sfxMenuSelect;
        [SerializeField] AudioClip sfxMenuMove;
        [SerializeField] AudioClip sfxHit;
        [SerializeField] AudioClip sfxRest;

        AudioSource _musicSource;
        AudioSource _sfxSource;
        MusicTrack _currentTrack = MusicTrack.None;

        public enum MusicTrack { None, Town, Path, QuietGrove }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Create audio sources
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.volume = GameConstants.MusicVolume;
            _musicSource.playOnAwake = false;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.volume = GameConstants.SFXVolume;
            _sfxSource.playOnAwake = false;
        }

        void OnEnable()
        {
            World.AreaManager.OnAreaChanged += OnAreaChanged;
        }

        void OnDisable()
        {
            World.AreaManager.OnAreaChanged -= OnAreaChanged;
        }

        void OnAreaChanged(AreaID area)
        {
            MusicTrack target = GetTrackForArea(area);
            if (target != _currentTrack)
                PlayTrack(target);
        }

        MusicTrack GetTrackForArea(AreaID area)
        {
            switch (area)
            {
                case AreaID.WillowEnd:
                    return MusicTrack.Town;
                case AreaID.QuietGrove:
                    return MusicTrack.QuietGrove;
                default:
                    return MusicTrack.Path;
            }
        }

        AudioClip GetClipForTrack(MusicTrack track)
        {
            switch (track)
            {
                case MusicTrack.Town: return townMusic;
                case MusicTrack.Path: return pathMusic;
                case MusicTrack.QuietGrove: return quietGroveMusic;
                default: return null;
            }
        }

        public void PlayTrack(MusicTrack track)
        {
            _currentTrack = track;
            var clip = GetClipForTrack(track);

            if (clip == null)
            {
                // Silence is allowed between loops
                _musicSource.Stop();
                return;
            }

            if (_musicSource.clip == clip && _musicSource.isPlaying)
                return;

            _musicSource.clip = clip;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.Stop();
            _currentTrack = MusicTrack.None;
        }

        // --- SFX ---

        public void PlaySFX(AudioClip clip)
        {
            if (clip != null)
                _sfxSource.PlayOneShot(clip);
        }

        public void PlayStep() => PlaySFX(sfxStep);
        public void PlayTap() => PlaySFX(sfxTap);
        public void PlayChime() => PlaySFX(sfxChime);
        public void PlayWaterClick() => PlaySFX(sfxWaterClick);
        public void PlayMenuSelect() => PlaySFX(sfxMenuSelect);
        public void PlayMenuMove() => PlaySFX(sfxMenuMove);
        public void PlayHit() => PlaySFX(sfxHit);
        public void PlayRest() => PlaySFX(sfxRest);

        /// <summary>
        /// Play the appropriate creature SFX based on type.
        /// </summary>
        public void PlayCreatureSFX(CreatureType type)
        {
            switch (type)
            {
                case CreatureType.Field:
                    PlayTap();
                    break;
                case CreatureType.Water:
                    PlayWaterClick();
                    break;
                case CreatureType.Wind:
                case CreatureType.Meadow:
                    PlayChime();
                    break;
            }
        }
    }
}
