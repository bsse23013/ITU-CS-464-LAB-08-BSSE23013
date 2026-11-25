using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Editor-only helper that builds the Canvas, the ten screens and the button wiring once.
// After the scene is saved this file (and the whole Editor folder) is not needed any more.
public static class MenuBuilder
{
    static UIManager ui;
    static Shop shop;
    static SkinPicker picker;
    static SettingsMenu settings;
    static SceneLoader loader;
    static VolumeSettings volume;
    static List<TMP_Text> levelLabels = new List<TMP_Text>();
    static List<TMP_Text> coinTexts = new List<TMP_Text>();

    // ------------------------------------------------------------ small helpers
    static RectTransform NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    // position = centre of the element, measured from the top / bottom / middle of its parent
    static RectTransform Top(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    static RectTransform Bottom(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    static RectTransform Mid(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    static RectTransform Corner(RectTransform rt, bool left, float margin, float top, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(left ? 0f : 1f, 1f);
        rt.pivot = new Vector2(left ? 0f : 1f, 1f);
        rt.anchoredPosition = new Vector2(left ? margin : -margin, -top);
        rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    static RectTransform Fill(RectTransform rt, float left = 0f, float right = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, 0f);
        rt.offsetMax = new Vector2(-right, 0f);
        return rt;
    }

    static Image NewImage(string name, Transform parent, string sprite, Color colour, bool sliced = false, bool raycast = false)
    {
        RectTransform rt = NewRect(name, parent);
        Image img = rt.gameObject.AddComponent<Image>();
        if (sprite != null) img.sprite = Art.Sprite(sprite);
        img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        img.color = colour;
        img.raycastTarget = raycast;
        return img;
    }

    static TMP_Text NewText(string name, Transform parent, string text, int size, Color colour, TMP_FontAsset font, bool wrap = false)
    {
        RectTransform rt = NewRect(name, parent);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font;
        t.fontSize = size;
        t.color = colour;
        t.alignment = TextAlignmentOptions.Center;
        t.text = text;
        t.raycastTarget = false;
        t.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        return t;
    }

    // a rounded card
    static RectTransform Card(Transform parent, string name, Color colour)
    {
        RectTransform root = NewRect(name, parent);
        Image face = NewImage("Face", root, "rrect_b44", colour, true);
        Fill(face.rectTransform);
        return root;
    }

    static Button MakeButton(Transform parent, string name, float w, float h, Color face, string label, int size, Color labelColour, string icon, TMP_FontAsset font, string sprite = "pill_b64")
    {
        RectTransform root = NewRect(name, parent);
        root.sizeDelta = new Vector2(w, h);
        Image bg = NewImage("Face", root, sprite, face, sprite != "circle", true);
        Fill(bg.rectTransform);
        if (icon != null)
        {
            Image ic = NewImage("Icon", root, icon, labelColour);
            if (label == null) Mid(ic.rectTransform, 0f, 0f, 64f, 64f);
            else Mid(ic.rectTransform, -w / 2f + 90f, 0f, 60f, 60f);
        }
        if (label != null)
        {
            TMP_Text t = NewText("Label", root, label, size, labelColour, font);
            Fill(t.rectTransform, icon != null ? 40f : 0f, 0f);
        }
        Button b = root.gameObject.AddComponent<Button>();
        b.targetGraphic = bg;
        ColorBlock cb = b.colors;
        cb.pressedColor = new Color(0.8f, 0.8f, 0.82f);
        cb.disabledColor = Color.white;          // a locked button is drawn by hand (dark face, lock icon, word)
        b.colors = cb;
        root.gameObject.AddComponent<ButtonPress>();
        return b;
    }

    static RectTransform RT(Component c) { return (RectTransform)c.transform; }

    // ------------------------------------------------------------ Lab 08 helpers
    const string AudioDir = "Assets/Lab08/Audio/";

    static AudioClip Clip(string path)
    {
        AudioClip c = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioDir + path);
        if (c == null) Debug.LogError("[Lab08] Missing audio clip " + path);
        return c;
    }

    static AudioSource NewSource(Transform parent, string name, AudioMixerGroup group)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        AudioSource s = go.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.spatialBlend = 0f;                 // 2D
        s.outputAudioMixerGroup = group;     // every source goes to a mixer group
        return s;
    }

    static AudioSource[] NewSources(Transform parent, string name, AudioMixerGroup group, int count)
    {
        AudioSource[] list = new AudioSource[count];
        for (int i = 0; i < count; i++) list[i] = NewSource(parent, name + (i + 1), group);
        return list;
    }

    // One "Audio" object with the four reusable audio scripts and all the AudioSources.
    static void BuildAudio(Transform parent)
    {
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(AudioDir + "GameMixer.mixer");
        AudioMixerGroup music = mixer.FindMatchingGroups("Master/Music")[0];
        AudioMixerGroup sfx = mixer.FindMatchingGroups("Master/SFX")[0];
        AudioMixerGroup uiGroup = mixer.FindMatchingGroups("Master/UI")[0];

        GameObject audio = new GameObject("Audio");

        UIAudio ua = audio.AddComponent<UIAudio>();
        ua.sources = NewSources(audio.transform, "UiSource", uiGroup, 2);
        ua.hover = Clip("UI/ui_hover.wav");
        ua.click = Clip("UI/ui_click.wav");
        ua.back = Clip("UI/ui_back.wav");
        ua.open = Clip("UI/ui_open.wav");
        ua.close = Clip("UI/ui_close.wav");
        ua.pause = Clip("UI/ui_pause.wav");
        ua.resume = Clip("UI/ui_resume.wav");
        ua.success = Clip("UI/ui_success.wav");
        ua.error = Clip("UI/ui_error.wav");

        SfxPlayer sp = audio.AddComponent<SfxPlayer>();
        sp.sources = NewSources(audio.transform, "SfxSource", sfx, 4);

        MusicPlayer mp = audio.AddComponent<MusicPlayer>();
        mp.sources = NewSources(audio.transform, "MusicSource", music, 2);

        MusicController mc = audio.AddComponent<MusicController>();
        mc.menuMusic = Clip("Music/music_menu.ogg");
        mc.gameMusic = Clip("Music/music_game.ogg");
        mc.resultsMusic = Clip("Music/music_results.ogg");

        ResultSounds rs = audio.AddComponent<ResultSounds>();
        rs.winSound = Clip("SFX/sfx_win.wav");
        rs.loseSound = Clip("SFX/sfx_lose.wav");

        volume = audio.AddComponent<VolumeSettings>();
        volume.mixer = mixer;
    }

    // wire a button to UIManager.Show(index) or to any method, the same as filling the OnClick list by hand
    static void Go(Button b, int index) { UnityEventTools.AddIntPersistentListener(b.onClick, ui.Show, index); }
    static void Call(Button b, UnityAction action) { UnityEventTools.AddPersistentListener(b.onClick, action); }

    static RectTransform Panel(Transform parent, string name)
    {
        RectTransform p = NewRect(name, parent);
        Fill(p);
        p.gameObject.AddComponent<CanvasGroup>();
        p.gameObject.SetActive(false);
        return p;
    }

    static void Header(Transform panel, string title, int backIndex)
    {
        Button back = MakeButton(panel, "BackButton", 132f, 132f, Palette.PanelHi, null, 0, Palette.Text, "icon_back", Art.Bold, "circle");
        Corner(RT(back), true, 60f, 70f, 132f, 132f);
        back.GetComponent<ButtonPress>().pressSound = UISound.Back;
        Go(back, backIndex);
        TMP_Text t = NewText("Title", panel, title, 88, Palette.Text, Art.Display);
        Top(t.rectTransform, 0f, 136f, 640f, 120f);
    }

    static TMP_Text CoinChip(Transform parent, bool corner)
    {
        RectTransform root = NewRect("CoinChip", parent);
        if (corner) Corner(root, false, 60f, 70f, 330f, 110f);
        Image face = NewImage("Face", root, "pill_b64", Palette.Panel, true); Fill(face.rectTransform);
        Image outline = NewImage("Outline", root, "pill_outline_b64", Palette.Stroke, true); Fill(outline.rectTransform);
        Image ic = NewImage("Icon", root, "icon_coin", Palette.Gold); Mid(ic.rectTransform, -115f, 0f, 70f, 70f);
        TMP_Text t = NewText("Value", root, "300", 46, Palette.Text, Art.Bold);
        Mid(t.rectTransform, 30f, 0f, 200f, 80f);
        coinTexts.Add(t);
        return t;
    }

    // ------------------------------------------------------------ build everything
    public static void Build()
    {
        levelLabels.Clear();
        coinTexts.Clear();
        GameObject canvasGo = new GameObject("MenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        RectTransform root = (RectTransform)canvasGo.transform;

        // background: dark colour + a few soft circles (in one object, so it can be hidden while playing)
        RectTransform backdrop = NewRect("Backdrop", root);
        Fill(backdrop);
        Image bg = NewImage("Background", backdrop, null, Palette.BgDeep); Fill(bg.rectTransform);
        Image glow = NewImage("Glow", backdrop, "fade_v", Palette.BgMid); Fill(glow.rectTransform);
        float[] xs = { -420f, 440f, -300f, 380f };
        float[] ys = { 700f, 380f, -520f, -760f };
        float[] sizes = { 700f, 520f, 640f, 460f };
        for (int i = 0; i < 4; i++)
        {
            Image c = NewImage("Circle" + i, backdrop, "circle", new Color(0.2f, 0.24f, 0.58f, 0.26f));
            Mid(c.rectTransform, xs[i], ys[i], sizes[i], sizes[i]);
        }

        GameObject mgr = new GameObject("UIManager");
        ui = mgr.AddComponent<UIManager>();
        shop = mgr.AddComponent<Shop>();
        picker = mgr.AddComponent<SkinPicker>();
        settings = mgr.AddComponent<SettingsMenu>();
        loader = mgr.AddComponent<SceneLoader>();
        ui.shop = shop;
        ui.loader = loader;
        ui.backdrop = backdrop.gameObject;
        loader.ui = ui;
        BuildAudio(root);

        RectTransform screens = NewRect("Screens", root);
        Fill(screens);
        ui.panels = new GameObject[]
        {
            BuildTitle(screens), BuildMain(screens), BuildLevels(screens), BuildLoadout(screens), BuildGame(screens),
            BuildResults(screens), BuildShop(screens), BuildSettings(screens), BuildCredits(screens), BuildLoading(screens)
        };
        ui.pausePanel = BuildPause(screens);

        // brightness layer: black, above everything, never blocks a tap
        Image dimmer = NewImage("Dimmer", root, null, new Color(0f, 0f, 0f, 0f));
        Fill(dimmer.rectTransform);
        settings.dimmer = dimmer;

        ui.levelLabels = levelLabels.ToArray();
        shop.coinTexts = coinTexts.ToArray();

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }
    }

    // ------------------------------------------------------------ 1 Title
    static GameObject BuildTitle(Transform parent)
    {
        RectTransform p = Panel(parent, "Screen_01_Title");
        Image tap = NewImage("TapArea", p, null, new Color(1f, 1f, 1f, 0f), false, true);
        Fill(tap.rectTransform);
        Button b = tap.gameObject.AddComponent<Button>();
        b.transition = Selectable.Transition.None;
        Go(b, 1);

        TMP_Text roll = NewText("Roll", p, "ROLL", 200, Palette.Text, Art.Display);
        Top(roll.rectTransform, 0f, 400f, 900f, 220f);
        TMP_Text rush = NewText("Rush", p, "RUSH", 200, Palette.Accent, Art.Display);
        Top(rush.rectTransform, 0f, 600f, 900f, 220f);
        Image ball = NewImage("Ball", p, "ball", Hex3(0x3CCFC4));
        Mid(ball.rectTransform, 0f, -150f, 330f, 330f);
        TMP_Text tapText = NewText("TapToStart", p, "TAP TO START", 56, Palette.Text, Art.Bold);
        Bottom(tapText.rectTransform, 0f, 340f, 800f, 90f);
        tapText.gameObject.AddComponent<Pulse>();
        TMP_Text foot = NewText("Footer", p, "CS464  ·  LAB 08", 32, Palette.Muted, Art.Body);
        Bottom(foot.rectTransform, 0f, 120f, 800f, 60f);
        return p.gameObject;
    }

    static Color Hex3(int rgb)
    {
        return new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);
    }

