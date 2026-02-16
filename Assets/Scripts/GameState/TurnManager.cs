using System.Collections;
using UnityEngine;

namespace Billiards.GameState
{
    /// <summary>
    /// Manages game flow for single-player billiards.
    /// Supports two game modes: Training (unlimited shots) and VsComputer (player vs AI).
    /// Listens to BallSleepMonitor.OnAllBallsStopped to progress turns.
    /// Controls input enable/disable for CueAim and ShotPower.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [Header("Game Mode")]
        [Tooltip("Training = unlimited shots, VsComputer = player vs AI")]
        [SerializeField] private GameMode gameMode = GameMode.Training;

        [Header("Input Control")]
        [Tooltip("Cue aim component to control")]
        [SerializeField] private Cue.CueAim cueAim;

        [Tooltip("Shot power component to control")]
        [SerializeField] private Cue.ShotPower shotPower;

        [Tooltip("Rule engine for shot validation")]
        [SerializeField] private RuleEngine ruleEngine;

        [Header("Turn State")]
        [SerializeField] private TurnState currentState = TurnState.PlayerAiming;

        [Tooltip("Current turn owner (Player or AI)")]
        [SerializeField] private TurnOwner currentTurnOwner = TurnOwner.Player;

        [Header("Debug")]
        [SerializeField] private bool logTurnEvents = true;

        [Header("AI Fallback")]
        [Tooltip("Temporary fallback while AI shot logic is not implemented.")]
        [SerializeField] private bool autoReturnTurnWhenAIUnavailable = true;

        [Tooltip("Delay before returning control to player when no AI logic exists.")]
        [SerializeField] private float aiFallbackDelaySeconds = 0.4f;

        [Header("Recovery")]
        [Tooltip("Safety timeout to force-turn recovery if ball-stop event is missed after a shot.")]
        [SerializeField] private float ballsMovingTimeoutSeconds = 20f;

        [Tooltip("Enable timeout recovery for rare stuck BallsMoving states.")]
        [SerializeField] private bool enableBallsMovingTimeoutRecovery = true;

        private Coroutine ballsMovingTimeoutCoroutine;

        // === Events ===
        /// <summary>Fired when turn changes to AI</summary>
        public static event System.Action OnAITurnStart;

        /// <summary>Fired when turn returns to player</summary>
        public static event System.Action OnPlayerTurnStart;

        /// <summary>Fired when foul occurs</summary>
        public static event System.Action<string> OnFoul;

        // === Properties ===
        public GameMode CurrentGameMode => gameMode;
        public TurnState CurrentState => currentState;
        public TurnOwner CurrentTurnOwner => currentTurnOwner;

        private void Awake()
        {
            // Auto-find components if not assigned
            if (cueAim == null)
            {
                cueAim = FindAnyObjectByType<Cue.CueAim>();
            }

            if (shotPower == null)
            {
                shotPower = FindAnyObjectByType<Cue.ShotPower>();
            }

            if (ruleEngine == null)
            {
                ruleEngine = FindAnyObjectByType<RuleEngine>();
            }

            // Subscribe to ball sleep events
            Physics.BallSleepMonitor.OnAllBallsStopped += HandleAllBallsStopped;

            // Subscribe to shot released event
            if (shotPower != null)
            {
                shotPower.OnShotReleased += HandleShotReleased;
            }

            // Subscribe to pocket events for scratch detection
            Table.PocketTrigger.OnBallPocketed += HandleBallPocketed;
        }

        private void OnDestroy()
        {
            Physics.BallSleepMonitor.OnAllBallsStopped -= HandleAllBallsStopped;

            if (shotPower != null)
            {
                shotPower.OnShotReleased -= HandleShotReleased;
            }

            Table.PocketTrigger.OnBallPocketed -= HandleBallPocketed;

            if (ballsMovingTimeoutCoroutine != null)
            {
                StopCoroutine(ballsMovingTimeoutCoroutine);
                ballsMovingTimeoutCoroutine = null;
            }
        }

        private void Start()
        {
            // Initialize game
            StartPlayerTurn();

            if (logTurnEvents)
            {
                UnityEngine.Debug.Log($"[TurnManager] Game started in {gameMode} mode.", this);
            }
        }

