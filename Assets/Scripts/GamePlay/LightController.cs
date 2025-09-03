using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] float minY = -500f;
    [SerializeField] float maxY = -365f;
    [SerializeField] float speed = 1f;

    [SerializeField] float offsetX = 15f;
    [SerializeField] float offsetZ = -150f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float y = Mathf.PingPong(Time.time * speed, maxY - minY) + minY;

        transform.rotation = Quaternion.Euler(offsetX, y, -offsetZ);
    }
}
