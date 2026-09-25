using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

public static class BuildCountryRoad
{
    const string Root="Assets/Countryside";
    static Material plant, meadow, road, wood;
    static Sprite oak,birch,flowers,grass;
    static Sprite spruce,beech,oldOak,fern,hazel,sedge,bluebells,poppies,shelter;
    static Color C(string s) { ColorUtility.TryParseHtmlString(s,out var c); return c; }

    [MenuItem("Flat Depth/Build countryside 256 scene")]
    public static void Build()
    {
        Directory.CreateDirectory(Root+"/Materials"); Directory.CreateDirectory(Root+"/Sprites");
        Directory.CreateDirectory(Root+"/Meshes");
        AssetDatabase.Refresh();
        string atlasPath=Root+"/Art/MeadowAtlas.png";
        var ti=(TextureImporter)AssetImporter.GetAtPath(atlasPath);
        ti.textureType=TextureImporterType.Default; ti.alphaIsTransparency=true; ti.mipmapEnabled=false;
        ti.filterMode=FilterMode.Point; ti.textureCompression=TextureImporterCompression.Uncompressed;
        ti.maxTextureSize=2048; ti.npotScale=TextureImporterNPOTScale.None; ti.SaveAndReimport();
        var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
        // Generator produced an asymmetric atlas: explicit rectangles preserve whole trees.
        oak=SpriteAsset("Oak",atlas,new Rect(0,534,675,720),new Vector2(.51f,.01f));
        birch=SpriteAsset("Birch",atlas,new Rect(700,534,554,720),new Vector2(.49f,.01f));
        flowers=SpriteAsset("Flowers",atlas,new Rect(0,0,655,480),new Vector2(.5f,.085f));
        grass=SpriteAsset("Grass",atlas,new Rect(665,0,589,490),new Vector2(.5f,.085f));
        LoadMedievalSprites();
        plant=Mat("Plants","FlatDepth/Card",Color.white);
        meadow=Mat("Meadow","FlatDepth/CountryGround",C("#6C813D"));
        road=Mat("Road","FlatDepth/CountryGround",C("#A99065")); road.SetFloat("_Dirt",1);
        wood=Mat("Fence","FlatDepth/Card",C("#66553B"));
        var sky=Mat("Sky","FlatDepth/CountrySky",Color.white);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        RenderSettings.skybox=sky; RenderSettings.fog=true; RenderSettings.fogMode=FogMode.Linear;
        RenderSettings.fogColor=C("#B5CCAF"); RenderSettings.fogStartDistance=32; RenderSettings.fogEndDistance=103;
        RenderSettings.ambientLight=Color.white;
        var landscape=new GameObject("COUNTRYSIDE · rolling road");
        MakeStrip("Rolling meadow",landscape.transform,65,0,24,meadow);
        MakeStrip("Dirt road · two wheel tracks",landscape.transform,2.35f,.045f,8,road);
        var vegetation=new GameObject("FLAT SPRITES · editable plants");
        var trees=new GameObject("Trees"); trees.transform.SetParent(vegetation.transform);
        var flowersRoot=new GameObject("Wildflowers"); flowersRoot.transform.SetParent(vegetation.transform);
        var grassRoot=new GameObject("Grass"); grassRoot.transform.SetParent(vegetation.transform);
        var random=new System.Random(731);
        float R(float a,float b)=>(float)(a+random.NextDouble()*(b-a));
        Sprite[] treeTypes={oak,birch,spruce,beech,oldOak};
        Sprite[] groundTypes={grass,sedge,fern,grass,sedge};
        Sprite[] blossomTypes={flowers,bluebells,poppies};
        for(int k=0;k<62;k++) for(int side=-1;side<=1;side+=2)
        {
            float z=-2+k*2.7f+R(-.8f,.8f); float off=side*R(5.1f,8.8f);
            var sprite=treeTypes[random.Next(treeTypes.Length)];
            bool clearing=side>0&&(Mathf.Abs(z-18)<4||Mathf.Abs(z-76)<4||Mathf.Abs(z-123)<4);
            if(!clearing) AddPlant(sprite,trees.transform,CountryPath.Shoulder(z,off),R(6.5f,9.4f),new Color(R(.82f,1),R(.87f,1),R(.76f,.95f),1),random.Next(2)==0);
            AddPlant(treeTypes[random.Next(treeTypes.Length)],trees.transform,CountryPath.Shoulder(z+R(-1,1),side*R(11,18)),R(8,12),C("#A5B28C"),random.Next(2)==0);
            if(k%2==0) AddPlant(treeTypes[random.Next(treeTypes.Length)],trees.transform,CountryPath.Shoulder(z+2,side*R(21,31)),R(9,13),C("#94A681"),false);
        }
        for(int k=0;k<360;k++) for(int side=-1;side<=1;side+=2)
        {
            float z=-1+k*.46f+R(-.18f,.18f);
            AddPlant(groundTypes[random.Next(groundTypes.Length)],grassRoot.transform,CountryPath.Shoulder(z,side*R(2.55f,4.2f)),R(.35f,.85f),Color.white,random.Next(2)==0);
            AddPlant(groundTypes[random.Next(groundTypes.Length)],grassRoot.transform,CountryPath.Shoulder(z+.2f,side*R(4.3f,7.1f)),R(.65f,1.25f),C("#B9C799"),random.Next(2)==0);
            if(k%2==0) AddPlant(blossomTypes[random.Next(blossomTypes.Length)],flowersRoot.transform,CountryPath.Shoulder(z,side*R(2.8f,5.0f)),R(.38f,.82f),Color.white,random.Next(2)==0);
            if(k%3==0) AddPlant(hazel,grassRoot.transform,CountryPath.Shoulder(z,side*R(5.6f,9.5f)),R(.95f,1.7f),C("#C8CEA2"),random.Next(2)==0);
            if(k%2==0) AddPlant(fern,grassRoot.transform,CountryPath.Shoulder(z,side*R(7,16)),R(.85f,1.4f),C("#AABB92"),false);
        }
        var fences=new GameObject("Village fence"); fences.transform.SetParent(landscape.transform);
        for(int k=0;k<24;k++)
        {
            float z=21+k*2.1f; var a=CountryPath.Shoulder(z,4.4f);
            Box("Fence post",fences.transform,a+Vector3.up*.53f,new Vector3(.14f,1.06f,.14f));
            if(k<23) for(int rail=0;rail<2;rail++)
            {
                var b=CountryPath.Shoulder(z+2.1f,4.4f); var go=Box("Fence rail",fences.transform,(a+b)*.5f+Vector3.up*(.4f+rail*.38f),new Vector3(.095f,.11f,Vector3.Distance(a,b)));
                go.transform.rotation=Quaternion.LookRotation(b-a);
            }
        }
        var medieval=new GameObject("MEDIEVAL · wayside shelters and old milestones"); medieval.transform.SetParent(landscape.transform);
        foreach(float z in new[]{18f,76f,123f}) AddPlant(shelter,medieval.transform,CountryPath.Shoulder(z,5.3f),3.7f,Color.white,false);
        var stone=Mat("Old stone","FlatDepth/Card",C("#737768"));
        foreach(float z in new[]{8f,36f,61f,95f,132f})
        {
            var pos=CountryPath.Shoulder(z,-2.9f);
            var marker=Box("Weathered stone milestone",medieval.transform,pos+Vector3.up*.4f,new Vector3(.38f,.8f,.32f));
            marker.transform.rotation=Quaternion.Euler(0,R(-15,15),R(-5,5)); marker.GetComponent<Renderer>().sharedMaterial=stone;
            var cap=Box("Milestone cap",medieval.transform,pos+Vector3.up*.85f,new Vector3(.45f,.14f,.38f)); cap.GetComponent<Renderer>().sharedMaterial=stone;
        }
        var camera=new GameObject("PLAYER · follow country road"); camera.tag="MainCamera";
        var cam=camera.AddComponent<Camera>(); cam.clearFlags=CameraClearFlags.Skybox; cam.fieldOfView=66;
        cam.nearClipPlane=.06f; cam.farClipPlane=130; cam.allowHDR=false; cam.allowMSAA=false; cam.aspect=1;
        camera.AddComponent<AudioListener>(); var walker=camera.AddComponent<CountryWalker>(); walker.RestartWalk();
        var pixel=camera.AddComponent<PixelViewport>(); pixel.helpStrip=MakeHelp();
        foreach(var bb in UnityEngine.Object.FindObjectsByType<UprightBillboard>(FindObjectsSortMode.None)) bb.FaceCamera();
        QualitySettings.antiAliasing=0;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),"Assets/Scenes/CountryRoad256.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/CountryRoad256.unity",true),new EditorBuildSettingsScene("Assets/Scenes/FlatDungeon.unity",true)};
        AssetDatabase.SaveAssets();
        Validate(); Capture();
        Selection.activeGameObject=camera;
        Debug.Log("COUNTRY_ROAD_BUILD_OK");
    }

    static void LoadMedievalSprites()
    {
        string path=Root+"/Art/MedievalAtlas.png";
        var ti=(TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType=TextureImporterType.Default; ti.alphaIsTransparency=true; ti.mipmapEnabled=false;
        ti.filterMode=FilterMode.Point; ti.textureCompression=TextureImporterCompression.Uncompressed;
        ti.maxTextureSize=2048; ti.npotScale=TextureImporterNPOTScale.None; ti.SaveAndReimport();
        var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        // Actual sheet has taller tree row. Keep complete silhouettes with explicit row bounds.
        int[] top={0,530,850}; int[] bottom={520,850,1254};
        Sprite Slice(string name,int col,int row)=>SpriteAsset(name,atlas,new Rect(col*418,1254-bottom[row],418,bottom[row]-top[row]),new Vector2(.5f,row==0?.035f:row==1?.02f:.10f));
        spruce=Slice("Spruce",0,0); beech=Slice("Beech",1,0); oldOak=Slice("AncientOak",2,0);
        fern=Slice("Fern",0,1); hazel=Slice("HazelBush",1,1); sedge=Slice("MeadowSedge",2,1);
        bluebells=Slice("Bluebells",0,2); poppies=Slice("Poppies",1,2); shelter=Slice("WaysideShelter",2,2);
    }

    static Sprite SpriteAsset(string name,Texture2D atlas,Rect rect,Vector2 pivot)
    {
        string p=Root+"/Sprites/"+name+".asset";
        if(AssetDatabase.LoadAssetAtPath<Sprite>(p)!=null) AssetDatabase.DeleteAsset(p);
        var s=Sprite.Create(atlas,rect,pivot,100,0,SpriteMeshType.FullRect); s.name=name; AssetDatabase.CreateAsset(s,p); return s;
    }
    static Material Mat(string name,string shader,Color color)
    {
        string path=Root+"/Materials/"+name+".mat"; var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null) { m=new Material(Shader.Find(shader)); AssetDatabase.CreateAsset(m,path); }
        if(m.HasProperty("_Color")) m.color=color; return m;
    }
    static void AddPlant(Sprite sprite,Transform parent,Vector3 pos,float height,Color tint,bool flip)
    {
        var go=new GameObject(sprite.name); go.transform.SetParent(parent); go.transform.position=pos;
        go.transform.localScale=Vector3.one*(height/(sprite.rect.height/sprite.pixelsPerUnit));
        var sr=go.AddComponent<SpriteRenderer>(); sr.sprite=sprite; sr.sharedMaterial=plant; sr.color=tint; sr.flipX=flip;
        sr.shadowCastingMode=ShadowCastingMode.Off; go.AddComponent<UprightBillboard>();
    }
    static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 size)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.SetParent(parent);
        go.transform.position=pos; go.transform.localScale=size; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial=wood; return go;
    }
    static void MakeStrip(string name,Transform parent,float half,float lift,int columns,Material mat)
    {
        int rows=186; var v=new Vector3[(rows+1)*(columns+1)]; var uv=new Vector2[v.Length]; var tri=new List<int>();
        for(int row=0;row<=rows;row++) for(int col=0;col<=columns;col++)
        {
            int i=row*(columns+1)+col; float z=-16+row; float u=(float)col/columns; float off=Mathf.Lerp(-half,half,u);
            var p=CountryPath.Shoulder(z,off); p.y+=lift;
            if(half>10) p.y+=Mathf.Max(0,Mathf.Abs(off)-7)*(.017f*Mathf.Sin(z*.06f+off*.1f));
            v[i]=p; uv[i]=new Vector2(half>10?off*.16f:u,z*.3f);
            if(row<rows&&col<columns) { int j=i+columns+1; tri.AddRange(new[]{i,j,i+1,i+1,j,j+1}); }
        }
        var mesh=new Mesh { name=name }; mesh.vertices=v; mesh.uv=uv; mesh.triangles=tri.ToArray(); mesh.RecalculateNormals(); mesh.RecalculateBounds();
        string path=Root+"/Meshes/"+(half>10?"Meadow":"Road")+".asset";
        if(AssetDatabase.LoadAssetAtPath<Mesh>(path)!=null) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(mesh,path);
        var go=new GameObject(name); go.transform.SetParent(parent); go.AddComponent<MeshFilter>().sharedMesh=mesh; go.AddComponent<MeshRenderer>().sharedMaterial=mat;
    }

    static Texture2D MakeHelp()
    {
        var glyphs=new Dictionary<char,string>{
            ['W']="10101101011010110101101011010101010",['A']="01110100011000111111100011000110001",
            ['L']="10000100001000010000100001000011111",['K']="10001100101010011000101001001010001",
            ['S']="01111100001000001110000010000111110",['P']="11110100011000111110100001000010000",
            ['C']="01111100001000010000100001000001111",['E']="11111100001000011110100001000011111",
            ['U']="10001100011000110001100011000101110",['T']="11111001000010000100001000010000100",
            ['O']="01110100011000110001100011000101110",['R']="11110100011000111110101001001010001",
            [' ']="00000000000000000000000000000000000"};
        var t=new Texture2D(256,12,TextureFormat.RGBA32,false); var bg=new Color32(31,49,40,235);
        for(int y=0;y<12;y++) for(int x=0;x<256;x++) t.SetPixel(x,y,bg);
        string text="W WALK   SPACE AUTO   R RESET";
        int start=(256-text.Length*6)/2;
        for(int n=0;n<text.Length;n++) if(glyphs.TryGetValue(text[n],out string glyph))
            for(int y=0;y<7;y++) for(int x=0;x<5;x++) if(glyph[y*5+x]=='1') t.SetPixel(start+n*6+x,9-y,C("#E8E1AA"));
        t.Apply(); string path=Root+"/Art/PixelControls.png"; File.WriteAllBytes(path,t.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(t); AssetDatabase.ImportAsset(path);
        var ti=(TextureImporter)AssetImporter.GetAtPath(path); ti.filterMode=FilterMode.Point; ti.mipmapEnabled=false; ti.textureCompression=TextureImporterCompression.Uncompressed; ti.npotScale=TextureImporterNPOTScale.None; ti.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    public static void Validate()
    {
        var w=UnityEngine.Object.FindFirstObjectByType<CountryWalker>(); if(w==null) throw new Exception("No country walker");
        float min=10,max=-10; bool rises=false,falls=false;
        for(int i=0;i<140;i++) { float h=CountryPath.Height(i); min=Mathf.Min(min,h); max=Mathf.Max(max,h); float delta=CountryPath.Height(i+1)-h; rises|=delta>.02f; falls|=delta<-.02f; }
        if(!rises||!falls||max-min<2) throw new Exception("Missing terrain relief");
        w.Place(1000); if(w.distance!=140) throw new Exception("End clamp"); w.RestartWalk();
        if(PixelViewport.Resolution!=256) throw new Exception("Wrong resolution");
        foreach(var sr in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)) if(sr.sprite==null||sr.sharedMaterial==null) throw new Exception("Missing plant artwork");
        Debug.Log($"COUNTRY_VALIDATION_OK relief={max-min:F2}m, camera path, sprites, 256x256");
    }
    public static void Capture()
    {
        var cam=Camera.main; var w=cam.GetComponent<CountryWalker>(); float saved=w.distance; var old=cam.targetTexture;
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Previews")); Directory.CreateDirectory(folder);
        foreach(float z in new[]{0f,22f,46f,80f,118f})
        {
            w.Place(z); foreach(var b in UnityEngine.Object.FindObjectsByType<UprightBillboard>(FindObjectsSortMode.None)) b.FaceCamera();
            var rt=new RenderTexture(256,256,24); rt.filterMode=FilterMode.Point; rt.antiAliasing=1; cam.targetTexture=rt; cam.aspect=1; cam.Render(); RenderTexture.active=rt;
            var img=new Texture2D(256,256,TextureFormat.RGB24,false); img.ReadPixels(new Rect(0,0,256,256),0,0); img.Apply();
            File.WriteAllBytes(Path.Combine(folder,$"Country256-{z:000}.png"),img.EncodeToPNG());
            RenderTexture.active=null; cam.targetTexture=old; UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(img);
        }
        w.Place(saved); foreach(var b in UnityEngine.Object.FindObjectsByType<UprightBillboard>(FindObjectsSortMode.None)) b.FaceCamera();
        Debug.Log("COUNTRY_PREVIEWS_OK");
    }
    public static void Open()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/CountryRoad256.unity");
        var game=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")); game.maximized=true; game.Focus();
    }
}
