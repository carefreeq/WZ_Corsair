using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class HitArrow : Hit,IAttackArrow
    {
        public GameObject arrow;
        public void Hurt(AttackInfo a)
        {
        }

        public void OnArrow(Attack_Arrow arrow)
        {
            //Debug.Log(arrow.transform.name + ":" + arrow.transform.position);
            if (this.arrow)
            {
                GameObject a = GameObject.Instantiate(this.arrow, arrow.Info.Position, arrow.Info.Rotation);
                a.transform.SetParent(transform);
            }
        }

    }
}