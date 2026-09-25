using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Rebuilds the illustrated, layered dungeon. Duplicate the scene before hand editing.</summary>
public static class BuildDungeonScene
{
    const string Root = "Assets/Dungeon";
    const string ScenePath = "Assets/Scenes/Scene1_Dungeon.unity";
    static Material card, lanternCard, floor, backing;
    static Sprite chain, lantern, candles, skulls, web, doorLeft, doorRight;
    static readonly string[] Names = { "01 · Катакомбы", "02 · Мрачные пещеры", "03 · Древний мегалитический туннель" };
    static readonly string[] AtlasNames = { "CatacombsAtlas", "CavesAtlas", "MegalithAtlas" };
    static Color C(string hex) { ColorUtility.TryParseHtmlString(hex, out Color c); return c; }

    [MenuItem("Flat Depth/Scene 1 Dungeon/Rebuild illustrated dungeon")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode before rebuilding the dungeon.");
        Directory.CreateDirectory(Root + "/Sprites");
        Directory.CreateDirectory(Root + "/Materials");
        Directory.CreateDirectory("Assets/Scenes");
        AssetDatabase.Refresh();
        card = MaterialAsset("Illustrated cards", "FlatDepth/DungeonCard", Color.white);
        card.SetFloat("_Ambient", 1.05f);
        lanternCard = MaterialAsset("Lantern glow", "FlatDepth/DungeonCard", Color.white);
        lanternCard.SetFloat("_Emission", 1);
        floor = MaterialAsset("Ancient stone floor", "FlatDepth/DungeonGround", C("#D3CCBE"));
        Texture2D floorDrawing = LoadAtlas("DungeonFloor");
        var floorImporter = (TextureImporter)AssetImporter.GetAtPath(Root + "/Art/DungeonFloor.png");
        floorImporter.wrapMode = TextureWrapMode.Repeat;
        floorImporter.anisoLevel = 4;
        floorImporter.SaveAndReimport();
        floor.mainTexture = floorDrawing;
        backing = MaterialAsset("Recessed darkness", "FlatDepth/Card", C("#0D1116"));
        LoadProps();

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.skybox = null;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = C("#101319");
        RenderSettings.fogStartDistance = 8;
        RenderSettings.fogEndDistance = 35;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = C("#656973");
        var level = new GameObject("Сцена 1 — Подземелье");
        var shell = Group("Dark shell · real depth behind illustrated layers", level.transform);
        Box("Stone floor", shell, new Vector3(0, -.16f, 55), new Vector3(12, .3f, 118), floor);
        Box("Left recessed wall", shell, new Vector3(-5.65f, 3.7f, 55), new Vector3(.3f, 7.6f, 118), backing);
        Box("Right recessed wall", shell, new Vector3(5.65f, 3.7f, 55), new Vector3(.3f, 7.6f, 118), backing);
        Box("Recessed vault", shell, new Vector3(0, 7.4f, 55), new Vector3(11.6f, .3f, 118), backing);
        Box("Terminal darkness", shell, new Vector3(0, 3.7f, 113), new Vector3(12, 7.6f, .3f), backing);

        var random = new System.Random(1709);
        float R(float lo, float hi) => Mathf.Lerp(lo, hi, (float)random.NextDouble());
        for (int location = 0; location < 3; location++)
        {
            float start = location * 36;
            var section = Group(Names[location], level.transform);
            var architecture = Group("Nested portal layers", section);
            var walls = Group("Side layers · large and small", section);
            var ground = Group("Ground layers · rubble and relics", section);
            var ceiling = Group("Overhead layers · chains and lanterns", section);
            Texture2D atlas = LoadAtlas(AtlasNames[location]);
            Rect[] crops = location == 0
                ? new[] { new Rect(0, 0, 754, 761), new Rect(782, 0, 472, 761), new Rect(0, 877, 750, 361), new Rect(765, 799, 489, 439) }
                : location == 1
                    ? new[] { new Rect(0, 0, 754, 757), new Rect(834, 0, 420, 757), new Rect(0, 850, 695, 388), new Rect(700, 785, 554, 453) }
                    : new[] { new Rect(0, 0, 820, 800), new Rect(835, 0, 419, 800), new Rect(0, 858, 600, 396), new Rect(705, 818, 549, 424) };
            Sprite arch = CropRect(AtlasNames[location] + "_Portal", atlas, crops[0], new Vector2(.5f, 0));
            Sprite buttress = CropRect(AtlasNames[location] + "_Wall", atlas, crops[1], new Vector2(.5f, 0));
            Sprite rubble = CropRect(AtlasNames[location] + "_Rubble", atlas, crops[2], new Vector2(.5f, 0));
            Sprite fringe = CropRect(AtlasNames[location] + "_Ceiling", atlas, crops[3], new Vector2(.5f, 1));
            for (int layer = 0; layer < 8; layer++)
            {
                float z = start + 2 + layer * 4.8f;
                Color tint = Color.Lerp(Color.white, C("#C7C9CF"), (layer % 3) * .13f);
                Card("Portal " + (layer + 1).ToString("00"), arch, architecture, new Vector3(R(-.18f, .18f), -.12f, z + R(-.3f, .3f)), R(10.8f, 11.6f), R(6.9f, 7.4f), tint, layer % 2 == 1);
                for (int side = -1; side <= 1; side += 2)
                {
                    Card("Near wall buttress", buttress, walls, new Vector3(side * R(3.8f, 4.35f), -.1f, z - .65f), R(2.7f, 3.5f), R(5.2f, 6.8f), Color.white, side < 0);
                    Card("Inset wall relief", buttress, walls, new Vector3(side * R(3.3f, 3.8f), -.1f, z + 1.8f), R(1.65f, 2.25f), R(2.8f, 3.7f), C("#C5C8C8"), side > 0);
                    Card("Rubble bank", rubble, ground, new Vector3(side * R(2.75f, 3.2f), -.08f, z - 1.15f), R(2.3f, 3.15f), R(1.1f, 1.7f), Color.white, side < 0);
                    Card("Small rubble", rubble, ground, new Vector3(side * R(1.9f, 2.35f), -.055f, z + 1.5f), R(.7f, 1.15f), R(.35f, .62f), C("#D2D0C9"), side > 0);
                    Card("Overhanging edge", fringe, ceiling, new Vector3(side * R(2.9f, 3.8f), R(5.1f, 5.7f), z + .45f), R(3.6f, 4.6f), R(1.7f, 2.4f), Color.white, side < 0);
                    if ((layer + location + (side > 0 ? 1 : 0)) % 3 == 0)
                        Card("Skulls among stones", skulls, ground, new Vector3(side * R(1.9f, 2.7f), .015f, z + .15f), R(.65f, 1f), R(.38f, .58f), Color.white, side < 0);
                    if (layer % 3 == 1)
                        Card("Wax candle cluster", candles, ground, new Vector3(side * 1.75f, .015f, z + 2), .65f, .72f, Color.white, side > 0);
                    if (layer % 3 == 0)
                        Card("Spider silk in recess", web, walls, new Vector3(side * 2.9f, 3.25f, z - .4f), 1.8f, 1.5f, C("#B8BDC5"), side > 0);
                    // Rotation is around the top attachment point, not the middle of the drawing.
                    if (layer % 2 == 0)
                        Hanging("Loose iron chain", chain, ceiling, new Vector3(side * R(1.9f, 2.65f), R(4.8f, 5.6f), z + 1), .29f, R(1.5f, 2.4f), R(0, 6.28f), false, location);
                }
            }
            for (int lightIndex = 0; lightIndex < 4; lightIndex++)
            {
                int globalIndex = location * 4 + lightIndex;
                float z = 6 + globalIndex * 9;
                Hanging("Suspended lantern", lantern, ceiling, new Vector3(globalIndex % 2 == 0 ? -2.65f : 2.65f, 5.1f, z), .82f, 2.65f, R(0, 6.28f), true, location);
            }
        }
        CreateDoor("Door 1 · Catacombs to caves", level.transform, 36);
        CreateDoor("Door 2 · Caves to megalithic vault", level.transform, 72);
        var end = Group("Sealed exit at the end of the vault", level.transform);
        Card("Exit left leaf", doorLeft, end, new Vector3(-2.7f, 0, 111), 2.7f, 5.65f, C("#B8BDC8"), false);
        Card("Exit right leaf", doorRight, end, new Vector3(2.7f, 0, 111), 2.7f, 5.65f, C("#B8BDC8"), false);

        var player = new GameObject("PLAYER · Сцена 1 · W / Space / R");
        player.tag = "MainCamera";
        var cam = player.AddComponent<Camera>();
        cam.fieldOfView = 64; cam.nearClipPlane = .055f; cam.farClipPlane = 52;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = RenderSettings.fogColor;
        cam.allowHDR = false; cam.allowMSAA = true;
        player.AddComponent<AudioListener>();
        var walker = player.AddComponent<DungeonWalker>();
        walker.startZ = 0; walker.endZ = 108; walker.eyeHeight = 1.7f; walker.speed = 3;
        walker.RestartWalk();
        PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        var builds = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
        foreach (var entry in EditorBuildSettings.scenes)
            if (entry.path != ScenePath) builds.Add(entry);
        if (File.Exists("Assets/Scenes/CountryRoad256.unity") && !builds.Exists(s => s.path == "Assets/Scenes/CountryRoad256.unity"))
            builds.Add(new EditorBuildSettingsScene("Assets/Scenes/CountryRoad256.unity", true));
        EditorBuildSettings.scenes = builds.ToArray();
        AssetDatabase.SaveAssets();
        Validate();
        Selection.activeGameObject = level;
        Debug.Log("DUNGEON_SCENE_1_BUILD_OK · 3 locations · 108 metres · illustrated sprite layers");
    }

