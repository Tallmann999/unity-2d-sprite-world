using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class BuildFourRealms
{
    const string Root = "Assets/FourRealms";
    public const string ScenePath = "Assets/Scenes/Scene2_FourRealms.unity";
    static readonly string[] Themes = { "Aurora", "BlueCastle", "GoldenValley", "ForestPass" };
    static readonly string[] Names = { "01 · Северное сияние", "02 · Замок в ущелье", "03 · Золотая долина", "04 · Лесная крепость" };
    static Material[] cards = new Material[4];
    static Material vistaMaterial, propMaterial, doorMaterial;
    static Sprite sword, horse, gate, leftDoor, rightDoor;
    static Color C(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }

    [MenuItem("Flat Depth/Scene 2 Four Realms/Rebuild")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop Play Mode before rebuilding.");
        Directory.CreateDirectory(Root+"/Meshes"); Directory.CreateDirectory(Root+"/Materials"); Directory.CreateDirectory(Root+"/Sprites");
        AssetDatabase.Refresh();
        string[] haze = { "#87ADC2", "#99ADBA", "#C8B786", "#96B5A0" };
        for (int i=0;i<4;i++)
        {
            cards[i]=Mat(Themes[i]+" layers","FlatDepth/FourRealmsCard",Color.white);
            cards[i].SetColor("_Haze",C(haze[i])); cards[i].SetFloat("_HazeStrength",.19f);
        }
        vistaMaterial=Mat("Distant paintings","FlatDepth/FourRealmsCard",Color.white); vistaMaterial.SetFloat("_HazeStrength",0);
        propMaterial=Mat("Sword horse and doors","FlatDepth/FourRealmsCard",Color.white); propMaterial.SetFloat("_HazeStrength",0);
        LoadProps();
        var terrain=Load("JourneyTerrain",false);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        RenderSettings.fog=false; RenderSettings.skybox=null; RenderSettings.ambientLight=Color.white;
        var root=new GameObject("Сцена 2 — Четыре мира · 4 × 10 секунд");
        var sections=new GameObject[4];
        var random=new System.Random(4962);
        float R(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
        for(int room=0;room<4;room++)
        {
            var section=new GameObject(Names[room]); section.transform.SetParent(root.transform);
            section.transform.position=Vector3.forward*room*20; sections[room]=section;
            var distant=Group("Distant castle and mountains",section.transform);
            var painting=Load(Themes[room]+"Vista",false);
            var panorama=SpriteAsset(Themes[room]+"Vista",painting,new Rect(0,0,painting.width,painting.height),new Vector2(.5f,.5f));
            // Far enough that the castle moves much less than the close cards.
            Card("Far painted vista",panorama,distant,new Vector3(0,room==2?3:8,150),135,168.75f,vistaMaterial,Color.white);
            var floor=Mat(Themes[room]+" ground","FlatDepth/FourRealmsGround",Color.white);
            floor.mainTexture=terrain; floor.SetVector("_Tile",new Vector4(room%2,1-room/2,0,0));
            Box("Painted path",section.transform,new Vector3(0,-.105f,38),new Vector3(45,.2f,90),floor);
            var texture=Load(Themes[room]+"Layers",true);
            var tree=Quadrant(Themes[room]+"_Tree",texture,0,0);
            var rocks=Quadrant(Themes[room]+"_Rocks",texture,1,0);
            var grass=Quadrant(Themes[room]+"_Undergrowth",texture,0,1);
            var cliff=Quadrant(Themes[room]+"_Cliff",texture,1,1);
            var big=Group("Large silhouettes · staggered depth",section.transform);
            var medium=Group("Rock banks and shrubs",section.transform);
            var small=Group("Dense low plants · near parallax",section.transform);
            for(int layer=0;layer<22;layer++) for(int side=-1;side<=1;side+=2)
            {
                float z=-1+layer*1.55f+R(-.3f,.3f);
                // An open sightline keeps the distant castle readable between nearby sprites.
                Card("Rock bank",rocks,medium,new Vector3(side*R(3.5f,4.3f),-.09f,z),R(2.2f,3.4f),R(.9f,1.9f),cards[room],Color.white,side<0);
                Card("Near low plants",grass,small,new Vector3(side*R(1.6f,2.2f),-.035f,z+.35f),R(.85f,1.5f),R(.32f,.65f),cards[room],Color.white,random.Next(2)==0);
                Card("Dense bank plants",grass,small,new Vector3(side*R(2.8f,3.9f),-.04f,z+.85f),R(1.8f,2.6f),R(.55f,1.0f),cards[room],Color.white,side>0);
                if(layer%3==0)
                {
                    float x=side*R(5.8f,7.2f);
                    Card("Tall rock silhouette",cliff,big,new Vector3(x,-.15f,z+1.2f),R(5.2f,6.7f),R(5.4f,7.4f),cards[room],C("#DDE1DD"),side<0);
                }
                if((room==2 && layer%7==3) || (room!=2 && (layer%4==1 || layer==0)))
                {
                    float x=side*(room==2?R(6.8f,8):R(4.4f,5.2f));
                    var t=Card("Tree silhouette",tree,big,new Vector3(x,-.08f,z+.2f),room==2?R(3.7f,4.8f):R(4.3f,5.8f),room==2?R(5,6.5f):R(6.5f,8.7f),cards[room],Color.white,side>0);
                    // Fixed cards, no billboarding: the camera never changes yaw.
                    t.transform.localRotation=Quaternion.Euler(0,0,R(-2,2));
                }
            }
            if(room<3) MakeDoor(section.transform,20);
        }
        var player=new GameObject("PLAYER · fixed gaze · 2 metres per second"); player.tag="MainCamera";
        var cam=player.AddComponent<Camera>();cam.fieldOfView=55;cam.nearClipPlane=.035f;cam.farClipPlane=240;
        cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.black;cam.allowHDR=false;
        player.AddComponent<AudioListener>();
        var walker=player.AddComponent<FourRealmsWalker>();walker.locations=sections;
        var weapon=Card("Sword · first person",sword,player.transform,new Vector3(.245f,-.50f,.92f),.45f,.92f,propMaterial,Color.white);
        var mount=Card("Horse · rider view",horse,player.transform,new Vector3(0,-.52f,.93f),.70f,.50f,propMaterial,Color.white);
        walker.sword=weapon.gameObject;walker.horse=mount.gameObject;
        player.AddComponent<PortraitJourneyViewport>();walker.RestartWalk();
        PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);
        var scenes=new List<EditorBuildSettingsScene>{new EditorBuildSettingsScene(ScenePath,true)};
        foreach(var s in EditorBuildSettings.scenes) if(s.path!=ScenePath)scenes.Add(s);
        EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
        Validate();Capture();Selection.activeGameObject=player;
        Debug.Log("FOUR_REALMS_BUILD_OK");
    }

    static Texture2D Load(string name,bool readable)
    {
        string path=Root+"/Art/"+name+".png";
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var ti=(TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType=TextureImporterType.Default;ti.alphaIsTransparency=true;ti.isReadable=readable;
        ti.mipmapEnabled=true;ti.filterMode=FilterMode.Bilinear;ti.textureCompression=TextureImporterCompression.Uncompressed;
        ti.npotScale=TextureImporterNPOTScale.None;ti.maxTextureSize=4096;ti.wrapMode=TextureWrapMode.Clamp;ti.anisoLevel=4;ti.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static Sprite Quadrant(string name,Texture2D texture,int col,int row)
    {
        // Generated sheets are not an exact grid. These bounds exclude neighbouring silhouettes.
        Rect[] boxes = name.StartsWith("Aurora") ? new[]{new Rect(0,0,645,638),new Rect(650,0,604,625),new Rect(0,738,638,516),new Rect(640,640,614,614)}
            : name.StartsWith("BlueCastle") ? new[]{new Rect(0,0,650,649),new Rect(712,0,542,640),new Rect(0,800,655,454),new Rect(730,646,524,608)}
            : name.StartsWith("GoldenValley") ? new[]{new Rect(0,0,643,635),new Rect(650,55,604,565),new Rect(0,716,705,538),new Rect(705,635,549,619)}
            : new[]{new Rect(0,0,650,639),new Rect(655,100,599,531),new Rect(0,770,735,484),new Rect(745,631,509,623)};
        Rect crop=boxes[row*2+col];
        int x0=Mathf.RoundToInt(crop.x*texture.width/1254),x1=Mathf.Min(texture.width,Mathf.RoundToInt(crop.xMax*texture.width/1254));
        int y0=Mathf.Max(0,texture.height-Mathf.RoundToInt(crop.yMax*texture.height/1254)),y1=texture.height-Mathf.RoundToInt(crop.y*texture.height/1254);
        var pixels=texture.GetPixels32();int xmin=x1,xmax=x0,ymin=y1,ymax=y0;
        for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(pixels[y*texture.width+x].a>85)
        {xmin=Math.Min(xmin,x);xmax=Math.Max(xmax,x);ymin=Math.Min(ymin,y);ymax=Math.Max(ymax,y);}
        return SpriteAsset(name,texture,new Rect(xmin,ymin,xmax-xmin+1,ymax-ymin+1),new Vector2(.5f,0));
    }
    static Sprite SpriteAsset(string name,Texture2D texture,Rect rect,Vector2 pivot)
    {
        var s=Sprite.Create(texture,rect,pivot,100,0,SpriteMeshType.FullRect);s.name=name;
        string path=Root+"/Sprites/"+name+".asset";var previous=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if(previous!=null){EditorUtility.CopySerialized(s,previous);UnityEngine.Object.DestroyImmediate(s);EditorUtility.SetDirty(previous);return previous;}
        AssetDatabase.CreateAsset(s,path);return s;
    }
    static Sprite Crop(string name,Texture2D t,float x,float top,float width,float height,Vector2 pivot)
    {
        float sx=t.width/2172f,sy=t.height/724f;
        return SpriteAsset(name,t,new Rect(x*sx,t.height-(top+height)*sy,width*sx,height*sy),pivot);
    }
    static void LoadProps()
    {
        var t=Load("JourneyProps",true);
        sword=Crop("Sword",t,180,0,460,724,new Vector2(.5f,0));
        horse=Crop("Horse",t,705,194,748,530,new Vector2(.5f,0));
        gate=Crop("GateStoneFrame",t,1460,0,712,724,new Vector2(.5f,0));
        leftDoor=Crop("DoorLeft",t,1608,110,208,585,new Vector2(0,0));
        rightDoor=Crop("DoorRight",t,1816,110,207,585,new Vector2(1,0));
        doorMaterial=Mat("Door cutouts","FlatDepth/FourRealmsCard",Color.white);
        doorMaterial.mainTexture=t;doorMaterial.SetFloat("_HazeStrength",0);
    }
    static void Drawing(string name,Sprite source,Transform parent,float width,float height,Vector2[] points,int[] triangles)
    {
        var vertices=new Vector3[points.Length];var uv=new Vector2[points.Length];var colors=new Color[points.Length];
        Vector2 pivot=new Vector2(source.pivot.x/source.rect.width,source.pivot.y/source.rect.height);
        for(int i=0;i<points.Length;i++)
        {
            vertices[i]=new Vector3((points[i].x-pivot.x)*width,(points[i].y-pivot.y)*height,0);
            uv[i]=new Vector2((source.rect.x+points[i].x*source.rect.width)/source.texture.width,(source.rect.y+points[i].y*source.rect.height)/source.texture.height);
            colors[i]=Color.white;
        }
        var mesh=new Mesh{name=name.Replace(' ','_'),vertices=vertices,uv=uv,colors=colors,triangles=triangles};mesh.RecalculateBounds();
        string path=Root+"/Meshes/"+name.Replace(' ','_')+".asset";var previous=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(previous!=null){EditorUtility.CopySerialized(mesh,previous);UnityEngine.Object.DestroyImmediate(mesh);mesh=previous;EditorUtility.SetDirty(mesh);}
        else AssetDatabase.CreateAsset(mesh,path);
        var g=Group(name,parent).gameObject;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=doorMaterial;
    }
    static void MakeDoor(Transform parent,float z)
    {
        var root=Group("Door · threshold at ten seconds",parent);root.localPosition=new Vector3(0,0,z);
        root.localScale=new Vector3(.72f,.72f,1);
        // Flat textured polygons leave the centre of the painted arch genuinely open.
        var vertices=new List<Vector2>();var triangles=new List<int>();
        void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d)
        {
            int n=vertices.Count;vertices.AddRange(new[]{a,b,c,d});
            triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
        }
        Quad(new Vector2(0,0),new Vector2(.208f,0),new Vector2(.208f,1),new Vector2(0,1));
        Quad(new Vector2(.791f,0),new Vector2(1,0),new Vector2(1,1),new Vector2(.791f,1));
        Vector2[] roof={new Vector2(.208f,.67f),new Vector2(.33f,.8f),new Vector2(.5f,.86f),new Vector2(.67f,.8f),new Vector2(.791f,.67f)};
        for(int i=0;i<roof.Length-1;i++)Quad(roof[i],roof[i+1],new Vector2(roof[i+1].x,1),new Vector2(roof[i].x,1));
        Drawing("Stone arch",gate,root,5.5f,5.6f,vertices.ToArray(),triangles.ToArray());
        var left=Group("Left hinge",root);left.localPosition=new Vector3(-1.606f,.224f,-.025f);
        var right=Group("Right hinge",root);right.localPosition=new Vector3(1.600f,.224f,-.025f);
        Drawing("Wood left",leftDoor,left,1.606f,4.525f,new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(.45f,.90f),new Vector2(0,.67f)},new[]{0,1,2,0,2,3,0,3,4});
        Drawing("Wood right",rightDoor,right,1.600f,4.525f,new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,.67f),new Vector2(.55f,.90f),new Vector2(0,1)},new[]{0,1,2,0,2,3,0,3,4});
        var d=root.gameObject.AddComponent<FourRealmsDoor>();d.leftLeaf=left;d.rightLeaf=right;
    }
    static Transform Group(string name,Transform parent)
    {var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
    static SpriteRenderer Card(string name,Sprite sprite,Transform parent,Vector3 p,float width,float height,Material mat,Color tint,bool flip=false)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=p;
        g.transform.localScale=new Vector3(width/sprite.bounds.size.x,height/sprite.bounds.size.y,1);
        var sr=g.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sharedMaterial=mat;sr.color=tint;sr.flipX=flip;return sr;
    }
    static Material Mat(string name,string shader,Color color)
    {
        string path=Root+"/Materials/"+name.Replace(' ','_')+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}m.color=color;EditorUtility.SetDirty(m);return m;
    }
    static void Box(string name,Transform parent,Vector3 p,Vector3 scale,Material mat)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;
        UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=mat;
    }
    [MenuItem("Flat Depth/Scene 2 Four Realms/Validate")]
    public static void Validate()
    {
        var w=UnityEngine.Object.FindFirstObjectByType<FourRealmsWalker>();if(w==null||w.locations.Length!=4)throw new Exception("Four locations missing");
        var sprites=UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        foreach(var s in sprites)if(s.sprite==null||s.sharedMaterial==null)throw new Exception("Missing art "+s.name);
        if(sprites.Length<500)throw new Exception("Insufficient environment density");
        for(int i=0;i<4;i++){w.SetDistance(i*20);w.Advance(20);if(Mathf.Abs(w.distance-(i+1)*20)>.001f)throw new Exception("10 second traversal failed");}
        w.Advance(100);if(w.distance!=80)throw new Exception("End clamp failed");w.RestartWalk();w.Advance(-1);
        if(w.distance!=0||w.transform.rotation!=Quaternion.identity)throw new Exception("Fixed gaze/reset failed");
        if(UnityEngine.Object.FindObjectsByType<FourRealmsDoor>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length!=3)throw new Exception("Expected three transition doors");
        foreach(var d in UnityEngine.Object.FindObjectsByType<FourRealmsDoor>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(d.GetComponentsInChildren<MeshFilter>(true).Length!=3)throw new Exception("Door drawing incomplete");
            d.Apply(d.transform.position.z-4);if(d.OpenAmount!=0)throw new Exception("Door should be shut");
            d.Apply(d.transform.position.z-.8f);if(d.OpenAmount<.999f)throw new Exception("Door should be open");
        }
        w.RestartWalk();
        Debug.Log("FOUR_REALMS_VALIDATE_OK sprites="+sprites.Length+" four rooms x 10 seconds; fixed gaze; three doors");
    }
    [MenuItem("Flat Depth/Scene 2 Four Realms/Capture")]
    public static void Capture()
    {
        var cam=Camera.main;var w=cam.GetComponent<FourRealmsWalker>();var vp=cam.GetComponent<PortraitJourneyViewport>();
        float saved=w.distance;bool framing=vp.framingEnabled;vp.framingEnabled=false;
        Rect rect=cam.rect;float aspect=cam.aspect;var old=cam.targetTexture;var active=RenderTexture.active;
        var rt=new RenderTexture(800,1000,24);var image=new Texture2D(800,1000,TextureFormat.RGB24,false);
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Previews"));Directory.CreateDirectory(folder);
        try
        {
            cam.rect=new Rect(0,0,1,1);cam.aspect=.8f;cam.targetTexture=rt;
            foreach(float z in new[]{0f,8f,17.5f,19.1f,20f,28f,40f,48f,60f,68f})
            {w.SetDistance(z);cam.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,800,1000),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,"FourRealms-"+z.ToString("00.0",System.Globalization.CultureInfo.InvariantCulture)+".png"),image.EncodeToPNG());}
        }
        finally
        {w.SetDistance(saved);cam.targetTexture=old;cam.rect=rect;cam.aspect=aspect;vp.framingEnabled=framing;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
        Debug.Log("FOUR_REALMS_CAPTURE_OK");
    }
    [MenuItem("Flat Depth/Scene 2 Four Realms/Open")]
    public static void Open()
    {EditorSceneManager.OpenScene(ScenePath);var window=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));window.maximized=true;window.Focus();}
}
