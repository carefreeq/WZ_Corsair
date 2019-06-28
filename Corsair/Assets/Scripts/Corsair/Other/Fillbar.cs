using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Fillbar : MonoBehaviour
    {
        private static int FillId;
        static Fillbar()
        {
            FillId = Shader.PropertyToID("_Fill");
        }
        public bool isHide = true;
        private MeshRenderer render;
        private void Awake()
        {
            render = gameObject.GetComponent<MeshRenderer>();
            if (isHide)
                gameObject.SetActive(false);
        }
        public void SetFill(float i)
        {
            if (isHide)
            {
                if (i > 0.99f)
                    gameObject.SetActive(false);
                else
                    gameObject.SetActive(true);
            }
            render.material.SetFloat(FillId, i);
        }
    }
}