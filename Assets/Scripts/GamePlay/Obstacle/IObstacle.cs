using UnityEngine;

public interface IObstacle
{ 
    bool IsStop { get; set; }

    void Movement();
}
