using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class Ship_Auto : Ship
    {
        public float time = 600f;
        public ShipStatus status;
        public Ship[] ship;
        private float max;
        private List<GameObject> fires = new List<GameObject>();
        protected override void Awake()
        {
            base.Awake();
            target = ship[0];
            Status = status;
            max = time;
        }
        protected override void Update()
        {
            switch (Manager.GameStatus)
            {
                case GameStatus.Playing:
                    target = ship[(int)(Time.time / 10) % ship.Length];
                    if ((time -= Time.deltaTime) > 0)
                    {
                        int f = (int)((1.0f - (time / max)) / 0.1f);
                        while (fires.Count < f)
                        {
                            GameObject g = Instantiate(death[Random.Range(0, death.Length)], GetPosition(), Quaternion.identity);
                            g.transform.SetParent(transform, true);
                            fires.Add(g);
                        }
                    }
                    else
                    {
                        Death();
                    }
                    break;
            }
            base.Update();
        }
    }
}