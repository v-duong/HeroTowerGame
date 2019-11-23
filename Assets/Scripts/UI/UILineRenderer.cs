using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : MaskableGraphic
{
    private enum SegmentType
    {
        Start,
        Middle,
        End,
    }

    public enum JoinType
    {
        Bevel,
        Miter
    }

    private const float MIN_MITER_JOIN = 15 * Mathf.Deg2Rad;

    // A bevel 'nice' join displaces the vertices of the line segment instead of simply rendering a
    // quad to connect the endpoints. This improves the look of textured and transparent lines, since
    // there is no overlapping.
    private const float MIN_BEVEL_NICE_JOIN = 30 * Mathf.Deg2Rad;

    private static readonly Vector2 UV_TOP_LEFT = Vector2.zero;
    private static readonly Vector2 UV_BOTTOM_LEFT = new Vector2(0, 1);
    private static readonly Vector2 UV_TOP_CENTER = new Vector2(0.5f, 0);
    private static readonly Vector2 UV_BOTTOM_CENTER = new Vector2(0.5f, 1);
    private static readonly Vector2 UV_TOP_RIGHT = new Vector2(1, 0);
    private static readonly Vector2 UV_BOTTOM_RIGHT = new Vector2(1, 1);

    private static readonly Vector2[] startUvs = new[] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_CENTER, UV_TOP_CENTER };
    private static readonly Vector2[] middleUvs = new[] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_CENTER, UV_TOP_CENTER };
    private static readonly Vector2[] endUvs = new[] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };

    [SerializeField]
    private Texture m_Texture;

    [SerializeField]
    private Rect m_UVRect = new Rect(0f, 0f, 1f, 1f);

    public float LineThickness = 2;
    public bool UseMargins;
    public Vector2 Margin;
    [SerializeField]
    private List<LinePoint> _points = new List<LinePoint>();
    public bool relativeSize;
    public bool localToParent;

    public bool LineList = false;
    public bool LineCaps = false;
    public JoinType LineJoins = JoinType.Bevel;

    public override Texture mainTexture
    {
        get
        {
            return m_Texture == null ? s_WhiteTexture : m_Texture;
        }
    }

    /// <summary>
    /// Texture to be used.
    /// </summary>
    public Texture Texture
    {
        get
        {
            return m_Texture;
        }
        set
        {
            if (m_Texture == value)
                return;

            m_Texture = value;
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }

    /// <summary>
    /// UV rectangle used by the texture.
    /// </summary>
    public Rect UvRect
    {
        get
        {
            return m_UVRect;
        }
        set
        {
            if (m_UVRect == value)
                return;
            m_UVRect = value;
            SetVerticesDirty();
        }
    }

    public class LinePoint
    {
        public Vector2 start;
        public Vector2 end;
        public Color color;

        public LinePoint(Vector2 start, Vector2 end, Color color)
        {
            this.start = start;
            this.end = end;
            this.color = color;
        }
    }

    public List<LinePoint> Points
    {
        get
        {
            return _points;
        }

        set
        {
            if (_points == value)
                return;
            _points = value;
            SetAllDirty();
        }
    }

    public void AddPoints(LinePoint item)
    {
        Points.Add(item);
        SetAllDirty();
    }

    public void SetPointColor(LinePoint item, Color color)
    {
        Points.Find(x => x == item).color = color;
        SetAllDirty();
    }

    public void SetDirty()
    {
        SetAllDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (Points == null)
            return;

        var sizeX = rectTransform.rect.width;
        var sizeY = rectTransform.rect.height;
        var offsetX = -rectTransform.pivot.x * rectTransform.rect.width;
        var offsetY = -rectTransform.pivot.y * rectTransform.rect.height;

        // don't want to scale based on the size of the rect, so this is switchable now
        if (!relativeSize)
        {
            sizeX = 1;
            sizeY = 1;
        }

        if (UseMargins)
        {
            sizeX -= Margin.x;
            sizeY -= Margin.y;
            offsetX += Margin.x / 2f;
            offsetY += Margin.y / 2f;
        }

        vh.Clear();

        // Generate the quads that make up the wide line
        var segments = new List<UIVertex[]>();
        if (LineList)
        {
            for (var i = 0; i < Points.Count; i++)
            {
                var start = Points[i].start;
                var end = Points[i].end;
                if (localToParent)
                {
                    start = new Vector3(start.x * sizeX, start.y * sizeY);
                    end = new Vector3(end.x * sizeX, end.y * sizeY);

                } else
                {
                    start = new Vector3(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
                    end = new Vector3(end.x * sizeX + offsetX, end.y * sizeY + offsetY);
                }
                if (LineCaps)
                {
                    segments.Add(CreateLineCap(start, end, SegmentType.Start, Points[i].color));
                }

                segments.Add(CreateLineSegment(start, end, SegmentType.Middle, Points[i].color));

                if (LineCaps)
                {
                    segments.Add(CreateLineCap(start, end, SegmentType.End, Points[i].color));
                }
            }
        }
        else
        {
            for (var i = 0; i < Points.Count; i++)
            {
                var start = Points[i].start;
                var end = Points[i].end;
                if (localToParent)
                {
                    start = new Vector2(start.x * sizeX, start.y * sizeY);
                    end = new Vector2(end.x * sizeX, end.y * sizeY);

                }
                else
                {
                    start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
                    end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);
                }
                if (LineCaps && i == 1)
                {
                    segments.Add(CreateLineCap(start, end, SegmentType.Start, Points[i].color));
                }

                segments.Add(CreateLineSegment(start, end, SegmentType.Middle, Points[i].color));

                if (LineCaps && i == Points.Count - 1)
                {
                    segments.Add(CreateLineCap(start, end, SegmentType.End, Points[i].color));
                }
            }
        }

        // Add the line segments to the vertex helper, creating any joins as needed
        for (var i = 0; i < segments.Count; i++)
        {
            if (!LineList && i < segments.Count - 1)
            {
                var vec1 = segments[i][1].position - segments[i][2].position;
                var vec2 = segments[i + 1][2].position - segments[i + 1][1].position;
                var angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;

                // Positive sign means the line is turning in a 'clockwise' direction
                var sign = Mathf.Sign(Vector3.Cross(vec1.normalized, vec2.normalized).z);

                // Calculate the miter point
                var miterDistance = LineThickness / (2 * Mathf.Tan(angle / 2));
                var miterPointA = segments[i][2].position - vec1.normalized * miterDistance * sign;
                var miterPointB = segments[i][3].position + vec1.normalized * miterDistance * sign;

                var joinType = LineJoins;
                if (joinType == JoinType.Miter)
                {
                    // Make sure we can make a miter join without too many artifacts.
                    if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_MITER_JOIN)
                    {
                        segments[i][2].position = miterPointA;
                        segments[i][3].position = miterPointB;
                        segments[i + 1][0].position = miterPointB;
                        segments[i + 1][1].position = miterPointA;
                    }
                    else
                    {
                        joinType = JoinType.Bevel;
                    }
                }

                if (joinType == JoinType.Bevel)
                {
                    if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_BEVEL_NICE_JOIN)
                    {
                        if (sign < 0)
                        {
                            segments[i][2].position = miterPointA;
                            segments[i + 1][1].position = miterPointA;
                        }
                        else
                        {
                            segments[i][3].position = miterPointB;
                            segments[i + 1][0].position = miterPointB;
                        }
                    }

                    var join = new UIVertex[] { segments[i][2], segments[i][3], segments[i + 1][0], segments[i + 1][1] };
                    vh.AddUIVertexQuad(join);
                }
            }
            vh.AddUIVertexQuad(segments[i]);
        }
    }

    private UIVertex[] CreateLineCap(Vector2 start, Vector2 end, SegmentType type, Color vertColor)
    {
        if (type == SegmentType.Start)
        {
            var capStart = start - ((end - start).normalized * LineThickness / 2);
            return CreateLineSegment(capStart, start, SegmentType.Start, vertColor);
        }
        else if (type == SegmentType.End)
        {
            var capEnd = end + ((end - start).normalized * LineThickness / 2);
            return CreateLineSegment(end, capEnd, SegmentType.End, vertColor);
        }

        Debug.LogError("Bad SegmentType passed in to CreateLineCap. Must be SegmentType.Start or SegmentType.End");
        return null;
    }

    private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end, SegmentType type, Color vertColor)
    {
        List<UIVertex> points = new List<UIVertex>();

        var uvs = middleUvs;
        if (type == SegmentType.Start)
            uvs = startUvs;
        else if (type == SegmentType.End)
            uvs = endUvs;

        Vector2 offset = new Vector2(start.y - end.y, end.x - start.x).normalized * LineThickness / 2;
        var v1 = start - offset;
        var v2 = start + offset;
        var v3 = end + offset;
        var v4 = end - offset;
        return SetVbo(new[] { v1, v2, v3, v4 }, uvs, vertColor);
    }

    protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs, Color vertColor)
    {
        UIVertex[] vbo = new UIVertex[4];
        for (int i = 0; i < vertices.Length; i++)
        {
            var vert = UIVertex.simpleVert;
            vert.color = vertColor;
            vert.position = vertices[i];
            vert.uv0 = uvs[i];
            vbo[i] = vert;
        }
        return vbo;
    }
}