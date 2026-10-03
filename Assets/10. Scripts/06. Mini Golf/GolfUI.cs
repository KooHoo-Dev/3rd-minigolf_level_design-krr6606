using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Study3D.MiniGolf
{
    /// <summary>
    /// 씬에 따로 만들 필요 없이 코드로 Canvas / HUD / 클리어 패널을 생성합니다.
    /// GameManager가 Build()를 호출해서 사용합니다.
    /// </summary>
    public class GolfUI : MonoBehaviour
    {
        // 다시 시작 버튼이 눌렸을 때
        public event System.Action RestartClicked;

        // ===== 표시 문구 (여기서 바꾸세요) =====
        private const string HudPrefix = "시간  ";
        private const string ClearTitle = "클리어!";
        private const string TimeLabel = "걸린 시간";
        private const string RestartLabel = "다시 시작";

        private static readonly Color Gold = new Color(1f, 0.84f, 0.2f);
        private static readonly Color[] ConfettiColors =
        {
            new Color(1f, 0.35f, 0.35f), new Color(1f, 0.8f, 0.2f), new Color(0.3f, 0.85f, 0.45f),
            new Color(0.3f, 0.65f, 1f),  new Color(0.8f, 0.45f, 1f), new Color(1f, 0.6f, 0.8f)
        };

        private Font font;
        private RectTransform canvasRoot;

        // HUD
        private Text hudText;

        // 클리어 패널
        private GameObject clearRoot;
        private CanvasGroup clearGroup;
        private RectTransform card;
        private RectTransform confettiLayer;
        private Text titleText;
        private Text timeText;
        private Coroutine confettiRoutine;
        private float shownTime;

        // =====================================================================
        // 생성
        // =====================================================================
        public void Build(Font customFont = null)
        {
            font = customFont != null ? customFont : LoadDefaultFont();

            EnsureEventSystem();

            GameObject canvasGo = new GameObject("GolfCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);

            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasRoot = (RectTransform)canvasGo.transform;

            BuildHud();
            BuildClearPanel();
            clearRoot.SetActive(false);
        }

        private void BuildHud()
        {
            hudText = CreateText(canvasRoot, "HudTime", HudPrefix + FormatTime(0f), 44, Color.white,
                new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(600f, 70f), TextAnchor.UpperLeft);

            Outline outline = hudText.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        private void BuildClearPanel()
        {
            // 전체 화면을 덮는 어두운 배경 (뒤쪽 클릭도 막아줌)
            clearRoot = new GameObject("ClearPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            RectTransform rootRt = (RectTransform)clearRoot.transform;
            rootRt.SetParent(canvasRoot, false);
            Stretch(rootRt);
            clearRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
            clearGroup = clearRoot.GetComponent<CanvasGroup>();

            // 축하 꽃가루 레이어 (카드 뒤쪽에 그려짐)
            GameObject confettiGo = new GameObject("ConfettiLayer", typeof(RectTransform));
            confettiLayer = (RectTransform)confettiGo.transform;
            confettiLayer.SetParent(rootRt, false);
            Stretch(confettiLayer);

            // 중앙 카드
            GameObject cardGo = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
            card = (RectTransform)cardGo.transform;
            card.SetParent(rootRt, false);
            card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(820f, 560f);
            cardGo.GetComponent<Image>().color = new Color(0.10f, 0.12f, 0.18f, 0.96f);
            Outline cardOutline = cardGo.GetComponent<Outline>();
            cardOutline.effectColor = Gold;
            cardOutline.effectDistance = new Vector2(4f, -4f);

            Vector2 center = new Vector2(0.5f, 0.5f);

            titleText = CreateText(card, "Title", ClearTitle, 110, Gold,
                center, new Vector2(0f, 170f), new Vector2(760f, 140f), TextAnchor.MiddleCenter);

            CreateText(card, "TimeLabel", TimeLabel, 42, new Color(0.75f, 0.8f, 0.9f),
                center, new Vector2(0f, 50f), new Vector2(760f, 60f), TextAnchor.MiddleCenter);

            timeText = CreateText(card, "Time", FormatTime(0f), 96, Color.white,
                center, new Vector2(0f, -40f), new Vector2(760f, 120f), TextAnchor.MiddleCenter);

            // 다시 시작 버튼
            GameObject btnGo = new GameObject("RestartButton", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform btnRt = (RectTransform)btnGo.transform;
            btnRt.SetParent(card, false);
            btnRt.anchorMin = btnRt.anchorMax = btnRt.pivot = center;
            btnRt.anchoredPosition = new Vector2(0f, -170f);
            btnRt.sizeDelta = new Vector2(380f, 100f);

            Image btnImage = btnGo.GetComponent<Image>();
            btnImage.color = new Color(0.22f, 0.6f, 1f);

            Button button = btnGo.GetComponent<Button>();
            button.targetGraphic = btnImage;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.85f, 0.92f, 1f);
            colors.pressedColor = new Color(0.65f, 0.75f, 0.9f);
            button.colors = colors;
            button.onClick.AddListener(() => RestartClicked?.Invoke());

            CreateText(btnRt, "Label", RestartLabel, 48, Color.white,
                center, Vector2.zero, new Vector2(380f, 100f), TextAnchor.MiddleCenter);
        }

        // =====================================================================
        // 외부에서 호출하는 함수
        // =====================================================================
        public void SetTime(float seconds)
        {
            if (hudText != null)
                hudText.text = HudPrefix + FormatTime(seconds);
        }

        public void ShowClear(float seconds)
        {
            HideClear(); // 이전 연출 정리
            clearRoot.SetActive(true);

            // 배경 페이드 인
            clearGroup.alpha = 0f;
            clearGroup.DOFade(1f, 0.35f);

            // 카드 팝업
            card.localScale = Vector3.zero;
            card.DOScale(1f, 0.6f).SetDelay(0.1f).SetEase(Ease.OutBack);

            // 타이틀 두근두근
            titleText.rectTransform.localScale = Vector3.one;
            titleText.rectTransform.DOScale(1.08f, 0.6f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

            // 시간 카운트업
            shownTime = 0f;
            timeText.text = FormatTime(0f);
            DOTween.To(() => shownTime, v => { shownTime = v; timeText.text = FormatTime(v); }, seconds, 1.2f)
                .SetDelay(0.4f)
                .SetEase(Ease.OutCubic)
                .SetTarget(timeText);

            // 꽃가루
            confettiRoutine = StartCoroutine(ConfettiRoutine());
        }

        public void HideClear()
        {
            if (confettiRoutine != null)
            {
                StopCoroutine(confettiRoutine);
                confettiRoutine = null;
            }

            if (clearRoot == null) return;

            DOTween.Kill(clearGroup);
            DOTween.Kill(card);
            DOTween.Kill(titleText.rectTransform);
            DOTween.Kill(timeText);
            ClearConfetti();

            clearRoot.SetActive(false);
        }

        // =====================================================================
        // 축하 이펙트 (UI 꽃가루)
        // =====================================================================
        private IEnumerator ConfettiRoutine()
        {
            // 처음에 한 번 팡! 터뜨리기
            for (int i = 0; i < 80; i++)
                SpawnConfetti(true);

            // 이후에는 위에서 계속 흩날림
            WaitForSeconds wait = new WaitForSeconds(0.06f);
            while (true)
            {
                SpawnConfetti(false);
                yield return wait;
            }
        }

        private void SpawnConfetti(bool burst)
        {
            GameObject go = new GameObject("Confetti", typeof(RectTransform), typeof(Image));
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(confettiLayer, false);
            rt.sizeDelta = new Vector2(Random.Range(12f, 24f), Random.Range(8f, 16f));
            rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            Image img = go.GetComponent<Image>();
            img.color = ConfettiColors[Random.Range(0, ConfettiColors.Length)];
            img.raycastTarget = false;

            float w = confettiLayer.rect.width;
            float h = confettiLayer.rect.height;
            float bottom = -h * 0.5f - 40f;

            Sequence seq = DOTween.Sequence().SetTarget(rt);
            Vector2 peak;

            if (burst)
            {
                // 카드 위쪽 중앙에서 사방으로 튀어 오름
                Vector2 start = new Vector2(0f, 150f);
                rt.anchoredPosition = start;

                Vector2 dir = Random.insideUnitCircle;
                dir.y = Mathf.Abs(dir.y) + 0.2f;
                peak = start + new Vector2(dir.x * Random.Range(350f, 800f), dir.y * Random.Range(250f, 550f));

                seq.Append(rt.DOAnchorPos(peak, Random.Range(0.45f, 0.8f)).SetEase(Ease.OutCubic));
            }
            else
            {
                // 화면 위 아무 곳에서 시작
                peak = new Vector2(Random.Range(-w * 0.5f, w * 0.5f), h * 0.5f + 30f);
                rt.anchoredPosition = peak;
            }

            Vector2 end = new Vector2(peak.x + Random.Range(-150f, 150f), bottom);
            float fallTime = Random.Range(2f, 3.5f);
            seq.Append(rt.DOAnchorPos(end, fallTime).SetEase(burst ? Ease.InQuad : Ease.Linear));
            seq.OnComplete(() =>
            {
                DOTween.Kill(rt);
                Destroy(go);
            });

            // 빙글빙글 회전
            rt.DOLocalRotate(new Vector3(0f, 0f, Random.Range(-1080f, 1080f)), seq.Duration(), RotateMode.FastBeyond360)
                .SetRelative(true)
                .SetEase(Ease.Linear);

            // 종이가 팔랑이는 느낌 (세로 크기 반복 변화)
            rt.DOScaleY(Random.Range(0.1f, 0.4f), Random.Range(0.12f, 0.3f))
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void ClearConfetti()
        {
            for (int i = confettiLayer.childCount - 1; i >= 0; i--)
            {
                Transform child = confettiLayer.GetChild(i);
                DOTween.Kill(child);
                Destroy(child.gameObject);
            }
        }

        // =====================================================================
        // 유틸
        // =====================================================================
        /// <summary>초 → "mm:ss.cc" 문자열 (문화권 영향 없음)</summary>
        public static string FormatTime(float seconds)
        {
            int cs = Mathf.Max(0, (int)(seconds * 100f)); // 1/100초 단위
            return $"{cs / 6000:00}:{cs / 100 % 60:00}.{cs % 100:00}";
        }

        private Text CreateText(Transform parent, string objName, string content, int size, Color color,
            Vector2 anchor, Vector2 anchoredPos, Vector2 sizeDelta, TextAnchor align)
        {
            GameObject go = new GameObject(objName, typeof(RectTransform), typeof(Text));
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Text t = go.GetComponent<Text>();
            t.font = font;
            t.text = content;
            t.fontSize = size;
            t.fontStyle = FontStyle.Bold;
            t.color = color;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // 한글이 깨지지 않도록 OS 폰트를 우선 사용
        private static Font LoadDefaultFont()
        {
            Font f = Font.CreateDynamicFontFromOSFont(
                new[] { "Malgun Gothic", "Apple SD Gothic Neo", "NanumGothic", "Noto Sans CJK KR", "Arial" }, 48);
            return f != null ? f : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        // 버튼 클릭을 받으려면 EventSystem이 필요합니다. 없으면 자동 생성.
        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;

            GameObject go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.UI.InputSystemUIInputModule module =
                go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            module.AssignDefaultActions();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
