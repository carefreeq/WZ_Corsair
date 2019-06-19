using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class CorsairCannons : MonoBehaviour
    {
        public Cannon_Auto[] cannons;
        private void Awake()
        {
            cannons = GetComponentsInChildren<Cannon_Auto>();
        }
        public void Launch(Life target)
        {
            foreach (Cannon_Auto c in cannons)
                c.Launch(target.GetPosition());
        }
    }
}