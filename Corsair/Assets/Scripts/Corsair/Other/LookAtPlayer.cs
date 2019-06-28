using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class LookAtPlayer : MonoBehaviour
    {
        public bool inversion = false;
        void Update()
        {
            if (Player_Vive.Main)
            {
                Vector3 p = Player_Vive.Main.Camera.transform.position - transform.position;
                transform.LookAt((inversion ? -p : p) + transform.position, Vector3.up);
                transform.eulerAngles = new Vector3(0.0f, transform.eulerAngles.y, 0.0f);
            }
        }
    }
}