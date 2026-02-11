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
        private const float RollingResistanceCoefficient = 0.01f; // cloth rolling resistance
        private const float SlidingFrictionCoefficient = 0.2f; // ball-on-cloth sliding friction
        private const float MinVelocityThreshold = 0.001f; // below this, force stop

        // === Cached References ===
        private Rigidbody rb;
        private SphereCollider sphereCollider;

        // === Public Read Accessors ===
        public Vector3 Velocity => rb != null ? rb.linearVelocity : Vector3.zero;
        public Vector3 AngularVelocity => rb != null ? rb.angularVelocity : Vector3.zero;
        public float Speed => Velocity.magnitude;
        public bool IsMoving => Speed > MinVelocityThreshold;
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
            ApplySlidingFriction();
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
            float speed = velocity.magnitude;

            if (speed < MinVelocityThreshold)
                return;

            // Rolling resistance force: mu * m * g
            float resistanceForce = RollingResistanceCoefficient * BallMass * Mathf.Abs(UnityEngine.Physics.gravity.y);

            // Deceleration = F / m = mu * g
            float deceleration = resistanceForce / BallMass;

            // Don't overshoot — cap deceleration so we don't reverse direction
            float speedReduction = deceleration * Time.fixedDeltaTime;
            if (speedReduction > speed)
            {
                rb.linearVelocity = Vector3.zero;
                return;
            }

            // Apply opposite to velocity direction
            Vector3 resistanceDir = -velocity.normalized;
            rb.AddForce(resistanceDir * resistanceForce, ForceMode.Force);
        }

        /// <summary>
        /// Sliding friction: when the ball is sliding (not pure rolling),
        /// apply friction to transition toward pure roll.
        /// Pure roll condition: v = omega * r (surface speed matches linear speed)
        /// </summary>
        private void ApplySlidingFriction()
        {
            Vector3 velocity = rb.linearVelocity;
            Vector3 angularVel = rb.angularVelocity;

            // Surface velocity from spin: v_surface = omega x r (upward)
            // For a ball on a flat surface, the contact point is at -Y
            // Surface velocity at contact = angularVelocity cross (0, -radius, 0)
            Vector3 contactOffset = Vector3.down * BallRadius;
            Vector3 surfaceVelocity = Vector3.Cross(angularVel, contactOffset);

            // Slip velocity = linear velocity - surface velocity at contact
            Vector3 slipVelocity = velocity - surfaceVelocity;

            // Only consider horizontal slip (XZ plane)
            slipVelocity.y = 0f;

            float slipSpeed = slipVelocity.magnitude;
            if (slipSpeed < MinVelocityThreshold)
                return;

            // Sliding friction force opposes slip
            float frictionForce = SlidingFrictionCoefficient * BallMass * Mathf.Abs(UnityEngine.Physics.gravity.y);
            Vector3 frictionDir = -slipVelocity.normalized;

            // Don't overshoot the slip correction
            float maxForce = slipSpeed * BallMass / Time.fixedDeltaTime;
            float appliedForce = Mathf.Min(frictionForce, maxForce);

            // Apply friction force (decelerates linear, accelerates angular toward pure roll)
            rb.AddForce(frictionDir * appliedForce, ForceMode.Force);

            // Torque from friction: T = r x F (cross product of contact to friction)
            Vector3 torque = Vector3.Cross(contactOffset, frictionDir * appliedForce);
            rb.AddTorque(torque, ForceMode.Force);
        }

        /// <summary>
        /// If velocity drops below minimum threshold, force it to zero
        /// to prevent infinite micro-sliding.
        /// </summary>
        private void ClampMinimumVelocity()
        {
            if (rb.linearVelocity.magnitude < MinVelocityThreshold)
            {
                rb.linearVelocity = Vector3.zero;
            }

            if (rb.angularVelocity.magnitude < MinVelocityThreshold)
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
