using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 8f;
    public float minX = -28f, maxX = 28f, minZ = -9f, maxZ = 9f;

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 p = transform.position + new Vector3(h, 0f, v) * speed * Time.deltaTime;
        p.x = Mathf.Clamp(p.x, minX, maxX);
        p.z = Mathf.Clamp(p.z, minZ, maxZ);
        p.y = 1f;
        transform.position = p;
    }
}
