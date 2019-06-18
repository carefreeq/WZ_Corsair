using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Rampart : Life
    {
        public static List<Rampart> Ramparts { get; private set; }

        static Rampart()
        {
            Ramparts = new List<Rampart>();
        }
        private void Awake()
        {
            Ramparts.Add(this);
        }

        public override void Hit(AttackInfo a)
        {
            if (a.Faction == AttackFaction.Enemy)
            {
                base.Hit(a);
            }
        }
        private void OnDestroy()
        {
            Ramparts.Remove(this);
        }
    }
}