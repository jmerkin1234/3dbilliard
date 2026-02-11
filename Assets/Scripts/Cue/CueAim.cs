using UnityEngine;

namespace Billiards.Cue
{
    /// <summary>
    /// Handles cue aiming via mouse input.
    /// Rotates around the cue ball to set shot direction.
    /// Provides normalized direction vector for CueStrike.
    /// Input can be locked by TurnManager between turns.
    /// </summary>
    public class CueAim : MonoBehaviour
    {
        [Header("Aiming Configuration")]
        [Tooltip("Cue ball to aim around")]
        [SerializeField] private Transform cueBall;

        [Tooltip("Distance from cue ball")]
        [SerializeField] private float aimDistance = 0.5f;

        [Tooltip("Mouse sensitivity for rotation")]
        [SerializeField] private float mouseSensitivity = 2f;

        [Tooltip("Smooth rotation speed")]
        [SerializeField] private float rotationSmoothSpeed = 10f;

        [Tooltip("Starting angle (degrees from forward)")]
        [SerializeField] private float initialAngle = 0f;

        [Header("Input Lock")]
        [Tooltip("Allow input (controlled by TurnManager)")]
        [SerializeField] private bool inputEnabled = true;

        // === State ===
        private float currentAngle;
        private float targetAngle;
        private Vector3 aimDirection;

        // === Public Read Accessors ===
        /// <summary>Normalized direction vector for the shot</summary>
        public Vector3 AimDirection => aimDirection;

        /// <summary>Current aim angle in degrees</summary>
        public float AimAngle => currentAngle;

        /// <summary>Enable/disable aiming input</summary>
        public bool InputEnabled
        {
            get => inputEnabled;
            set => inputEnabled = value;
        }

        private void Awake()
        {
            if (cueBall == null)
            {
                // Try to find cue ball by tag
                GameObject cueBallObj = GameObject.FindGameObjectWithTag("CueBall");
                if (cueBallObj != null)
                {
                    cueBall = cueBallObj.transform;
                }
                else
                {
                    Debug.LogError("[CueAim] No cue ball assigned and none found with tag 'CueBall'", this);
                }
            }

            currentAngle = initialAngle;
            targetAngle = initialAngle;
            UpdateAimDirection();
        }

        private void Update()
        {
            if (!inputEnabled || cueBall == null)
                return;

            HandleMouseInput();
            SmoothRotation();
            UpdateAimDirection();
            UpdateCuePosition();
        }

        /// <summary>
        /// Captures mouse input and updates target angle.
        /// </summary>
        private void HandleMouseInput()
        {
            // Horizontal mouse movement rotates around cue ball
            float mouseX = Input.GetAxis("Mouse X");

            if (Mathf.Abs(mouseX) > 0.001f)
            {
                targetAngle += mouseX * mouseSensitivity;

                // Wrap angle to 0-360 range
                targetAngle = Mathf.Repeat(targetAngle, 360f);
            }
        }

        /// <summary>
        /// Smoothly interpolates current angle toward target angle.
        /// Prevents jitter.
        /// </summary>
        private void SmoothRotation()
        {
            // Smooth damp toward target angle
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
            currentAngle += angleDiff * rotationSmoothSpeed * Time.deltaTime;

            // Wrap to 0-360
            currentAngle = Mathf.Repeat(currentAngle, 360f);
        }

        /// <summary>
        /// Updates the normalized aim direction vector based on current angle.
        /// </summary>
        private void UpdateAimDirection()
        {
            // Convert angle to direction vector (Y-axis rotation around cue ball)
            float angleRad = currentAngle * Mathf.Deg2Rad;
            aimDirection = new Vector3(Mathf.Sin(angleRad), 0f, Mathf.Cos(angleRad));
            aimDirection.Normalize();
        }

        /// <summary>
        /// Positions the cue stick behind the cue ball along aim direction.
        /// </summary>
        private void UpdateCuePosition()
        {
            if (cueBall == null)
                return;

            // Position cue behind the ball
            Vector3 cuePosition = cueBall.position - aimDirection * aimDistance;
            transform.position = cuePosition;

            // Rotate cue to point toward cue ball
            transform.LookAt(cueBall.position);
        }

        /// <summary>
        /// Manually set the aim angle (used for AI or reset).
        /// </summary>
        public void SetAimAngle(float angle)
        {
            currentAngle = angle;
            targetAngle = angle;
            UpdateAimDirection();
        }

        /// <summary>
        /// Reset to initial angle.
        /// </summary>
        public void ResetAim()
        {
            SetAimAngle(initialAngle);
        }

        private void OnDrawGizmos()
        {
            if (cueBall == null || !Application.isPlaying)
                return;

            // Visualize aim direction
            Gizmos.color = Color.green;
            Gizmos.DrawRay(cueBall.position, aimDirection * 1f);

            // Visualize cue position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.05f);
        }
    }
}
