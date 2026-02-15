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
        private const float BallMass = 0.17f;
        private const float SlidingFrictionCoefficient = 0.2f; // cloth friction
        private const float SpinDecayRate = 0.3f; // additional angular decay for sidespin
        private const float MinAngularThreshold = 0.01f;
        private const float MinVelocityThreshold = 0.001f;

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
                if (velocity.magnitude < MinVelocityThreshold)
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
        public bool HasSpin => TotalSpinSpeed > MinAngularThreshold;

        public Vector3 AppliedSpin => appliedSpin;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (rb.linearVelocity.magnitude < MinVelocityThreshold &&
                rb.angularVelocity.magnitude < MinAngularThreshold)
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
            if (slipSpeed < MinVelocityThreshold)
                return;

            // Friction force: F = mu * m * g
            float frictionForce = SlidingFrictionCoefficient * BallMass * Mathf.Abs(UnityEngine.Physics.gravity.y);
            Vector3 frictionDir = -slipVelocity.normalized;

            // Don't overshoot the slip correction
            float maxForce = slipSpeed * BallMass / Time.fixedDeltaTime;
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
            if (angVel.magnitude < MinAngularThreshold)
            {
                rb.angularVelocity = Vector3.zero;
                appliedSpin = Vector3.zero;
                return;
            }

            float decayFactor = 1f - SpinDecayRate * Time.fixedDeltaTime;
            rb.angularVelocity = angVel * Mathf.Clamp01(decayFactor);
            appliedSpin *= Mathf.Clamp01(decayFactor);
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
            appliedSpin = spinVector;
            rb.angularVelocity += spinVector;
        }

        /// <summary>
        /// Clear all spin state.
        /// </summary>
        public void ClearSpin()
        {
            appliedSpin = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
