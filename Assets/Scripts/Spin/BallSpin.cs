using UnityEngine;

namespace Billiards.Spin
{
    /// <summary>
    /// Manages ball spin state: topspin, backspin, sidespin.
    /// Works alongside BallPhysics to transfer spin into linear velocity.
    /// All spin logic runs in FixedUpdate.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BallSpin : MonoBehaviour
    {
        // === Constants ===
        private const float BallRadius = 0.028575f;

        [Header("Realism Tuning")]
        [Tooltip("Sliding friction coefficient while ball transitions from slip to roll")]
        [SerializeField] private float slidingFrictionCoefficient = 0.12f;

        [Tooltip("Base spin decay rate from cloth + micro losses")]
        [SerializeField] private float spinDecayRate = 0.08f;

        [Tooltip("Extra decay on sidespin (english)")]
        [SerializeField] private float sideSpinExtraDecayRate = 0.16f;

        [Tooltip("Angular speed below this is treated as zero")]
        [SerializeField] private float minAngularThreshold = 0.008f;

        [Tooltip("Linear speed below this is treated as near-still")]
        [SerializeField] private float minVelocityThreshold = 0.0008f;

        // === Cached References ===
        private Rigidbody rb;

        // === Spin State (injected by CueStrike, read by other systems) ===
        private Vector3 appliedSpin; // extra spin from cue strike offset

        // === Public Read Accessors ===
        /// <summary>Forward/back spin component (positive = topspin, negative = backspin)</summary>
        public float TopBackSpin
        {
            get
            {
                // Topspin is angular velocity around the horizontal axis perpendicular to travel
                Vector3 velocity = rb.linearVelocity;
                if (velocity.magnitude < minVelocityThreshold)
                    return 0f;

                Vector3 forward = velocity.normalized;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

                // Topspin rotates around the right axis (perpendicular to travel direction)
                return Vector3.Dot(rb.angularVelocity, right);
            }
        }

        /// <summary>Sidespin component (english)</summary>
        public float SideSpin => rb.angularVelocity.y;

        /// <summary>Total angular speed</summary>
        public float TotalSpinSpeed => rb.angularVelocity.magnitude;

        /// <summary>Whether ball has significant spin</summary>
        public bool HasSpin => TotalSpinSpeed > minAngularThreshold;

        public Vector3 AppliedSpin => appliedSpin;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (rb.linearVelocity.magnitude < minVelocityThreshold &&
                rb.angularVelocity.magnitude < minAngularThreshold)
                return;

            ApplySlidingFriction();
            ApplySpinDecay();
        }

        /// <summary>
        /// Sliding friction: when the ball is sliding (not pure rolling),
        /// apply friction to transition toward pure roll and transfer spin to velocity.
        /// </summary>
        private void ApplySlidingFriction()
        {
            Vector3 velocity = rb.linearVelocity;
            Vector3 angularVel = rb.angularVelocity;

            // Contact point is at the bottom of the ball
            Vector3 contactOffset = Vector3.down * BallRadius;
            Vector3 surfaceVelocity = Vector3.Cross(angularVel, contactOffset);

            // Slip velocity = linear velocity - surface velocity at contact
            Vector3 slipVelocity = velocity - surfaceVelocity;
            slipVelocity.y = 0f;

            float slipSpeed = slipVelocity.magnitude;
            if (slipSpeed < minVelocityThreshold)
                return;

            // Friction force: F = mu * m * g
            float frictionForce = slidingFrictionCoefficient * rb.mass * Mathf.Abs(UnityEngine.Physics.gravity.y);
            Vector3 frictionDir = -slipVelocity.normalized;

            // Don't overshoot the slip correction
            float maxForce = slipSpeed * rb.mass / Time.fixedDeltaTime;
            float appliedForce = Mathf.Min(frictionForce, maxForce);

            // Apply friction force to linear velocity
            rb.AddForce(frictionDir * appliedForce, ForceMode.Force);

            // Apply torque from friction to angular velocity
            Vector3 torque = Vector3.Cross(contactOffset, frictionDir * appliedForce);
            rb.AddTorque(torque, ForceMode.Force);
        }

        /// <summary>
        /// General spin decay due to air resistance and micro-friction.
        /// </summary>
        private void ApplySpinDecay()
        {
            Vector3 angVel = rb.angularVelocity;
            if (angVel.magnitude < minAngularThreshold)
            {
                rb.angularVelocity = Vector3.zero;
                appliedSpin = Vector3.zero;
                return;
            }

            float baseDecay = Mathf.Clamp01(1f - spinDecayRate * Time.fixedDeltaTime);
            float sideDecay = Mathf.Clamp01(1f - sideSpinExtraDecayRate * Time.fixedDeltaTime);

            Vector3 decayed = angVel * baseDecay;
            decayed.y *= sideDecay;

            rb.angularVelocity = decayed;
            appliedSpin = rb.angularVelocity;
        }

        /// <summary>
        /// Inject spin from cue strike. Called by CueStrike.cs.
        /// Offset from center determines spin type:
        ///   - Below center → backspin (draw)
        ///   - Above center → topspin (follow)
        ///   - Left/right of center → sidespin (english)
        /// </summary>
        public void InjectSpin(Vector3 spinVector)
        {
            rb.angularVelocity += spinVector;
            appliedSpin = rb.angularVelocity;
        }

        /// <summary>
        /// Clear all spin state.
        /// </summary>
        public void ClearSpin()
        {
            appliedSpin = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        /// <summary>
        /// Synchronize tracked spin state from Rigidbody angular velocity.
        /// Useful after external systems (e.g., rail response) modify spin directly.
        /// </summary>
        public void SyncSpinStateFromRigidbody()
        {
            appliedSpin = rb.angularVelocity;
        }
    }
}
