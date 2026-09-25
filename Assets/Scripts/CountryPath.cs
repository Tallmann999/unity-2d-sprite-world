using UnityEngine;

public static class CountryPath
{
    public const float Length = 140f;
    public static Vector3 Point(float z) => new Vector3(4.2f * Mathf.Sin(z*.052f) + 1.6f*Mathf.Sin(z*.021f), Height(z), z);
    public static float Height(float z) => 1.15f*Mathf.Sin(z*.079f) + .9f*Mathf.Cos(z*.037f) - .9f;
    public static Vector3 Tangent(float z) => (Point(z+.1f)-Point(z-.1f)).normalized;
    public static Vector3 Right(float z) => Vector3.Cross(Vector3.up,Tangent(z)).normalized;
    public static Vector3 Shoulder(float z,float offset)
    {
        var p=Point(z)+Right(z)*offset;
        p.y=Height(p.z)+Mathf.Max(0,Mathf.Abs(offset)-3)*.023f;
        return p;
    }
}
