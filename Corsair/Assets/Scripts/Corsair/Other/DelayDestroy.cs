using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class DelayDestroy : Delay
    {
        private void Awake()
        {
            Destroy(gameObject, time);
        }
    }
}