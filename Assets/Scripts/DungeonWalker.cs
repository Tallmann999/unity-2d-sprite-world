using UnityEngine;

/// <summary>A forward-only camera walk through the three layered dungeon rooms.</summary>
public sealed class DungeonWalker : MonoBehaviour
{
    public float startZ = 0f;
    public float endZ = 108f;
    [Min(0.1f)] public float speed = 3f;
    public float eyeHeight = 1.7f;
    public bool autoWalk;
    public bool showOverlay = true;
    [Range(0f, 0.06f)] public float bobAmount = 0.018f;

    private float walkedTime;
    private GUIStyle headingStyle, roomStyle, controlsStyle;
    public float Progress => Mathf.InverseLerp(startZ, Mathf.Max(startZ, endZ), transform.position.z);
    public int RoomIndex => transform.position.z < 36f ? 0 : transform.position.z < 72f ? 1 : 2;

    private void Awake() => SetDistance(transform.position.z);

    public void SetDistance(float z)
    {
        float position = float.IsNaN(z) ? startZ : Mathf.Clamp(z, startZ, Mathf.Max(startZ, endZ));
        transform.SetPositionAndRotation(new Vector3(0f, eyeHeight, position), Quaternion.identity);
        walkedTime = 0f;
    }

    public void Advance(float distance)
    {
        if (float.IsNaN(distance) || distance <= 0f) return;
        Vector3 position = transform.position;
        position.x = 0f;
        position.z = Mathf.Clamp(position.z + distance, startZ, Mathf.Max(startZ, endZ));
        transform.SetPositionAndRotation(position, Quaternion.identity);
    }

    public void RestartWalk()
    {
        autoWalk = false;
        SetDistance(startZ);
        foreach (DungeonDoor door in FindObjectsByType<DungeonDoor>(FindObjectsSortMode.None))
            if (door.gameObject.scene == gameObject.scene) door.ResetDoor();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartWalk();
            return;
        }
        if (Input.GetKeyDown(KeyCode.Space)) autoWalk = !autoWalk;

        bool moving = transform.position.z < Mathf.Max(startZ, endZ)
            && (autoWalk || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow));
        if (moving)
        {
            Advance(speed * Time.deltaTime);
            walkedTime += Time.deltaTime;
        }
        if (transform.position.z >= endZ) autoWalk = false;

        Vector3 position = transform.position;
        position.x = 0f;
        float targetHeight = eyeHeight + (moving ? Mathf.Sin(walkedTime * 6f) * bobAmount : 0f);
        position.y = Mathf.Lerp(position.y, targetHeight, 1f - Mathf.Exp(-10f * Time.deltaTime));
        transform.SetPositionAndRotation(position, Quaternion.identity);
    }

    private void OnGUI()
    {
        if (!showOverlay) return;
        if (headingStyle == null)
        {
            headingStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            roomStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            controlsStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            headingStyle.normal.textColor = new Color(0.68f, 0.65f, 0.56f);
            roomStyle.normal.textColor = new Color(0.88f, 0.84f, 0.73f);
            controlsStyle.normal.textColor = new Color(0.74f, 0.72f, 0.65f);
        }

        float margin = Screen.width < 600 ? 12f : 24f;
        string room = RoomIndex == 0 ? "I · Катакомбы" : RoomIndex == 1 ? "II · Мрачные пещеры" : "III · Мегалитический подвал";
        DrawLabel(new Rect(margin, margin, Screen.width - margin * 2f, 20f), "СЦЕНА 1 · ПОДЗЕМЕЛЬЕ", headingStyle);
        DrawLabel(new Rect(margin, margin + 20f, Screen.width - margin * 2f, 28f), room, roomStyle);

        string hint = Progress >= 1f ? "Путь пройден · R — сначала" : "W / ↑ — идти   ·   Пробел — " + (autoWalk ? "пауза" : "автоход") + "   ·   R — сначала";
        DrawLabel(new Rect(margin, Screen.height - 36f, Screen.width - margin * 2f, 24f), hint, controlsStyle);
        Color previous = GUI.color;
        GUI.color = new Color(0.56f, 0.44f, 0.25f, 0.7f);
        GUI.DrawTexture(new Rect(margin, Screen.height - 12f, (Screen.width - margin * 2f) * Progress, 2f), Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private static void DrawLabel(Rect rect, string text, GUIStyle style)
    {
        Color previous = GUI.contentColor;
        GUI.contentColor = new Color(0f, 0f, 0f, 0.9f);
        GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), text, style);
        GUI.contentColor = previous;
        GUI.Label(rect, text, style);
    }
}
