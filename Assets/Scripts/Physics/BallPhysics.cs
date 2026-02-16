using UnityEngine;

namespace Billiards.Physics
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class BallPhysics : MonoBehaviour
    {
        // === Constants ===
        private const float BallMass = 0.17f;
        private const float BallRadius = 0.028575f; // regulation ball radius in meters

        [Header("Realism Tuning")]
        [Tooltip("Rolling resistance coefficient for cloth (higher = quicker slowdown)")]
        [SerializeField] private float rollingResistanceCoefficient = 0.0075f;

        [Tooltip("Below this speed, horizontal velocity is clamped to zero")]
        [SerializeField] private float minVelocityThreshold = 0.0008f;

        // === Cached References ===
        private Rigidbody rb;
        private SphereCollider sphereCollider;

        // === Public Read Accessors ===
        public Vector3 Velocity => rb != null ? rb.linearVelocity : Vector3.zero;
        public Vector3 AngularVelocity => rb != null ? rb.angularVelocity : Vector3.zero;
        public float Speed => Velocity.magnitude;
        public bool IsMoving => Speed > minVelocityThreshold;
        public Rigidbody Rb => rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            sphereCollider = GetComponent<SphereCollider>();
            ConfigureRigidbody();
        }

        private void ConfigureRigidbody()
        {
            rb.mass = BallMass;
            rb.linearDamping = 0f; // we handle friction manually
            rb.angularDamping = 0f; // we handle spin decay manually
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.useGravity = true;

            sphereCollider.radius = BallRadius;
        }

        private void FixedUpdate()
        {
            if (!IsMoving)
                return;

            ApplyRollingResistance();
            ClampMinimumVelocity();
        }

        /// <summary>
        /// Rolling resistance opposes the direction of motion.
        /// F_roll = mu_roll * m * g
        /// Applied as deceleration opposite to velocity direction.
        /// </summary>
        private void ApplyRollingResistance()
        {
            Vector3 velocity = rb.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            float speed = horizontalVelocity.magnitude;

            if (speed < minVelocityThreshold)
                return;

            // Rolling resistance force: mu * m * g
            float resistanceForce = rollingResistanceCoefficient * rb.mass * Mathf.Abs(UnityEngine.Physics.gravity.y);

            // Deceleration = F / m = mu * g
            float deceleration = resistanceForce / rb.mass;

            // Don't overshoot — cap deceleration so we don't reverse direction
            float speedReduction = deceleration * Time.fixedDeltaTime;
            if (speedReduction > speed)
            {
                rb.linearVelocity = new Vector3(0f, velocity.y, 0f);
                return;
            }

            // Apply opposite to velocity direction
            Vector3 resistanceDir = -horizontalVelocity.normalized;
            rb.AddForce(resistanceDir * resistanceForce, ForceMode.Force);
        }

        /// <summary>
        /// If velocity drops below minimum threshold, force it to zero
        /// to prevent infinite micro-sliding.
        /// </summary>
        private void ClampMinimumVelocity()
        {
            if (new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude < minVelocityThreshold)
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }

            if (rb.angularVelocity.magnitude < minVelocityThreshold)
            {
                rb.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// External force application (used by CueStrike later).
        /// </summary>
        public void ApplyImpulse(Vector3 force)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }

        /// <summary>
        /// Stop all motion immediately.
        /// </summary>
        public void StopMotion()
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
