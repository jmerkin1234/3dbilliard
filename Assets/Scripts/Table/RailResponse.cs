using UnityEngine;

namespace Billiards.Table
{
    /// <summary>
    /// Handles ball collisions with table rails (cushions).
    /// Applies reflection physics, restitution scaling, energy loss, and spin inversion.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class RailResponse : MonoBehaviour
    {
        // === Constants ===
        private const float RailRestitution = 0.9f; // from Rails physics material
        private const float EnergyLossFactor = 0.95f; // additional energy loss per bounce
        private const float SpinInversionFactor = 0.7f; // how much spin inverts on rail contact

        [Header("Rail Configuration")]
        [Tooltip("Tag used to identify balls (must match ball GameObjects)")]
        [SerializeField] private string ballTag = "Ball";

        [Tooltip("Surface normal direction (automatically calculated if zero)")]
        [SerializeField] private Vector3 railNormal = Vector3.zero;

        private void Awake()
        {
            // Auto-calculate rail normal from collider orientation if not set
            if (railNormal == Vector3.zero)
            {
                // Assume rail faces inward toward table center
                // For box colliders, use the transform's forward as the normal
                railNormal = -transform.forward;
            }

            railNormal = railNormal.normalized;
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Only process ball collisions (check both Ball and CueBall tags)
            if (!collision.gameObject.CompareTag(ballTag) && !collision.gameObject.CompareTag("CueBall"))
                return;

            Rigidbody ballRb = collision.rigidbody;
            if (ballRb == null)
                return;

            // Get ball physics and spin components
            Physics.BallPhysics ballPhysics = collision.gameObject.GetComponent<Physics.BallPhysics>();
            Spin.BallSpin ballSpin = collision.gameObject.GetComponent<Spin.BallSpin>();

            if (ballPhysics == null)
                return;

            // Use the collision contact normal if available, otherwise use configured rail normal
            Vector3 normal = collision.contacts.Length > 0
                ? collision.contacts[0].normal
                : railNormal;

            normal = normal.normalized;

            // Apply reflection with restitution and energy loss
            ApplyReflection(ballRb, normal);

            // Invert spin on rail contact
            if (ballSpin != null)
            {
                ApplySpinInversion(ballSpin, normal);
            }
        }

        /// <summary>
        /// Applies reflection formula with restitution and energy loss.
        /// Formula: R = V - 2(V·N)N
        /// </summary>
        private void ApplyReflection(Rigidbody ballRb, Vector3 normal)
        {
            Vector3 velocity = ballRb.linearVelocity;

            // Reflection formula: R = V - 2(V·N)N
            float dotProduct = Vector3.Dot(velocity, normal);
            Vector3 reflection = velocity - 2f * dotProduct * normal;

            // Apply restitution (bounciness factor from rail physics material)
            // Restitution only affects the normal component
            Vector3 normalComponent = dotProduct * normal;
            Vector3 tangentialComponent = velocity - normalComponent;

            Vector3 reflectedNormal = -normalComponent * RailRestitution;
            Vector3 finalVelocity = tangentialComponent + reflectedNormal;

            // Apply energy loss (simulates inelastic collision and friction)
            finalVelocity *= EnergyLossFactor;

            // Ensure no energy gain
            if (finalVelocity.magnitude > velocity.magnitude)
            {
                finalVelocity = finalVelocity.normalized * velocity.magnitude;
            }

            ballRb.linearVelocity = finalVelocity;
        }

        /// <summary>
        /// Inverts spin components based on rail contact.
        /// Sidespin inverts on rail contact due to friction.
        /// </summary>
        private void ApplySpinInversion(Spin.BallSpin ballSpin, Vector3 normal)
        {
            Vector3 angularVel = ballSpin.AppliedSpin;

            // Determine which spin component to invert based on collision normal
            // For side rails (normal along X), invert Y-axis spin (sidespin)
            // For end rails (normal along Z), invert Y-axis spin
            // Simplified: invert the sidespin (Y component) on any rail contact

            Vector3 invertedSpin = new Vector3(
                angularVel.x,
                -angularVel.y * SpinInversionFactor, // sidespin inverts
                angularVel.z
            );

            // Clear old spin and re-inject inverted spin
            ballSpin.ClearSpin();
            ballSpin.InjectSpin(invertedSpin);
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize rail normal in editor
            Gizmos.color = Color.cyan;
            Vector3 center = GetComponent<Collider>().bounds.center;
            Gizmos.DrawRay(center, railNormal * 0.3f);
        }
    }
}
