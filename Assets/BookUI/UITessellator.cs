using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
    public class UITessellator : BaseMeshEffect
    {
        [SerializeField, Range(0, 5)]
		int Level = 2;

        public override void ModifyMesh(VertexHelper vh)
        {
            var list = new List<UIVertex>();
            var newlist = new List<UIVertex>();
            vh.GetUIVertexStream(list);

            for (int i = 0; i < list.Count; i += 3)
            {
                newlist.AddRange(TessellateTriangleStream(list[i], list[i + 1], list[i + 2], Level));
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(newlist);
        }

        List<UIVertex> TessellateTriangleStream(UIVertex a, UIVertex b, UIVertex c, int depth)
        {
            if (depth <= 0)
                return new List<UIVertex> { a, b, c };
            var list = TessellateTriangle(a, b, c);
            var newlist = new List<UIVertex>();
            for (int i = 0; i < list.Count; i += 3)
            {
                newlist.AddRange(TessellateTriangleStream(list[i], list[i + 1], list[i + 2], depth - 1));
            }
            return newlist;
        }

        List<UIVertex> TessellateTriangle(UIVertex a, UIVertex b, UIVertex c)
        {
            var ab = Lerp(a, b, 0.5f);
            var bc = Lerp(b, c, 0.5f);
            var ca = Lerp(c, a, 0.5f);
            return new List<UIVertex> {
            a, ab,ca,
            ab,bc,ca,
            ab,b,bc,
            ca,bc,c
        };
        }

        UIVertex Lerp(UIVertex a, UIVertex b, float t)
        {
            var ret = new UIVertex();
            ret.color = Color.Lerp(a.color, b.color, t);
            ret.normal = Vector3.Lerp(a.normal, b.normal, t);
            ret.position = Vector3.Lerp(a.position, b.position, t);
            ret.tangent = Vector3.Lerp(a.tangent, b.tangent, t);
            ret.uv0 = Vector2.Lerp(a.uv0, b.uv0, t);
            ret.uv1 = Vector2.Lerp(a.uv1, b.uv1, t);
            ret.uv2 = Vector2.Lerp(a.uv2, b.uv2, t);
            ret.uv3 = Vector2.Lerp(a.uv3, b.uv3, t);
            return ret;
        }
    }
}
