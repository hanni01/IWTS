using UnityEngine;

public class AbilityPlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject playerCube;
    private float _speed = 100f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        playerCube.transform.Rotate(0f, _speed * Time.deltaTime, 0f);
    }
}
