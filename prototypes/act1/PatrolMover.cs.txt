using UnityEngine;

public class PatrolMover : MonoBehaviour
{
    public Vector3 pointA = new Vector3(-5f, 1f, -4f);
    public Vector3 pointB = new Vector3(-5f, 1f, 4f);
    public float speed = 3f;
    private float t;
    private int dir = 1;

    void Update()
    {
        t += dir * speed * Time.deltaTime / Mathf.Max(0.01f, Vector3.Distance(pointA, pointB));
        if (t >= 1f) { t = 1f; dir = -1; }
        if (t <= 0f) { t = 0f; dir = 1; }
        transform.position = Vector3.Lerp(pointA, pointB, t);
    }
}
