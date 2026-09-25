using UnityEngine;

[ExecuteAlways]
public sealed class UprightBillboard : MonoBehaviour
{
    public void FaceCamera()
    {
        var cam=Camera.main;
        if(cam!=null) transform.rotation=Quaternion.Euler(0,cam.transform.eulerAngles.y,0);
    }
    private void LateUpdate() => FaceCamera();
}
