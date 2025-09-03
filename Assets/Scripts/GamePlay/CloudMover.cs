using UnityEngine;

public class CloudMover : MonoBehaviour
{
    private float _speed;
    private float _endZ;
    private bool _initialized;

    public void Init(float speed, float endZ)
    {
        _speed = speed;
        _endZ = endZ;
        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized) return;

        transform.position += Vector3.forward * _speed * Time.deltaTime;

        if (transform.position.z >= _endZ)
        {
            Destroy(gameObject);
        }
    }
}
