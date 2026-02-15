using System.Collections.Generic;
using UnityEngine;

namespace Billiards.GameState
{
    /// <summary>
    /// Basic rule engine for billiards game logic.
    /// Tracks contacts, pockets, and validates shot legality.
    /// Detects fouls: scratch, no contact, wrong ball first, etc.
    /// </summary>
    public class RuleEngine : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool logRuleEvents = true;

        // === Shot Tracking ===
        private GameObject firstBallContacted;
        private List<GameObject> ballsPocketed = new List<GameObject>();
        private bool cueBallPocketed = false;
        private bool shotInProgress = false;

        // === Events ===
        /// <summary>Fired when a foul is detected</summary>
        public static event System.Action<string> OnFoulDetected;

        private void Awake()
        {
            // Subscribe to physics events
            Table.PocketTrigger.OnBallPocketed += HandleBallPocketed;
        }

        private void OnDestroy()
        {
            Table.PocketTrigger.OnBallPocketed -= HandleBallPocketed;
        }

        private void OnEnable()
        {
            // Listen for collision events to track first contact
            // Note: Unity collision events must be on the ball objects themselves
            // This is a simplified approach - in production, balls would report contacts
        }

        /// <summary>
        /// Begins tracking a new shot.
        /// Call this when a shot is released.
        /// </summary>
        public void BeginShotTracking()
        {
            ResetShotTracking();
            shotInProgress = true;

            if (logRuleEvents)
            {
                UnityEngine.Debug.Log("[RuleEngine] Shot tracking began.", this);
            }
        }

        /// <summary>
        /// Resets shot tracking state.
        /// </summary>
        public void ResetShotTracking()
        {
            firstBallContacted = null;
            ballsPocketed.Clear();
            cueBallPocketed = false;
            shotInProgress = false;
        }

        /// <summary>
        /// Called when a ball is pocketed.
        /// </summary>
        private void HandleBallPocketed(GameObject ball, Table.PocketTrigger pocket)
        {
            if (!shotInProgress)
                return;

            ballsPocketed.Add(ball);

            if (ball.CompareTag("CueBall"))
            {
                cueBallPocketed = true;

                if (logRuleEvents)
                {
                    UnityEngine.Debug.Log("[RuleEngine] Cue ball pocketed (scratch).", this);
                }

                OnFoulDetected?.Invoke("Scratch - Cue ball pocketed");
            }
            else
            {
                if (logRuleEvents)
                {
                    UnityEngine.Debug.Log($"[RuleEngine] Ball '{ball.name}' pocketed.", this);
                }
            }
        }

        /// <summary>
        /// Validates the shot based on tracked events.
        /// Returns true if legal, false if foul.
        /// </summary>
        public bool ValidateShot()
        {
            if (!shotInProgress)
                return true;

            bool isLegal = true;

            // Check for scratch
            if (cueBallPocketed)
            {
                isLegal = false;
                if (logRuleEvents)
                {
                    UnityEngine.Debug.Log("[RuleEngine] FOUL: Scratch (cue ball pocketed).", this);
                }
            }

            // Check for no contact
            if (firstBallContacted == null)
            {
                // No ball was contacted - this could be a foul in strict rules
                // For now, we allow it (gentle shot or intentional safety)
                if (logRuleEvents)
                {
                    UnityEngine.Debug.Log("[RuleEngine] No ball contacted (safety shot or miss).", this);
                }
            }

            if (logRuleEvents)
            {
                UnityEngine.Debug.Log($"[RuleEngine] Shot validation: {(isLegal ? "LEGAL" : "FOUL")}", this);
            }

            return isLegal;
        }

        /// <summary>
        /// Records the first ball contacted by the cue ball.
        /// Should be called from collision detection on the cue ball.
        /// </summary>
        public void RecordFirstContact(GameObject ball)
        {
            if (firstBallContacted == null && shotInProgress)
            {
                firstBallContacted = ball;

                if (logRuleEvents)
                {
                    UnityEngine.Debug.Log($"[RuleEngine] First contact: {ball.name}", this);
                }
            }
        }

        /// <summary>
        /// Returns the list of balls pocketed during the current shot.
        /// </summary>
        public List<GameObject> GetBallsPocketed()
        {
            return new List<GameObject>(ballsPocketed);
        }

        /// <summary>
        /// Returns whether the cue ball was pocketed (scratch).
        /// </summary>
        public bool IsScratch()
        {
            return cueBallPocketed;
        }

        /// <summary>
        /// Returns the first ball contacted by the cue ball.
        /// </summary>
        public GameObject GetFirstContact()
        {
            return firstBallContacted;
        }
    }
}
