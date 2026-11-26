// Is file ko Assets/Editor folder mein rakho (folder ka naam bilkul "Editor" ho).
using NED2024;
using Cinemachine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class NEDAutoBuild
{
    const string GameScenePath = "Assets/Scenes/GameScene.unity";
    const string MenuScenePath = "Assets/Scenes/MainMenu.unity";

    // ---------------------------------------------------------------- MENU 1
    [MenuItem("NED/1 - Build GameScene (Level, Player, Keys, Bowling, Enemy, UI)")]
    public static void BuildGame()
    {
        EnsureFolder("Assets/Scenes"); EnsureFolder("Assets/MATERIALS");
        EnsureFolder("Assets/PREFABS"); EnsureFolder("Assets/AUDIO");

        AddTagAndLayer();

        var scene = EditorSceneManager.GetActiveScene();
        foreach (var go in scene.GetRootGameObjects())
            if (go.name != "Main Camera" && go.name != "Directional Light") Object.DestroyImmediate(go);

        // ---- Materials
        Material floorMat = FindMat(new[] { "flooorMat", "FloorMat" }, new Color(0.6f, 0.6f, 0.6f), "FloorMat");
        Material wallMat  = FindMat(new[] { "wallMart", "WallMat" }, new Color(0.45f, 0.3f, 0.25f), "WallMat");
        Material yellow   = FindMat(new[] { "YellowMat" }, Color.yellow, "YellowMat");
        Material red      = FindMat(new[] { "RedMat" }, Color.red, "RedMat");
        PhysicMaterial ice = FindPhys("Ice", "Ice");
        ice.dynamicFriction = 0; ice.staticFriction = 0; ice.bounciness = 0;
        PhysicMaterial bounce = FindPhys("Bounce", "Bounce");
        bounce.bounciness = 0.9f; bounce.bounceCombine = PhysicMaterialCombine.Maximum;
        EditorUtility.SetDirty(ice); EditorUtility.SetDirty(bounce);

        // ---- Level
        var floor = Cube("Floor", new Vector3(0, 0, 0), new Vector3(30, 1, 30), floorMat);
        Cube("Wall_N", new Vector3(0, 2, 15), new Vector3(30, 4, 1), wallMat);
        Cube("Wall_S", new Vector3(0, 2, -15), new Vector3(30, 4, 1), wallMat);
        Cube("Wall_E", new Vector3(15, 2, 0), new Vector3(1, 4, 30), wallMat);
        Cube("Wall_W", new Vector3(-15, 2, 0), new Vector3(1, 4, 30), wallMat);
        var door = Cube("Door", new Vector3(0, 2, 14), new Vector3(4, 4, 1), wallMat);
        var trig = door.AddComponent<BoxCollider>();
        trig.isTrigger = true; trig.size = new Vector3(1.5f, 1, 6);
        door.AddComponent<ExitDoor>();

        var iceFloor = Cube("IceFloor", new Vector3(0, 0.55f, 7), new Vector3(4, 0.1f, 3), null);
        iceFloor.GetComponent<BoxCollider>().sharedMaterial = ice;

        // ---- Player
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player"; player.transform.position = new Vector3(0, 1, 0);
        player.tag = "Player";
        var prb = player.AddComponent<Rigidbody>();
        prb.constraints = RigidbodyConstraints.FreezeRotation;
        player.AddComponent<PlayerMove>();
        var hp = player.AddComponent<Health>(); hp.isPlayer = true;

        var hit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hit.name = "Hitbox"; hit.transform.SetParent(player.transform, false);
        hit.transform.localPosition = new Vector3(0, 0, 0.8f);
        hit.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        hit.GetComponent<BoxCollider>().isTrigger = true;
        hit.AddComponent<Hitbox>();

        int enemyLayer = LayerMask.NameToLayer("Enemy"); if (enemyLayer < 0) enemyLayer = 0;
        var pc = player.AddComponent<PlayerCombat>();
        pc.hitbox = hit.GetComponent<Collider>();
        pc.enemyMask = 1 << enemyLayer;

        // ---- Camera (Cinemachine 2.x)
        var cam = Camera.main;
        if (cam != null && cam.GetComponent<CinemachineBrain>() == null) cam.gameObject.AddComponent<CinemachineBrain>();
        try
        {
            var vgo = new GameObject("CM vcam1");
            var vcam = vgo.AddComponent<CinemachineVirtualCamera>();
            vcam.Follow = player.transform; vcam.LookAt = player.transform;
            var orb = vcam.AddCinemachineComponent<CinemachineOrbitalTransposer>();
            orb.m_FollowOffset = new Vector3(0, 4, -7);
            vcam.AddCinemachineComponent<CinemachineHardLookAt>();
        }
        catch (System.Exception e) { Debug.LogWarning("Cinemachine camera auto-setup fail: " + e.Message + " (haath se banao)"); }

        // ---- Keys
        Vector3[] keyPos = { new Vector3(5, 1, 5), new Vector3(-8, 1, 8), new Vector3(8, 1, -8) };
        for (int i = 0; i < 3; i++)
        {
            var k = Cube(i == 0 ? "Key" : "Key (" + i + ")", keyPos[i], Vector3.one * 0.5f, yellow);
            k.GetComponent<BoxCollider>().isTrigger = true;
            k.AddComponent<KeyPickup>();
        }

        // ---- Bowling
        Vector3[] pinPos = { new Vector3(0, 1, 10), new Vector3(-0.5f, 1, 11), new Vector3(0.5f, 1, 11),
                             new Vector3(-1, 1, 12), new Vector3(0, 1, 12), new Vector3(1, 1, 12) };
        for (int i = 0; i < pinPos.Length; i++)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            p.name = i == 0 ? "Pin" : "Pin (" + i + ")";
            p.transform.position = pinPos[i]; p.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
            p.AddComponent<Rigidbody>().mass = 0.5f;
        }

        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Ball"; ball.transform.localScale = Vector3.one * 0.5f;
        ball.AddComponent<Rigidbody>().mass = 2f;
        ball.GetComponent<SphereCollider>().sharedMaterial = bounce;
        var prefab = PrefabUtility.SaveAsPrefabAsset(ball, "Assets/PREFABS/Ball.prefab");
        Object.DestroyImmediate(ball);

        var launcher = new GameObject("Launcher");
        launcher.transform.position = new Vector3(0, 1, 2);
        var bl = launcher.AddComponent<BowlingLauncher>();
        bl.ballPrefab = prefab; bl.spawnPoint = launcher.transform;

        var pad = Cube("SpeedPad", new Vector3(0, 0.6f, 4), new Vector3(3, 0.3f, 2), null);
        pad.GetComponent<BoxCollider>().isTrigger = true;
        pad.AddComponent<SpeedPad>();

        // ---- Enemy
        var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy"; enemy.transform.position = new Vector3(-10, 1, -10);
        enemy.GetComponent<Renderer>().sharedMaterial = red;
        enemy.tag = "Enemy"; enemy.layer = enemyLayer;
        enemy.AddComponent<NavMeshAgent>();
        enemy.AddComponent<Health>();
        var ai = enemy.AddComponent<EnemyAI>();
        var p1 = new GameObject("P1"); p1.transform.position = new Vector3(-10, 0, -10);
        var p2 = new GameObject("P2"); p2.transform.position = new Vector3(10, 0, -10);
        var p3 = new GameObject("P3"); p3.transform.position = new Vector3(0, 0, 10);
        ai.patrolPoints = new[] { p1.transform, p2.transform, p3.transform };
        ai.player = player.transform;

        // ---- NavMesh Surface (AI Navigation package, reflection se)
        var navType = System.Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
        if (navType != null)
        {
            var surf = floor.AddComponent(navType);
            navType.GetMethod("BuildNavMesh").Invoke(surf, null);
        }
        else Debug.LogWarning("AI Navigation package install nahi hai: Window > Package Manager > Unity Registry > AI Navigation > Install. Phir menu 1 dobara chalao.");

        // ---- UI
        BuildGameUI();

        // ---- Music
        var music = new GameObject("Music");
        var src = music.AddComponent<AudioSource>();
        src.clip = FirstClip(0); src.loop = true; src.playOnAwake = true;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, GameScenePath);
        Selection.activeGameObject = player;
        Debug.Log("NED: GameScene ban gayi aur save ho gayi. Ab menu NED > 2 chalao.");
    }

    // ---------------------------------------------------------------- MENU 2
    [MenuItem("NED/2 - Build MainMenu scene + Build Settings")]
    public static void BuildMenu()
    {
        if (!System.IO.File.Exists(GameScenePath)) { Debug.LogError("Pehle menu NED > 1 chalao."); return; }
        EditorSceneManager.SaveOpenScenes();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var canvas = MakeCanvas();

        var btnGo = DefaultControls.CreateButton(Res());
        btnGo.name = "PlayButton"; btnGo.transform.SetParent(canvas.transform, false);
        btnGo.GetComponentInChildren<Text>().text = "Play";
        Place(btnGo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 80));
        var bs = btnGo.AddComponent<ButtonSound>(); bs.clickSfx = FirstClip(1);

        var panel = DefaultControls.CreatePanel(Res());
        panel.name = "LoadingPanel"; panel.transform.SetParent(canvas.transform, false);
        Stretch(panel);
        var bar = DefaultControls.CreateSlider(Res());
        bar.name = "LoadingBar"; bar.transform.SetParent(panel.transform, false);
        Place(bar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 30));
        var barSlider = bar.GetComponent<Slider>(); barSlider.value = 0;
        panel.SetActive(false);

        var loader = canvas.AddComponent<SceneLoader>();
        loader.loadingBar = barSlider; loader.loadingPanel = panel;
        UnityEventTools.AddStringPersistentListener(btnGo.GetComponent<Button>().onClick, loader.LoadGame, "GameScene");

        EditorSceneManager.SaveScene(scene, MenuScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };
        EditorSceneManager.OpenScene(GameScenePath);
        Debug.Log("NED: MainMenu ban gayi, Build Settings mein dono scenes add ho gayin.");
    }

    // ---------------------------------------------------------------- UI helpers
    static void BuildGameUI()
    {
        var canvas = MakeCanvas();

        var key = MakeText(canvas, "KeyText", "Keys: 0/3", 40);
        Place(key, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(400, 60));
        var timer = MakeText(canvas, "TimerText", "Time: 0.0", 40);
        timer.GetComponent<Text>().alignment = TextAnchor.UpperRight;
        Place(timer, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(400, 60));

        var hs = DefaultControls.CreateSlider(Res());
        hs.name = "HealthSlider"; hs.transform.SetParent(canvas.transform, false);
        Place(hs, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(400, 30));
        var hsc = hs.GetComponent<Slider>(); hsc.value = 1; hsc.interactable = false;

        var vs = DefaultControls.CreateSlider(Res());
        vs.name = "VolumeSlider"; vs.transform.SetParent(canvas.transform, false);
        Place(vs, new Vector2(0, 0), new Vector2(0, 0), new Vector2(20, 20), new Vector2(300, 30));
        vs.GetComponent<Slider>().value = 1;

        var mt = DefaultControls.CreateToggle(Res());
        mt.name = "MuteToggle"; mt.transform.SetParent(canvas.transform, false);
        Place(mt, new Vector2(0, 0), new Vector2(0, 0), new Vector2(20, 70), new Vector2(160, 30));
        var lbl = mt.GetComponentInChildren<Text>(); lbl.text = "Mute"; lbl.color = Color.white;

        var gop = DefaultControls.CreatePanel(Res());
        gop.name = "GameOverPanel"; gop.transform.SetParent(canvas.transform, false);
        Stretch(gop);
        var go = DefaultControls.CreateText(Res());
        go.name = "GameOverText"; go.transform.SetParent(gop.transform, false);
        Stretch(go);
        var got = go.GetComponent<Text>(); got.text = "GAME OVER"; got.fontSize = 90;
        got.color = Color.white; got.alignment = TextAnchor.MiddleCenter;
        gop.SetActive(false);

        // Logo + Aspect Ratio Fitter (holder ke andar, taake poori screen na bhare)
        var holder = new GameObject("LogoHolder", typeof(RectTransform));
        holder.transform.SetParent(canvas.transform, false);
        Place(holder, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, 20), new Vector2(160, 160));
        var logo = DefaultControls.CreateImage(Res());
        logo.name = "Logo"; logo.transform.SetParent(holder.transform, false);
        Stretch(logo);
        var arf = logo.AddComponent<AspectRatioFitter>();
        arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent; arf.aspectRatio = 1f;

        // Sample button with ButtonSound
        var b = DefaultControls.CreateButton(Res());
        b.name = "SampleButton"; b.transform.SetParent(canvas.transform, false);
        b.GetComponentInChildren<Text>().text = "Click";
        Place(b, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -90), new Vector2(160, 50));
        var bs = b.AddComponent<ButtonSound>(); bs.clickSfx = FirstClip(1);

        var ui = canvas.AddComponent<UIManager>();
        ui.keyText = key.GetComponent<Text>(); ui.timerText = timer.GetComponent<Text>();
        ui.healthSlider = hsc; ui.volumeSlider = vs.GetComponent<Slider>();
        ui.muteToggle = mt.GetComponent<Toggle>(); ui.gameOverPanel = gop;
    }

    static GameObject MakeCanvas()
    {
        var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = go.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.5f;
        if (Object.FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        return go;
    }

    static GameObject MakeText(GameObject parent, string name, string text, int size)
    {
        var go = DefaultControls.CreateText(Res());
        go.name = name; go.transform.SetParent(parent.transform, false);
        var t = go.GetComponent<Text>(); t.text = text; t.fontSize = size; t.color = Color.white;
        return go;
    }

    static void Place(GameObject g, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var r = g.GetComponent<RectTransform>();
        r.anchorMin = anchor; r.anchorMax = anchor; r.pivot = pivot;
        r.anchoredPosition = pos; r.sizeDelta = size;
    }

    static void Stretch(GameObject g)
    {
        var r = g.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }

    static DefaultControls.Resources Res()
    {
        var r = new DefaultControls.Resources();
        r.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        r.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        r.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
        r.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        r.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
        r.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
        r.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
        return r;
    }

    // ---------------------------------------------------------------- misc helpers
    static GameObject Cube(string name, Vector3 pos, Vector3 scale, Material m)
    {
        var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.name = name; c.transform.position = pos; c.transform.localScale = scale;
        if (m != null) c.GetComponent<Renderer>().sharedMaterial = m;
        return c;
    }

    static Material FindMat(string[] names, Color fallback, string createName)
    {
        foreach (var n in names)
            foreach (var g in AssetDatabase.FindAssets(n + " t:Material"))
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (string.Equals(System.IO.Path.GetFileNameWithoutExtension(p), n, System.StringComparison.OrdinalIgnoreCase))
                    return AssetDatabase.LoadAssetAtPath<Material>(p);
            }
        var m = new Material(Shader.Find("Standard")); m.color = fallback;
        AssetDatabase.CreateAsset(m, "Assets/MATERIALS/" + createName + ".mat");
        return m;
    }

    static PhysicMaterial FindPhys(string name, string createName)
    {
        foreach (var g in AssetDatabase.FindAssets(name + " t:PhysicMaterial"))
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            if (string.Equals(System.IO.Path.GetFileNameWithoutExtension(p), name, System.StringComparison.OrdinalIgnoreCase))
                return AssetDatabase.LoadAssetAtPath<PhysicMaterial>(p);
        }
        var m = new PhysicMaterial(createName);
        AssetDatabase.CreateAsset(m, "Assets/MATERIALS/" + createName + ".physicMaterial");
        return m;
    }

    static AudioClip FirstClip(int index)
    {
        var guids = AssetDatabase.FindAssets("t:AudioClip");
        if (guids.Length == 0) return null;
        if (index >= guids.Length) index = 0;
        return AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guids[index]));
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'), System.IO.Path.GetFileName(path));
    }

    static void AddTagAndLayer()
    {
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tm.FindProperty("tags");
        bool has = false;
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == "Enemy") has = true;
        if (!has) { tags.InsertArrayElementAtIndex(0); tags.GetArrayElementAtIndex(0).stringValue = "Enemy"; }

        var layers = tm.FindProperty("layers");
        bool hasLayer = false;
        for (int i = 8; i < layers.arraySize; i++)
            if (layers.GetArrayElementAtIndex(i).stringValue == "Enemy") hasLayer = true;
        if (!hasLayer)
        {
            var sp = layers.GetArrayElementAtIndex(6);
            if (string.IsNullOrEmpty(sp.stringValue)) sp.stringValue = "Enemy";
        }
        tm.ApplyModifiedProperties();
    }
}