    // ------------------------------------------------------------ 2 Main menu
    static GameObject BuildMain(Transform parent)
    {
        RectTransform p = Panel(parent, "Screen_02_MainMenu");
        TMP_Text title = NewText("Title", p, "ROLL RUSH", 70, Palette.Text, Art.Display);
        Corner(title.rectTransform, true, 60f, 85f, 560f, 100f);
        title.alignment = TextAlignmentOptions.Left;
        CoinChip(p, true);

        Image ball = NewImage("Ball", p, "ball", Hex3(0x3CCFC4));
        Mid(ball.rectTransform, 0f, 400f, 340f, 340f);
        TMP_Text tip = NewText("Tip", p, "Tip: finish a level to earn coins", 38, Palette.Muted, Art.Body);
        Mid(tip.rectTransform, 0f, 110f, 900f, 60f);
        settings.tipsText = tip.gameObject;

        Button play = MakeButton(p, "PlayButton", 900f, 190f, Palette.Accent, "PLAY", 84, Palette.BgDeep, "icon_play", Art.Display);
        Mid(RT(play), 0f, -60f, 900f, 190f);
        Go(play, 2);
        Button shopBtn = MakeButton(p, "ShopButton", 900f, 140f, Palette.PanelHi, "SHOP", 52, Palette.Text, "icon_bag", Art.Bold);
        Mid(RT(shopBtn), 0f, -270f, 900f, 140f);
        Go(shopBtn, 6);
        Button set = MakeButton(p, "SettingsButton", 430f, 120f, Palette.Panel, "SETTINGS", 38, Palette.Text, "icon_gear", Art.Bold);
        Mid(RT(set), -235f, -460f, 430f, 120f);
        Go(set, 7);
        Button cr = MakeButton(p, "CreditsButton", 430f, 120f, Palette.Panel, "CREDITS", 38, Palette.Text, "icon_info", Art.Bold);
        Mid(RT(cr), 235f, -460f, 430f, 120f);
        Go(cr, 8);
        return p.gameObject;
    }

