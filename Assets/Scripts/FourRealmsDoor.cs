using UnityEngine;

/// <summary>Distance-driven opening: doors never delay the ten-second traversal.</summary>
public sealed class FourRealmsDoor : MonoBehaviour
{
    public Transform leftLeaf, rightLeaf;
    public float OpenAmount { get; private set; }
    public void Apply(float cameraZ)
    {
        OpenAmount = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(transform.position.z-3.2f,transform.position.z-.9f,cameraZ));
        if (leftLeaf != null) leftLeaf.localRotation = Quaternion.Euler(0,-OpenAmount*92,0);
        if (rightLeaf != null) rightLeaf.localRotation = Quaternion.Euler(0,OpenAmount*92,0);
    }
}
