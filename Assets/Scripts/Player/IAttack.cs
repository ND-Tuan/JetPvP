using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttack
{
    public void Attack(Team team);
    public void SetRotation(Vector3 Diraction);
}
