using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Cannon_Manual : Cannon
    {
        public void Launch(Vector3 p)
        {
            Attack b = GameObject.Instantiate(bullet, point.position, point.rotation);
            b.LaunchTo(p);
        }
        public void AimAt(Vector3 p)
        {
            transform.LookAt(new Vector3(p.x, transform.position.y, p.z), Vector3.up);
        }
    }
}