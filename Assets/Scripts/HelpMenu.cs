using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

// full-screen menu from double-tap on background: controls, reset pools, quit, X close
public class HelpMenu : MonoBehaviour
{
    // session for reset pools (set in Initialize)
    BreedingSession session;
    // refresh main ui after reset
    BreedingUIBuilder uiBuilder;
    TMP_FontAsset _menuFont;

    // white 1x1 for ui images
    static Sprite _white;

    static Sprite WhiteSprite()
    {
        if (_white == null)
        {
            var t = Texture2D.whiteTexture;
            _white = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100f);
        }
        return _white;
    }

    bool built;

    // called once from BreedingUIBuilder after this component is added
    public void Initialize(BreedingSession breedingSession, BreedingUIBuilder builder, TMP_FontAsset menuFont = null)
    {
        if (built) return;
        built = true;
        session = breedingSession;
        uiBuilder = builder;
        _menuFont = menuFont;
        BuildUi();
        gameObject.SetActive(false);
    }

    void ApplyMenuFont(TextMeshProUGUI tmp)
    {
        if (tmp != null && _menuFont != null)
            tmp.font = _menuFont;
    }

    void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void BuildUi()
    {
        var rootRt = GetComponent<RectTransform>();
        StretchFull(rootRt);

        var dim = gameObject.GetComponent<Image>();
        if (dim == null)
            dim = gameObject.AddComponent<Image>();
        dim.sprite = WhiteSprite();
        dim.color = new Color(0f, 0f, 0f, 0.55f);
        dim.raycastTarget = true;

        var panel = new GameObject("MenuPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        var panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.08f, 0.12f);
        panelRt.anchorMax = new Vector2(0.92f, 0.88f);
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        var panelImg = panel.GetComponent<Image>();
        panelImg.sprite = WhiteSprite();
        panelImg.color = new Color(0.12f, 0.13f, 0.16f, 1f);
        panelImg.raycastTarget = true;

        // x close top-right
        var xBtn = new GameObject("CloseX", typeof(RectTransform), typeof(Image), typeof(Button));
        xBtn.transform.SetParent(panel.transform, false);
        var xRt = xBtn.GetComponent<RectTransform>();
        xRt.anchorMin = new Vector2(0.88f, 0.86f);
        xRt.anchorMax = new Vector2(0.98f, 0.98f);
        xRt.offsetMin = Vector2.zero;
        xRt.offsetMax = Vector2.zero;
        var xImg = xBtn.GetComponent<Image>();
        xImg.sprite = WhiteSprite();
        xImg.color = new Color(0.35f, 0.2f, 0.2f, 1f);
        var xB = xBtn.GetComponent<Button>();
        xB.onClick.AddListener(Close);

        var xLabel = new GameObject("XText", typeof(RectTransform), typeof(TextMeshProUGUI));
        xLabel.transform.SetParent(xBtn.transform, false);
        StretchFull(xLabel.GetComponent<RectTransform>());
        var xTmp = xLabel.GetComponent<TextMeshProUGUI>();
        xTmp.text = "X";
        xTmp.fontSize = 62;
        xTmp.alignment = TextAlignmentOptions.Center;
        xTmp.color = Color.white;
        xTmp.raycastTarget = false;

        // title
        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(panel.transform, false);
        var titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.05f, 0.82f);
        titleRt.anchorMax = new Vector2(0.85f, 0.96f);
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;
        var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
        titleTmp.text = "breeze mobile — menu";
        titleTmp.fontSize = 48;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Left;
        titleTmp.color = Color.white;
        ApplyMenuFont(titleTmp);

        // controls body
        var bodyGo = new GameObject("ControlsBody", typeof(RectTransform), typeof(TextMeshProUGUI));
        bodyGo.transform.SetParent(panel.transform, false);
        var bodyRt = bodyGo.GetComponent<RectTransform>();
        bodyRt.anchorMin = new Vector2(0.05f, 0.38f);
        bodyRt.anchorMax = new Vector2(0.95f, 0.80f);
        bodyRt.offsetMin = Vector2.zero;
        bodyRt.offsetMax = Vector2.zero;
        var bodyTmp = bodyGo.GetComponent<TextMeshProUGUI>();
        bodyTmp.text =
            "controls\n" +
            "• swipe left/right on a column to change mare, stallion, or owned álogo\n" +
            "• hold blue button to breed (ring fills)\n" +
            "• hold brown button to simulate a race\n" +
            "• double-tap empty background: open this menu\n" +
            "• triple-tap empty background: reset session pools (new mares + stallions)\n";
        bodyTmp.fontSize = 40;
        bodyTmp.alignment = TextAlignmentOptions.TopLeft;
        bodyTmp.color = new Color(0.92f, 0.92f, 0.92f);
        bodyTmp.textWrappingMode = TextWrappingModes.Normal;
        ApplyMenuFont(bodyTmp);

        // reset pools row
        AddMenuButton(panel.transform, "ResetBreedingPoolsBtn", "reset session pools (new mares/stallions)",
            new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.34f), OnResetPools);

        // quit row
        AddMenuButton(panel.transform, "QuitBtn", "exit game",
            new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.18f), ExitGame);
    }

    void AddMenuButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.sprite = WhiteSprite();
        img.color = new Color(0.22f, 0.28f, 0.4f, 1f);
        go.GetComponent<Button>().onClick.AddListener(onClick);

        var tGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        tGo.transform.SetParent(go.transform, false);
        StretchFull(tGo.GetComponent<RectTransform>());
        var tmp = tGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 40;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        ApplyMenuFont(tmp);
    }

    void OnResetPools()
    {
        if (session != null)
            session.RegeneratePools();
        uiBuilder?.RefreshAllText();
    }

    void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Open()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
