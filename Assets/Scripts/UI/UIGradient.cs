using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIGradient : BaseMeshEffect
{
    [SerializeField] private Color topColor = Color.white;
    [SerializeField] private Color bottomColor = Color.black;
    [SerializeField] private bool horizontal;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0) return;

        var verts = new System.Collections.Generic.List<UIVertex>();
        vh.GetUIVertexStream(verts);

        float minY = float.MaxValue, maxY = float.MinValue;
        float minX = float.MaxValue, maxX = float.MinValue;

        for (int i = 0; i < verts.Count; i++)
        {
            var v = verts[i].position;
            if (horizontal) { minX = Mathf.Min(minX, v.x); maxX = Mathf.Max(maxX, v.x); }
            else { minY = Mathf.Min(minY, v.y); maxY = Mathf.Max(maxY, v.y); }
        }

        for (int i = 0; i < verts.Count; i++)
        {
            var vert = verts[i];
            float t = horizontal
                ? (maxX - minX > 0 ? (vert.position.x - minX) / (maxX - minX) : 0)
                : (maxY - minY > 0 ? (vert.position.y - minY) / (maxY - minY) : 0);
            vert.color *= Color.Lerp(bottomColor, topColor, t);
            verts[i] = vert;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }
}
