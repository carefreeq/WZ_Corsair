using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Score : MonoBehaviour
    {
        [SerializeField]
        private int score = 10;
        private void OnDestroy()
        {
            Manager.AddScore(score);
        }
    }
}