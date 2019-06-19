using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public interface IAttackArrow : IAttack
    {
        void OnArrow(Attack_Arrow arrow);
    }
    public class Attack_Arrow : Attack
    {
        protected override void OnAttack(IAttack a)
        {
            IAttackArrow _a = a as IAttackArrow;
            if (_a != null)
                _a.OnArrow(this);
        }
    }
}