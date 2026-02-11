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
        private const float SpinDecayRate = 0.5f; // angular velocity decay per second
        private const float SpinTransferRate = 0.15f; // how aggressively spin converts to velocity
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

            ApplySpinDecay();
            ApplySpinToVelocityTransfer();
        }

        /// <summary>
        /// Spin decays over time due to friction between ball and cloth.
        /// This is separate from the sliding friction in BallPhysics.
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

            // Exponential decay: omega *= (1 - decay * dt)
            float decayFactor = 1f - SpinDecayRate * Time.fixedDeltaTime;
            decayFactor = Mathf.Clamp01(decayFactor);
            rb.angularVelocity = angVel * decayFactor;

            // Decay applied spin tracking
            appliedSpin *= decayFactor;
        }

        /// <summary>
        /// Transfers spin differential into linear velocity.
        /// 
        /// Backspin (negative topspin): when ball is moving forward but spinning backward,
        ///   the spin opposes motion, eventually reversing the ball (draw shot).
        /// 
        /// Topspin (positive topspin): spin adds to forward motion (follow shot).
        /// 
        /// Sidespin: deflects the ball laterally (english/throw).
        /// </summary>
        private void ApplySpinToVelocityTransfer()
        {
            Vector3 velocity = rb.linearVelocity;
            Vector3 angVel = rb.angularVelocity;

            // Calculate surface velocity at the contact point
            Vector3 contactOffset = Vector3.down * BallRadius;
            Vector3 surfaceVelocity = Vector3.Cross(angVel, contactOffset);

            // The difference between surface velocity and linear velocity
            Vector3 spinDiff = surfaceVelocity - velocity;

            // Only horizontal component matters for table contact
            spinDiff.y = 0f;

            if (spinDiff.magnitude < MinVelocityThreshold)
                return;

            // Transfer spin difference into linear velocity
            Vector3 transfer = spinDiff * SpinTransferRate * Time.fixedDeltaTime;
            rb.linearVelocity += transfer;
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
