using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class OutlineMesh : Outline
    {
        public MeshFilter mesh;

        protected override MeshRenderer GetRender()
        {
            GameObject g = new GameObject("Outline");
            g.transform.SetParent(mesh.transform);
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            g.transform.localScale = Vector3.one;
            MeshFilter mf = g.AddComponent<MeshFilter>();
            mf.mesh = new Mesh();
            mf.mesh = mesh.mesh;
            MeshRenderer mr = g.AddComponent<MeshRenderer>();
            mr.material = mat;
            return mr;
        }
    }
}