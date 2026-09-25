using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class PixelViewport : MonoBehaviour
{
    public const int Resolution=256;
    public Texture2D helpStrip;
    public RenderTexture Target { get; private set; }
    private Camera view;
    private void OnEnable()
    {
        view=GetComponent<Camera>();
        Target=new RenderTexture(Resolution,Resolution,24,RenderTextureFormat.ARGB32);
        Target.name="Countryside • 256 x 256"; Target.filterMode=FilterMode.Point;
        Target.antiAliasing=1; Target.useMipMap=false; Target.Create();
        view.targetTexture=Target; view.aspect=1; view.allowMSAA=false; view.allowHDR=false;
    }
    private void OnDisable()
    {
        if(view!=null) view.targetTexture=null;
        if(Target!=null) { Target.Release(); Destroy(Target); }
    }
    private void OnGUI()
    {
        if(Target==null) return;
        GUI.depth=100;
        GUI.color=new Color(.055f,.09f,.10f,1);
        GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);
        GUI.color=Color.white;
        float scale=Mathf.Max(1,Mathf.Floor(Mathf.Min(Screen.width,Screen.height)/256f));
        if(Mathf.Min(Screen.width,Screen.height)<256) scale=Mathf.Min(Screen.width,Screen.height)/256f;
        float size=256*scale;
        var rect=new Rect(Mathf.Floor((Screen.width-size)/2),Mathf.Floor((Screen.height-size)/2),size,size);
        GUI.DrawTexture(rect,Target,ScaleMode.StretchToFill,false);
        if(helpStrip!=null) GUI.DrawTexture(new Rect(rect.x,rect.y+size-12*scale,size,12*scale),helpStrip);
        var walker=GetComponent<CountryWalker>();
        if(walker!=null)
        {
            GUI.color=new Color(.94f,.79f,.39f);
            GUI.DrawTexture(new Rect(rect.x,rect.y+size-1*scale,Mathf.Floor(256*walker.Progress)*scale,scale),Texture2D.whiteTexture);
            GUI.color=Color.white;
        }
    }
}
