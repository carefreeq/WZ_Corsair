using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Enemies : MonoBehaviour
    {
        public Emission[] emissions;
        public int total = 0;
        private void Awake()
        {
            emissions = GetComponentsInChildren<Emission>();

            total = 0;
            foreach (Emission e in emissions)
                total += e.total;
        }
    }
}