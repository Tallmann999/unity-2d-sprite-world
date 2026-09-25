using UnityEngine;

/// <summary>Four twenty-metre chapters. At two metres/second each takes ten moving seconds.</summary>
public sealed class FourRealmsWalker : MonoBehaviour
{
    public const float RoomLength = 20f;
    public const float WalkSpeed = 2f;
    public const float EndDistance = 80f;
    public GameObject[] locations;
    public GameObject sword, horse;
    public bool autoWalk;
    public bool showOverlay = true;
    public float distance;
    public int RoomIndex => Mathf.Clamp(Mathf.FloorToInt(distance / RoomLength), 0, 3);
    public float Progress => distance / EndDistance;
    public float RoomSeconds => (distance - RoomIndex * RoomLength) / WalkSpeed;
    public readonly string[] Titles = { "Северное сияние", "Замок в ущелье", "Золотая долина", "Лесная крепость" };
    int activeRoom = -1;
    float gait, transition;
    GUIStyle title, hint;
    Vector3 swordRest, horseRest;

    void Awake()
    {
        if (sword != null) swordRest = sword.transform.localPosition;
        if (horse != null) horseRest = horse.transform.localPosition;
        SetDistance(0);
    }
    public void SetDistance(float z)
    {
        distance = float.IsNaN(z) ? 0 : Mathf.Clamp(z, 0, EndDistance);
        transform.SetPositionAndRotation(new Vector3(0, 1.7f, distance), Quaternion.identity);
        ApplyLocation(false);
        RefreshDoors();
    }
    public void RestartWalk()
    {
        autoWalk = false; gait = 0; transition = 0;
        SetDistance(0);
    }
    public void Advance(float metres)
    {
        if (float.IsNaN(metres) || metres <= 0) return;
        distance = Mathf.Min(EndDistance, distance + metres);
        transform.SetPositionAndRotation(new Vector3(0, 1.7f, distance), Quaternion.identity);
        ApplyLocation(true);
        if (distance >= EndDistance) autoWalk = false;
    }
    void ApplyLocation(bool fade)
    {
        int room = RoomIndex;
        if (room != activeRoom)
        {
            if (fade && activeRoom >= 0) transition = .18f;
            activeRoom = room;
        }
        if (locations != null)
            for (int i = 0; i < locations.Length; i++)
                if (locations[i] != null && locations[i].activeSelf != (i == room)) locations[i].SetActive(i == room);
        if (sword != null) sword.SetActive(room != 2);
        if (horse != null) horse.SetActive(room == 2);
    }
    public void RefreshDoors()
    {
        foreach (var door in FindObjectsByType<FourRealmsDoor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (door.gameObject.scene == gameObject.scene) door.Apply(distance);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) { RestartWalk(); return; }
        if (Input.GetKeyDown(KeyCode.Space)) autoWalk = !autoWalk;
        bool walking = distance < EndDistance && (autoWalk || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow));
        if (walking) { Advance(WalkSpeed * Time.deltaTime); gait += Time.deltaTime; }
        transition = Mathf.Max(0, transition - Time.deltaTime);
        // Only the handheld sprite bobs. The player's gaze remains exactly forward.
        if (sword != null) sword.transform.localPosition = swordRest + Vector3.up * (walking ? Mathf.Sin(gait * 7) * .008f : 0);
        if (horse != null) horse.transform.localPosition = horseRest + Vector3.up * (walking ? Mathf.Sin(gait * 8) * .013f : 0);
        transform.rotation = Quaternion.identity;
        RefreshDoors();
    }
    void OnGUI()
    {
        if (!showOverlay) return;
        if (title == null)
        {
            title = new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold };
            hint = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            title.normal.textColor = new Color(.96f,.91f,.79f);
            hint.normal.textColor = new Color(.86f,.85f,.8f);
        }
        Rect frame = GetComponent<Camera>().pixelRect;
        float x = frame.x + 15, y = Screen.height - frame.yMax + 12;
        GUI.color = new Color(0,0,0,.5f);
        GUI.DrawTexture(new Rect(frame.x, y - 12, frame.width, 67), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(x,y,frame.width-30,26), $"{RoomIndex + 1:00} / 04   {Titles[RoomIndex]}", title);
        GUI.Label(new Rect(x,y+28,frame.width-30,20), $"{Mathf.Min(RoomSeconds,10):0.0} / 10 с", hint);
        string controls = distance >= EndDistance ? "Путь пройден · R — сначала" : "W / ↑ — идти   ·   Пробел — автоход   ·   R — сначала";
        GUI.Label(new Rect(x, Screen.height-frame.y-28,frame.width-30,20), controls, hint);
        if (transition > 0)
        {
            GUI.color = new Color(0,0,0,transition/.18f);
            GUI.DrawTexture(new Rect(frame.x, Screen.height-frame.yMax,frame.width,frame.height),Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
