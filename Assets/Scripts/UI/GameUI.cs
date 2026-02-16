using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Billiards.UI
{
    /// <summary>
    /// Builds and manages the in-game UI for cue aiming and power control.
    /// Creates Canvas + all UI elements programmatically.
    ///
    /// Layout:
    /// - Left side:  POWER vertical slider (0-1), release to shoot
    /// - Right side: AIM vertical slider (0-360) + LOCK button
    /// - Top center: Status text
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Cue.CueAim cueAim;
        [SerializeField] private Cue.ShotPower shotPower;

        [Header("Layout")]
        [Tooltip("Horizontal anchor (0-1) for the shot/power cluster")]
        [SerializeField, Range(0.05f, 0.4f)] private float shotClusterAnchorX = 0.14f;

        [Tooltip("Vertical anchor (0-1) for the shot/power cluster")]
        [SerializeField, Range(0.2f, 0.8f)] private float shotClusterAnchorY = 0.52f;

        [Tooltip("Horizontal anchor (0-1) for the aim cluster")]
        [SerializeField, Range(0.6f, 0.95f)] private float aimClusterAnchorX = 0.86f;

        [Tooltip("Vertical anchor (0-1) for the aim cluster")]
        [SerializeField, Range(0.2f, 0.8f)] private float aimClusterAnchorY = 0.52f;

        [Tooltip("Status text Y offset from top center (more negative moves it closer to table)")]
        [SerializeField, Range(-220f, -10f)] private float statusTextOffsetY = -115f;

        // === UI Elements ===
        private Canvas canvas;
        private Slider powerSlider;
        private Slider aimSlider;
        private Button lockButton;
        private TextMeshProUGUI statusText;
        private TextMeshProUGUI powerLabel;
        private TextMeshProUGUI powerValueText;
        private TextMeshProUGUI aimLabel;
        private TextMeshProUGUI aimValueText;
        private TextMeshProUGUI lockButtonText;

        // === State ===
        private enum UIPhase { Aiming, Locked, BallsMoving }
        private UIPhase currentPhase = UIPhase.Aiming;

        private void Awake()
        {
            if (cueAim == null)
                cueAim = FindAnyObjectByType<Cue.CueAim>();
            if (shotPower == null)
                shotPower = FindAnyObjectByType<Cue.ShotPower>();
        }

        private void Start()
        {
            BuildUI();
            SyncUIWithCueState();

            // Always start from an unlocked aiming state.
            if (cueAim != null)
            {
                cueAim.UnlockAim();
                cueAim.SetShotVisualsHidden(false);
            }
            SetPhase(UIPhase.Aiming);

            // Subscribe to events
            if (shotPower != null)
                shotPower.OnShotReleased += OnShotFired;
            if (cueAim != null)
                cueAim.OnAimAngleChanged += OnAimAngleChangedByCueInput;

            Physics.BallSleepMonitor.OnAllBallsStopped += OnBallsStopped;
        }

        private void OnDestroy()
        {
            if (shotPower != null)
                shotPower.OnShotReleased -= OnShotFired;
            if (cueAim != null)
                cueAim.OnAimAngleChanged -= OnAimAngleChangedByCueInput;

            Physics.BallSleepMonitor.OnAllBallsStopped -= OnBallsStopped;
        }

        // ========== UI CONSTRUCTION ==========

        private void BuildUI()
        {
            // Enforce reliable side split even if serialized values drift.
            float shotX = shotClusterAnchorX;
            float aimX = aimClusterAnchorX;
            if (shotX < 0.05f || shotX > 0.45f)
                shotX = 0.14f;
            if (aimX < 0.55f || aimX > 0.95f)
                aimX = 0.86f;
            if ((aimX - shotX) < 0.35f)
            {
                shotX = 0.14f;
                aimX = 0.86f;
            }

            // Keep both clusters on the exact same visible Y band.
            // Use both serialized Y values and average them, then force exact alignment.
            float sharedY = (shotClusterAnchorY + aimClusterAnchorY) * 0.5f;
            float shotY = Mathf.Clamp(sharedY, 0.45f, 0.65f);
            float aimY = shotY;

            Vector2 shotAnchor = new Vector2(shotX, shotY);
            Vector2 aimAnchor = new Vector2(aimX, aimY);
            Vector2 clusterPivot = new Vector2(0.5f, 0.5f);

            // Canvas
            GameObject canvasObj = new GameObject("GameCanvas");
            canvasObj.transform.SetParent(transform, false);
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            EnsureEventSystem();

            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();

            // Status text (top center)
            statusText = CreateText(canvasRect, "StatusText", "Aim your shot",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, statusTextOffsetY), new Vector2(400, 50), 28, TextAlignmentOptions.Center);

            // POWER slider cluster (left side)
            powerLabel = CreateText(canvasRect, "PowerLabel", "POWER",
                shotAnchor, shotAnchor, clusterPivot,
                new Vector2(0, 180), new Vector2(90, 30), 18, TextAlignmentOptions.Center);

            powerSlider = CreateVerticalSlider(canvasRect, "PowerSlider",
                shotAnchor, shotAnchor, clusterPivot,
                new Vector2(0, -20), new Vector2(50, 300), 0f, 1f);

            powerValueText = CreateText(canvasRect, "PowerValue", "0%",
                shotAnchor, shotAnchor, clusterPivot,
                new Vector2(0, -195), new Vector2(90, 30), 16, TextAlignmentOptions.Center);

            // AIM slider cluster (right side)
            aimLabel = CreateText(canvasRect, "AimLabel", "AIM",
                aimAnchor, aimAnchor, clusterPivot,
                new Vector2(0, 180), new Vector2(90, 30), 18, TextAlignmentOptions.Center);

            aimSlider = CreateVerticalSlider(canvasRect, "AimSlider",
                aimAnchor, aimAnchor, clusterPivot,
                new Vector2(0, -20), new Vector2(50, 300), 0f, 360f);

            aimValueText = CreateText(canvasRect, "AimValue", "0\u00b0",
                aimAnchor, aimAnchor, clusterPivot,
                new Vector2(0, -195), new Vector2(90, 30), 16, TextAlignmentOptions.Center);

            // LOCK button (below aim slider)
            lockButton = CreateButton(canvasRect, "LockButton", "LOCK AIM",
                aimAnchor, aimAnchor, clusterPivot,
                new Vector2(0, -220), new Vector2(160, 44),
                new Color(0.2f, 0.6f, 0.2f, 1f));
            lockButtonText = lockButton.GetComponentInChildren<TextMeshProUGUI>();

            // Wire callbacks
            powerSlider.onValueChanged.AddListener(OnPowerChanged);
            aimSlider.onValueChanged.AddListener(OnAimChanged);
            lockButton.onClick.AddListener(OnLockPressed);

            // Shoot automatically when power slider is released.
            AddPointerReleaseTrigger(powerSlider.gameObject, OnPowerReleased);
        }

        /// <summary>
        /// Ensures there is an EventSystem with a usable input module.
        /// Prefers the new Input System module when available; falls back to StandaloneInputModule.
        /// </summary>
        private void EnsureEventSystem()
        {
            EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                eventSystem = esObj.AddComponent<EventSystem>();
            }

            StandaloneInputModule standaloneModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (standaloneModule == null)
            {
                standaloneModule = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }

            System.Type inputSystemUiModuleType = System.Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");

            BaseInputModule inputSystemModule = null;
            if (inputSystemUiModuleType != null)
            {
                inputSystemModule = eventSystem.GetComponent(inputSystemUiModuleType) as BaseInputModule;
                if (inputSystemModule == null)
                {
                    inputSystemModule = eventSystem.gameObject.AddComponent(inputSystemUiModuleType) as BaseInputModule;
                }
            }

            bool useInputSystemModule = ShouldUseInputSystemModule(inputSystemModule);
            if (inputSystemModule != null)
                inputSystemModule.enabled = useInputSystemModule;

            standaloneModule.enabled = !useInputSystemModule;
        }

        private bool ShouldUseInputSystemModule(BaseInputModule inputSystemModule)
        {
#if ENABLE_INPUT_SYSTEM
            if (inputSystemModule == null)
                return false;

            return Mouse.current != null || Touchscreen.current != null || Pen.current != null;
#else
            return false;
#endif
        }

        /// <summary>
        /// Initializes slider/value labels from current cue and shot state.
        /// Keeps first user interaction from causing an abrupt aim jump.
        /// </summary>
        private void SyncUIWithCueState()
        {
            if (cueAim != null)
            {
                float angle = Mathf.Repeat(cueAim.AimAngle, 360f);
                aimSlider.SetValueWithoutNotify(angle);
                aimValueText.text = $"{Mathf.RoundToInt(angle)}\u00b0";
            }
            else
            {
                aimSlider.SetValueWithoutNotify(0f);
                aimValueText.text = "0\u00b0";
                UnityEngine.Debug.LogWarning("[GameUI] CueAim reference not found. Aim slider is not functional.", this);
            }

            powerSlider.SetValueWithoutNotify(0f);
            powerValueText.text = "0%";

            if (shotPower == null)
            {
                UnityEngine.Debug.LogWarning("[GameUI] ShotPower reference not found. Shot UI is not functional.", this);
            }
        }

        // ========== UI CALLBACKS ==========

        private void OnPowerChanged(float value)
        {
            if (shotPower != null)
                shotPower.SetPower(value);

            powerValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        private void OnAimChanged(float value)
        {
            if (cueAim != null)
                cueAim.SetAimAngle(value);

            aimValueText.text = $"{Mathf.RoundToInt(value)}\u00b0";
        }

        private void OnAimAngleChangedByCueInput(float angle)
        {
            if (aimSlider == null || aimValueText == null)
                return;

            float normalized = Mathf.Repeat(angle, 360f);
            aimSlider.SetValueWithoutNotify(normalized);
            aimValueText.text = $"{Mathf.RoundToInt(normalized)}\u00b0";
        }

        private void OnLockPressed()
        {
            if (cueAim == null)
                return;

            if (!cueAim.IsLocked)
            {
                cueAim.LockAim();
                SetPhase(UIPhase.Locked);
            }
            else
            {
                cueAim.UnlockAim();
                SetPhase(UIPhase.Aiming);
            }
        }

        private void OnPowerReleased(BaseEventData _)
        {
            if (currentPhase != UIPhase.Locked || shotPower == null || !powerSlider.interactable)
                return;

            // Use current slider power and fire shot on release.
            // Only transition UI phase if a shot is actually emitted.
            if (shotPower.TryShoot())
            {
                SetPhase(UIPhase.BallsMoving);
            }
        }

        private void AddPointerReleaseTrigger(GameObject target, UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            if (target == null)
                return;

            SliderReleaseRelay relay = target.GetComponent<SliderReleaseRelay>();
            if (relay == null)
            {
                relay = target.AddComponent<SliderReleaseRelay>();
            }

            relay.SetCallback(callback);
        }

        // ========== EVENTS ==========

        private void OnShotFired(float impulse)
        {
            if (cueAim != null)
                cueAim.SetShotVisualsHidden(true);

            SetPhase(UIPhase.BallsMoving);
        }

        private void OnBallsStopped()
        {
            // Reset UI for next shot
            if (cueAim != null)
            {
                cueAim.UnlockAim();
                cueAim.SetShotVisualsHidden(false);
            }

            if (shotPower != null)
                shotPower.ResetPower();

            powerSlider.SetValueWithoutNotify(0f);
            powerValueText.text = "0%";

            SetPhase(UIPhase.Aiming);
        }

        // ========== PHASE MANAGEMENT ==========

        private void SetPhase(UIPhase phase)
        {
            currentPhase = phase;

            switch (phase)
            {
                case UIPhase.Aiming:
                    statusText.text = "Aim your shot";
                    aimSlider.interactable = true;
                    powerSlider.interactable = true;
                    lockButton.interactable = true;
                    lockButtonText.text = "LOCK AIM";
                    SetButtonColor(lockButton, new Color(0.2f, 0.6f, 0.2f, 1f));
                    break;

                case UIPhase.Locked:
                    statusText.text = "Set power, release to shoot";
                    aimSlider.interactable = false;
                    powerSlider.interactable = true;
                    lockButton.interactable = true;
                    lockButtonText.text = "UNLOCK";
                    SetButtonColor(lockButton, new Color(0.6f, 0.6f, 0.2f, 1f));
                    break;

                case UIPhase.BallsMoving:
                    statusText.text = "Balls in motion...";
                    aimSlider.interactable = false;
                    powerSlider.interactable = false;
                    lockButton.interactable = false;
                    break;
            }
        }

        // ========== UI FACTORY HELPERS ==========

        private TextMeshProUGUI CreateText(RectTransform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size, int fontSize, TextAlignmentOptions alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.raycastTarget = false; // Don't block UI raycasts
            return tmp;
        }

        private Slider CreateVerticalSlider(RectTransform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size, float minVal, float maxVal)
        {
            bool isPowerSlider = name.Contains("Power");

            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent, false);
            var rt = sliderObj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            var bgRt = bgObj.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bgRt.offsetMin = new Vector2(0, 0);
            bgRt.offsetMax = new Vector2(0, 0);
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.raycastTarget = false; // Background shouldn't block handle clicks

            if (isPowerSlider)
            {
                // Power slider: Gradient (green→yellow→orange→red→black)
                Texture2D gradientTex = CreatePowerGradient((int)size.x, (int)size.y);
                bgImg.sprite = Sprite.Create(gradientTex,
                    new Rect(0, 0, gradientTex.width, gradientTex.height),
                    new Vector2(0.5f, 0.5f));
                bgImg.type = Image.Type.Sliced;
            }
            else
            {
                // Aim slider: Black with white notches
                bgImg.color = Color.black;
                CreateAimNotches(bgObj.transform, size);
            }

            // Fill area (disabled for custom look)
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            var fillAreaRt = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.sizeDelta = Vector2.zero;

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            var fillRt = fillObj.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            var fillImg = fillObj.AddComponent<Image>();
            fillImg.color = new Color(0, 0, 0, 0); // Transparent
            fillImg.raycastTarget = false; // Fill shouldn't block clicks

            // Handle slide area
            GameObject handleAreaObj = new GameObject("Handle Slide Area");
            handleAreaObj.transform.SetParent(sliderObj.transform, false);
            var handleAreaRt = handleAreaObj.AddComponent<RectTransform>();
            handleAreaRt.anchorMin = Vector2.zero;
            handleAreaRt.anchorMax = Vector2.one;
            handleAreaRt.sizeDelta = Vector2.zero;

            // Red circular handle
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleAreaObj.transform, false);
            var handleRt = handleObj.AddComponent<RectTransform>();
            float handleSize = size.x * 1.5f; // Larger than slider width
            handleRt.sizeDelta = new Vector2(handleSize, handleSize);
            var handleImg = handleObj.AddComponent<Image>();
            handleImg.sprite = CreateCircleSprite(64);
            handleImg.color = new Color(0.9f, 0.2f, 0.2f, 1f); // Red

            // Add hover feedback to handle
            AddHoverFeedback(handleObj, handleImg, handleRt, handleSize);

            // Slider component
            var slider = sliderObj.AddComponent<Slider>();
            slider.direction = Slider.Direction.BottomToTop;
            slider.minValue = minVal;
            slider.maxValue = maxVal;
            slider.value = minVal;
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;

            return slider;
        }

        private Texture2D CreatePowerGradient(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height);

            for (int y = 0; y < height; y++)
            {
                float t = (float)y / height;
                Color color;

                if (t < 0.25f) // Bottom: Green
                    color = Color.Lerp(new Color(0.2f, 0.8f, 0.2f), new Color(0.8f, 0.8f, 0.2f), t * 4f);
                else if (t < 0.5f) // Yellow
                    color = Color.Lerp(new Color(0.8f, 0.8f, 0.2f), new Color(1f, 0.5f, 0f), (t - 0.25f) * 4f);
                else if (t < 0.75f) // Orange
                    color = Color.Lerp(new Color(1f, 0.5f, 0f), new Color(0.9f, 0.2f, 0.2f), (t - 0.5f) * 4f);
                else // Top: Red to Black
                    color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f), Color.black, (t - 0.75f) * 4f);

                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
            return tex;
        }

        private void CreateAimNotches(Transform parent, Vector2 size)
        {
            // Create white horizontal notch lines
            int notchCount = 8;
            float notchHeight = 2f;
            float notchWidth = size.x * 0.8f;

            for (int i = 0; i <= notchCount; i++)
            {
                GameObject notch = new GameObject($"Notch_{i}");
                notch.transform.SetParent(parent, false);
                var rt = notch.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, (float)i / notchCount);
                rt.anchorMax = new Vector2(0.5f, (float)i / notchCount);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(notchWidth, notchHeight);
                rt.anchoredPosition = Vector2.zero;

                var img = notch.AddComponent<Image>();
                img.color = Color.white;
                img.raycastTarget = false; // Notches shouldn't block clicks
            }
        }

        private Sprite CreateCircleSprite(int resolution)
        {
            Texture2D tex = new Texture2D(resolution, resolution);
            float center = resolution / 2f;
            float radius = center;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);

                    if (distance <= radius)
                    {
                        // Smooth edge antialiasing
                        float alpha = 1f - Mathf.Clamp01((distance - radius + 1f));
                        tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
        }

        private Button CreateButton(RectTransform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size, Color bgColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            var rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
            textRt.anchoredPosition = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btn;
        }

        private void SetButtonColor(Button btn, Color color)
        {
            var img = btn.GetComponent<Image>();
            if (img != null)
                img.color = color;
        }

        /// <summary>
        /// Adds hover feedback to UI element: cursor change + visual highlight
        /// </summary>
        private void AddHoverFeedback(GameObject obj, Image img, RectTransform rt, float normalSize)
        {
            SliderHandleHoverFeedback hover = obj.GetComponent<SliderHandleHoverFeedback>();
            if (hover == null)
            {
                hover = obj.AddComponent<SliderHandleHoverFeedback>();
            }

            hover.Initialize(img, rt, normalSize);
        }

        /// <summary>
        /// Relay for power-slider release events without using EventTrigger,
        /// so drag routing to Slider stays intact.
        /// </summary>
        private sealed class SliderReleaseRelay : MonoBehaviour, IPointerUpHandler, IEndDragHandler
        {
            private UnityEngine.Events.UnityAction<BaseEventData> callback;

            public void SetCallback(UnityEngine.Events.UnityAction<BaseEventData> cb)
            {
                callback = cb;
            }

            public void OnPointerUp(PointerEventData eventData)
            {
                callback?.Invoke(eventData);
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                callback?.Invoke(eventData);
            }
        }

        /// <summary>
        /// Hover-only visual feedback on slider handles.
        /// Implements only enter/exit to avoid intercepting drag events.
        /// </summary>
        private sealed class SliderHandleHoverFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
        {
            private Image handleImage;
            private RectTransform handleRect;
            private Color originalColor;
            private Vector2 originalSize;
            private bool initialized;

            public void Initialize(Image image, RectTransform rect, float normalSize)
            {
                handleImage = image;
                handleRect = rect;
                originalColor = image != null ? image.color : Color.white;
                originalSize = new Vector2(normalSize, normalSize);
                initialized = true;
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                if (!initialized || handleImage == null || handleRect == null)
                    return;

                handleImage.color = Color.Lerp(originalColor, Color.white, 0.25f);
                handleRect.sizeDelta = originalSize * 1.12f;
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                if (!initialized || handleImage == null || handleRect == null)
                    return;

                handleImage.color = originalColor;
                handleRect.sizeDelta = originalSize;
            }
        }
    }
}
