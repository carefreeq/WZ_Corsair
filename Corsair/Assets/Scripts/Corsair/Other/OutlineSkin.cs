using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Corsair
{
    public class OutlineSkin : Outline
    {
        public SkinnedMeshRenderer skin;

        protected override MeshRenderer GetRender()
        {
            GameObject g = new GameObject("Outline");
            g.transform.SetParent(skin.transform);
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            g.transform.localScale =new Vector3( 1.0f/skin.transform.lossyScale.x, 1.0f / skin.transform.lossyScale.y, 1.0f / skin.transform.lossyScale.z);
            MeshFilter mf = g.AddComponent<MeshFilter>();
            mf.mesh = new Mesh();
            skin.BakeMesh(mf.mesh);
            MeshRenderer mr = g.AddComponent<MeshRenderer>();
            mr.material = mat;
            return mr;
        }
    }
}