using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField] float minY = 937.587f;
    [SerializeField] float maxY = 1064.389f;
    [SerializeField] float speed = 10f;

    [SerializeField] float offsetX = 17.13f;
    [SerializeField] float offsetZ = 76.833f;

    private float fixedX;
    private float fixedZ;
    private float currentY;
    private bool goingUp = true; // y값 증가 방향

    public bool IsStop = false;

    void Awake()
    {
        fixedX = offsetX % 360f;
        fixedZ = (-offsetZ) % 360f;

        currentY = minY;
        transform.rotation = Quaternion.Euler(fixedX, currentY, fixedZ);
    }

    void Update()
    {
        if (!IsStop)
        {
            // 방향에 따라 y값 증가 또는 감소
            if (goingUp)
            {
                currentY += speed * Time.deltaTime;
                if (currentY >= maxY)
                {
                    currentY = maxY;
                    goingUp = false; // 방향 반전
                }
            }
            else
            {
                currentY -= speed * Time.deltaTime;
                if (currentY <= minY)
                {
                    currentY = minY;
                    goingUp = true; // 방향 반전
                }
            }

            transform.rotation = Quaternion.Euler(fixedX, currentY, fixedZ);
        }
    }
}
