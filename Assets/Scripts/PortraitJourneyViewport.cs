using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class PortraitJourneyViewport : MonoBehaviour
{
    public bool portrait = true;
    public bool framingEnabled = true;
    Camera view;
    void OnEnable() { view = GetComponent<Camera>(); Apply(); }
    void Update() { if (framingEnabled) Apply(); }
    public void Apply()
    {
        if (view == null) view = GetComponent<Camera>();
        if (!portrait) { view.rect = new Rect(0,0,1,1); view.ResetAspect(); return; }
        float aspect = .8f;
        float available = (float)Screen.width / Mathf.Max(1, Screen.height);
        view.rect = available > aspect ? new Rect((1-aspect/available)*.5f,0,aspect/available,1) : new Rect(0,(1-available/aspect)*.5f,1,available/aspect);
        view.aspect = aspect;
    }
    void OnGUI()
    {
        if (!portrait || !framingEnabled) return;
        GUI.depth = 100;
        var r = view.pixelRect;
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0,0,r.x,Screen.height),Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.xMax,0,Screen.width-r.xMax,Screen.height),Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x,0,r.width,Screen.height-r.yMax),Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x,Screen.height-r.y,r.width,r.y),Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