    // ------------------------------------------------------------ 3 Levels
    static GameObject BuildLevels(Transform parent)
    {
        RectTransform p = Panel(parent, "Screen_03_Levels");
        Header(p, "LEVELS", 1);
        string[] names = { "Warm-Up", "Zig-Zag", "Crusher Lane", "Sneaky Guards", "Final Rush" };
        for (int i = 0; i < 5; i++)
        {
            bool locked = i >= 2;
            Button b = MakeButton(p, "Level" + (i + 1), 880f, 170f, locked ? Palette.Disabled : Palette.Panel, null, 0, Palette.Text, null, Art.Bold, "pill_b64");
            Top(RT(b), 0f, 400f + i * 200f, 880f, 170f);
            Image badge = NewImage("Badge", b.transform, "circle", locked ? Palette.BgDeep : Palette.Accent);
            Mid(badge.rectTransform, -350f, 0f, 110f, 110f);
            if (locked)
            {
                Image lockIcon = NewImage("Lock", b.transform, "icon_lock", Palette.Muted);
                Mid(lockIcon.rectTransform, -350f, 0f, 56f, 56f);
            }
            else
            {
                TMP_Text num = NewText("Number", b.transform, (i + 1).ToString(), 60, Palette.BgDeep, Art.Display);
                Mid(num.rectTransform, -350f, 0f, 110f, 90f);
            }
            TMP_Text n = NewText("Name", b.transform, names[i], 52, locked ? Palette.Muted : Palette.Text, Art.Bold);
            n.alignment = TextAlignmentOptions.Left;
            Mid(n.rectTransform, 70f, 0f, 560f, 80f);
            if (locked)
            {
                TMP_Text word = NewText("Locked", b.transform, "LOCKED", 32, Palette.Muted, Art.Bold);
                Mid(word.rectTransform, 330f, 0f, 200f, 60f);
                b.interactable = false;
            }
            else
            {
                UnityEventTools.AddIntPersistentListener(b.onClick, ui.SelectLevel, i + 1);
            }
        }
        return p.gameObject;
    }

