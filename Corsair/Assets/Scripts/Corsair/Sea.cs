using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Sea : MonoBehaviour, IAttackArrow, IAttackCannonball
    {
        [SerializeField]
        private GameObject ballEnter;
        public void Hit(AttackInfo a) { }

        public void OnArrow(Attack_Arrow arrow)
        {
            GameObject.Instantiate(ballEnter, arrow.Info.Position, ballEnter.transform.rotation);
        }

        public void OnCannonball(Attack_Cannonball ball)
        {
            GameObject.Instantiate(ballEnter, ball.Info.Position, ballEnter.transform.rotation);
        }
    }
}