using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BreedingUIBuilder : MonoBehaviour
{
    static readonly int UnderlayColorId = Shader.PropertyToID("_UnderlayColor");
    static readonly int UnderlayDilateId = Shader.PropertyToID("_UnderlayDilate");
    static readonly int UnderlayOffsetXId = Shader.PropertyToID("_UnderlayOffsetX");
    static readonly int UnderlayOffsetYId = Shader.PropertyToID("_UnderlayOffsetY");
    static readonly int UnderlaySoftnessId = Shader.PropertyToID("_UnderlaySoftness");
    static readonly int GlowInnerId = Shader.PropertyToID("_GlowInner");
    static readonly int GlowOuterId = Shader.PropertyToID("_GlowOuter");

    enum HorsePanelTab
    {
        Stats,
        Looks
    }

    // link to the breeding session
    [SerializeField] BreedingSession session;

    [FormerlySerializedAs("asciiFont")]
    [SerializeField]
    [Tooltip("Primary TMP font for all normal UI text (letters, numbers, punctuation). Stars still come from Symbols Fallback only. Leave empty to use Resources/BreezePrimaryFont if present, else TMP Settings default.")]
    TMP_FontAsset primaryTextFont;

    [Tooltip("Noto Sans Symbols 2 (or any SDF with U+2605 ★). Fallback on the primary font only — not used as main body text. See Resources/FontSetup_NotoSansSymbols2.txt")]
    [SerializeField] TMP_FontAsset symbolsFallbackFont;

    // assign a TMP font asset built from Noto Sans Egyptian Hieroglyphs (or similar) so U+130D7 shows; see comment on HelpMenu / readme in chat
    [Tooltip("Dynamic TMP font: atlas texture must be Read/Write enabled (or TryAddCharacters cannot pack U+130D7). Optional Resources/HorseGlyphFont.asset if this is empty.")]
    [SerializeField] TMP_FontAsset horseGlyphFont;

    // 𐂃 is U+10083 (Linear B); Noto Egyptian does not cover it — assign a Linear B TMP font or the center slot may show □ before first breed.
    [SerializeField] TMP_FontAsset noOwnedYetGlyphFont;

    // race distance in furlongs (5–16 like RaceGenerator.txt)
    [SerializeField] int raceFurlongs = 8;

    // unicode horse (egyptian hieroglyph); needs font fallback with this range or device shows a box
    const string HorseUnicodeGlyph = "\U000130D7";

    // linear b 𐂃 — shown in center when ownedHorses is empty
    const string NoOwnedHorseYetGlyph = "\U00010083";

    // TryAddCharacters(string) walks UTF-16 code units, so supplementary chars (U+10000+) must use the uint[] overload.
    static readonly uint[] HorseUnicodeCodePoint = { 0x130D7 };
    static readonly uint[] NoOwnedYetUnicodeCodePoint = { 0x10083 };
    static readonly uint[] BlackStarCodePoint = { 0x2605 }; // ★ cosmetic tiers

    // point size for the big horse glyph in each column
    [SerializeField] float horseIconFontSize = 115f;

    // smaller type + tight leading so full stat block fits in narrow columns (assign a monospace TMP on primaryTextFont for aligned columns)
    [SerializeField] float horseStatsFontSize = 20f;

    // ui refs
    // root transform for everything we spawn
    Transform uiRoot;
    // ascii text for left column
    TextMeshProUGUI mareAscii;
    // ascii text for right column
    TextMeshProUGUI stallionAscii;
    // ascii text for owned horse in center
    TextMeshProUGUI ownedAscii;
    // stats under mare list
    TextMeshProUGUI mareStatsText;
    // stats under stallion list
    TextMeshProUGUI stallionStatsText;
    // stats for picked owned horse
    TextMeshProUGUI ownedStatsText;
    // mare names with > selected <
    TextMeshProUGUI mareListText;
    // stallion names with > selected <
    TextMeshProUGUI stallionListText;
    // race finish order output
    TextMeshProUGUI resultText;

    HorsePanelTab _horsePanelTab = HorsePanelTab.Stats;

    // per-column stats | looks (all switch the same global tab)
    Image _mareTabStatsBg, _mareTabLooksBg;
    Image _stallTabStatsBg, _stallTabLooksBg;
    Image _ownedTabStatsBg, _ownedTabLooksBg;

    GameObject _foalNameOverlay;
    TMP_InputField _foalNameInput;
    HorseTemplate _pendingFoal;

    // canvas we parent under (used to fix sibling order in Start)
    Canvas hostCanvas;

    // cached white sprite for ui Image (filled ring needs a sprite)
    static Sprite _uiWhiteSprite;

    // create or return single shared 1x1 white sprite
    static Sprite UiWhiteSprite()
    {
        // first time only, build sprite from built-in white texture
        if (_uiWhiteSprite == null)
        {
            // unity gives us a 1x1 white texture
            var t = Texture2D.whiteTexture;
            // wrap it as a sprite for uGUI Image
            _uiWhiteSprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100f);
        }
        return _uiWhiteSprite;
    }

    /// <summary>
    /// Links Symbols 2 (or any font with U+2605) as a fallback on <see cref="primaryTextFont"/> and tries to pack ★ into dynamic atlases.
    /// </summary>
    void RegisterSymbolsFontSupport()
    {
        // Pack ★ into the symbols SDF first. TMP consults fallbacks via GetCharacterFromFontAsset, which
        // calls TryAddCharacterInternal on Dynamic fonts only in the Regular / non-Italic code path — so we
        // pre-pack when possible and force Normal+Regular on stats TMP (see StyleHorseStatsPanel).
        if (symbolsFallbackFont != null)
        {
            const char blackStar = '\u2605';
            if (symbolsFallbackFont.atlasPopulationMode == AtlasPopulationMode.Dynamic)
            {
                if (!symbolsFallbackFont.HasCharacter(blackStar, false))
                    symbolsFallbackFont.TryAddCharacters(BlackStarCodePoint, out _);
                if (!symbolsFallbackFont.HasCharacter(blackStar, false))
                {
                    Debug.LogWarning(
                        "BreedingUIBuilder: could not pack U+2605 (★) into symbolsFallbackFont. " +
                        "Confirm the asset is Dynamic, atlas texture is Read/Write, and Source Font File is the Noto Symbols TTF.",
                        symbolsFallbackFont);
                }
            }
            else if (!symbolsFallbackFont.HasCharacter(blackStar, false))
            {
                Debug.LogWarning(
                    "BreedingUIBuilder: symbolsFallbackFont is Static but has no U+2605 (★). Rebake the SDF with that character or switch to Dynamic.",
                    symbolsFallbackFont);
            }

            // Global fallbacks (TMP Settings) — GetTextElement searches here after the primary font.
            if (TMP_Settings.fallbackFontAssets != null && !TMP_Settings.fallbackFontAssets.Contains(symbolsFallbackFont))
                TMP_Settings.fallbackFontAssets.Add(symbolsFallbackFont);
        }

        void AppendSymbolsToFont(TMP_FontAsset target)
        {
            if (target == null || symbolsFallbackFont == null) return;
            if (target.fallbackFontAssetTable == null)
                target.fallbackFontAssetTable = new List<TMP_FontAsset>();
            if (!target.fallbackFontAssetTable.Contains(symbolsFallbackFont))
                target.fallbackFontAssetTable.Insert(0, symbolsFallbackFont);
        }

        // Primary font + project default (HelpMenu / unset TMP still use default until assigned).
        AppendSymbolsToFont(primaryTextFont);
        AppendSymbolsToFont(TMP_Settings.defaultFontAsset);
        SanitizeFontMaterial(primaryTextFont);
        SanitizeFontMaterial(symbolsFallbackFont);
        SanitizeFontMaterial(TMP_Settings.defaultFontAsset);

        if (primaryTextFont != null && primaryTextFont.atlasPopulationMode == AtlasPopulationMode.Dynamic)
            primaryTextFont.TryAddCharacters(BlackStarCodePoint, out _);
    }

    void ResolvePrimaryTextFont()
    {
        if (primaryTextFont == null)
            primaryTextFont = Resources.Load<TMP_FontAsset>("BreezePrimaryFont");
        if (primaryTextFont == null)
            primaryTextFont = TMP_Settings.defaultFontAsset;
        SanitizeFontMaterial(primaryTextFont);
    }

    static void SanitizeFontMaterial(TMP_FontAsset font)
    {
        if (font == null || font.material == null) return;
        var mat = font.material;

        // Remove the bright rectangle effect from generated TMP materials.
        mat.DisableKeyword("UNDERLAY_ON");
        mat.DisableKeyword("UNDERLAY_INNER");
        mat.DisableKeyword("GLOW_ON");

        if (mat.HasProperty(UnderlayDilateId)) mat.SetFloat(UnderlayDilateId, 0f);
        if (mat.HasProperty(UnderlayOffsetXId)) mat.SetFloat(UnderlayOffsetXId, 0f);
        if (mat.HasProperty(UnderlayOffsetYId)) mat.SetFloat(UnderlayOffsetYId, 0f);
        if (mat.HasProperty(UnderlaySoftnessId)) mat.SetFloat(UnderlaySoftnessId, 0f);
        if (mat.HasProperty(GlowInnerId)) mat.SetFloat(GlowInnerId, 0f);
        if (mat.HasProperty(GlowOuterId)) mat.SetFloat(GlowOuterId, 0f);
        if (mat.HasProperty(UnderlayColorId))
            mat.SetColor(UnderlayColorId, new Color(0f, 0f, 0f, 0f));
    }

    void Awake()
    {
        ResolvePrimaryTextFont();

        // try same gameobject for session
        if (session == null)
            session = GetComponent<BreedingSession>();
        // fallback: any session in scene
        if (session == null)
            session = FindAnyObjectByType<BreedingSession>(FindObjectsInactive.Include);

        // need a canvas to parent under
        Canvas canvas = FindAnyObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            Debug.LogError("BreedingUIBuilder: no Canvas.");
            return;
        }

        hostCanvas = canvas;

        // phone / laptop: scale ui by screen size so text and buttons stay readable
        ApplyMobileFriendlyCanvasScaler(canvas);

        // empty parent for all built ui (must be RectTransform under Canvas or StretchFull gets null)
        var rootGo = new GameObject("BreedingUI_Built", typeof(RectTransform));
        uiRoot = rootGo.transform;
        uiRoot.SetParent(canvas.transform, false);
        // stretch to full canvas
        StretchFull(rootGo.GetComponent<RectTransform>());
        // must draw above Background but below ControlsPanel (controls stays last = on top when open)
        PlaceBreedingUiBeforeControlsPanel();

        RegisterSymbolsFontSupport();

        // create columns, buttons, overlays
        BuildLayout();
        BuildFoalNameOverlay();
        if (session != null)
            session.racePreviewFurlongs = raceFurlongs;
        // first paint of text
        RefreshAllText();

        // triple-tap menu (always on top when open)
        var menuGo = new GameObject("HelpMenu", typeof(RectTransform), typeof(HelpMenu));
        menuGo.transform.SetParent(hostCanvas.transform, false);
        StretchFull(menuGo.GetComponent<RectTransform>());
        menuGo.GetComponent<HelpMenu>().Initialize(session, this, primaryTextFont);
        menuGo.transform.SetAsLastSibling();
    }

    void Start()
    {
        // other scripts may reorder canvas children in Awake; fix stack again so ui is not hidden
        PlaceBreedingUiBeforeControlsPanel();

        // when menu or triple-tap path calls RegeneratePools
        if (session != null && session.onPoolsChanged != null)
            session.onPoolsChanged.AddListener(RefreshAllText);

        if (session != null)
            session.racePreviewFurlongs = raceFurlongs;

        RegisterSymbolsFontSupport();

        // stats/lists after all Awakes (session pools are ready)
        RefreshAllText();
        Canvas.ForceUpdateCanvases();
        StartCoroutine(RefreshUiEndOfFrame());
    }

    IEnumerator RefreshUiEndOfFrame()
    {
        yield return null;
        RefreshAllText();
        Canvas.ForceUpdateCanvases();
    }

    // sibling order: Background ... BreedingUI_Built ... ControlsPanel (controls must be last)
    void PlaceBreedingUiBeforeControlsPanel()
    {
        if (uiRoot == null || hostCanvas == null) return;
        Transform controls = hostCanvas.transform.Find("ControlsPanel");
        if (controls != null)
            uiRoot.SetSiblingIndex(controls.GetSiblingIndex());
        else
            uiRoot.SetSiblingIndex(Mathf.Min(1, hostCanvas.transform.childCount - 1));
    }

    void OnDestroy()
    {
        // avoid listener leak if object destroyed in play mode
        if (session != null && session.onPoolsChanged != null)
            session.onPoolsChanged.RemoveListener(RefreshAllText);
    }

    void BuildLayout()
    {
        // dark sheet so white TMP is readable over skybox / light game camera
        CreateDarkBackdrop();

        // vertical split: more room for results + buttons on phones (fractions of screen height)
        const float bottomBarH = 0.26f;
        const float resultsBarH = 0.15f;
        float contentYMin = bottomBarH + resultsBarH;

        // bottom: long-press breed + race
        Transform bottom = CreateRegion("BottomRow", new Vector2(0f, 0f), new Vector2(1f, bottomBarH));
        // results strip above buttons (full width)
        Transform resultsRegion = CreateRegion("ResultsRow", new Vector2(0f, bottomBarH), new Vector2(1f, contentYMin));
        // three columns above that
        Transform left = CreateRegion("LeftColumn", new Vector2(0f, contentYMin), new Vector2(0.33f, 1f));
        Transform center = CreateRegion("CenterColumn", new Vector2(0.34f, contentYMin), new Vector2(0.66f, 1f));
        Transform right = CreateRegion("RightColumn", new Vector2(0.67f, contentYMin), new Vector2(1f, 1f));

        // mare horse glyph (flipped horizontally vs stallions)
        mareAscii = CreateTmp("MareAscii", left, new Vector2(0.02f, 0.55f), new Vector2(0.98f, 0.98f), horseIconFontSize, TextAlignmentOptions.Center);
        ApplyHorseGlyphFont(mareAscii);
        ApplyMareGlyphFlip();
        // mare name list middle-left
        // mareList.anchorMin.y must stay above mareStats.anchorMax.y so stats never overlap names
        mareListText = CreateTmp("MareList", left, new Vector2(0.02f, 0.44f), new Vector2(0.98f, 0.54f), 25f, TextAlignmentOptions.Top);
        mareStatsText = CreateTmp("MareStats", left, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.20f), 24f, TextAlignmentOptions.Top);
        StyleHorseStatsPanel(mareStatsText);
        CreateSwipeOverlay(left, "MareSwipe", true, false);
        CreateViewTabStrip(left, "MareViewTabs", new Vector2(0.02f, 0.21f), new Vector2(0.98f, 0.28f), out _mareTabStatsBg, out _mareTabLooksBg);

        // stallion horse glyph (not flipped)
        stallionAscii = CreateTmp("StallionAscii", right, new Vector2(0.02f, 0.55f), new Vector2(0.98f, 0.98f), horseIconFontSize, TextAlignmentOptions.Center);
        ApplyHorseGlyphFont(stallionAscii);
        stallionAscii.transform.localScale = Vector3.one;
        // stallion names
        stallionListText = CreateTmp("StallionList", right, new Vector2(0.02f, 0.44f), new Vector2(0.98f, 0.54f), 25f, TextAlignmentOptions.Top);
        stallionStatsText = CreateTmp("StallionStats", right, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.20f), 24f, TextAlignmentOptions.Top);
        StyleHorseStatsPanel(stallionStatsText);
        CreateSwipeOverlay(right, "StallionSwipe", false, false);
        CreateViewTabStrip(right, "StallionViewTabs", new Vector2(0.02f, 0.21f), new Vector2(0.98f, 0.28f), out _stallTabStatsBg, out _stallTabLooksBg);

        // small title above owned ascii
        CreateTmp("OwnedLabel", center, new Vector2(0.05f, 0.84f), new Vector2(0.95f, 0.98f), 27f, TextAlignmentOptions.Top)
            .text = "your álogo (swipe)";

        // owned horse glyph
        ownedAscii = CreateTmp("OwnedAscii", center, new Vector2(0.05f, 0.48f), new Vector2(0.95f, 0.82f), horseIconFontSize, TextAlignmentOptions.Center);
        ApplyOwnedSlotFontAndGlyphs();
        ownedAscii.transform.localScale = Vector3.one;
        // owned stats block
        ownedStatsText = CreateTmp("OwnedStats", center, new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.40f), 24f, TextAlignmentOptions.Top);
        StyleHorseStatsPanel(ownedStatsText);
        CreateSwipeOverlay(center, "OwnedSwipe", false, true);
        CreateViewTabStrip(center, "OwnedViewTabs", new Vector2(0.03f, 0.42f), new Vector2(0.97f, 0.48f), out _ownedTabStatsBg, out _ownedTabLooksBg);

        // results text area
        resultText = CreateTmp("RaceResults", resultsRegion, new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.95f), 25f, TextAlignmentOptions.Top);
        resultText.text = "results: (long-press run race — brown button)";

        // narrow portrait: stack breed above race so both stay tappable
        bool narrow = hostCanvas.pixelRect.width < 620f;
        if (narrow)
        {
            CreateBreedButton(bottom, new Vector2(0.04f, 0.52f), new Vector2(0.96f, 0.96f));
            CreateRaceButton(bottom, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.46f));
        }
        else
        {
            CreateBreedButton(bottom, new Vector2(0.02f, 0.08f), new Vector2(0.48f, 0.92f));
            CreateRaceButton(bottom, new Vector2(0.52f, 0.08f), new Vector2(0.98f, 0.92f));
        }
    }

    // mirror mare column glyph (same unicode, faces the other way)
    void ApplyMareGlyphFlip()
    {
        if (mareAscii == null) return;
        mareAscii.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        mareAscii.transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    // TMP default font has no U+130D7; use inspector font or Resources/HorseGlyphFont.
    // SDF assets often bake only ASCII; Dynamic mode + TryAddCharacters pulls U+130D7 from the source TTF at runtime.
    void ApplyHorseGlyphFont(TextMeshProUGUI tmp)
    {
        if (tmp == null) return;
        TMP_FontAsset glyphFont = ResolveHorseGlyphFont();
        if (glyphFont == null) return;
        tmp.font = glyphFont;
        EnsureCodePointsInFont(glyphFont, HorseUnicodeCodePoint);
    }

    TMP_FontAsset ResolveHorseGlyphFont()
    {
        if (horseGlyphFont != null) return horseGlyphFont;
        return Resources.Load<TMP_FontAsset>("HorseGlyphFont");
    }

    // owned column: horse font + U+130D7 once bred; optional second font for U+10083 𐂃 when barn is empty
    void ApplyOwnedSlotFontAndGlyphs()
    {
        if (ownedAscii == null) return;
        var horseFont = ResolveHorseGlyphFont();
        var unownedFont = noOwnedYetGlyphFont != null ? noOwnedYetGlyphFont : horseFont;
        bool hasOwned = session != null && session.SelectedOwned != null;
        ownedAscii.font = hasOwned ? horseFont : unownedFont;
        if (horseFont != null) EnsureCodePointsInFont(horseFont, HorseUnicodeCodePoint);
        if (noOwnedYetGlyphFont != null) EnsureCodePointsInFont(noOwnedYetGlyphFont, NoOwnedYetUnicodeCodePoint);
    }

    static void EnsureCodePointsInFont(TMP_FontAsset font, uint[] codePoints)
    {
        if (font == null || codePoints == null || codePoints.Length == 0) return;
        if (font.atlasPopulationMode != AtlasPopulationMode.Dynamic) return;
        font.TryAddCharacters(codePoints, out _);
    }

    // constant pixel size makes phones tiny; scale with reference resolution instead
    void ApplyMobileFriendlyCanvasScaler(Canvas canvas)
    {
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.55f;
    }

    // full-screen dim layer behind all breeding ui (does not eat raycasts)
    void CreateDarkBackdrop()
    {
        var go = new GameObject("DarkBackdrop", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(uiRoot, false);
        go.transform.SetAsFirstSibling();
        var rt = go.GetComponent<RectTransform>();
        StretchFull(rt);
        var img = go.GetComponent<Image>();
        img.sprite = UiWhiteSprite();
        img.color = new Color(0.07f, 0.08f, 0.11f, 0.96f);
        img.raycastTarget = false;
    }

    // make an empty rect under uiRoot with given anchor box (0-1 screen fractions)
    Transform CreateRegion(string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        // new empty ui object
        var go = new GameObject(name, typeof(RectTransform));
        // parent under our built root
        go.transform.SetParent(uiRoot, false);
        var rt = go.GetComponent<RectTransform>();
        // anchor box defines where on screen
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        // no extra pixel inset
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go.transform;
    }

    // stretch rect to parent edges
    void StretchFull(RectTransform rt)
    {
        if (rt == null)
        {
            Debug.LogError("BreedingUIBuilder.StretchFull: RectTransform was null.");
            return;
        }
        // full stretch anchors
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        // flush to parent
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // create a TextMeshProUGUI in a anchored box
    TextMeshProUGUI CreateTmp(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, float size, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        tmp.text = "";
        tmp.textWrappingMode = TextWrappingModes.Normal;
        // text should not steal raycasts (swipe overlays sit on top where needed)
        tmp.raycastTarget = false;
        if (primaryTextFont != null)
            tmp.font = primaryTextFont;
        return tmp;
    }

    void StyleHorseStatsPanel(TextMeshProUGUI tmp)
    {
        if (tmp == null) return;
        // Bold/Italic changes TMP's lookup path; without alternative typefaces, Dynamic fallbacks may skip TryAdd.
        tmp.fontStyle = FontStyles.Normal;
        tmp.fontWeight = FontWeight.Regular;
        if (horseStatsFontSize > 0f)
            tmp.fontSize = horseStatsFontSize;
        tmp.lineSpacing = -28f;
        tmp.overflowMode = TextOverflowModes.Overflow;
    }

    // nearly invisible image + ParentSwipeSelector covering whole column
    void CreateSwipeOverlay(Transform column, string name, bool mare, bool ownedPicker)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ParentSwipeSelector));
        go.transform.SetParent(column, false);
        var rt = go.GetComponent<RectTransform>();
        StretchFull(rt);
        var img = go.GetComponent<Image>();
        img.sprite = UiWhiteSprite();
        img.color = new Color(1f, 1f, 1f, 0.02f);
        img.raycastTarget = true;
        var swipe = go.GetComponent<ParentSwipeSelector>();
        swipe.Configure(session, mare, ownedPicker, RefreshAllText);
    }

    void CreateViewTabStrip(Transform column, string name, Vector2 anchorMin, Vector2 anchorMax, out Image statsBg, out Image looksBg)
    {
        statsBg = null;
        looksBg = null;
        var row = new GameObject(name, typeof(RectTransform));
        row.transform.SetParent(column, false);
        var rt = row.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        statsBg = CreateTabChoice(row.transform, "stats", new Vector2(0f, 0f), new Vector2(0.48f, 1f), () => SetHorsePanelTab(HorsePanelTab.Stats));
        looksBg = CreateTabChoice(row.transform, "looks", new Vector2(0.52f, 0f), new Vector2(1f, 1f), () => SetHorsePanelTab(HorsePanelTab.Looks));
        row.transform.SetAsLastSibling();
    }

    Image CreateTabChoice(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Tab_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = anchorMin;
        r.anchorMax = anchorMax;
        r.offsetMin = new Vector2(2f, 1f);
        r.offsetMax = new Vector2(-2f, -1f);
        var img = go.GetComponent<Image>();
        img.sprite = UiWhiteSprite();
        img.color = new Color(0.22f, 0.24f, 0.3f, 0.95f);
        img.raycastTarget = true;
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var tmpGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        tmpGo.transform.SetParent(go.transform, false);
        var tr = tmpGo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;
        var tmp = tmpGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label == "stats" ? "stats" : "looks";
        tmp.fontSize = 22f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (primaryTextFont != null)
            tmp.font = primaryTextFont;
        return img;
    }

    void SetHorsePanelTab(HorsePanelTab tab)
    {
        _horsePanelTab = tab;
        SyncTabButtonColors();
        RefreshAllText();
    }

    void SyncTabButtonColors()
    {
        bool stats = _horsePanelTab == HorsePanelTab.Stats;
        ApplyTabColor(_mareTabStatsBg, _mareTabLooksBg, stats);
        ApplyTabColor(_stallTabStatsBg, _stallTabLooksBg, stats);
        ApplyTabColor(_ownedTabStatsBg, _ownedTabLooksBg, stats);
    }

    static void ApplyTabColor(Image statsBg, Image looksBg, bool statsSelected)
    {
        if (statsBg != null)
            statsBg.color = statsSelected ? new Color(0.35f, 0.48f, 0.38f, 0.98f) : new Color(0.22f, 0.24f, 0.3f, 0.95f);
        if (looksBg != null)
            looksBg.color = !statsSelected ? new Color(0.35f, 0.42f, 0.52f, 0.98f) : new Color(0.22f, 0.24f, 0.3f, 0.95f);
    }

    void BuildFoalNameOverlay()
    {
        if (hostCanvas == null) return;
        _foalNameOverlay = new GameObject("FoalNameOverlay", typeof(RectTransform), typeof(Image));
        _foalNameOverlay.transform.SetParent(hostCanvas.transform, false);
        StretchFull(_foalNameOverlay.GetComponent<RectTransform>());
        var dim = _foalNameOverlay.GetComponent<Image>();
        dim.sprite = UiWhiteSprite();
        dim.color = new Color(0f, 0f, 0f, 0.65f);
        dim.raycastTarget = true;

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(_foalNameOverlay.transform, false);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.1f, 0.38f);
        prt.anchorMax = new Vector2(0.9f, 0.62f);
        prt.offsetMin = prt.offsetMax = Vector2.zero;
        var pimg = panel.GetComponent<Image>();
        pimg.sprite = UiWhiteSprite();
        pimg.color = new Color(0.14f, 0.15f, 0.18f, 1f);

        var title = CreateTmp("Title", panel.transform, new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.95f), 30f, TextAlignmentOptions.Center);
        title.text = "name your foal";

        _foalNameInput = CreateSimpleTmpInput(panel.transform, new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.65f));

        CreateDialogButton(panel.transform, "ok", new Vector2(0.52f, 0.08f), new Vector2(0.92f, 0.32f), new Color(0.3f, 0.45f, 0.35f, 1f), OnFoalNameOkClicked);
        CreateDialogButton(panel.transform, "cancel", new Vector2(0.08f, 0.08f), new Vector2(0.48f, 0.32f), new Color(0.35f, 0.28f, 0.28f, 1f), OnFoalNameCancelClicked);

        _foalNameOverlay.SetActive(false);
    }

    TMP_InputField CreateSimpleTmpInput(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var root = new GameObject("FoalNameInput", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        root.transform.SetParent(parent, false);
        var rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = anchorMin;
        rootRt.anchorMax = anchorMax;
        rootRt.offsetMin = rootRt.offsetMax = Vector2.zero;
        var rootImg = root.GetComponent<Image>();
        rootImg.sprite = UiWhiteSprite();
        rootImg.color = new Color(0.08f, 0.09f, 0.11f, 1f);

        var input = root.GetComponent<TMP_InputField>();

        var area = new GameObject("Area", typeof(RectTransform), typeof(RectMask2D));
        area.transform.SetParent(root.transform, false);
        var art = area.GetComponent<RectTransform>();
        art.anchorMin = new Vector2(0.03f, 0.12f);
        art.anchorMax = new Vector2(0.97f, 0.88f);
        art.offsetMin = art.offsetMax = Vector2.zero;

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(area.transform, false);
        StretchFull(textGo.GetComponent<RectTransform>());
        var t = textGo.GetComponent<TextMeshProUGUI>();
        t.color = Color.white;
        t.fontSize = 28f;
        t.alignment = TextAlignmentOptions.Left;
        t.margin = new Vector4(12f, 4f, 12f, 4f);
        if (primaryTextFont != null)
            t.font = primaryTextFont;

        var phGo = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        phGo.transform.SetParent(area.transform, false);
        StretchFull(phGo.GetComponent<RectTransform>());
        var ph = phGo.GetComponent<TextMeshProUGUI>();
        ph.text = "foal name";
        ph.color = new Color(1f, 1f, 1f, 0.35f);
        ph.fontSize = 28f;
        ph.fontStyle = FontStyles.Italic;
        ph.alignment = TextAlignmentOptions.Left;
        ph.margin = t.margin;
        if (primaryTextFont != null)
            ph.font = primaryTextFont;

        input.textViewport = art;
        input.textComponent = t;
        input.placeholder = ph;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterLimit = 48;
        return input;
    }

    Button CreateDialogButton(Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Color col, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(4f, 2f);
        rt.offsetMax = new Vector2(-4f, -2f);
        var img = go.GetComponent<Image>();
        img.sprite = UiWhiteSprite();
        img.color = col;
        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(onClick);
        var tmp = CreateTmp("Lbl", go.transform, Vector2.zero, Vector2.one, 26f, TextAlignmentOptions.Center);
        tmp.text = label;
        return btn;
    }

    void ShowFoalNameDialog(HorseTemplate foal)
    {
        _pendingFoal = foal;
        if (_foalNameInput != null)
        {
            string suggest = "Álogo " + (session != null ? session.ownedHorses.Count + 1 : 1);
            _foalNameInput.text = suggest;
            _foalNameInput.caretPosition = suggest.Length;
        }
        if (_foalNameOverlay != null)
        {
            _foalNameOverlay.SetActive(true);
            _foalNameOverlay.transform.SetAsLastSibling();
        }
        StartCoroutine(FocusFoalInputEndOfFrame());
    }

    IEnumerator FocusFoalInputEndOfFrame()
    {
        yield return null;
        if (_foalNameInput != null)
        {
            _foalNameInput.Select();
            _foalNameInput.ActivateInputField();
        }
    }

    void HideFoalNameDialog()
    {
        if (_foalNameOverlay != null)
            _foalNameOverlay.SetActive(false);
    }

    void OnFoalNameOkClicked()
    {
        if (_pendingFoal == null || session == null)
        {
            _pendingFoal = null;
            HideFoalNameDialog();
            return;
        }
        string n = _foalNameInput != null ? _foalNameInput.text : "";
        var foal = _pendingFoal;
        _pendingFoal = null;
        session.CommitFoalToBarn(foal, n);
        HideFoalNameDialog();
        RefreshAllText();
    }

    void OnFoalNameCancelClicked()
    {
        _pendingFoal = null;
        HideFoalNameDialog();
    }

    // long-press breed button with radial ring child
    void CreateBreedButton(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject("BreedButton", typeof(RectTransform), typeof(Image), typeof(LongPressRingButton));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var bg = go.GetComponent<Image>();
        bg.sprite = UiWhiteSprite();
        bg.color = new Color(0.25f, 0.35f, 0.55f, 0.95f);
        bg.raycastTarget = true;

        var label = CreateTmp("Label", go.transform, new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.85f), 40f, TextAlignmentOptions.Center);
        label.text = "hold to breed";

        var ringGo = new GameObject("Ring", typeof(RectTransform), typeof(Image));
        ringGo.transform.SetParent(go.transform, false);
        var ringRt = ringGo.GetComponent<RectTransform>();
        StretchFull(ringRt);
        var ring = ringGo.GetComponent<Image>();
        ring.sprite = UiWhiteSprite();
        ring.type = Image.Type.Filled;
        ring.fillMethod = Image.FillMethod.Radial360;
        ring.fillOrigin = (int)Image.Origin360.Top;
        ring.fillClockwise = true;
        ring.fillAmount = 0f;
        ring.color = new Color(1f, 1f, 1f, 0.45f);
        ring.raycastTarget = false;

        var lpr = go.GetComponent<LongPressRingButton>();
        lpr.SetRingImage(ring);
        lpr.AddCompleteListener(OnBreedHoldComplete);
    }

    // long-press run race button
    void CreateRaceButton(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject("RaceButton", typeof(RectTransform), typeof(Image), typeof(LongPressRingButton));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var bg = go.GetComponent<Image>();
        bg.sprite = UiWhiteSprite();
        bg.color = new Color(0.4f, 0.3f, 0.25f, 0.95f);
        bg.raycastTarget = true;

        var label = CreateTmp("Label", go.transform, new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.85f), 40f, TextAlignmentOptions.Center);
        label.text = "hold to run race";

        var ringGo = new GameObject("Ring", typeof(RectTransform), typeof(Image));
        ringGo.transform.SetParent(go.transform, false);
        var ringRt = ringGo.GetComponent<RectTransform>();
        StretchFull(ringRt);
        var ring = ringGo.GetComponent<Image>();
        ring.sprite = UiWhiteSprite();
        ring.type = Image.Type.Filled;
        ring.fillMethod = Image.FillMethod.Radial360;
        ring.fillOrigin = (int)Image.Origin360.Top;
        ring.fillClockwise = true;
        ring.fillAmount = 0f;
        ring.color = new Color(1f, 1f, 1f, 0.45f);
        ring.raycastTarget = false;

        var lpr = go.GetComponent<LongPressRingButton>();
        lpr.SetRingImage(ring);
        lpr.AddCompleteListener(OnRaceHoldComplete);
    }

    // fired when breed long-press completes
    void OnBreedHoldComplete()
    {
        var foal = session.BuildFoalFromSelection();
        if (foal == null)
        {
            if (resultText != null)
                resultText.text = "could not breed (need mare + stallion in pool).";
            return;
        }
        ShowFoalNameDialog(foal);
    }

    // fired when race long-press completes
    void OnRaceHoldComplete()
    {
        var runner = session.SelectedOwned;
        if (runner == null)
        {
                resultText.text = "breed a horse first (no owned álogo).";
            return;
        }

        int nRunners = Random.Range(5, 13);
        var field = new List<HorseTemplate> { runner };
        for (int i = 1; i < nRunners; i++)
            field.Add(session.GenerateRandomRacer("Rival " + i));

        Surface surf = (Surface)Random.Range(0, 3);
        Going going = (Going)Random.Range(0, 5);

        var ticketWeights = new Dictionary<HorseTemplate, int>(field.Count);
        foreach (var h in field)
            ticketWeights[h] = RacePerformanceCalculator.CalculateActualPerformance(h, raceFurlongs, surf, going);

        var order = SimpleRaceSimulator.RunRace(field, raceFurlongs, surf, going, ticketWeights);

        runner.nextRunPerformancePreview = RacePerformanceCalculator.CalculateActualPerformance(
            runner, raceFurlongs, runner.preferredSurface, runner.preferredGoing);

        var sb = new StringBuilder();
        sb.AppendLine("finish order (" + nRunners + " runners, " + raceFurlongs + "f, " + surf + ", " + going + "):");
        for (int i = 0; i < order.Count; i++)
            sb.AppendLine((i + 1) + ". " + order[i]);

        resultText.text = sb.ToString();
        RefreshAllText();
    }

    // set hieroglyph horse on all three columns (mare column is flipped)
    void ApplyHorseGlyphs()
    {
        if (mareAscii != null)
        {
            mareAscii.text = HorseUnicodeGlyph;
            ApplyMareGlyphFlip();
        }
        if (stallionAscii != null)
        {
            stallionAscii.text = HorseUnicodeGlyph;
            stallionAscii.transform.localScale = Vector3.one;
        }
        if (ownedAscii != null)
        {
            var o = session != null ? session.SelectedOwned : null;
            ApplyOwnedSlotFontAndGlyphs();
            ownedAscii.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            if (o == null)
            {
                ownedAscii.text = NoOwnedHorseYetGlyph;
                ownedAscii.transform.localScale = Vector3.one;
            }
            else
            {
                ownedAscii.text = HorseUnicodeGlyph;
                // mares + fillies face like left column; stallions + colts face like right column
                bool femaleFacing = o.gender == Gender.Mare || o.gender == Gender.Filly;
                ownedAscii.transform.localScale = femaleFacing ? new Vector3(-1f, 1f, 1f) : Vector3.one;
            }
        }
    }

    // refresh lists and stats from session
    public void RefreshAllText()
    {
        ApplyHorseGlyphs();
        SyncTabButtonColors();

        if (mareListText != null)
            mareListText.text = BuildNameList(session.mares, session.mareIndex);
        if (stallionListText != null)
            stallionListText.text = BuildNameList(session.stallions, session.stallionIndex);

        if (mareStatsText != null)
        {
            var m = session.SelectedMare;
            mareStatsText.text = m == null ? "" : FormatHorse(m, false);
        }

        if (stallionStatsText != null)
        {
            var s2 = session.SelectedStallion;
            stallionStatsText.text = s2 == null ? "" : FormatHorse(s2, false);
        }

        if (ownedStatsText != null)
        {
            var o = session.SelectedOwned;
            ownedStatsText.text = o == null ? "none yet — hold breed." : FormatHorse(o, true);
        }
    }

    // build multiline name list with > around selected index
    string BuildNameList(List<HorseTemplate> list, int selected)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            string n = list[i].name;
            if (i == selected)
                sb.AppendLine("> " + n + " <");
            else
                sb.AppendLine(n);
        }
        return sb.ToString();
    }

    string FormatHorse(HorseTemplate h, bool isOwnedHorse)
    {
        return _horsePanelTab == HorsePanelTab.Stats
            ? FormatHorseStats(h, isOwnedHorse)
            : FormatHorseLooks(h);
    }

    string FormatHorseStats(HorseTemplate h, bool isOwnedHorse)
    {
        var surf = h.preferredSurface.ToString().ToLowerInvariant();
        var going = h.preferredGoing.ToString().ToLowerInvariant();
        var sb = new StringBuilder();
        sb.AppendLine($"{h.name} ({h.gender})");
        sb.AppendLine($"sire {h.sireName}  dam {h.damName}");
        sb.AppendLine();
        sb.AppendLine($"reputation {h.reputation}  fitness {h.fitness}  preferences {surf} / {going}");
        sb.AppendLine($"realized/potential {h.realizedPotential}/{h.potential}  form/condition {h.condition}/{h.conditionPercent}");
        sb.AppendLine();
        sb.AppendLine($"spd {h.spd}  stm {h.stamina}  acl {h.acceleration}  sta {h.start}  str {h.strength}  int {h.intelligence}  mov {h.movement}");
        sb.AppendLine();
        sb.AppendLine(
            $"ten {h.tenacity}  ent {h.enthusiasm}  cfd {h.confidence}  BQ {h.battlingQualities}  CB {h.cruisingBurst}  XSR {h.extraSpeedRating}");
        sb.AppendLine(
            $"FA {h.finishApplication}  cst {h.consistency}  DA {h.distanceAdaptability}  GSA {h.goingSurfaceAdaptability}  qrk {h.quirks}");
        sb.AppendLine();
        if (isOwnedHorse && h.nextRunPerformancePreview > 0)
            sb.AppendLine($"performance (next run, prefs) {h.nextRunPerformancePreview}");
        else
        {
            int scout = RacePerformanceCalculator.CalculateActualPerformance(h, raceFurlongs, h.preferredSurface, h.preferredGoing);
            sb.AppendLine($"performance (scout, prefs) {scout}");
        }
        return sb.ToString();
    }

    string FormatHorseLooks(HorseTemplate h)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{h.name} ({h.gender})");
        sb.AppendLine($"sire {h.sireName}  dam {h.damName}");
        sb.AppendLine($"coat {h.coat}  rarity {h.coatRarity}");
        sb.AppendLine($"modifier {BreezeCosmeticTables.JoinList(h.modifiers)}  marking {BreezeCosmeticTables.JoinList(h.markings)}");
        sb.AppendLine($"dilution {h.dilution}");
        if (!string.IsNullOrEmpty(h.modifierAnomaly)) sb.AppendLine($"modifier anomaly {h.modifierAnomaly}");
        if (!string.IsNullOrEmpty(h.dilutionAnomaly)) sb.AppendLine($"dilution anomaly {h.dilutionAnomaly}");
        if (!string.IsNullOrEmpty(h.markingAnomaly)) sb.AppendLine($"marking anomaly {h.markingAnomaly}");
        sb.AppendLine($"anomaly {h.anomaly}");
        sb.AppendLine($"mane {h.mane}  tail {h.tail}  nose {h.nose}");
        sb.AppendLine($"height {h.heightHands:0.0} hh  progeny {h.progenyPotential}");
        if (h.recordedDamCoat != "N/A")
        {
            sb.AppendLine(
                $"at birth — dam coat {h.recordedDamCoat} ({h.recordedDamCoatRarity})  dam trim {h.recordedDamMane} / {h.recordedDamTail} / {h.recordedDamNose}");
            sb.AppendLine(
                $"at birth — dam mod {BreezeCosmeticTables.JoinList(h.recordedDamModifiers)}  dam mk {BreezeCosmeticTables.JoinList(h.recordedDamMarkings)}");
        }
        if (h.recordedSireMane != "N/A")
            sb.AppendLine($"at birth — sire trim {h.recordedSireMane} / {h.recordedSireTail} / {h.recordedSireNose}");
        return sb.ToString();
    }
}
