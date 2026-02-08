using UnityEngine;
using System.Collections;

namespace PartyArmWrestling
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;

        [Header("Music Tracks")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip characterSelectMusic;
        [SerializeField] private AudioClip battleMusic;
        [SerializeField] private AudioClip victoryMusic;

        [Header("UI SFX")]
        [SerializeField] private AudioClip buttonClickSFX;
        [SerializeField] private AudioClip buttonHoverSFX;
        [SerializeField] private AudioClip selectSFX;
        [SerializeField] private AudioClip confirmSFX;

        [Header("Gameplay SFX")]
        [SerializeField] private AudioClip mashSFX;
        [SerializeField] private AudioClip roundStartSFX;
        [SerializeField] private AudioClip roundEndSFX;
        [SerializeField] private AudioClip matchWinSFX;

        [Header("Ability SFX")]
        [SerializeField] private AudioClip powerSurgeSFX;
        [SerializeField] private AudioClip flashSFX;
        [SerializeField] private AudioClip trashTalkSFX;
        [SerializeField] private AudioClip shieldActivateSFX;
        [SerializeField] private AudioClip shieldBlockSFX;
        [SerializeField] private AudioClip winkFlirtSFX;
        [SerializeField] private AudioClip danceSFX;
        [SerializeField] private AudioClip cakeTossSFX;
        [SerializeField] private AudioClip fakeOutSFX;

        [Header("Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float voiceVolume = 1f;
        [SerializeField] private float musicFadeDuration = 1f;

        private GameManager gameManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            gameManager = GameManager.Instance;

            if (gameManager != null)
            {
                gameManager.OnStateChanged += HandleStateChanged;
                gameManager.OnRoundStart += HandleRoundStart;
                gameManager.OnRoundEnd += HandleRoundEnd;
                gameManager.OnMatchEnd += HandleMatchEnd;
            }

            UpdateVolumes();
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnStateChanged -= HandleStateChanged;
                gameManager.OnRoundStart -= HandleRoundStart;
                gameManager.OnRoundEnd -= HandleRoundEnd;
                gameManager.OnMatchEnd -= HandleMatchEnd;
            }
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null || clip == null) return;
            StartCoroutine(CrossfadeMusic(clip, loop));
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip, bool loop)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < musicFadeDuration / 2f)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (musicFadeDuration / 2f));
                yield return null;
            }

            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.Play();

            elapsed = 0f;
            float targetVolume = musicVolume * masterVolume;

            while (elapsed < musicFadeDuration / 2f)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (musicFadeDuration / 2f));
                yield return null;
            }

            musicSource.volume = targetVolume;
        }

        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
            }
        }

        public void PlayAbilitySFX(AbilityType type)
        {
            AudioClip clip = type switch
            {
                AbilityType.PowerSurge => powerSurgeSFX,
                AbilityType.Flash => flashSFX,
                AbilityType.TrashTalk => trashTalkSFX,
                AbilityType.Shield => shieldActivateSFX,
                AbilityType.WinkFlirt => winkFlirtSFX,
                AbilityType.Dance => danceSFX,
                AbilityType.CakeToss => cakeTossSFX,
                AbilityType.FakeOut => fakeOutSFX,
                _ => null
            };
            PlaySFX(clip);
        }

        public void PlayVoiceLine(AudioClip clip)
        {
            if (voiceSource != null && clip != null)
            {
                voiceSource.Stop();
                voiceSource.clip = clip;
                voiceSource.volume = voiceVolume * masterVolume;
                voiceSource.Play();
            }
        }

        public void PlayCharacterVoiceLine(CharacterData character, VoiceLineCategory category)
        {
            if (character == null) return;
            AudioClip clip = character.GetRandomVoiceLine(category);
            if (clip != null) PlayVoiceLine(clip);
        }

        private void HandleStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.CharacterSelect:
                    PlayMusic(characterSelectMusic);
                    break;
                case GameState.PreRound:
                case GameState.Playing:
                    if (musicSource.clip != battleMusic)
                        PlayMusic(battleMusic);
                    break;
                case GameState.MatchEnd:
                    PlayMusic(victoryMusic, false);
                    break;
            }
        }

        private void HandleRoundStart(int round) => PlaySFX(roundStartSFX);

        private void HandleRoundEnd(int winner, int round)
        {
            PlaySFX(roundEndSFX);
            CharacterData winnerCharacter = winner == 1 ? gameManager.Player1Character : gameManager.Player2Character;
            if (winnerCharacter != null)
                PlayCharacterVoiceLine(winnerCharacter, VoiceLineCategory.Victory);
        }

        private void HandleMatchEnd(int winner) => PlaySFX(matchWinSFX);

        private void UpdateVolumes()
        {
            if (musicSource != null) musicSource.volume = musicVolume * masterVolume;
            if (sfxSource != null) sfxSource.volume = sfxVolume * masterVolume;
            if (voiceSource != null) voiceSource.volume = voiceVolume * masterVolume;
        }

        public void SetMasterVolume(float volume) { masterVolume = Mathf.Clamp01(volume); UpdateVolumes(); }
        public void SetMusicVolume(float volume) { musicVolume = Mathf.Clamp01(volume); UpdateVolumes(); }
        public void SetSFXVolume(float volume) { sfxVolume = Mathf.Clamp01(volume); UpdateVolumes(); }
        public void SetVoiceVolume(float volume) { voiceVolume = Mathf.Clamp01(volume); UpdateVolumes(); }
    }

    public enum UISoundType
    {
        ButtonClick,
        ButtonHover,
        Select,
        Confirm
    }
}
