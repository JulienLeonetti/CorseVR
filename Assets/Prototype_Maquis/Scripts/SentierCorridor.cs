using UnityEngine;

namespace CorseVR
{
    [DisallowMultipleComponent]
    public sealed class SentierCorridor : MonoBehaviour
    {
        [SerializeField] private Vector3[] centreLine;
        [SerializeField, Min(.1f)] private float halfWidth = .68f;
        [SerializeField] private float[] sectionHalfWidths;
        [SerializeField] private Transform startRoom;
        [SerializeField] private Vector2 roomHalfSize;
        [SerializeField] private Vector2 roomCentre;
        public float HalfWidth { get { return halfWidth; } }
        public Vector3[] CentreLine { get { return (Vector3[])centreLine.Clone(); } }
        public void ConfigureStartRoom(Transform room, Vector2 centre, Vector2 halfSize)
        {
            startRoom = room; roomCentre = centre; roomHalfSize = halfSize;
        }

        public void Configure(Vector3[] points, float width)
        {
            centreLine = (Vector3[])points.Clone();
            halfWidth = Mathf.Max(.1f, width);
            sectionHalfWidths = null;
        }

        public void ConfigureWidths(float[] widths)
        {
            if (widths == null || centreLine == null || widths.Length != centreLine.Length)
            { sectionHalfWidths = null; return; }
            sectionHalfWidths = (float[])widths.Clone();
        }

        public Vector3 ConstrainPosition(Vector3 proposed)
        {
            if (centreLine == null || centreLine.Length < 2) return proposed;
            Vector2 p = new Vector2(proposed.x, proposed.z);
            Vector2 closest = p;
            float best = float.PositiveInfinity;
            float availableWidth = halfWidth;
            for (int i = 1; i < centreLine.Length; i++)
            {
                Vector2 a = new Vector2(centreLine[i - 1].x, centreLine[i - 1].z);
                Vector2 b = new Vector2(centreLine[i].x, centreLine[i].z);
                Vector2 ab = b - a;
                float t = ab.sqrMagnitude > .000001f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
                Vector2 q = a + ab * t;
                float d = (p - q).sqrMagnitude;
                if (d < best) {
                    best = d; closest = q;
                    availableWidth = sectionHalfWidths != null && sectionHalfWidths.Length == centreLine.Length
                        ? Mathf.Max(.1f, Mathf.Lerp(sectionHalfWidths[i-1], sectionHalfWidths[i], t)) : halfWidth;
                }
            }
            Vector2 limited = closest + Vector2.ClampMagnitude(p - closest, availableWidth);
            if (startRoom != null)
            {
                Vector3 local = startRoom.InverseTransformPoint(proposed);
                local.x = Mathf.Clamp(local.x, roomCentre.x-roomHalfSize.x, roomCentre.x+roomHalfSize.x);
                local.z = Mathf.Clamp(local.z, roomCentre.y-roomHalfSize.y, roomCentre.y+roomHalfSize.y);
                Vector3 roomWorld = startRoom.TransformPoint(local);
                Vector2 roomPoint = new Vector2(roomWorld.x, roomWorld.z);
                if ((roomPoint-p).sqrMagnitude < (limited-p).sqrMagnitude) limited = roomPoint;
            }
            return new Vector3(limited.x, proposed.y, limited.y);
        }
    }
}
