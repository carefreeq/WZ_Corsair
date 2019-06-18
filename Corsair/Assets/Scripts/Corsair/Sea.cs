using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Sea : MonoBehaviour, IAttack
    {
        [SerializeField]
        private GameObject ballEnter;
        public void Hit(AttackInfo a)
        {
            GameObject.Instantiate(ballEnter, a.Position, ballEnter.transform.rotation);
        }
    }
}