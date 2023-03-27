namespace TerraAngel.Utility;

public static class CollisionUtil
{
    public static bool TileSolid(this Tile tile)
    {
        if (!tile.active() || tile.inActive())
        {
            return false;
        }

        if (!Main.tileSolid[tile.type] || Main.tileSolidTop[tile.type])
        {
            return false;
        }

        return true;
    }
    public static bool TileSlope(this Tile tile)
    {
        if (!tile.active() || tile.inActive())
        {
            return false;
        }

        if (tile.slope() != 0 || tile.halfBrick())
        {
            return true;
        }

        return false;
    }

    public static bool LineIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 hit, out Vector2 normal)
    {
        normal = new Vector2((p4.Y - p3.Y), -(p4.X - p3.X)).Normalized();
        float ua, ub, denom = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
        if (denom == 0)
        {
            hit = Vector2.Zero;
            return false;
        }
        ua = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / denom;
        ub = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / denom;
        hit = new Vector2(p1.X + ua * (p2.X - p1.X), p1.Y + ua * (p2.Y - p1.Y));
        return (ua >= 0 && ua <= 1) && (ub >= 0 && ub <= 1);
    }
    public static bool LineRect(Vector2 p1, Vector2 p2, Vector2 rmin, Vector2 rmax, out Vector2 hit, out Vector2 hitNormal)
    {
        return LineRect(p1, p2, rmin, new Vector2(rmax.X, rmin.Y), new Vector2(rmin.X, rmax.Y), rmax, out hit, out hitNormal);
    }


    /*
     * 
     * r1         r2
     * |----------|
     * |          |
     * |          |
     * |          |
     * |          |
     * |----------|
     * r3         r4
     * 
     * 
     * 
     * 
     */
    public static bool LineRect(Vector2 p1, Vector2 p2, Vector2 r1, Vector2 r2, Vector2 r3, Vector2 r4, out Vector2 hit, out Vector2 hitNormal)
    {
        return LinePolyN(p1, p2, out hit, out hitNormal, r1, r4, r3, r1, r2, r4);
    }
    public static bool LineTri(Vector2 p1, Vector2 p2, Vector2 t1, Vector2 t2, Vector2 t3, out Vector2 hit, out Vector2 hitNormal)
    {
        return LinePolyN(p1, p2, out hit, out hitNormal, t1, t2, t3);
    }

    public static bool LinePolyN(Vector2 p1, Vector2 p2, out Vector2 hit, out Vector2 hitNormal, params Vector2[] points)
    {
        int pointCount = points.Length;
        bool hitAtAll = false;
        float minDist = float.MaxValue;
        Vector2 minHitPoint = p1;
        Vector2 minHitNormal = Vector2.Zero;
        for (int i = 0; i < pointCount; i++)
        {
            bool didHit = LineIntersect(p1, p2, points[i], points[(i + 1) % pointCount], out Vector2 point, out Vector2 normal);

            if (didHit)
            {
                hitAtAll = true;
                float dist = p1.DistanceSQ(point);
                if (dist < minDist)
                {
                    minDist = dist;
                    minHitPoint = point;
                    minHitNormal = normal;
                }
            }
        }

        hit = minHitPoint;
        hitNormal = minHitNormal;

        return hitAtAll;
    }
}
=