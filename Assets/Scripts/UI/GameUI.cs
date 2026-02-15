using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Billiards.UI
{
    /// <summary>
    /// Builds and manages the in-game UI for cue aiming and power control.
    /// Creates Canvas + all UI elements programmatically.
    ///
    /// Layout:
    /// - Left side:  POWER vertical slider (0-1)
    /// - Right side: AIM vertical slider (0-360) + Lock button
    /// - Top center: Status text
    /// - Bottom center: SHOOT button
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Cue.CueAim cueAim;
        [SerializeField] private Cue.ShotPower shotPower;

        // === UI Elements ===
        private Canvas canvas;
        private Slider powerSlider;
        private Slider aimSlider;
        private Button lockButton;
        private Button shootButton;
        private TextMeshProUGUI statusText;
        private TextMeshProUGUI powerLabel;
        private TextMeshProUGUI powerValueText;
        private TextMeshProUGUI aimLabel;
        private TextMeshProUGUI aimValueText;
        private TextMeshProUGUI lockButtonText;

        // === State ===
        private enum UIPhase { Aiming, Locked, Shooting, BallsMoving }
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
            SetPhase(UIPhase.Aiming);

            // Subscribe to events
            if (shotPower != null)
                shotPower.OnShotReleased += OnShotFired;

            Physics.BallSleepMonitor.OnAllBallsStopped += OnBallsStopped;
        }

        private void OnDestroy()
        {
            if (shotPower != null)
                shotPower.OnShotReleased -= OnShotFired;

            Physics.BallSleepMonitor.OnAllBallsStopped -= OnBallsStopped;
        }

        // ========== UI CONSTRUCTION ==========

        private void BuildUI()
        {
            // Canvas
            GameObject canvasObj = new GameObject("GameCanvas");
            canvasObj.transform.SetParent(transform);
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();

            // Status text (top center)
            statusText = CreateText(canvasRect, "StatusText", "Aim your shot",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -20), new Vector2(400, 50), 28, TextAlignmentOptions.Center);

            // === BOTH SLIDERS ON RIGHT SIDE VERTICALLY (CLOSER TO TABLE) ===

            // POWER slider (right side, upper position)
            powerLabel = CreateText(canvasRect, "PowerLabel", "POWER",
                new Vector2(1, 0.75f), new Vector2(1, 0.75f), new Vector2(1, 0.75f),
                new Vector2(-250, 180), new Vector2(80, 30), 18, TextAlignmentOptions.Center);

            powerSlider = CreateVerticalSlider(canvasRect, "PowerSlider",
                new Vector2(1, 0.75f), new Vector2(1, 0.75f), new Vector2(1, 0.75f),
                new Vector2(-250, -20), new Vector2(50, 300), 0f, 1f);

            powerValueText = CreateText(canvasRect, "PowerValue", "0%",
                new Vector2(1, 0.75f), new Vector2(1, 0.75f), new Vector2(1, 0.75f),
                new Vector2(-250, -195), new Vector2(80, 30), 16, TextAlignmentOptions.Center);

            // AIM slider (right side, lower position)
            aimLabel = CreateText(canvasRect, "AimLabel", "AIM",
                new Vector2(1, 0.25f), new Vector2(1, 0.25f), new Vector2(1, 0.25f),
                new Vector2(-250, 180), new Vector2(80, 30), 18, TextAlignmentOptions.Center);

            aimSlider = CreateVerticalSlider(canvasRect, "AimSlider",
                new Vector2(1, 0.25f), new Vector2(1, 0.25f), new Vector2(1, 0.25f),
                new Vector2(-250, -20), new Vector2(50, 300), 0f, 360f);

            aimValueText = CreateText(canvasRect, "AimValue", "0\u00b0",
                new Vector2(1, 0.25f), new Vector2(1, 0.25f), new Vector2(1, 0.25f),
                new Vector2(-250, -195), new Vector2(80, 30), 16, TextAlignmentOptions.Center);

            // LOCK button (right side, below aim slider)
            lockButton = CreateButton(canvasRect, "LockButton", "LOCK AIM",
                new Vector2(1, 0.25f), new Vector2(1, 0.25f), new Vector2(1, 0.25f),
                new Vector2(-250, -240), new Vector2(120, 40),
                new Color(0.2f, 0.6f, 0.2f, 1f));
            lockButtonText = lockButton.GetComponentInChildren<TextMeshProUGUI>();

            // === SHOOT button (bottom center) ===
            shootButton = CreateButton(canvasRect, "ShootButton", "SHOOT",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0, 60), new Vector2(160, 50),
                new Color(0.8f, 0.2f, 0.2f, 1f));

            // Wire callbacks
            powerSlider.onValueChanged.AddListener(OnPowerChanged);
            aimSlider.onValueChanged.AddListener(OnAimChanged);
            lockButton.onClick.AddListener(OnLockPressed);
            shootButton.onClick.AddListener(OnShootPressed);
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

        private void OnShootPressed()
        {
            if (shotPower != null)
            {
                SetPhase(UIPhase.BallsMoving);
                shotPower.Shoot();
            }
        }

        // ========== EVENTS ==========

        private void OnShotFired(float impulse)
        {
            SetPhase(UIPhase.BallsMoving);
        }

        private void OnBallsStopped()
        {
            // Reset UI for next shot
            if (cueAim != null)
                cueAim.UnlockAim();

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
                    powerSlider.interactable = false;
                    shootButton.interactable = false;
                    lockButton.interactable = true;
                    lockButtonText.text = "LOCK AIM";
                    SetButtonColor(lockButton, new Color(0.2f, 0.6f, 0.2f, 1f));
                    SetButtonColor(shootButton, new Color(0.4f, 0.4f, 0.4f, 1f));
                    break;

                case UIPhase.Locked:
                    statusText.text = "Set power";
                    aimSlider.interactable = false;
                    powerSlider.interactable = true;
                    shootButton.interactable = true;
                    lockButton.interactable = true;
                    lockButtonText.text = "UNLOCK";
                    SetButtonColor(lockButton, new Color(0.6f, 0.6f, 0.2f, 1f));
                    SetButtonColor(shootButton, new Color(0.8f, 0.2f, 0.2f, 1f));
                    break;

                case UIPhase.BallsMoving:
                    statusText.text = "Balls in motion...";
                    aimSlider.interactable = false;
                    powerSlider.interactable = false;
                    shootButton.interactable = false;
                    lockButton.interactable = false;
                    SetButtonColor(shootButton, new Color(0.4f, 0.4f, 0.4f, 1f));
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
            EventTrigger trigger = obj.AddComponent<EventTrigger>();

            // Store original values
            Color originalColor = img.color;
            Vector2 originalSize = rt.sizeDelta;

            // Pointer Enter (hover start)
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) =>
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Hand cursor (system default for clickable)
                img.color = new Color(originalColor.r * 1.2f, originalColor.g * 1.2f, originalColor.b * 1.2f, 1f); // Brighten
                rt.sizeDelta = originalSize * 1.15f; // Slightly larger
            });
            trigger.triggers.Add(enterEntry);

            // Pointer Exit (hover end)
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) =>
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Reset to normal
                img.color = originalColor; // Reset color
                rt.sizeDelta = originalSize; // Reset size
            });
            trigger.triggers.Add(exitEntry);
        }
    }
}
