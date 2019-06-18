using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Enemy : Life
    {
        [SerializeField]
        private int score = 10;
        protected virtual void OnDestroy()
        {
            Manager.AddScore(score);
        }
    }
}