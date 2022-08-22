using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Terraria;
using static Terraria.WorldBuilding.Searches;

namespace TerraAngel.Utility
{
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

        public static bool LineIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 hit)
        {
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
        public static bool LineRect(Vector2 p1, Vector2 p2, Vector2 rmin, Vector2 rmax, out Vector2 hit)
        {
            return LineRect(p1, p2, rmin, new Vector2(rmax.X, rmin.Y), new Vector2(rmin.X, rmax.Y), rmax, out hit);
        }
        public static bool LineRect(Vector2 p1, Vector2 p2, Vector2 r1, Vector2 r2, Vector2 r3, Vector2 r4, out Vector2 hit)
        {
            return LinePolyN(p1, p2, out hit, r1, r2, r1, r3, r2, r4, r3, r4);
        }
        public static bool LineTri(Vector2 p1, Vector2 p2, Vector2 t1, Vector2 t2, Vector2 t3, out Vector2 hit)
        {
            return LinePolyN(p1, p2, out hit, t1, t2, t3);
        }

        public static bool LinePolyN(Vector2 p1, Vector2 p2, out Vector2 hit, params Vector2[] points)
        {
            int pointCount = points.Length;
            bool hitAtAll = false;
            float minDist = float.MaxValue;
            Vector2 minHitPoint = p1;
            for (int i = 0; i < pointCount; i++)
            {
                bool didHit = LineIntersect(p1, p2, points[i], points[(i + 1) % pointCount], out Vector2 point);

                if (didHit)
                {
                    hitAtAll = true;
                    float dist = p1.DistanceSQ(point);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minHitPoint = point;
                    }
                }
            }

            hit = minHitPoint;

            return hitAtAll;
        }
    }
}