    static Texture2D LoadAtlas(string name)
    {
        string path = Root + "/Art/" + name + ".png";
        if (!File.Exists(path)) throw new FileNotFoundException("Dungeon illustration atlas missing", path);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Default;
        importer.alphaIsTransparency = true; importer.mipmapEnabled = true;
        importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048; importer.npotScale = TextureImporterNPOTScale.None;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static Sprite Cell(string name, Texture2D texture, int columns, int rows, int column, int rowFromTop, Vector2 pivot)
    {
        int width = texture.width / columns, height = texture.height / rows;
        return SpriteAsset(name, texture, new Rect(column * width, texture.height - (rowFromTop + 1) * height, width, height), pivot);
    }

    static Sprite SpriteAsset(string name, Texture2D texture, Rect rect, Vector2 pivot)
    {
        var sprite = Sprite.Create(texture, rect, pivot, 100, 0, SpriteMeshType.FullRect);
        sprite.name = name;
        string path = Root + "/Sprites/" + name + ".asset";
        var previous = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (previous != null)
        {
            EditorUtility.CopySerialized(sprite, previous);
            UnityEngine.Object.DestroyImmediate(sprite);
            EditorUtility.SetDirty(previous);
            return previous;
        }
        AssetDatabase.CreateAsset(sprite, path);
        return sprite;
    }

    // The generated atlas sheets are deliberately cropped by artwork bounds, not an assumed grid.
    // Coordinates are measured from the top-left of the 1254-pixel source sheets.
    static Sprite Crop(string name, Texture2D texture, float x, float top, float width, float height, Vector2 pivot)
    {
        float sx = texture.width / 1254f, sy = texture.height / 1254f;
        return SpriteAsset(name, texture, new Rect(x * sx, texture.height - (top + height) * sy, width * sx, height * sy), pivot);
    }

    static Sprite CropRect(string name, Texture2D texture, Rect bounds, Vector2 pivot)
    {
        return Crop(name, texture, bounds.x, bounds.y, bounds.width, bounds.height, pivot);
    }

    static void LoadProps()
    {
        Texture2D texture = LoadAtlas("DungeonProps");
        doorLeft = Crop("Door_Left", texture, 15, 30, 228, 657, new Vector2(0, 0));
        doorRight = Crop("Door_Right", texture, 243, 30, 227, 657, new Vector2(1, 0));
        chain = Crop("Chain", texture, 596, 0, 121, 745, new Vector2(.5f, 1));
        lantern = Crop("Lantern", texture, 956, 0, 230, 738, new Vector2(.5f, 1));
        candles = Crop("Candles", texture, 10, 785, 385, 448, new Vector2(.5f, 0));
        skulls = Crop("Skulls", texture, 398, 842, 502, 385, new Vector2(.5f, 0));
        web = Crop("Cobweb", texture, 912, 744, 342, 500, new Vector2(.5f, 0));
    }

    static Transform Group(string name, Transform parent)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform;
    }

