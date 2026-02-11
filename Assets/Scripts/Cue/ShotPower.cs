using UnityEngine;

namespace Billiards.Cue
{
    /// <summary>
    /// Handles shot power via hold-to-charge mechanic.
    /// Mouse button hold duration determines impulse magnitude.
    /// Provides clamped power value with optional exponential scaling.
    /// </summary>
    public class ShotPower : MonoBehaviour
    {
        [Header("Power Configuration")]
        [Tooltip("Minimum impulse force (Newtons)")]
        [SerializeField] private float minPower = 0.5f;

        [Tooltip("Maximum impulse force (Newtons)")]
        [SerializeField] private float maxPower = 8f;

        [Tooltip("Time to reach max power (seconds)")]
        [SerializeField] private float chargeTime = 2f;

        [Tooltip("Use exponential power curve (faster start, slower end)")]
        [SerializeField] private bool useExponentialCurve = true;

        [Tooltip("Exponential curve power (1 = linear, 2 = quadratic, etc.)")]
        [SerializeField] private float curvePower = 1.5f;

        [Header("Input")]
        [Tooltip("Mouse button for charging (0 = left, 1 = right)")]
        [SerializeField] private int chargeButton = 0;

        [Tooltip("Allow input (controlled by TurnManager)")]
        [SerializeField] private bool inputEnabled = true;

        // === State ===
        private float currentCharge = 0f;
        private bool isCharging = false;
        private float chargeStartTime;

        // === Events ===
        /// <summary>Fired when shot is released</summary>
        public System.Action<float> OnShotReleased;

        // === Public Read Accessors ===
        /// <summary>Current power value (0-1 normalized)</summary>
        public float NormalizedPower => Mathf.Clamp01(currentCharge / chargeTime);

        /// <summary>Current impulse force in Newtons</summary>
        public float CurrentImpulse => Mathf.Lerp(minPower, maxPower, GetScaledPower());

        /// <summary>Whether currently charging a shot</summary>
        public bool IsCharging => isCharging;

        /// <summary>Enable/disable input</summary>
        public bool InputEnabled
        {
            get => inputEnabled;
            set => inputEnabled = value;
        }

        private void Update()
        {
            if (!inputEnabled)
            {
                // Reset charge if input disabled mid-charge
                if (isCharging)
                {
                    ResetCharge();
                }
                return;
            }

            HandleChargeInput();
        }

        /// <summary>
        /// Handles mouse button input for power charging.
        /// </summary>
        private void HandleChargeInput()
        {
            // Start charging
            if (Input.GetMouseButtonDown(chargeButton) && !isCharging)
            {
                StartCharge();
            }

            // Continue charging
            if (Input.GetMouseButton(chargeButton) && isCharging)
            {
                UpdateCharge();
            }

            // Release shot
            if (Input.GetMouseButtonUp(chargeButton) && isCharging)
            {
                ReleaseShot();
            }
        }

        /// <summary>
        /// Begins power charge.
        /// </summary>
        private void StartCharge()
        {
            isCharging = true;
            chargeStartTime = Time.time;
            currentCharge = 0f;
        }

        /// <summary>
        /// Updates charge based on hold duration.
        /// </summary>
        private void UpdateCharge()
        {
            float elapsed = Time.time - chargeStartTime;
            currentCharge = Mathf.Clamp(elapsed, 0f, chargeTime);
        }

        /// <summary>
        /// Releases the shot and fires event with final power.
        /// </summary>
        private void ReleaseShot()
        {
            float finalImpulse = CurrentImpulse;
            OnShotReleased?.Invoke(finalImpulse);

            if (Application.isPlaying)
            {
                Debug.Log($"[ShotPower] Shot released: {finalImpulse:F2}N (charge: {NormalizedPower:P0})");
            }

            ResetCharge();
        }

        /// <summary>
        /// Resets charge state.
        /// </summary>
        private void ResetCharge()
        {
            isCharging = false;
            currentCharge = 0f;
        }

        /// <summary>
        /// Applies exponential or linear scaling to power curve.
        /// </summary>
        private float GetScaledPower()
        {
            float normalized = NormalizedPower;

            if (useExponentialCurve)
            {
                // Exponential curve: faster charge at start, slower at end
                return Mathf.Pow(normalized, curvePower);
            }

            return normalized;
        }

        /// <summary>
        /// Manually trigger a shot with specific power (for AI or testing).
        /// </summary>
        public void TriggerShot(float impulse)
        {
            float clampedImpulse = Mathf.Clamp(impulse, minPower, maxPower);
            OnShotReleased?.Invoke(clampedImpulse);
        }

        private void OnGUI()
        {
            if (!isCharging)
                return;

            // Simple on-screen power meter
            float powerPercent = NormalizedPower * 100f;
            string powerText = $"Power: {powerPercent:F0}% ({CurrentImpulse:F2}N)";

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 24;
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 100, 200, 50), powerText, style);

            // Power bar
            float barWidth = 300f;
            float barHeight = 30f;
            float barX = Screen.width / 2 - barWidth / 2;
            float barY = Screen.height - 60;

            GUI.Box(new Rect(barX, barY, barWidth, barHeight), "");
            GUI.Box(new Rect(barX, barY, barWidth * NormalizedPower, barHeight), "");
        }
    }
}