    // ------------------------------------------------------------ 4 Loadout
    static GameObject BuildLoadout(Transform parent)
    {
        RectTransform p = Panel(parent, "Screen_04_Loadout");
        Header(p, "LOADOUT", 2);
        TMP_Text level = NewText("Level", p, "LEVEL 1", 44, Palette.Muted, Art.Bold);
        Top(level.rectTransform, 0f, 290f, 700f, 60f);
        levelLabels.Add(level);

        RectTransform card = Card(p, "PreviewCard", Palette.Panel);
        Top(card, 0f, 650f, 900f, 560f);
        Image preview = NewImage("Preview", card, "ball", Hex3(0x3CCFC4));
        Mid(preview.rectTransform, 0f, 60f, 280f, 280f);
        TMP_Text skinName = NewText("SkinName", card, "CLASSIC", 70, Palette.Text, Art.Display);
        Mid(skinName.rectTransform, 0f, -190f, 800f, 100f);

        TMP_Text label = NewText("SkinLabel", p, "CHOOSE A SKIN", 38, Palette.Muted, Art.Bold);
        Top(label.rectTransform, 0f, 1010f, 800f, 60f);

        string[] names = { "CLASSIC", "RACER", "DOTTY", "ROYAL" };
        int[] cols = { 0x3CCFC4, 0xFF6B5A, 0xB48CFF, 0xFFE680 };
        string[] patterns = { null, "ball_stripe", "ball_dots", "ball_star" };
        RectTransform tilesRoot = NewRect("Tiles", p);
        Top(tilesRoot, 0f, 1180f, 900f, 200f);
        Image marker = NewImage("Selected", tilesRoot, "rrect_outline_b44", Palette.Accent, true);
        marker.rectTransform.sizeDelta = new Vector2(220f, 220f);
        RectTransform[] tiles = new RectTransform[4];
        Color[] colors = new Color[4];
        for (int i = 0; i < 4; i++)
        {
            colors[i] = Hex3(cols[i]);
            Button b = MakeButton(tilesRoot, "Skin" + i, 190f, 190f, Palette.Panel, null, 0, Palette.Text, null, Art.Bold, "rrect_b44");
            RT(b).anchoredPosition = new Vector2(-330f + i * 220f, 0f);
            Image ball = NewImage("Ball", b.transform, "ball", colors[i]);
            Mid(ball.rectTransform, 0f, 0f, 130f, 130f);
            if (patterns[i] != null)
            {
                Image pat = NewImage("Pattern", b.transform, patterns[i], new Color(0.06f, 0.07f, 0.19f, 0.6f));
                Mid(pat.rectTransform, 0f, 0f, 130f, 130f);
            }
            UnityEventTools.AddIntPersistentListener(b.onClick, picker.Pick, i);
            tiles[i] = RT(b);
        }
        marker.transform.SetAsFirstSibling();       // the frame sits behind the tiles
        picker.preview = preview;
        picker.skinName = skinName;
        picker.marker = marker.rectTransform;
        picker.tiles = tiles;
        picker.colors = colors;
        picker.names = names;
        picker.patterns = new string[] { "", "ball_stripe", "ball_dots", "ball_star" };

        Button start = MakeButton(p, "StartButton", 900f, 170f, Palette.Accent, "START", 80, Palette.BgDeep, "icon_play", Art.Display);
        Bottom(RT(start), 0f, 200f, 900f, 170f);
        Call(start, ui.StartGame);
        return p.gameObject;
    }

