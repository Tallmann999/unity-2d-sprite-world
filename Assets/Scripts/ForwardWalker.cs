using UnityEngine;

public sealed class ForwardWalker : MonoBehaviour
{
    [Min(0.1f)] public float speed = 3.5f;
    public float finishZ = 48f;
    public bool autoWalk;
    private float walkedTime;
    private GUIStyle title, body;
    public float Progress => Mathf.Clamp01(transform.position.z / finishZ);

    public void Advance(float distance)
    {
        var p = transform.position;
        p.x = 0;
        p.z = Mathf.Clamp(p.z + Mathf.Max(0, distance), 0, finishZ);
        transform.position = p;
        transform.rotation = Quaternion.identity;
    }

    public void RestartWalk()
    {
        transform.SetPositionAndRotation(new Vector3(0, 1.65f, 0), Quaternion.identity);
        walkedTime = 0;
        autoWalk = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) RestartWalk();
        if (Input.GetKeyDown(KeyCode.Space)) autoWalk = !autoWalk;
        bool moving = (autoWalk || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && Progress < 1;
        if (moving)
        {
            Advance(speed * Time.deltaTime);
            walkedTime += Time.deltaTime;
        }
        var p = transform.position;
        p.y = Mathf.Lerp(p.y, 1.65f + (moving ? Mathf.Sin(walkedTime * 6) * .025f : 0), Time.deltaTime * 10);
        transform.position = p;
        transform.rotation = Quaternion.identity;
    }

    private void OnGUI()
    {
        if (title == null)
        {
            title = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
            body = new GUIStyle(GUI.skin.label) { fontSize = 15 };
            title.normal.textColor = new Color(.65f, .95f, .86f);
            body.normal.textColor = new Color(.82f, .85f, .89f);
        }
        GUI.Box(new Rect(20, 20, 450, 106), GUIContent.none);
        GUI.Label(new Rect(36, 28, 420, 34), "FLAT / DEPTH", title);
        GUI.Label(new Rect(36, 66, 420, 24), "W / ↑ — вперёд   ·   Пробел — автопрогулка", body);
        GUI.Label(new Rect(36, 91, 420, 24), "R — сначала   ·   Камера без поворотов", body);
        if (GUI.Button(new Rect(20, Screen.height - 64, 190, 40), autoWalk ? "Остановиться" : "Автопрогулка")) autoWalk = !autoWalk;
        if (GUI.Button(new Rect(220, Screen.height - 64, 120, 40), "Сначала")) RestartWalk();
        GUI.Label(new Rect(Screen.width - 200, Screen.height - 58, 180, 30), $"Путь: {Progress * 100:0}%", body);
        if (Progress >= 1)
        {
            GUI.Box(new Rect(Screen.width / 2f - 220, Screen.height / 2f - 50, 440, 100), GUIContent.none);
            GUI.Label(new Rect(Screen.width / 2f - 190, Screen.height / 2f - 36, 400, 36), "Уровень пройден", title);
            GUI.Label(new Rect(Screen.width / 2f - 190, Screen.height / 2f + 4, 400, 30), "Нажми R, чтобы пройти ещё раз.", body);
        }
    }
}
