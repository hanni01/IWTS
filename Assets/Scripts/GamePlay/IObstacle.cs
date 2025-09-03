using UnityEngine;

public interface IObstacle
{ 
    bool IsStop { get; }

    void Movement();
}
