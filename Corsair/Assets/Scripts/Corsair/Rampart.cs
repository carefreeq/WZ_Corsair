using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Rampart : Life, IAttackCannonball
    {
        public static List<Rampart> Ramparts { get; private set; }
        static Rampart()
        {
            Ramparts = new List<Rampart>();
        }

        public GameObject[] booms;
        private void Awake()
        {
            Ramparts.Add(this);
        }
        protected override void Death()
        {
            Ramparts.Remove(this);
        }

        public void OnCannonball(Attack_Cannonball ball)
        {
            foreach (var b in booms)
                GameObject.Instantiate(b, ball.Info.Position, ball.Info.Rotation);
        }
    }
}