    // ------------------------------------------------------------ 5 Game (HUD over the Gameplay scene)
    static GameObject BuildGame(Transform parent)
    {
        RectTransform p = Panel(parent, "Screen_05_Game");

        // red flash over the whole screen when the player is hit (never blocks a tap)
        Image flash = NewImage("HitFlash", p, null, new Color(0.94f, 0.27f, 0.35f, 0f));
        Fill(flash.rectTransform);
        HitFlash hf = flash.gameObject.AddComponent<HitFlash>();
        hf.flash = flash;

        // health bar: a filled Image on a dark track
        RectTransform hud = NewRect("HealthBar", p);
        Corner(hud, true, 60f, 70f, 470f, 100f);
        Image track = NewImage("Track", hud, "pill_b64", new Color(0.06f, 0.07f, 0.19f, 0.8f), true); Fill(track.rectTransform);
        Image heart = NewImage("Icon", hud, "icon_heart", Palette.Red);
        Mid(heart.rectTransform, -190f, 0f, 60f, 60f);
        Image fill = NewImage("Fill", hud, "bar_8x1", Palette.Green);
        Mid(fill.rectTransform, 25f, 0f, 300f, 30f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 1f;
        TMP_Text hp = NewText("Value", hud, "3/3", 36, Palette.Text, Art.Bold);
        Mid(hp.rectTransform, 195f, 0f, 100f, 60f);
        HealthBarListener hbl = hud.gameObject.AddComponent<HealthBarListener>();
        hbl.fill = fill;
        hbl.label = hp;

        // coin counter
        RectTransform coin = NewRect("CoinCounter", p);
        Corner(coin, true, 60f, 190f, 250f, 90f);
        Image cface = NewImage("Face", coin, "pill_b64", new Color(0.06f, 0.07f, 0.19f, 0.8f), true); Fill(cface.rectTransform);
        Image cicon = NewImage("Icon", coin, "icon_coin", Palette.Gold);
        Mid(cicon.rectTransform, -75f, 0f, 56f, 56f);
        TMP_Text ctext = NewText("Value", coin, "0/8", 40, Palette.Text, Art.Bold);
        Mid(ctext.rectTransform, 35f, 0f, 130f, 60f);
        coin.gameObject.AddComponent<CoinCounter>().label = ctext;

        TMP_Text level = NewText("Level", p, "LEVEL 1", 44, Palette.Muted, Art.Bold);
        Top(level.rectTransform, 0f, 330f, 500f, 60f);
        levelLabels.Add(level);

        Button pause = MakeButton(p, "PauseButton", 132f, 132f, Palette.PanelHi, null, 0, Palette.Text, "icon_pause", Art.Bold, "circle");
        Corner(RT(pause), false, 60f, 70f, 132f, 132f);
        pause.GetComponent<ButtonPress>().pressSound = UISound.None;     // UIManager plays the pause sound
        Call(pause, ui.Pause);
        return p.gameObject;
    }

    // ------------------------------------------------------------ 6 Pause (sits on top of the game)
    static GameObject BuildPause(Transform parent)
    {
        RectTransform p = Panel(parent, "Screen_06_Pause");
        Image dim = NewImage("Dim", p, null, new Color(0.03f, 0.04f, 0.14f, 0.72f), false, true);
        Fill(dim.rectTransform);
        RectTransform card = Card(p, "Card", Palette.Panel);
        Mid(card, 0f, 0f, 860f, 760f);
        Image outline = NewImage("Outline", card, "rrect_outline_b44", Palette.Stroke, true); Fill(outline.rectTransform);
        TMP_Text title = NewText("Title", card, "PAUSED", 110, Palette.Text, Art.Display);
        Top(title.rectTransform, 0f, 120f, 760f, 140f);
        TMP_Text level = NewText("Level", card, "LEVEL 1", 44, Palette.Accent, Art.Bold);
        Top(level.rectTransform, 0f, 235f, 760f, 60f);
        levelLabels.Add(level);

        Button resume = MakeButton(card, "ResumeButton", 700f, 180f, Palette.Accent, "RESUME", 80, Palette.BgDeep, "icon_play", Art.Display);
        Top(RT(resume), 0f, 410f, 700f, 180f);
        resume.GetComponent<ButtonPress>().pressSound = UISound.None;    // UIManager plays the resume sound
        Call(resume, ui.Resume);
        // Fitts's law: Quit is far from Resume and red
        Button quit = MakeButton(card, "QuitButton", 700f, 130f, Palette.Red, "QUIT TO MENU", 44, Palette.BgDeep, "icon_home", Art.Bold);
        Top(RT(quit), 0f, 620f, 700f, 130f);
        quit.GetComponent<ButtonPress>().pressSound = UISound.None;      // QuitToMenu plays the close sound
        Call(quit, ui.QuitToMenu);
        return p.gameObject;
    }

    // ------------------------------------------------------------ 7 Results
    static GameObject BuildResults(Transform parent)
    {
        RectTransform p = Panel(parent, "Screen_07_Results");
        RectTransform card = Card(p, "Card", Palette.Panel);
        Mid(card, 0f, 0f, 900f, 1300f);

        Image banner = NewImage("Banner", card, "rrect_b44", Palette.Gold, true);
        Top(banner.rectTransform, 0f, 150f, 900f, 300f);
        Image trophy = NewImage("Trophy", card, "icon_trophy", Palette.BgDeep);
        Top(trophy.rectTransform, 0f, 90f, 100f, 100f);
        TMP_Text title = NewText("Title", card, "LEVEL CLEARED!", 100, Palette.BgDeep, Art.Display);
        Top(title.rectTransform, 0f, 205f, 840f, 130f);

        RectTransform stars = NewRect("Stars", card);
        Top(stars, 0f, 440f, 700f, 170f);
        for (int i = 0; i < 3; i++)
        {
            Image s = NewImage("Star" + i, stars, "icon_star", Palette.Gold);
            Mid(s.rectTransform, -190f + i * 190f, 0f, 170f, 170f);
        }
        TMP_Text coins = NewText("Reward", card, "+50 coins", 56, Palette.Gold, Art.Bold);
        Top(coins.rectTransform, 0f, 630f, 800f, 80f);

        Button next = MakeButton(card, "NextButton", 780f, 160f, Palette.Accent, "NEXT LEVEL", 66, Palette.BgDeep, "icon_next", Art.Display);
        Top(RT(next), 0f, 840f, 780f, 160f);
        Call(next, ui.NextLevel);
        Button retry = MakeButton(card, "RetryButton", 780f, 130f, Palette.PanelHi, "RETRY", 46, Palette.Text, "icon_restart", Art.Bold);
        Top(RT(retry), 0f, 1020f, 780f, 130f);
        Call(retry, ui.Retry);
        Button menu = MakeButton(card, "MenuButton", 780f, 130f, Palette.PanelHi, "MENU", 46, Palette.Text, "icon_home", Art.Bold);
        Top(RT(menu), 0f, 1170f, 780f, 130f);
        menu.GetComponent<ButtonPress>().pressSound = UISound.None;
        Call(menu, ui.QuitToMenu);

        ui.resultTitle = title;
        ui.resultBanner = banner;
        ui.starsRow = stars.gameObject;
        ui.nextButton = next.gameObject;
        return p.gameObject;
    }

    // ------------------------------------------------------------ 8 Shop
    static GameObject BuildShop(Transform parent)
    {
        RectTransform p = Panel(parent, "Screen_08_Shop");
        Header(p, "SHOP", 1);
        RectTransform chip = (RectTransform)CoinChip(p, false).transform.parent;
        Top(chip, 0f, 330f, 330f, 110f);

        string[] names = { "Speed boost", "Shield", "Coin burst" };
        string[] icons = { "icon_bolt", "icon_shield", "icon_burst" };
        int[] prices = { 40, 60, 80 };
        TMP_Text[] labels = new TMP_Text[3];
        for (int i = 0; i < 3; i++)
        {
            RectTransform row = Card(p, "Item" + i, Palette.Panel);
            Top(row, 0f, 560f + i * 230f, 880f, 200f);
            Image badge = NewImage("Badge", row, "circle", Palette.BgDeep);
            Mid(badge.rectTransform, -340f, 0f, 120f, 120f);
            Image ic = NewImage("Icon", row, icons[i], Palette.Gold);
            Mid(ic.rectTransform, -340f, 0f, 70f, 70f);
            TMP_Text n = NewText("Name", row, names[i], 50, Palette.Text, Art.Bold);
            n.alignment = TextAlignmentOptions.Left;
            Mid(n.rectTransform, -80f, 0f, 360f, 80f);
            Button buy = MakeButton(row, "BuyButton", 260f, 120f, Palette.Accent, "BUY " + prices[i], 44, Palette.BgDeep, null, Art.Bold);
            Mid(RT(buy), 290f, 0f, 260f, 120f);
            UnityEventTools.AddIntPersistentListener(buy.onClick, shop.Buy, i);
            labels[i] = buy.transform.Find("Label").GetComponent<TMP_Text>();
        }
        TMP_Text msg = NewText("Message", p, "Spend coins on boosts", 40, Palette.Muted, Art.Body);
        Top(msg.rectTransform, 0f, 1330f, 880f, 70f);
        shop.prices = prices;
        shop.buyLabels = labels;
        shop.message = msg;
        return p.gameObject;
    }

    // ------------------------------------------------------------ 9 Settings
    static Toggle MakeToggle(Transform parent, float x, float y)
    {
        RectTransform root = NewRect("Toggle", parent);
        Mid(root, x, y, 110f, 110f);
        Image box = NewImage("Box", root, "rrect_b44", Palette.BgDeep, true, true); Fill(box.rectTransform);
        Image outline = NewImage("Outline", root, "rrect_outline_b44", Palette.Stroke, true); Fill(outline.rectTransform);
        Image check = NewImage("Check", root, "icon_check", Palette.Green);
        Mid(check.rectTransform, 0f, 0f, 70f, 70f);
        Toggle t = root.gameObject.AddComponent<Toggle>();
        t.targetGraphic = box;
        t.graphic = check;
        t.isOn = true;
        root.gameObject.AddComponent<ButtonPress>();
        return t;
    }

    static RectTransform SettingRow(Transform parent, string name, string icon, string label, float y, float h, bool sliderRow)
    {
        RectTransform row = Card(parent, name, Palette.Panel);
        Top(row, 0f, y, 880f, h);
        float top = sliderRow ? h / 2f - 58f : 0f;         // slider rows: label on top, slider underneath
        Image badge = NewImage("Badge", row, "circle", Palette.BgDeep);
        Mid(badge.rectTransform, -355f, top, 90f, 90f);
        Image ic = NewImage("Icon", row, icon, Palette.Muted);
        Mid(ic.rectTransform, -355f, top, 50f, 50f);
        TMP_Text t = NewText("Label", row, label, 46, Palette.Text, Art.Bold);
        t.alignment = TextAlignmentOptions.Left;
        Mid(t.rectTransform, -90f, top, 440f, 64f);
        return row;
    }

    // a slider row: icon + label + percentage on top, the slider underneath
    static Slider MakeSlider(RectTransform row, float min, float max, float value, out TMP_Text valueText)
    {
        valueText = NewText("Value", row, "100%", 46, Palette.Accent, Art.Bold);
        valueText.alignment = TextAlignmentOptions.Right;
        float h = row.sizeDelta.y;
        Mid(valueText.rectTransform, 290f, h / 2f - 58f, 200f, 64f);

        RectTransform sl = NewRect("Slider", row);
        Mid(sl, 0f, -38f, 760f, 80f);
        Image track = NewImage("Background", sl, "pill_b64", Palette.BgDeep, true, true);
        track.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        track.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        track.rectTransform.offsetMin = new Vector2(0f, -14f);
        track.rectTransform.offsetMax = new Vector2(0f, 14f);
        RectTransform fillArea = NewRect("Fill Area", sl);
        fillArea.anchorMin = new Vector2(0f, 0.5f);
        fillArea.anchorMax = new Vector2(1f, 0.5f);
        fillArea.offsetMin = new Vector2(10f, -14f);
        fillArea.offsetMax = new Vector2(-10f, 14f);
        Image fill = NewImage("Fill", fillArea, "pill_b64", Palette.Accent, true);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = new Vector2(0f, 1f);
        fill.rectTransform.offsetMin = new Vector2(-10f, 0f);
        fill.rectTransform.offsetMax = new Vector2(10f, 0f);
        RectTransform handleArea = NewRect("Handle Slide Area", sl);
        Fill(handleArea, 36f, 36f);
        Image handle = NewImage("Handle", handleArea, "circle", Palette.Text, false, true);
        handle.rectTransform.anchorMin = new Vector2(0f, 0f);
        handle.rectTransform.anchorMax = new Vector2(0f, 1f);
        handle.rectTransform.sizeDelta = new Vector2(72f, 0f);
        Slider slider = sl.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        return slider;
    }

    static void VolumeRow(Transform p, string name, string icon, string label, float y, string parameter, AudioClip sample)
    {
        RectTransform row = SettingRow(p, name, icon, label, y, 200f, true);
        TMP_Text value;
        Slider s = MakeSlider(row, 0.0001f, 1f, VolumeSettings.DefaultFor(parameter), out value);   // 0.0001, never 0: log10(0) is minus infinity
        VolumeSlider vs = s.gameObject.AddComponent<VolumeSlider>();
        vs.parameter = parameter;
        vs.valueLabel = value;
        vs.sfxSample = sample;
    }

    static GameObject BuildSettings(Transform parent)
    {
        RectTransform p = Panel(parent, "Screen_09_Settings");
        Header(p, "SETTINGS", 1);

        RectTransform r1 = SettingRow(p, "VibrationRow", "icon_vibrate", "Vibration", 330f, 120f, false);
        Toggle vib = MakeToggle(r1, 330f, 0f);
        UnityEventTools.AddPersistentListener<bool>(vib.onValueChanged, settings.SetVibrate);
        RectTransform r2 = SettingRow(p, "TipsRow", "icon_info", "Show tips", 470f, 120f, false);
        Toggle tips = MakeToggle(r2, 330f, 0f);
        UnityEventTools.AddPersistentListener<bool>(tips.onValueChanged, settings.SetTips);
        RectTransform r3 = SettingRow(p, "MuteRow", "icon_sound_off", "Mute all", 610f, 120f, false);
        Toggle mute = MakeToggle(r3, 330f, 0f);
        mute.isOn = false;
        UnityEventTools.AddPersistentListener<bool>(mute.onValueChanged, volume.SetMuted);
        volume.muteToggle = mute;

        RectTransform r4 = SettingRow(p, "BrightnessRow", "icon_clock", "Brightness", 790f, 200f, true);
        TMP_Text value;
        Slider bright = MakeSlider(r4, 0.5f, 1f, 1f, out value);
        settings.brightnessLabel = value;
        UnityEventTools.AddPersistentListener<float>(bright.onValueChanged, settings.SetBrightness);

        VolumeRow(p, "MasterRow", "icon_sound", "Master", 1010f, VolumeSettings.Master, null);
        VolumeRow(p, "MusicRow", "icon_music", "Music", 1230f, VolumeSettings.Music, null);
        VolumeRow(p, "SfxRow", "icon_star", "Effects", 1450f, VolumeSettings.Sfx, Clip("SFX/sfx_coin.wav"));
        VolumeRow(p, "UiRow", "icon_bolt", "Interface", 1670f, VolumeSettings.Ui, null);
        return p.gameObject;
    }

    // ------------------------------------------------------------ 10 Credits
    static GameObject BuildCredits(Transform parent)
    {
        RectTransform p = Panel(parent, "Screen_10_Credits");
        Header(p, "CREDITS", 1);
        string gold = "<color=#FFE680>";
        string text =
            gold + "DESIGN AND CODE</color>\nMuhammad Sarmad Ahmad\nBSSE23013, Information Technology University\n\n" +
            gold + "COURSE</color>\nCS464 Game Development\nLectures 08, 09 and 10\n\n" +
            gold + "MUSIC AND SOUNDS</color>\nMade by the author with Python and NumPy\n(Tools/make_audio.py). No third-party audio.\n\n" +
            gold + "FONTS</color>\nLilita One and Poppins (SIL Open Font License)\n\n" +
            gold + "ART</color>\nIcons and shapes drawn by the author with Python (Pillow)";
        TMP_Text t = NewText("Text", p, text, 36, Palette.Text, Art.Body, true);
        t.richText = true;
        t.lineSpacing = 12f;
        Top(t.rectTransform, 0f, 900f, 900f, 1200f);
        return p.gameObject;
    }

    // ------------------------------------------------------------ 11 Loading
    static GameObject BuildLoading(Transform parent)
    {
        RectTransform p = Panel(parent, "Screen_11_Loading");
        TMP_Text title = NewText("Title", p, "LOADING", 110, Palette.Text, Art.Display);
        Top(title.rectTransform, 0f, 620f, 900f, 150f);
        Image ball = NewImage("Ball", p, "ball", Hex3(0x3CCFC4));
        Mid(ball.rectTransform, 0f, 180f, 260f, 260f);
        ball.gameObject.AddComponent<Pulse>();

        RectTransform bar = NewRect("Bar", p);
        Mid(bar, 0f, -150f, 800f, 44f);
        Image track = NewImage("Track", bar, "pill_b64", Palette.BgDeep, true); Fill(track.rectTransform);
        Image outline = NewImage("Outline", bar, "pill_outline_b64", Palette.Stroke, true); Fill(outline.rectTransform);
        Image fill = NewImage("Fill", bar, "bar_8x1", Palette.Accent);
        Fill(fill.rectTransform, 10f, 10f);
        fill.rectTransform.offsetMin = new Vector2(10f, 8f);
        fill.rectTransform.offsetMax = new Vector2(-10f, -8f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 0f;

        TMP_Text percent = NewText("Percent", p, "0%", 70, Palette.Accent, Art.Display);
        Mid(percent.rectTransform, 0f, -270f, 500f, 100f);
        TMP_Text tip = NewText("Tip", p, "", 38, Palette.Muted, Art.Body, true);
        Bottom(tip.rectTransform, 0f, 300f, 860f, 140f);
        TMP_Text lvl = NewText("Level", p, "LEVEL 1", 44, Palette.Muted, Art.Bold);
        Top(lvl.rectTransform, 0f, 780f, 700f, 60f);
        levelLabels.Add(lvl);

        loader.barFill = fill;
        loader.percentText = percent;
        loader.tipText = tip;
        loader.tips = new string[]
        {
            "Tip: collect every coin to clear the level",
            "Tip: the blue bolt gives you a speed boost",
            "Tip: a heart gives back one life",
            "Tip: after a hit you are safe for a second",
            "Tip: press Esc or the pause button to pause"
        };
        return p.gameObject;
    }
}