        /// <summary>
        /// Called when shot is released.
        /// Transitions from Aiming to BallsMoving state.
        /// </summary>
        private void HandleShotReleased(float impulse)
        {
            if (currentState != TurnState.PlayerAiming && currentState != TurnState.AIAiming)
                return;

            SetTurnState(TurnState.BallsMoving);
            EnableInput(false);

            if (enableBallsMovingTimeoutRecovery)
            {
                if (ballsMovingTimeoutCoroutine != null)
                {
                    StopCoroutine(ballsMovingTimeoutCoroutine);
                }
                ballsMovingTimeoutCoroutine = StartCoroutine(BallsMovingTimeoutWatchdog());
            }

            // Begin shot tracking in rule engine
            if (ruleEngine != null)
            {
                ruleEngine.BeginShotTracking();
            }

            if (logTurnEvents)
            {
                string shooter = (currentTurnOwner == TurnOwner.Player) ? "Player" : "AI";
                UnityEngine.Debug.Log($"[TurnManager] {shooter} shot with {impulse:F2}N. Balls moving...", this);
            }
        }

        /// <summary>
        /// Called when all balls stop moving.
        /// Validates shot and determines next turn based on game mode.
        /// </summary>
        private void HandleAllBallsStopped()
        {
            if (currentState != TurnState.BallsMoving)
                return;

            if (ballsMovingTimeoutCoroutine != null)
            {
                StopCoroutine(ballsMovingTimeoutCoroutine);
                ballsMovingTimeoutCoroutine = null;
            }

            SetTurnState(TurnState.TurnEnding);

            // Validate shot via rule engine
            bool shotWasLegal = ValidateShot();

            // Reset rule engine for next turn
            if (ruleEngine != null)
            {
                ruleEngine.ResetShotTracking();
            }

            if (gameMode == GameMode.Training)
            {
                // Training mode: Always continue player turn regardless of fouls
                if (logTurnEvents)
                {
                    UnityEngine.Debug.Log("[TurnManager] Training mode - continuing player turn.", this);
                }
                StartPlayerTurn();
            }
            else if (gameMode == GameMode.VsComputer)
            {
                // VsComputer mode: Legal shot = continue, foul = switch turn
                if (shotWasLegal)
                {
                    if (currentTurnOwner == TurnOwner.Player)
                    {
                        if (logTurnEvents)
                        {
                            UnityEngine.Debug.Log("[TurnManager] Player legal shot. Continuing player turn.", this);
                        }
                        StartPlayerTurn();
                    }
                    else // AI's turn
                    {
                        if (logTurnEvents)
                        {
                            UnityEngine.Debug.Log("[TurnManager] AI legal shot. Continuing AI turn.", this);
                        }
                        StartAITurn();
                    }
                }
                else // Foul occurred
                {
                    if (currentTurnOwner == TurnOwner.Player)
                    {
                        if (logTurnEvents)
                        {
                            UnityEngine.Debug.Log("[TurnManager] Player foul. Switching to AI turn.", this);
                        }
                        StartAITurn();
                    }
                    else // AI foul
                    {
                        if (logTurnEvents)
                        {
                            UnityEngine.Debug.Log("[TurnManager] AI foul. Switching to player turn.", this);
                        }
                        StartPlayerTurn();
                    }
                }
            }
        }

        /// <summary>
        /// Called when a ball is pocketed.
        /// Used for scratch detection and pocket tracking.
        /// </summary>
        private void HandleBallPocketed(GameObject ball, Table.PocketTrigger pocket)
        {
            // Check for scratch (cue ball pocketed)
            if (ball.CompareTag("CueBall"))
            {
                string shooter = (currentTurnOwner == TurnOwner.Player) ? "Player" : "AI";

                if (logTurnEvents)
                {
                    UnityEngine.Debug.Log($"[TurnManager] SCRATCH! Cue ball pocketed by {shooter}.", this);
                }

                OnFoul?.Invoke("Scratch - Cue ball pocketed");
            }
        }

        /// <summary>
        /// Validates the shot based on game rules.
        /// Returns true if legal, false if foul.
        /// </summary>
        private bool ValidateShot()
        {
            // Validate shot via rule engine
            if (ruleEngine != null)
            {
                return ruleEngine.ValidateShot();
            }

            // Fallback: Assume all shots are legal unless scratch occurred
            return true;
        }

