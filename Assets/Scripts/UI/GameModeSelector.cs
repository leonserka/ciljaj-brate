using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameModeSelector : MonoBehaviour
{
    [SerializeField] private Transform cardContainer;

    private static AudioClip _clickClip;
    private static AudioClip _hoverClip;
    private static AudioSource _uiAudio;

    private static readonly Color Cherry = new Color(0.85f, 0.12f, 0.20f);
    private static readonly Color CardBG = new Color(0.08f, 0.08f, 0.10f, 0.95f);
    private static readonly Color CardHover = new Color(0.12f, 0.12f, 0.15f, 0.95f);
    private static readonly Color PlayBtnColor = new Color(0.85f, 0.12f, 0.20f, 1f);

    private struct ModeInfo
    {
        public string title;
        public string modeName;
        public string category;
        public string description;
        public string sceneName;
        public string thumbnail;
        public bool playable;
    }

    private readonly ModeInfo[] _modes = new[]
    {
        new ModeInfo { title = "COMBO SHREDDER", modeName = "Combo Shredder", category = "SPEED", description = "Targets spawn close in a tight center area. Build combos fast. 1 minute timer.", sceneName = "AimTraining", thumbnail = "Thumbnails/gridshot", playable = true },
        new ModeInfo { title = "CLASSIC", modeName = "Classic", category = "PRECISION", description = "Single targets spawn randomly across the wall. No time limit.", sceneName = "AimTraining", thumbnail = "Thumbnails/gridshot", playable = true },
        new ModeInfo { title = "GRIDSHOT", modeName = "Gridshot", category = "PRECISION", description = "Targets appear in a grid. Click as many as you can.", sceneName = "AimTraining" },
        new ModeInfo { title = "SPIDERSHOT", modeName = "Spidershot", category = "SPEED", description = "Single targets spawn randomly. React and eliminate fast.", sceneName = "AimTraining" },
        new ModeInfo { title = "FLICKING", modeName = "Flicking", category = "FLICK", description = "Flick between targets that appear at wide angles.", sceneName = "AimTraining" },
        new ModeInfo { title = "SIXSHOT", modeName = "Sixshot", category = "PRECISION", description = "Six targets at once. Clear them all as fast as possible.", sceneName = "AimTraining" },
    };

    private void Awake()
    {
        if (_clickClip == null) _clickClip = Resources.Load<AudioClip>("UI/click");
        if (_hoverClip == null) _hoverClip = Resources.Load<AudioClip>("UI/hover");
        if (_uiAudio == null)
        {
            var existing = GameObject.Find("UIAudio");
            if (existing != null) _uiAudio = existing.GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        BuildCards();
    }

    private void BuildCards()
    {
        if (cardContainer == null) return;

        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        foreach (var mode in _modes)
            CreateCard(mode);
    }

    private void CreateCard(ModeInfo mode)
    {
        // Card root
        var card = new GameObject(mode.title, typeof(RectTransform));
        card.transform.SetParent(cardContainer, false);

        var cardImg = card.AddComponent<Image>();
        cardImg.color = CardBG;

        card.AddComponent<Mask>().showMaskGraphic = true;

        var outline = card.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.04f);
        outline.effectDistance = new Vector2(1, 1);

        var layout = card.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 0;

        // Thumbnail area (placeholder for future image)
        var thumb = new GameObject("Thumbnail", typeof(RectTransform));
        thumb.transform.SetParent(card.transform, false);
        thumb.AddComponent<LayoutElement>().flexibleHeight = 1;
        var thumbImg = thumb.AddComponent<Image>();
        thumbImg.color = new Color(0.05f, 0.05f, 0.07f, 0.8f);
        thumbImg.raycastTarget = false;

        Sprite thumbSprite = string.IsNullOrEmpty(mode.thumbnail) ? null : Resources.Load<Sprite>(mode.thumbnail);
        if (thumbSprite != null)
        {
            // Image lives in a child that fills the box, so its native size never drives the card layout.
            var imgGO = new GameObject("Image", typeof(RectTransform));
            imgGO.transform.SetParent(thumb.transform, false);
            var imgRect = imgGO.GetComponent<RectTransform>();
            imgRect.anchorMin = Vector2.zero;
            imgRect.anchorMax = Vector2.one;
            imgRect.offsetMin = Vector2.zero;
            imgRect.offsetMax = Vector2.zero;
            var img = imgGO.AddComponent<Image>();
            img.sprite = thumbSprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }
        else
        {
            // Icon hint placeholder when no thumbnail is available
            var iconGO = new GameObject("Icon", typeof(RectTransform));
            iconGO.transform.SetParent(thumb.transform, false);
            var iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(40, 40);
            var iconTMP = iconGO.AddComponent<TextMeshProUGUI>();
            iconTMP.text = "◎";
            iconTMP.fontSize = 32;
            iconTMP.color = new Color(Cherry.r, Cherry.g, Cherry.b, 0.3f);
            iconTMP.alignment = TextAlignmentOptions.Center;
        }

        // Info section (title + category + description)
        var info = new GameObject("Info", typeof(RectTransform));
        info.transform.SetParent(card.transform, false);
        info.AddComponent<LayoutElement>().preferredHeight = 100;

        // Title
        var titleGO = new GameObject("Title", typeof(RectTransform));
        titleGO.transform.SetParent(info.transform, false);
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0, 1);
        titleRect.sizeDelta = new Vector2(-24, 26);
        titleRect.anchoredPosition = new Vector2(12, -8);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = mode.title;
        titleTMP.fontSize = 20;
        titleTMP.color = Color.white;
        titleTMP.fontStyle = FontStyles.Bold;

        // Category
        var catGO = new GameObject("Category", typeof(RectTransform));
        catGO.transform.SetParent(info.transform, false);
        var catRect = catGO.GetComponent<RectTransform>();
        catRect.anchorMin = new Vector2(0, 1);
        catRect.anchorMax = new Vector2(1, 1);
        catRect.pivot = new Vector2(0, 1);
        catRect.sizeDelta = new Vector2(-24, 18);
        catRect.anchoredPosition = new Vector2(12, -34);
        var catTMP = catGO.AddComponent<TextMeshProUGUI>();
        catTMP.text = "MODE: " + mode.category;
        catTMP.fontSize = 11;
        catTMP.color = Cherry;
        catTMP.fontStyle = FontStyles.Bold;

        // Description
        var descGO = new GameObject("Desc", typeof(RectTransform));
        descGO.transform.SetParent(info.transform, false);
        var descRect = descGO.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 1);
        descRect.anchorMax = new Vector2(1, 1);
        descRect.pivot = new Vector2(0, 1);
        descRect.sizeDelta = new Vector2(-24, 40);
        descRect.anchoredPosition = new Vector2(12, -54);
        var descTMP = descGO.AddComponent<TextMeshProUGUI>();
        descTMP.text = mode.description;
        descTMP.fontSize = 12;
        descTMP.color = new Color(1f, 1f, 1f, 0.4f);
        descTMP.textWrappingMode = TextWrappingModes.Normal;

        // Play button
        var playGO = new GameObject("PlayBtn", typeof(RectTransform));
        playGO.transform.SetParent(card.transform, false);
        playGO.AddComponent<LayoutElement>().preferredHeight = 36;
        var playImg = playGO.AddComponent<Image>();
        playImg.color = mode.playable ? PlayBtnColor : new Color(0.13f, 0.13f, 0.15f, 0.95f);
        var playBtn = playGO.AddComponent<Button>();
        playBtn.transition = Selectable.Transition.None;
        playBtn.interactable = mode.playable;
        if (mode.playable)
            playGO.AddComponent<MenuButtonEffect>();

        var playLbl = new GameObject("Label", typeof(RectTransform));
        playLbl.transform.SetParent(playGO.transform, false);
        var playLblR = playLbl.GetComponent<RectTransform>();
        playLblR.anchorMin = Vector2.zero;
        playLblR.anchorMax = Vector2.one;
        playLblR.offsetMin = Vector2.zero;
        playLblR.offsetMax = Vector2.zero;
        var playTMP = playLbl.AddComponent<TextMeshProUGUI>();
        playTMP.text = mode.playable ? "PLAY NOW" : "COMING SOON";
        playTMP.fontSize = 14;
        playTMP.color = mode.playable ? Color.white : new Color(1f, 1f, 1f, 0.35f);
        playTMP.alignment = TextAlignmentOptions.Center;
        playTMP.fontStyle = FontStyles.Bold;

        // Card hover
        var hover = card.AddComponent<CardHoverEffect>();
        hover.Init(cardImg, outline);

        string scene = mode.sceneName;
        string modeName = mode.modeName;
        if (mode.playable)
        {
            playBtn.onClick.AddListener(() =>
            {
                if (_clickClip != null && _uiAudio != null)
                    _uiAudio.PlayOneShot(_clickClip);

                AimModeSelection.SelectMode(modeName);

                if (LobbyMusic.Instance != null)
                    LobbyMusic.Instance.FadeOutAndStop(0.8f);

                StartCoroutine(LoadAfterFade(scene));
            });
        }
    }

    private System.Collections.IEnumerator LoadAfterFade(string sceneName)
    {
        var cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            float t = 0;
            while (t < 0.3f)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = 1f - t / 0.3f;
                yield return null;
            }
        }

        while (LobbyMusic.Instance != null && !LobbyMusic.Instance.IsSilent)
            yield return null;

        SceneManager.LoadScene(sceneName);
    }
}
