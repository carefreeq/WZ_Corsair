using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public interface IAttackArrow
    {
        void OnArrow(Attack_Arrow arrow);
    }
    public class Attack_Arrow : Attack
    {
        protected override void OnEnter(GameObject g)
        {
            if (g.GetComponent<Attack_Arrow>())
                Physics.IgnoreCollision(GetComponent<Collider>(), g.GetComponent<Collider>());
            else
                base.OnEnter(g);
            IAttackArrow a = g.GetComponent<IAttackArrow>();
            if (a != null)
            {
                a.OnArrow(this);
                Destroy(gameObject);
            }
        }
    }
}