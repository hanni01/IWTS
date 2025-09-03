using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0.01f, 1.58f, -9.8f);
    [SerializeField] float followSpeed = 5f;

    private float maxY = 4f;
    private float minZ = -9.87f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void LateUpdate()
    {
        if(target == null) return;

        Vector3 desirePosition = target.position + offset;
        desirePosition.y = Mathf.Min(desirePosition.y, maxY);
        desirePosition.z = Mathf.Max(minZ, desirePosition.z);
        transform.position = Vector3.Lerp(transform.position, desirePosition, followSpeed * Time.deltaTime);
    }
}
