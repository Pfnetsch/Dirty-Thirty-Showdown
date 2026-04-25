using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace DirtyThirtyShowdown
{
    public enum GameState
    {
        SplashScreen,
        TitleScreen,
        CharacterSelect,
        PreRound,
        Playing,
        RoundEnd,
        MatchEnd
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Match Settings")]
        [SerializeField] private int roundsToWin = 2;
        [SerializeField] private float roundTimeLimit = 45f;
        [SerializeField] private float preRoundDelay = 4f;
        [SerializeField] private float roundEndDelay = 2f;

        [Header("References")]
        [SerializeField] private ArmWrestleController armWrestleController;
        [SerializeField] private UIManager uiManager;

        // Match state
        public GameState CurrentState { get; private set; } = GameState.SplashScreen;
        public int Player1Score { get; private set; } = 0;
        public int Player2Score { get; private set; } = 0;
        public int CurrentRound { get; private set; } = 1;
        public float RoundTimer { get; private set; }

        // Selected characters
        public CharacterData Player1Character { get; private set; }
        public CharacterData Player2Character { get; private set; }

        // Events
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnRoundStart;
        public event Action<int, int> OnRoundEnd; // winner (1 or 2), round number
        public event Action<int> OnMatchEnd; // winner (1 or 2)
        public event Action<float> OnTimerUpdate;
        public event Action<bool> OnEscConfirmChanged; // true = waiting for second ESC, false = cancelled/confirmed

        // ESC cancel state
        private bool _escConfirmPending;
        private float _escConfirmTimer;
        private const float EscConfirmTimeout = 3f;

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
            if (GameConfig.Instance != null)
            {
                roundTimeLimit = GameConfig.Instance.RoundTimeLimit;
                roundsToWin    = GameConfig.Instance.RoundsToWin;
            }
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                UpdateRoundTimer();
                HandleEscInput();
            }
            else if (CurrentState == GameState.PreRound)
            {
                HandleEscInput();
            }
            else if (CurrentState == GameState.MatchEnd)
            {
                HandleMatchEndInput();
            }
        }

        private void HandleEscInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_escConfirmPending)
                {
                    // Second press — cancel the match
                    SetEscConfirmPending(false);
                    ReturnToCharacterSelect();
                }
                else
                {
                    // First press — ask for confirmation
                    SetEscConfirmPending(true);
                    _escConfirmTimer = EscConfirmTimeout;
                }
                return;
            }

            if (_escConfirmPending)
            {
                _escConfirmTimer -= Time.deltaTime;
                if (_escConfirmTimer <= 0f)
                    SetEscConfirmPending(false);
            }
        }

        private void SetEscConfirmPending(bool pending)
        {
            _escConfirmPending = pending;
            OnEscConfirmChanged?.Invoke(pending);
        }

        private void HandleMatchEndInput()
        {
            // Space or Enter = Rematch
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                RestartMatch();
            }
            // Escape or Backspace = Return to character select
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            {
                ReturnToCharacterSelect();
            }
        }

        private void UpdateRoundTimer()
        {
            RoundTimer -= Time.deltaTime;
            OnTimerUpdate?.Invoke(RoundTimer);

            if (RoundTimer <= 0)
            {
                RoundTimer = 0;
                EndRoundByTimeout();
            }
        }

        public void SetCharacterSelections(CharacterData player1, CharacterData player2)
        {
            Player1Character = player1;
            Player2Character = player2;
        }

        public void StartMatch()
        {
            Player1Score = 0;
            Player2Score = 0;
            CurrentRound = 1;
            StartRound();
        }

        public void StartRound()
        {
            ChangeState(GameState.PreRound);
            RoundTimer = roundTimeLimit;
            
            if (armWrestleController != null)
            {
                armWrestleController.ResetBar();
            }

            Invoke(nameof(BeginRound), preRoundDelay);
        }

        private void BeginRound()
        {
            ChangeState(GameState.Playing);
            OnRoundStart?.Invoke(CurrentRound);
        }

        public void EndRound(int winner)
        {
            if (CurrentState != GameState.Playing) return;

            ChangeState(GameState.RoundEnd);

            if (winner == 1)
                Player1Score++;
            else if (winner == 2)
                Player2Score++;

            OnRoundEnd?.Invoke(winner, CurrentRound);

            // Check for match winner
            if (Player1Score >= roundsToWin)
            {
                Invoke(nameof(EndMatchPlayer1Wins), roundEndDelay);
            }
            else if (Player2Score >= roundsToWin)
            {
                Invoke(nameof(EndMatchPlayer2Wins), roundEndDelay);
            }
            else
            {
                CurrentRound++;
                Invoke(nameof(StartRound), roundEndDelay);
            }
        }

        private void EndRoundByTimeout()
        {
            if (armWrestleController == null) return;

            float barPosition = armWrestleController.BarPosition;
            
            // Determine winner based on bar position
            // Negative = closer to P1 winning, Positive = closer to P2 winning
            int winner = barPosition < 0 ? 1 : 2;
            
            // If exactly neutral, random winner (unlikely but handle edge case)
            if (Mathf.Approximately(barPosition, 0f))
            {
                winner = UnityEngine.Random.Range(0, 2) == 0 ? 1 : 2;
            }

            EndRound(winner);
        }

        private void EndMatchPlayer1Wins() => EndMatch(1);
        private void EndMatchPlayer2Wins() => EndMatch(2);

        private void EndMatch(int winner)
        {
            ChangeState(GameState.MatchEnd);
            OnMatchEnd?.Invoke(winner);
        }

        public void GoToTitleScreen()
        {
            Player1Character = null;
            Player2Character = null;
            ChangeState(GameState.TitleScreen);
        }

        public void GoToCharacterSelect()
        {
            ChangeState(GameState.CharacterSelect);
        }

        public void ReturnToCharacterSelect()
        {
            CancelInvoke();
            SetEscConfirmPending(false);
            Player1Character = null;
            Player2Character = null;
            ChangeState(GameState.CharacterSelect);
        }

        public void RestartMatch()
        {
            StartMatch();
        }

        private void ChangeState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        public bool IsPlaying => CurrentState == GameState.Playing;
    }
}
