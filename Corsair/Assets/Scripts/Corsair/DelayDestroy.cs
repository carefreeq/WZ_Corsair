using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class DelayDestroy : MonoBehaviour
    {
        public float time = 5f;
        public UnityEngine.Events.UnityEvent DestroyEvent;
        private void Awake()
        {
            DestroyEvent.Invoke();
            Destroy(gameObject, time);
        }
    }
}