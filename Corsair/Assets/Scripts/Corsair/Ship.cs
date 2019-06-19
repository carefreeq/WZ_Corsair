using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public enum ShipStatus
    {
        Idle,
        Move,
        Attack,
    }
    public class Ship : Life, IAttackCannonball, IAttackArrow
    {
        [SerializeField]
        protected float speed = 10f;
        protected float time = 0.0f;
        public ShipStatus Status { get; set; }

        public GameObject arrow;
        public GameObject boom;
        public GameObject death;
        public void OnArrow(Attack_Arrow arrow)
        {
            if (this.arrow)
            {
                GameObject a = GameObject.Instantiate(this.arrow, arrow.Info.Position, arrow.Info.Rotation);
                a.transform.SetParent(transform);
            }
        }

        public void OnCannonball(Attack_Cannonball ball)
        {
            if (this.boom)
            {
                GameObject b = GameObject.Instantiate(this.boom, ball.Info.Position, ball.Info.Rotation);
                b.transform.SetParent(transform);
            }
        }

        protected virtual void Awake()
        {
            time = Random.Range(0f, 3.1514926f);
            Status = ShipStatus.Move;
            StartCoroutine(UpdateCor());
        }
        protected override void Death()
        {
            StopAllCoroutines();
            StartCoroutine(DeathCor());
        }
        private IEnumerator UpdateCor()
        {
            float yp = 0.0f;
            float xe = 0.0f;
            while (true)
            {
                transform.position = new Vector3(transform.position.x, yp + Mathf.Cos(Time.time - time), transform.position.z);
                //transform.eulerAngles = new Vector3(xe + Mathf.Cos(Time.time - time)*3f, transform.eulerAngles.y, transform.eulerAngles.z);
                switch (Status)
                {
                    case ShipStatus.Move:
                        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
                        break;
                    default:
                        break;
                }
                yield return new WaitForEndOfFrame();
            }
        }
        private IEnumerator DeathCor()
        {
            Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<Collider>().enabled = false;
            if (death)
                GameObject.Instantiate(death, transform.position, Quaternion.identity);
            float t = Time.time;
            while (Time.time - t < 12f)
            {
                if (transform.eulerAngles.x > -60f)
                    transform.eulerAngles += new Vector3(-10f * Time.deltaTime, 0.0f, 0.0f);
                transform.position += new Vector3(0.0f, -3.2f * Time.deltaTime, 0.0f);
                yield return new WaitForEndOfFrame();
            }
            Destroy(gameObject);
        }
    }
}