        /// <summary>
        /// Starts the player's turn.
        /// </summary>
        private void StartPlayerTurn()
        {
            currentTurnOwner = TurnOwner.Player;
            SetTurnState(TurnState.PlayerAiming);
            EnableInput(true);
            OnPlayerTurnStart?.Invoke();

            if (logTurnEvents)
            {
                UnityEngine.Debug.Log("[TurnManager] Player's turn begins.", this);
            }
        }

        /// <summary>
        /// Starts the AI's turn.
        /// </summary>
        private void StartAITurn()
        {
            currentTurnOwner = TurnOwner.AI;
            SetTurnState(TurnState.AIAiming);
            EnableInput(false);
            OnAITurnStart?.Invoke();

            if (logTurnEvents)
            {
                UnityEngine.Debug.Log("[TurnManager] AI's turn begins.", this);
            }

            // TODO: Replace this fallback with real AI shot logic.
            if (autoReturnTurnWhenAIUnavailable)
            {
                StartCoroutine(ReturnPlayerTurnAfterAIFallback());
            }
        }

        private IEnumerator ReturnPlayerTurnAfterAIFallback()
        {
            yield return new WaitForSeconds(aiFallbackDelaySeconds);

            if (currentTurnOwner == TurnOwner.AI && currentState == TurnState.AIAiming)
            {
                if (logTurnEvents)
                {
                    UnityEngine.Debug.LogWarning("[TurnManager] AI shot logic is not implemented yet. Returning turn to player.", this);
                }

                StartPlayerTurn();
            }
        }

        /// <summary>
        /// Sets the current turn state.
        /// </summary>
        private void SetTurnState(TurnState newState)
        {
            currentState = newState;
        }

        private IEnumerator BallsMovingTimeoutWatchdog()
        {
            if (ballsMovingTimeoutSeconds <= 0f)
                yield break;

            yield return new WaitForSeconds(ballsMovingTimeoutSeconds);

            ballsMovingTimeoutCoroutine = null;

            if (currentState != TurnState.BallsMoving)
                yield break;

            Physics.BallSleepMonitor sleepMonitor = FindAnyObjectByType<Physics.BallSleepMonitor>();
            if (sleepMonitor != null && sleepMonitor.MovingBallCount > 0)
                yield break;

            UnityEngine.Debug.LogWarning("[TurnManager] BallsMoving timeout recovery triggered. Forcing turn end.", this);
            HandleAllBallsStopped();
        }

        /// <summary>
        /// Enables or disables input for cue aiming and power.
        /// </summary>
        private void EnableInput(bool enabled)
        {
            if (cueAim != null)
            {
                cueAim.InputEnabled = enabled;
            }

            if (shotPower != null)
            {
                shotPower.InputEnabled = enabled;
            }
        }

        /// <summary>
        /// Resets the rack for a new game.
        /// </summary>
        public void ResetRack()
        {
            if (logTurnEvents)
            {
                UnityEngine.Debug.Log("[TurnManager] Rack reset requested.", this);
            }

            // TODO: Implement rack reset logic
            // - Reposition all balls to starting positions
            // - Reset cue ball
            // - Clear pocketed balls

            StartPlayerTurn();
        }

        /// <summary>
        /// Changes the game mode (Training or VsComputer).
        /// </summary>
        public void SetGameMode(GameMode mode)
        {
            gameMode = mode;

            if (logTurnEvents)
            {
                UnityEngine.Debug.Log($"[TurnManager] Game mode changed to {gameMode}.", this);
            }

            // Reset to player turn when mode changes
            StartPlayerTurn();
        }

        /// <summary>
        /// Manually trigger AI turn (for testing).
        /// </summary>
        public void ForceSwitchToAI()
        {
            if (gameMode == GameMode.VsComputer)
            {
                StartAITurn();
            }
        }
    }

    /// <summary>
    /// Game mode options.
    /// </summary>
    public enum GameMode
    {
        Training,       // Unlimited shots, no turn switching, free practice
        VsComputer      // Player vs AI opponent
    }

    /// <summary>
    /// Turn state machine states.
    /// </summary>
    public enum TurnState
    {
        PlayerAiming,   // Player is aiming and charging shot
        AIAiming,       // AI is calculating and executing shot
        BallsMoving,    // Balls are in motion after shot
        TurnEnding,     // Validating shot and determining next turn
        GameOver        // Game has ended
    }

    /// <summary>
    /// Turn owner (who is currently shooting).
    /// </summary>
    public enum TurnOwner
    {
        Player,
        AI
    }
}
