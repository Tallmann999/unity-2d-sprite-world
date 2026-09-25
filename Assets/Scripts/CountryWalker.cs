using UnityEngine;

public sealed class CountryWalker : MonoBehaviour
{
    public float speed=3.1f;
    public float distance;
    public bool autoWalk;
    public float Progress => distance/CountryPath.Length;
    public void Place(float z)
    {
        distance=Mathf.Clamp(z,0,CountryPath.Length);
        transform.position=CountryPath.Point(distance)+Vector3.up*1.65f;
        transform.rotation=Quaternion.LookRotation(CountryPath.Tangent(distance),Vector3.up)*Quaternion.Euler(4,0,0);
    }
    public void RestartWalk() { autoWalk=false; Place(0); }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)) RestartWalk();
        if(Input.GetKeyDown(KeyCode.Space)) autoWalk=!autoWalk;
        if(autoWalk||Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow))
        {
            float dz=speed*Time.deltaTime;
            float segment=Vector3.Distance(CountryPath.Point(distance),CountryPath.Point(distance+.1f))/.1f;
            Place(distance+dz/segment);
        }
        if(distance>=CountryPath.Length) autoWalk=false;
    }
}
