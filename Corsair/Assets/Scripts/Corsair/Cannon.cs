using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Cannon : MonoBehaviour
    {
        [SerializeField]
        protected Transform barrel;
        [SerializeField]
        protected Transform point;
        [SerializeField]
        protected Attack bullet;
    }
}