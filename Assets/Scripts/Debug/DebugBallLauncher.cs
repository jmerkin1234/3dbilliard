using UnityEngine;
using Billiards.Physics;

namespace Billiards.Debug
{
    /// <summary>
    /// Debug utility: press Space to launch the ball forward.
    /// Press R to reset ball position. Press B for backspin test.
    /// Remove this script before production.
    /// </summary>
    [RequireComponent(typeof(BallPhysics))]
    public class DebugBallLauncher : MonoBehaviour
    {
        [Header("Test Forces")]
        [SerializeField] private float launchForce = 2f;
        [SerializeField] private float backspinForce = 1.5f;

        private BallPhysics ballPhysics;
        private Spin.BallSpin ballSpin;
        private Vector3 startPosition;

        private void Awake()
        {
            ballPhysics = GetComponent<BallPhysics>();
            ballSpin = GetComponent<Spin.BallSpin>();
            startPosition = transform.position;
        }

        private void Update()
        {
            // Space: straight shot (should slow and stop naturally)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ballPhysics.StopMotion();
                ballSpin.ClearSpin();
                ballPhysics.ApplyImpulse(Vector3.forward * launchForce);
                UnityEngine.Debug.Log("[DebugLauncher] Straight shot applied: " + launchForce + "N forward");
            }

            // B: backspin/draw shot (should reverse)
            if (Input.GetKeyDown(KeyCode.B))
            {
                ballPhysics.StopMotion();
                ballSpin.ClearSpin();
                ballPhysics.ApplyImpulse(Vector3.forward * launchForce);
                // Backspin = negative rotation around X axis (spins backward)
                ballSpin.InjectSpin(Vector3.right * -backspinForce / 0.028575f);
                UnityEngine.Debug.Log("[DebugLauncher] Draw shot applied with backspin");
            }

            // F: follow/topspin shot (should roll further)
            if (Input.GetKeyDown(KeyCode.F))
            {
                ballPhysics.StopMotion();
                ballSpin.ClearSpin();
                ballPhysics.ApplyImpulse(Vector3.forward * launchForce);
                // Topspin = positive rotation around X axis
                ballSpin.InjectSpin(Vector3.right * backspinForce / 0.028575f);
                UnityEngine.Debug.Log("[DebugLauncher] Follow shot applied with topspin");
            }

            // S: stop shot test (no spin, should just decelerate)
            if (Input.GetKeyDown(KeyCode.S))
            {
                ballPhysics.StopMotion();
                ballSpin.ClearSpin();
                ballPhysics.ApplyImpulse(Vector3.forward * launchForce * 0.5f);
                UnityEngine.Debug.Log("[DebugLauncher] Stop shot applied (half power, no spin)");
            }

            // R: reset position
            if (Input.GetKeyDown(KeyCode.R))
            {
                ballPhysics.StopMotion();
                ballSpin.ClearSpin();
                transform.position = startPosition;
                UnityEngine.Debug.Log("[DebugLauncher] Ball reset to start position");
            }
        }
    }
}
