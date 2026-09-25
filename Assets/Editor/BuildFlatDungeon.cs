using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class BuildFlatDungeon
{
    const string Root = "Assets/FlatDungeon";
    static Sprite tile;
    static Material card;
    static Color C(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }

    [MenuItem("Flat Depth/Rebuild demo scene")]
    public static void Build()
    {
        Directory.CreateDirectory(Root + "/Textures");
        Directory.CreateDirectory(Root + "/Materials");
        Directory.CreateDirectory(Root + "/Prefabs");
        Directory.CreateDirectory("Assets/Scenes");
        var tex = new Texture2D(64,64,TextureFormat.RGBA32,false);
        for(int y=0;y<64;y++) for(int x=0;x<64;x++)
        {
            int d=Math.Min(Math.Min(x,y), Math.Min(63-x,63-y));
            float v=d<2?0:d<4?.15f:d<6?.82f:.56f+(((x/8+y/8)%2)*.075f);
            if(Math.Abs(x-32)+Math.Abs(y-32)<9) v=.86f;
            tex.SetPixel(x,y,new Color(v,v,v,d<2?0:1));
        }
        tex.Apply(); File.WriteAllBytes(Root+"/Textures/Square.png",tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(Root+"/Textures/Square.png");
        var ti=(TextureImporter)AssetImporter.GetAtPath(Root+"/Textures/Square.png");
        ti.textureType=TextureImporterType.Sprite; ti.spriteImportMode=SpriteImportMode.Single; ti.spritePixelsPerUnit=64; ti.mipmapEnabled=true;
        ti.alphaIsTransparency=true; ti.textureCompression=TextureImporterCompression.Uncompressed;
        ti.filterMode=FilterMode.Bilinear; ti.SaveAndReimport();
        tile=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Textures/Square.png");
        if(tile==null) throw new Exception("Square sprite import failed");
        card=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Card.mat");
        if(card==null) { card=new Material(Shader.Find("FlatDepth/Card")); AssetDatabase.CreateAsset(card,Root+"/Materials/Card.mat"); }
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        RenderSettings.fog=true; RenderSettings.fogColor=C("#101724"); RenderSettings.fogMode=FogMode.Linear;
        RenderSettings.fogStartDistance=5; RenderSettings.fogEndDistance=37;
        RenderSettings.ambientLight=C("#939EC0");
        var environment=new GameObject("LEVEL · square sprite decorations");
        Box("Floor",environment.transform,new Vector3(0,-.15f,27),new Vector3(12,.3f,62),C("#182835"));
        Box("Left backing",environment.transform,new Vector3(-6,3,27),new Vector3(.25f,6,62),C("#111E2C"));
        Box("Right backing",environment.transform,new Vector3(6,3,27),new Vector3(.25f,6,62),C("#111E2C"));
        Color[] palettes={C("#6FADAB"),C("#9E82C8"),C("#D7AA6C")};
        for(int section=0;section<9;section++)
        {
            var group=new GameObject($"Section {section+1:00} · {(section<3?"JADE":section<6?"VIOLET":"AMBER")}");
            group.transform.SetParent(environment.transform);
            group.transform.position=new Vector3(0,0,4+section*5.8f);
            var tint=palettes[Math.Min(section/3,2)];
            for(int side=-1;side<=1;side+=2)
            {
                Card("Wall square A",group.transform,new Vector3(side*4.35f,1.4f,0),2.8f,tint*.85f);
                Card("Wall square B",group.transform,new Vector3(side*4.45f,4.1f,.18f),2.8f,tint*.7f);
                Card("Foreground square",group.transform,new Vector3(side*(2.65f+(section%2)*.3f),.65f,-.9f),1.25f,tint);
                Card("Middle square",group.transform,new Vector3(side*3.2f,2.4f,.8f),1.15f,tint*.8f);
                Card("Hanging square",group.transform,new Vector3(side*2.4f,4.5f,-.4f),1.7f,tint*.9f);
                Card("Small marker",group.transform,new Vector3(side*2.3f,.24f,1.4f),.38f,C("#E0DC9D"));
            }
            Card("Ceiling square",group.transform,new Vector3(0,5.3f,.3f),2.1f,tint*.7f);
            Box("Floor seam",group.transform,new Vector3(0,.009f,0),new Vector3(12,.018f,.06f),tint*.23f);
            if(section==0)
            {
                var prefab=PrefabUtility.SaveAsPrefabAssetAndConnect(group,Root+"/Prefabs/EditableSection.prefab",InteractionMode.AutomatedAction);
            }
        }
        var end=new GameObject("Exit frame"); end.transform.SetParent(environment.transform); end.transform.position=new Vector3(0,0,53);
        Card("Exit square",end.transform,new Vector3(0,2.2f,0),3.5f,C("#E6C889"));
        Card("Exit inset",end.transform,new Vector3(0,2.2f,-.025f),2.8f,C("#20423E"));
        var player=new GameObject("PLAYER · Camera · W to walk"); player.tag="MainCamera";
        player.transform.position=new Vector3(0,1.65f,0);
        var cam=player.AddComponent<Camera>(); cam.fieldOfView=64; cam.nearClipPlane=.08f; cam.farClipPlane=80;
        cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=C("#101724");
        player.AddComponent<AudioListener>(); player.AddComponent<ForwardWalker>();
        PlayerSettings.companyName="Flat Depth Prototype"; PlayerSettings.productName="Flat Depth";
        PlayerSettings.defaultScreenWidth=1280; PlayerSettings.defaultScreenHeight=720;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),"Assets/Scenes/FlatDungeon.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/FlatDungeon.unity",true)};
        AssetDatabase.SaveAssets();
        Selection.activeGameObject=player;
        Validate();
        Capture();
        Debug.Log("FLAT_DUNGEON_BUILD_OK");
    }

    static void Card(string name,Transform parent,Vector3 pos,float size,Color tint)
    {
        var go=new GameObject(name); go.transform.SetParent(parent,false); go.transform.localPosition=pos;
        go.transform.localScale=Vector3.one*size;
        var sr=go.AddComponent<SpriteRenderer>(); sr.sprite=tile; sr.sharedMaterial=card;
        tint.a=1; sr.color=tint; sr.shadowCastingMode=ShadowCastingMode.Off; sr.receiveShadows=false;
    }

    static void Box(string name,Transform parent,Vector3 pos,Vector3 size,Color tint)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.SetParent(parent,false);
        go.transform.localPosition=pos; go.transform.localScale=size;
        UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        string path=Root+"/Materials/"+name.Replace(" ","_")+"_"+ColorUtility.ToHtmlStringRGB(tint)+".mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null) { mat=new Material(Shader.Find("FlatDepth/Card")); tint.a=1; mat.color=tint; AssetDatabase.CreateAsset(mat,path); }
        go.GetComponent<Renderer>().sharedMaterial=mat;
    }

    public static void Validate()
    {
        var w=UnityEngine.Object.FindFirstObjectByType<ForwardWalker>();
        if(w==null||Camera.main==null) throw new Exception("Missing player/camera");
        if(UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Length!=119) throw new Exception("Unexpected sprite count");
        foreach(var sr in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            if(sr.sprite==null||sr.sharedMaterial==null) throw new Exception("Missing sprite or material: "+sr.name);
        w.RestartWalk(); w.Advance(6); if(Mathf.Abs(w.transform.position.z-6)>.001f) throw new Exception("Forward movement failed");
        w.Advance(-4); if(w.transform.position.z!=6) throw new Exception("Backtracking must be disabled");
        w.Advance(1000); if(w.transform.position.z!=48) throw new Exception("End clamp failed");
        w.RestartWalk(); if(w.Progress!=0||w.transform.rotation!=Quaternion.identity) throw new Exception("Reset failed");
        Debug.Log("FLAT_DUNGEON_VALIDATION_OK: 119 sprites, forward movement, no reverse, end stop, restart");
    }

    [MenuItem("Flat Depth/Capture previews")]
    public static void Capture()
    {
        var cam=Camera.main; var saved=cam.transform.position;
        var folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Previews")); Directory.CreateDirectory(folder);
        foreach(float z in new[]{0f,16f,34f})
        {
            cam.transform.position=new Vector3(0,1.65f,z);
            var rt=new RenderTexture(1280,720,24); cam.targetTexture=rt; cam.Render(); RenderTexture.active=rt;
            var shot=new Texture2D(1280,720,TextureFormat.RGB24,false); shot.ReadPixels(new Rect(0,0,1280,720),0,0); shot.Apply();
            File.WriteAllBytes(Path.Combine(folder,$"Scene-{z:00}.png"),shot.EncodeToPNG());
            cam.targetTexture=null; RenderTexture.active=null; UnityEngine.Object.DestroyImmediate(shot); UnityEngine.Object.DestroyImmediate(rt);
        }
        cam.transform.position=saved; Debug.Log("FLAT_DUNGEON_PREVIEWS_OK");
    }

    public static void OpenPreview()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/FlatDungeon.unity");
        Capture();
        if(SceneView.lastActiveSceneView!=null)
            SceneView.lastActiveSceneView.LookAt(new Vector3(0,2,15),Quaternion.Euler(25,-35,0),30);
        Selection.activeGameObject=Camera.main.gameObject;
        EditorApplication.ExecuteMenuItem("Window/General/Game");
    }
}
