using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public interface IAttackCannonball : IAttack
    {
        void OnCannonball(Attack_Cannonball ball);
    }
    public class Attack_Cannonball : Attack
    {
        protected override void Awake()
        {
            base.Awake();
            
        }
        protected override void OnAttack(IAttack a)
        {
            IAttackCannonball _a = a as IAttackCannonball;
            if (_a != null)
                _a.OnCannonball(this);
        }
    }
}