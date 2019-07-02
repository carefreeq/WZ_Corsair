using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class HitCannonball : Hit, IAttackCannonball
    {
        public GameObject boom;
        public GameObjects[] gameObjects = new GameObjects[0];
        public void OnCannonball(Attack_Cannonball ball)
        {
            if (this.boom)
            {
                GameObject b = GameObject.Instantiate(this.boom, ball.Info.Position, ball.Info.Rotation);
                b.transform.SetParent(transform);
                for (int i = 0; i < gameObjects.Length; i++)
                    if (gameObjects[i].gameObjects.IsRandom())
                        GameObject.Instantiate(gameObjects[i].gameObjects.GetRandom(),ball.Info.Position,ball.Info.Rotation);
            }
        }
        public void OnTriggerEnter(Collider other)
        {
            Attack_Cannonball c= other.gameObject.GetComponent<Attack_Cannonball>();
            if (c)
            {
                c.OnCollision(this.gameObject);
            }
        }
    }
}