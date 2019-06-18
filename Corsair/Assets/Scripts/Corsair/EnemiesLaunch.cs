using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class EnemiesLaunch : MonoBehaviour
    {
        [SerializeField]
        private int max = 5;
        [SerializeField]
        private float spand = 3f;
        [SerializeField]
        private float range = 10f;
        public Corsair Corsair_1;
        private float t = 0.0f;
        private void Awake()
        {

        }
        private void Update()
        {
            if (Corsair.Corsairs.Count < max)
                if (Time.time - t > 0f)
                {
                    Corsair c = GameObject.Instantiate(Corsair_1, transform.TransformPoint(new Vector3(Random.Range(-range, range), 0.0f, 0.0f)), transform.rotation);
                    t += spand;
                }
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawLine(transform.position + transform.right * range, transform.position - transform.right * range);
        }
    }
}