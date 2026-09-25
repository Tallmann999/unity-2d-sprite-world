using UnityEngine;

/// <summary>Place each leaf transform at its outside hinge and offset its sprite inward.</summary>
public sealed class DungeonDoor : MonoBehaviour
{
    public Transform leftLeaf;
    public Transform rightLeaf;
    public float openDistance = 5f;
    [Range(0f, 100f)] public float openAngle = 82f;
    [Min(0.05f)] public float openSpeed = 1.5f;
    public Transform viewer;

    private Quaternion leftClosed, rightClosed;
    private bool initialized;
    private bool opened;
    private float amount;
    public float OpenAmount => amount;

    private void Awake() => Initialize();

    private void Initialize()
    {
        if (initialized) return;
        if (leftLeaf != null) leftClosed = leftLeaf.localRotation;
        if (rightLeaf != null) rightClosed = rightLeaf.localRotation;
        initialized = true;
    }

    public void ResetDoor()
    {
        Initialize();
        opened = false;
        amount = 0f;
        ApplyRotation();
    }

    private void Update()
    {
        if (viewer == null && Camera.main != null) viewer = Camera.main.transform;
        if (viewer == null) return;
        // Once passed, doors remain open until the walk is restarted.
        if (viewer.position.z >= transform.position.z - Mathf.Max(0f, openDistance)) opened = true;
        amount = Mathf.MoveTowards(amount, opened ? 1f : 0f, openSpeed * Time.deltaTime);
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        float angle = Mathf.SmoothStep(0f, openAngle, amount);
        if (leftLeaf != null) leftLeaf.localRotation = leftClosed * Quaternion.Euler(0f, -angle, 0f);
        if (rightLeaf != null) rightLeaf.localRotation = rightClosed * Quaternion.Euler(0f, angle, 0f);
    }
}
