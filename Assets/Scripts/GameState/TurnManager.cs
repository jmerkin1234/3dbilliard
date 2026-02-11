using UnityEngine;

namespace Billiards.GameState
{
    /// <summary>
    /// Manages turn flow, player switching, shot validation, and fouls.
    /// Listens to BallSleepMonitor.OnAllBallsStopped to progress turns.
    /// Controls input enable/disable for CueAim and ShotPower.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [Header("Player Configuration")]
        [Tooltip("Current active player (1 or 2)")]
        [SerializeField] private int currentPlayer = 1;

        [Header("Input Control")]
        [Tooltip("Cue aim component to control")]
        [SerializeField] private Cue.CueAim cueAim;

        [Tooltip("Shot power component to control")]
        [SerializeField] private Cue.ShotPower shotPower;

        [Header("Turn State")]
        [SerializeField] private TurnState currentState = TurnState.Aiming;

        [Header("Debug")]
        [SerializeField] private bool logTurnEvents = true;

        // === Events ===
        /// <summary>Fired when turn changes</summary>
        public static event System.Action<int> OnTurnChanged;

        /// <summary>Fired when foul occurs</summary>
        public static event System.Action<int, string> OnFoul;

        // === Properties ===
        public int CurrentPlayer => currentPlayer;
        public TurnState CurrentState => currentState;

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
        }

        private void Start()
        {
            // Initialize turn
            SetTurnState(TurnState.Aiming);
            EnableInput(true);

            if (logTurnEvents)
            {
                UnityEngine.Debug.Log($"[TurnManager] Game started. Player {currentPlayer}'s turn.", this);
            }
        }

        /// <summary>
        /// Called when shot is released.
        /// Transitions from Aiming to BallsMoving state.
        /// </summary>
        private void HandleShotReleased(float impulse)
        {
            if (currentState != TurnState.Aiming)
                return;

            SetTurnState(TurnState.BallsMoving);
            EnableInput(false);

            if (logTurnEvents)
            {
                UnityEngine.Debug.Log($"[TurnManager] Player {currentPlayer} shot with {impulse:F2}N. Balls moving...", this);
            }
        }

        /// <summary>
        /// Called when all balls stop moving.
        /// Validates shot and determines next turn.
        /// </summary>
        private void HandleAllBallsStopped()
        {
            if (currentState != TurnState.BallsMoving)
                return;

            SetTurnState(TurnState.TurnEnding);

            // Validate shot via rule engine
            bool shotWasLegal = ValidateShot();

            if (shotWasLegal)
            {
                if (logTurnEvents)
                {
                    UnityEngine.Debug.Log($"[TurnManager] Player {currentPlayer} legal shot. Continuing turn.", this);
                }
                // Continue current player's turn
                StartNextTurn(currentPlayer);
            }
            else
            {
                if (logTurnEvents)
                {
                    UnityEngine.Debug.Log($"[TurnManager] Player {currentPlayer} foul. Switching turn.", this);
                }
                // Switch to other player
                SwitchPlayer();
                StartNextTurn(currentPlayer);
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
                if (logTurnEvents)
                {
                    UnityEngine.Debug.Log($"[TurnManager] SCRATCH! Cue ball pocketed by Player {currentPlayer}.", this);
                }

                OnFoul?.Invoke(currentPlayer, "Scratch - Cue ball pocketed");
            }
        }

        /// <summary>
        /// Validates the shot based on game rules.
        /// Returns true if legal, false if foul.
        /// </summary>
        private bool ValidateShot()
        {
            // Basic validation for now
            // TODO: Integrate with RuleEngine for advanced validation

            // For now, assume all shots are legal unless scratch occurred
            // Scratch is handled in HandleBallPocketed
            return true;
        }

        /// <summary>
        /// Switches to the other player.
        /// </summary>
        private void SwitchPlayer()
        {
            currentPlayer = (currentPlayer == 1) ? 2 : 1;
            OnTurnChanged?.Invoke(currentPlayer);

            if (logTurnEvents)
            {
                UnityEngine.Debug.Log($"[TurnManager] Turn switched to Player {currentPlayer}.", this);
            }
        }

        /// <summary>
        /// Starts the next turn for the given player.
        /// </summary>
        private void StartNextTurn(int player)
        {
            currentPlayer = player;
            SetTurnState(TurnState.Aiming);
            EnableInput(true);

            if (logTurnEvents)
            {
                UnityEngine.Debug.Log($"[TurnManager] Player {currentPlayer}'s turn begins.", this);
            }
        }

        /// <summary>
        /// Sets the current turn state.
        /// </summary>
        private void SetTurnState(TurnState newState)
        {
            currentState = newState;
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
            // - Reset turn to Player 1

            currentPlayer = 1;
            SetTurnState(TurnState.Aiming);
            EnableInput(true);
            OnTurnChanged?.Invoke(currentPlayer);
        }

        /// <summary>
        /// Manually trigger player switch (for testing or game master control).
        /// </summary>
        public void ForcePlayerSwitch()
        {
            SwitchPlayer();
            StartNextTurn(currentPlayer);
        }
    }

    /// <summary>
    /// Turn state machine states.
    /// </summary>
    public enum TurnState
    {
        Aiming,         // Player is aiming and charging shot
        BallsMoving,    // Balls are in motion after shot
        TurnEnding,     // Validating shot and determining next turn
        GameOver        // Game has ended
    }
}
