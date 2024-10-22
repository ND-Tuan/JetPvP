using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectile
{
    public void Fire(Player player, Vector3 position, Quaternion rotation);
}
