using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Interfaces
{

}

public interface IPooledObject
{
    void OnSpawn();
    void OnDeSpawn();
}

public interface IPlayerShipInput
{
    void InitializeShip();
    void MoveLeftRight(int dir);
    void StartShipMovement(bool isTrue);
    void BoostShip(bool isTrue);
    int GetTargetPointIndex();

    event GameController.OnShipCollision onShipCollision;
}

