using UnityEngine;
using UnityEngine.UI;

namespace MiunaKKHelper.CardFavorites;

/// <summary>Shared transparent UI mesh; glow is drawn locally without camera postprocessing.</summary>
public sealed class CardFrameGraphic : MaskableGraphic
{
    public int Rating;
    public bool Favorite;
    CardFrameFlowGraphic flow;
    const int Segments = 96;
    const int PathSamples = 64;
    static readonly float[] Stops = { 0, .23f, .46f, .53f, .78f, 1 };
    static readonly Color BlueLight = Hex(0xd1f2ff), Blue = Hex(0x58adf5), BlueDark = Hex(0x245b99);
    static readonly Color GoldLight = Hex(0xfff1ae), Gold = Hex(0xe7b64c), GoldDark = Hex(0x805016);
    static readonly Color FlowOrange = Hex(0xff9d28), FlowPale = Hex(0xffe4a0), FlowThread = Hex(0xff9c28);
    static readonly Color FlowGold = Hex(0xffd76a), FlowSpark = Hex(0xfffef2);
    static readonly Vector2[] PathPts = new Vector2[PathSamples + 1];
    static readonly Vector2[] PathNrm = new Vector2[PathSamples + 1];
    static float pathW, pathH, pathRad = -1;

    void Update()
    {
        if (Rating == 5 && !canvasRenderer.cull && flow == null)
        {
            var go = new GameObject("Five Star Flow", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            flow = go.AddComponent<CardFrameFlowGraphic>();
            flow.Owner = this;
            flow.raycastTarget = false;
        }
        if (flow != null && flow.gameObject.activeSelf != (Rating == 5))
            flow.gameObject.SetActive(Rating == 5);
    }

    public override void OnDestroy()
    {
        if (flow != null) Destroy(flow.gameObject);
        base.OnDestroy();
    }

    public override void OnDisable()
    {
        if (flow != null) flow.gameObject.SetActive(false);
        base.OnDisable();
    }
    static Color Hex(int rgb) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);
    static Rect Inset(Rect r, float d) => new Rect(r.x + d, r.y + d, r.width - d * 2, r.height - d * 2);

