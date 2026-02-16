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
        private const float TangentialVelocityRetention = 0.97f; // small cushion energy loss along rail
        private const float AxialSpinRetention = 0.94f; // mild damping on top/back spin
        private const float SideSpinInversionRetention = 0.62f; // invert + damp english on cushion hit

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

            // Unity physics handles main reflection from collider + material restitution.
            // Apply only small realism corrections.
            ApplyRailVelocityCorrection(ballRb, normal);
            ApplySpinCorrection(ballRb, ballSpin);
        }

        /// <summary>
        /// Dampen rail-parallel velocity slightly to mimic cushion cloth losses.
        /// </summary>
        private void ApplyRailVelocityCorrection(Rigidbody ballRb, Vector3 normal)
        {
            Vector3 velocity = ballRb.linearVelocity;
            Vector3 normalComponent = Vector3.Project(velocity, normal);
            Vector3 tangentialComponent = velocity - normalComponent;

            ballRb.linearVelocity = normalComponent + tangentialComponent * TangentialVelocityRetention;
        }

        /// <summary>
        /// Preserve top/back spin mostly, but invert and damp sidespin on cushion contact.
        /// </summary>
        private void ApplySpinCorrection(Rigidbody ballRb, Spin.BallSpin ballSpin)
        {
            Vector3 angularVel = ballRb.angularVelocity;

            Vector3 corrected = new Vector3(
                angularVel.x * AxialSpinRetention,
                -angularVel.y * SideSpinInversionRetention,
                angularVel.z * AxialSpinRetention
            );

            // Avoid tiny perpetual spin values.
            if (Mathf.Abs(corrected.y) < 0.01f)
                corrected.y = 0f;

            ballRb.angularVelocity = corrected;

            if (ballSpin != null)
                ballSpin.SyncSpinStateFromRigidbody();
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
