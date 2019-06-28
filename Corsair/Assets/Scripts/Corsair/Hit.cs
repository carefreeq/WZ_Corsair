using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Hit : MonoBehaviour
    {
        //public LayerMask ignore;
        protected Collider col;
        private void Awake()
        {
            col = gameObject.GetComponent<Collider>();
        }
    }
}