    public override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = rectTransform.rect;
        if (r.width < 16 || r.height < 16) return;
        // Reference card is 252x352; leave room inside bounds for glow under native list clipping.
        float s = Mathf.Min(r.width / 252f, r.height / 352f);
        if (Rating == 4 || Rating == 5)
        {
            Rect path = Inset(r, 3 * s);
            Color tone = Rating == 5 ? Gold : Blue;
            for (int j = 5; j >= 1; j--)
                Strip(vh, path, 6*s, (4 + j*3)*s, tone, (Rating == 5 ? .045f : .035f), false, -1, 1);
            Strip(vh, path, 6*s, 4*s, tone, 1, true, -1, 1);
            if (Rating == 5)
            {
                Strip(vh, Inset(r, 7*s), 3*s, .7f*s, GoldLight, .75f, false, -1, 1);
                Corners(vh, r, s);
            }
        }
        if (Favorite) Heart(vh, new Vector2(r.xMax - 13f, r.yMax - 14f));
    }

    internal void PopulateFlow(VertexHelper vh)
    {
        vh.Clear();
        Rect r = rectTransform.rect;
        if (Rating != 5 || r.width < 16 || r.height < 16) return;
        float s = Mathf.Min(r.width / 252f, r.height / 352f);
        Rect path = Inset(r, 3*s);
        EnsurePath(path, 6*s);
        float head = Mathf.Repeat(Time.unscaledTime / 3f, 1);
        // One glow + one core per tail (was 3 blur strips). Same 5 colors / 3s lap.
        FlowTail(vh, s, head, FlowOrange, 12, 5, .05f, .24f);
        FlowTail(vh, s, head, FlowPale, 7, 2.5f, .02f, .42f);
        FlowTail(vh, s, head, FlowThread, 2, 1.2f, .10f, .42f);
        FlowTail(vh, s, head, FlowGold, 1.5f, .5f, .04f, .9f);
        FlowTail(vh, s, head, FlowSpark, 2, 1.3f, .006f, 1);
    }

    void FlowTail(VertexHelper vh, float s, float head, Color tone, float width, float blur, float length, float alpha)
    {
        // 3×0.09 stacked ≈ 0.25 coverage; one wider strip at 0.22 keeps the bloom without the extra overdraw.
        StripCached(vh, (width + blur*4)*s, tone, alpha*.22f, head, length);
        StripCached(vh, width*s, tone, alpha, head, length);
    }

    static void EnsurePath(Rect r, float radius)
    {
        if (pathRad > 0 && Mathf.Abs(pathW - r.width) < .05f && Mathf.Abs(pathH - r.height) < .05f
            && Mathf.Abs(pathRad - radius) < .05f) return;
        pathW = r.width; pathH = r.height; pathRad = radius;
        for (int i = 0; i <= PathSamples; i++)
            PathPts[i] = Point(r, radius, i / (float)PathSamples, out PathNrm[i]);
    }

    static void SamplePath(float t, out Vector2 p, out Vector2 n)
    {
        float x = Mathf.Repeat(t, 1f) * PathSamples;
        int i = (int)x;
        if (i >= PathSamples) i = PathSamples - 1;
        float f = x - i;
        p = Vector2.Lerp(PathPts[i], PathPts[i + 1], f);
        n = Vector2.Lerp(PathNrm[i], PathNrm[i + 1], f);
        float mag = n.sqrMagnitude;
        if (mag > 1e-8f) n *= 1f / Mathf.Sqrt(mag);
    }

    void StripCached(VertexHelper vh, float width, Color tone, float alpha, float head, float length)
    {
        int count = Mathf.Max(4, Mathf.CeilToInt(PathSamples * length));
        int start = vh.currentVertCount;
        for (int i = 0; i <= count; i++)
        {
            float u = (float)i / count;
            Vector2 normal, p;
            SamplePath(head - length + u * length, out p, out normal);
            Color c = tone;
            c.a = alpha * Mathf.Pow(u, .65f);
            vh.AddVert(p + normal * width * .5f, c, Vector2.zero);
            vh.AddVert(p - normal * width * .5f, c, Vector2.zero);
            if (i == count) continue;
            int v = start + i * 2;
            vh.AddTriangle(v, v + 2, v + 1);
            vh.AddTriangle(v + 1, v + 2, v + 3);
        }
    }

    void Strip(VertexHelper vh, Rect r, float radius, float width, Color tone, float alpha, bool metal, float head, float length)
    {
        int count = head < 0 ? Segments : Mathf.Max(4, Mathf.CeilToInt(Segments*length));
        int start = vh.currentVertCount;
        for (int i = 0; i <= count; i++)
        {
            float u = (float)i/count;
            float t = head < 0 ? u : head-length+u*length;
            Vector2 normal;
            Vector2 p = Point(r, radius, t, out normal);
            Color c = metal ? Metal((p.x-r.x+r.yMax-p.y)/(r.width+r.height)) : tone;
            c.a = alpha * (head < 0 ? 1 : Mathf.Pow(u, .65f));
            vh.AddVert(p+normal*width*.5f, c, Vector2.zero);
            vh.AddVert(p-normal*width*.5f, c, Vector2.zero);
            if (i == count) continue;
            int v = start+i*2;
            vh.AddTriangle(v, v+2, v+1);
            vh.AddTriangle(v+1, v+2, v+3);
        }
    }

    Color Metal(float t)
    {
        Color light = Rating == 5 ? GoldLight : BlueLight, dark = Rating == 5 ? GoldDark : BlueDark, main = Rating == 5 ? Gold : Blue;
        int i = 0;
        while (i < 4 && t > Stops[i+1]) i++;
        Color a = i == 0 || i == 3 ? light : i == 1 || i == 4 ? dark : main;
        Color b = i == 0 || i == 3 ? dark : i == 1 || i == 4 ? main : light;
        return Color.Lerp(a, b, Mathf.InverseLerp(Stops[i], Stops[i+1], t));
    }

    // Arc length, including straight edges: constant speed even on non-square cards.
    static Vector2 Point(Rect r, float radius, float t, out Vector2 normal)
    {
        float rad = Mathf.Min(radius, Mathf.Min(r.width, r.height)*.5f);
        float w = r.width-2*rad, h = r.height-2*rad, arc = Mathf.PI*rad*.5f;
        float d = Mathf.Repeat(t, 1)*(2*w+2*h+4*arc);
        for (int side = 0; side < 4; side++)
        {
            float line = side%2 == 0 ? w : h;
            if (d <= line)
            {
                if (side == 0) { normal = Vector2.up; return new Vector2(r.xMin+rad+d, r.yMax); }
                if (side == 1) { normal = Vector2.right; return new Vector2(r.xMax, r.yMax-rad-d); }
                if (side == 2) { normal = Vector2.down; return new Vector2(r.xMax-rad-d, r.yMin); }
                normal = Vector2.left; return new Vector2(r.xMin, r.yMin+rad+d);
            }
            d -= line;
            if (d <= arc)
            {
                float angle = (90-side*90)*Mathf.Deg2Rad - d/rad;
                normal = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 center = new Vector2(side < 2 ? r.xMax-rad : r.xMin+rad, side == 0 || side == 3 ? r.yMax-rad : r.yMin+rad);
                return center+normal*rad;
            }
            d -= arc;
        }
        normal = Vector2.up; return new Vector2(r.xMin+rad, r.yMax);
    }

    static void Corners(VertexHelper vh, Rect r, float s)
    {
        for (int i = 0; i < 4; i++)
        {
            float x = i%2 == 0 ? 1 : -1, y = i < 2 ? 1 : -1;
            Vector2 p = new Vector2(x > 0 ? r.xMin+7*s : r.xMax-7*s, y > 0 ? r.yMin+7*s : r.yMax-7*s);
            int v = vh.currentVertCount;
            vh.AddVert(p, GoldLight, Vector2.zero);
            vh.AddVert(p+new Vector2(x*5*s, 0), Gold, Vector2.zero);
            vh.AddVert(p+new Vector2(0, y*5*s), Gold, Vector2.zero);
            vh.AddTriangle(v, v+1, v+2);
        }
    }

    static void Heart(VertexHelper vh, Vector2 center)
    {
        for (int layer = 0; layer < 2; layer++)
        {
            int start = vh.currentVertCount;
            Color c = layer == 0 ? new Color(.12f, .04f, .09f, .9f) : new Color(1f, .3f, .52f);
            float scale = layer == 0 ? .56f : .46f;
            vh.AddVert(center, c, Vector2.zero);
            for (int i = 0; i <= 32; i++)
            {
                float t = i*Mathf.PI*2/32;
                var p = new Vector2(16*Mathf.Pow(Mathf.Sin(t), 3),
                    13*Mathf.Cos(t)-5*Mathf.Cos(2*t)-2*Mathf.Cos(3*t)-Mathf.Cos(4*t));
                vh.AddVert(center+p*scale, c, Vector2.zero);
                if (i > 0) vh.AddTriangle(start, start+i, start+i+1);
            }
        }
    }
}

/// <summary>Only the small moving ribbons rebuild; the frame and favorite icon remain static.</summary>
public sealed class CardFrameFlowGraphic : MaskableGraphic
{
    internal CardFrameGraphic Owner;
    int lastTick = int.MinValue;

    void Update()
    {
        if (Owner == null || Owner.Rating != 5 || canvasRenderer.cull || Owner.canvasRenderer.cull)
            return;
        // 30Hz, staggered by instance so visible gold cards do not rebuild on the same frame.
        int tick = (int)((Time.unscaledTime + (GetInstanceID() & 3) * (1f / 120f)) * 30f);
        if (tick == lastTick) return;
        lastTick = tick;
        SetVerticesDirty();
    }

    public override void OnPopulateMesh(VertexHelper vh)
    {
        if (Owner != null) Owner.PopulateFlow(vh);
        else vh.Clear();
    }
}
