using UnityEngine;

namespace Billiards.Cue
{
    /// <summary>
    /// Handles shot power via UI slider input.
    /// Power is set by UI slider (0-1 normalized).
    /// Shoot is triggered by UI button.
    /// Cue stick pulls back visually based on power level.
    /// </summary>
    public class ShotPower : MonoBehaviour
    {
        [Header("Power Configuration")]
        [Tooltip("Minimum impulse force (Newtons)")]
        [SerializeField] private float minPower = 0.5f;

        [Tooltip("Maximum impulse force (Newtons)")]
        [SerializeField] private float maxPower = 8f;

        [Tooltip("Use exponential power curve (faster start, slower end)")]
        [SerializeField] private bool useExponentialCurve = true;

        [Tooltip("Exponential curve power (1 = linear, 2 = quadratic, etc.)")]
        [SerializeField] private float curvePower = 1.5f;

        [Header("Cue Pullback")]
        [Tooltip("Max pullback distance at full power")]
        [SerializeField] private float maxPullbackDistance = 0.3f;

        [Header("Input")]
        [Tooltip("Allow input (controlled by TurnManager)")]
        [SerializeField] private bool inputEnabled = true;

        // === State ===
        private float normalizedPower = 0f;

        // === Cached References ===
        private CueAim cueAim;

        // === Events ===
        /// <summary>Fired when shot is released</summary>
        public System.Action<float> OnShotReleased;

        // === Public Read Accessors ===
        /// <summary>Current power value (0-1 normalized)</summary>
        public float NormalizedPower => normalizedPower;

        /// <summary>Current impulse force in Newtons</summary>
        public float CurrentImpulse => Mathf.Lerp(minPower, maxPower, GetScaledPower());

        /// <summary>Enable/disable input</summary>
        public bool InputEnabled
        {
            get => inputEnabled;
            set => inputEnabled = value;
        }

        private void Awake()
        {
            cueAim = GetComponent<CueAim>();
        }

        private void Update()
        {
            UpdateCuePullback();
        }

        /// <summary>
        /// Set power from UI slider (0-1 normalized).
        /// </summary>
        public void SetPower(float normalized)
        {
            if (!inputEnabled)
                return;

            normalizedPower = Mathf.Clamp01(normalized);
        }

        /// <summary>
        /// Fire the shot at current power. Called by Shoot button.
        /// </summary>
        public void Shoot()
        {
            if (!inputEnabled)
                return;

            if (normalizedPower < 0.01f)
                return;

            float finalImpulse = CurrentImpulse;
            OnShotReleased?.Invoke(finalImpulse);

            UnityEngine.Debug.Log($"[ShotPower] Shot released: {finalImpulse:F2}N (power: {normalizedPower:P0})");

            ResetPower();
        }

        /// <summary>
        /// Manually trigger a shot with specific power (for AI or testing).
        /// </summary>
        public void TriggerShot(float impulse)
        {
            float clampedImpulse = Mathf.Clamp(impulse, minPower, maxPower);
            OnShotReleased?.Invoke(clampedImpulse);
        }

        /// <summary>
        /// Reset power to zero.
        /// </summary>
        public void ResetPower()
        {
            normalizedPower = 0f;
        }

        /// <summary>
        /// Applies exponential or linear scaling to power curve.
        /// </summary>
        private float GetScaledPower()
        {
            if (useExponentialCurve)
            {
                return Mathf.Pow(normalizedPower, curvePower);
            }
            return normalizedPower;
        }

        /// <summary>
        /// Pulls cue stick back based on current power level.
        /// </summary>
        private void UpdateCuePullback()
        {
            if (cueAim == null)
                return;

            float pullback = normalizedPower * maxPullbackDistance;
            cueAim.SetPullback(pullback);
        }
    }
}
