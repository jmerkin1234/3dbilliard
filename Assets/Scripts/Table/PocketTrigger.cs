using System;
using UnityEngine;

namespace Billiards.Table
{
    /// <summary>
    /// Detects when a ball enters a pocket.
    /// Disables the ball's physics and renderer, then notifies GameState.
    /// Uses trigger colliders to prevent ghost collisions.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PocketTrigger : MonoBehaviour
    {
        // === Events ===
        /// <summary>Fired when a ball enters this pocket.</summary>
        public static event Action<GameObject, PocketTrigger> OnBallPocketed;

        [Header("Pocket Configuration")]
        [Tooltip("Tag used to identify balls (must match ball GameObjects)")]
        [SerializeField] private string ballTag = "Ball";

        [Tooltip("Delay before disabling ball (allows entry animation)")]
        [SerializeField] private float disableDelay = 0.1f;

        [Header("Debug")]
        [SerializeField] private bool logPocketEvents = true;

        // === State ===
        private Collider triggerCollider;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider>();

            // Ensure this is configured as a trigger
            if (!triggerCollider.isTrigger)
            {
                UnityEngine.Debug.LogWarning($"[PocketTrigger] {gameObject.name} collider is not set as Trigger. Auto-enabling.", this);
                triggerCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Only process balls (check both Ball and CueBall tags)
            if (!other.CompareTag(ballTag) && !other.CompareTag("CueBall"))
                return;

            GameObject ball = other.gameObject;

            // Prevent double-triggering from the same ball
            if (!ball.activeInHierarchy)
                return;

            // Get ball physics component (required to confirm it's a valid ball)
            Physics.BallPhysics ballPhysics = ball.GetComponent<Physics.BallPhysics>();
            if (ballPhysics == null)
                return;

            if (logPocketEvents)
            {
                UnityEngine.Debug.Log($"[PocketTrigger] Ball '{ball.name}' entered pocket '{gameObject.name}'", this);
            }

            // Fire event immediately
            OnBallPocketed?.Invoke(ball, this);

            // Disable ball after a short delay (allows for visual/audio feedback)
            if (disableDelay > 0f)
            {
                Invoke(nameof(DisableBallDelayed), disableDelay);
                // Store ball reference for delayed disable
                ballToDisable = ball;
            }
            else
            {
                DisableBall(ball);
            }
        }

        private GameObject ballToDisable;

        private void DisableBallDelayed()
        {
            if (ballToDisable != null)
            {
                DisableBall(ballToDisable);
                ballToDisable = null;
            }
        }

        /// <summary>
        /// Disables the ball's physics, renderer, and collider to remove it from play.
        /// </summary>
        private void DisableBall(GameObject ball)
        {
            if (ball == null || !ball.activeInHierarchy)
                return;

            // Stop all motion
            Physics.BallPhysics ballPhysics = ball.GetComponent<Physics.BallPhysics>();
            if (ballPhysics != null)
            {
                ballPhysics.StopMotion();
            }

            Spin.BallSpin ballSpin = ball.GetComponent<Spin.BallSpin>();
            if (ballSpin != null)
            {
                ballSpin.ClearSpin();
            }

            // Disable Rigidbody (prevents further physics interactions)
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            // Disable collider (prevents ghost collisions)
            Collider ballCollider = ball.GetComponent<Collider>();
            if (ballCollider != null)
            {
                ballCollider.enabled = false;
            }

            // Disable renderer (hide the ball)
            Renderer ballRenderer = ball.GetComponent<Renderer>();
            if (ballRenderer != null)
            {
                ballRenderer.enabled = false;
            }

            // Unregister from BallSleepMonitor if it exists
            Physics.BallSleepMonitor sleepMonitor = FindAnyObjectByType<Physics.BallSleepMonitor>();
            if (sleepMonitor != null && ballPhysics != null)
            {
                sleepMonitor.UnregisterBall(ballPhysics);
            }

            if (logPocketEvents)
            {
                UnityEngine.Debug.Log($"[PocketTrigger] Ball '{ball.name}' disabled", this);
            }

            // Optionally: Move ball far away or destroy it
            // For now, we just disable it in place
            ball.transform.position = new Vector3(0, -10, 0); // move below table
        }

        private void OnDrawGizmos()
        {
            // Visualize pocket trigger zone in editor
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Gizmos.DrawWireSphere(col.bounds.center, col.bounds.extents.magnitude);
            }
        }
    }
}