    static SpriteRenderer Card(string name, Sprite sprite, Transform parent, Vector3 position, float width, float height, Color tint, bool mirror)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position;
        go.transform.localScale = new Vector3(width / sprite.bounds.size.x, height / sprite.bounds.size.y, 1);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite; renderer.sharedMaterial = card; renderer.color = tint; renderer.flipX = mirror;
        renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
        return renderer;
    }

    static void Hanging(string name, Sprite sprite, Transform parent, Vector3 position, float width, float height, float phase, bool lit, int location)
    {
        var hinge = Group(name + " · top pivot", parent); hinge.localPosition = position;
        var drawing = Card(name + " sprite", sprite, hinge, Vector3.zero, width, height, Color.white, false);
        if (lit) drawing.sharedMaterial = lanternCard;
        var sway = hinge.gameObject.AddComponent<DungeonSway>();
        sway.swingDegrees = lit ? 2.1f : 1.3f; sway.swingSpeed = lit ? .34f : .26f; sway.phase = phase;
        if (!lit) return;
        var glow = Group("Lantern warm pool", hinge); glow.localPosition = new Vector3(0, -height * .79f, -.28f);
        var light = glow.gameObject.AddComponent<Light>();
        light.type = LightType.Point; light.range = 7; light.intensity = 1.2f;
        light.color = location == 1 ? C("#95BBC8") : C("#FFC37A");
        light.shadows = LightShadows.None;
        sway.lanternLight = light; sway.flickerAmount = .11f; sway.flickerSpeed = 4.8f;
    }

    static void CreateDoor(string name, Transform parent, float z)
    {
        var assembly = Group(name, parent); assembly.position = new Vector3(0, 0, z);
        var left = Group("Left hinge", assembly); left.localPosition = new Vector3(-2.7f, 0, 0);
        var right = Group("Right hinge", assembly); right.localPosition = new Vector3(2.7f, 0, 0);
        Card("Painted left leaf · light through slits", doorLeft, left, Vector3.zero, 2.7f, 5.65f, Color.white, false);
        Card("Painted right leaf · light through slits", doorRight, right, Vector3.zero, 2.7f, 5.65f, Color.white, false);
        var door = assembly.gameObject.AddComponent<DungeonDoor>();
        door.leftLeaf = left; door.rightLeaf = right; door.openDistance = 5; door.openAngle = 82;
    }

    static Material MaterialAsset(string name, string shaderName, Color color)
    {
        Shader shader = Shader.Find(shaderName);
        if (shader == null) throw new InvalidOperationException("Required dungeon shader missing: " + shaderName);
        string path = Root + "/Materials/" + name.Replace(' ', '_') + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
        material.shader = shader; material.color = color; EditorUtility.SetDirty(material); return material;
    }

    static void Box(string name, Transform parent, Vector3 position, Vector3 size, Material material)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name;
        go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = size;
        UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = material;
    }

    [MenuItem("Flat Depth/Scene 1 Dungeon/Validate")]
    public static void Validate()
    {
        var walker = UnityEngine.Object.FindFirstObjectByType<DungeonWalker>();
        if (walker == null || Camera.main == null) throw new Exception("Dungeon player or camera missing");
        var root = GameObject.Find("Сцена 1 — Подземелье");
        if (root == null) throw new Exception("Named dungeon scene root missing");
        foreach (string name in Names) if (root.transform.Find(name) == null) throw new Exception("Missing location " + name);
        var renderers = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        if (renderers.Length < 300) throw new Exception("Dungeon has insufficient sprite layers");
        foreach (var renderer in renderers)
            if (renderer.sprite == null || renderer.sharedMaterial == null || renderer.sharedMaterial.shader == null)
                throw new Exception("Missing dungeon artwork or material: " + renderer.name);
        var doors = UnityEngine.Object.FindObjectsByType<DungeonDoor>(FindObjectsSortMode.None);
        if (doors.Length != 2) throw new Exception("Expected two location transition doors");
        foreach (var door in doors)
            if (door.leftLeaf == null || door.rightLeaf == null) throw new Exception("Unassigned door leaves");
        float saved = walker.transform.position.z;
        walker.RestartWalk(); walker.Advance(6);
        if (Mathf.Abs(walker.transform.position.z - 6) > .001f) throw new Exception("Forward movement failed");
        walker.Advance(-4);
        if (Mathf.Abs(walker.transform.position.z - 6) > .001f) throw new Exception("Negative movement should be rejected");
        walker.Advance(1000);
        if (Mathf.Abs(walker.transform.position.z - 108) > .001f) throw new Exception("End of route clamp failed");
        walker.SetDistance(saved);
        foreach (var door in doors) door.ResetDoor();
        Debug.Log("DUNGEON_VALIDATION_OK sprites=" + renderers.Length + " locations=3 doors=2 route=108m");
    }

    [MenuItem("Flat Depth/Scene 1 Dungeon/Capture previews")]
    public static void Capture()
    {
        Camera cam = Camera.main;
        if (cam == null) throw new Exception("Open the dungeon scene before capturing");
        var walker = cam.GetComponent<DungeonWalker>();
        if (walker == null) throw new Exception("Dungeon camera missing");
        float saved = walker.transform.position.z, aspect = cam.aspect;
        RenderTexture oldTarget = cam.targetTexture, oldActive = RenderTexture.active;
        string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Previews"));
        Directory.CreateDirectory(folder);
        var target = new RenderTexture(1280, 720, 24) { antiAliasing = 1 };
        var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try
        {
            cam.targetTexture = target; cam.aspect = 1280f / 720f;
            foreach (float z in new[] { 0f, 12f, 31f, 39f, 51f, 67f, 75f, 88f, 104f })
            {
                walker.SetDistance(z);
                cam.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(folder, $"Dungeon-{z:000}.png"), image.EncodeToPNG());
            }
        }
        finally
        {
            walker.SetDistance(saved); cam.targetTexture = oldTarget; cam.aspect = aspect;
            RenderTexture.active = oldActive;
            UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(image);
        }
        Debug.Log("DUNGEON_PREVIEWS_OK " + folder);
    }

    [MenuItem("Flat Depth/Scene 1 Dungeon/Open")]
    public static void Open()
    {
        EditorSceneManager.OpenScene(ScenePath);
        var game = EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));
        game.maximized = true; game.Focus();
    }
}
