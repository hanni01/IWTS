using UnityEngine;

public class ObstacleSystem : MonoBehaviour, IObstacle
{
    public bool IsStop { get; set; } = false;
    [SerializeField] private float moveSpeed = 200f;

    void Start()
    {
        
    }

    void Update()
    {
        if (!IsStop)
        {
            Movement();
        }
    }

    public void Movement()
    {
        transform.Rotate(0f, 0f, moveSpeed * Time.deltaTime);
    }
}
