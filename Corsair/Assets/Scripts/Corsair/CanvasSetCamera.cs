using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class CanvasSetCamera : MonoBehaviour
    {
        private Canvas can;
        private void Awake()
        {
            can.worldCamera = Player_Vive.Main.Camera;
           
        }
    }
}