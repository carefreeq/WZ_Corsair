using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public interface IAttackCannonball
    {
        void OnCannonball(Attack_Cannonball ball);
    }
    public class Attack_Cannonball : Attack
    {
        protected override void OnEnter(GameObject g)
        {
            if (g.GetComponent<Attack_Cannonball>())
                Physics.IgnoreCollision(GetComponent<Collider>(), g.GetComponent<Collider>());
            else
                base.OnEnter(g);

            IAttackCannonball a = g.GetComponent<IAttackCannonball>();
            if (a != null)
            {
                a.OnCannonball(this);
                Destroy(gameObject);
            }
        }
    }
}