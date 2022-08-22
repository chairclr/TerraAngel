using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Terraria;

namespace TerraAngel.Utility
{
    public struct RaycastData
    {
        public bool Intersects;
        public float Distance;
        public Vector2 Origin;
        public Vector2 IntersectionPoint;
        public Vector2 Direction;

        public RaycastData(bool intersects, float distance, Vector2 origin, Vector2 intersectionPoint, Vector2 direction)
        {
            Intersects = intersects;
            Distance = distance;
            Origin = origin;
            IntersectionPoint = intersectionPoint;
            Direction = direction;
        }
    }

    public class Raycast
    {
        private static readonly Vector2[] slopeOffsets = new Vector2[] 
        {
            // down left
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), 

            // down right
            new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(1f, 1f), 

            // up left
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), 

            // up right
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f),
        };
        public static RaycastData Cast(Vector2 position, Vector2 direction, float maxDistance, bool intersectWithBounds = true)
        {
            Vector2 unitRayStepSize = new Vector2((float)Math.Sqrt(1f + (direction.Y / direction.X) * (direction.Y / direction.X)), (float)Math.Sqrt(1f + (direction.X / direction.Y) * (direction.X / direction.Y)));
            Vector2 originalPosition = position;
            Vector2i tilePoint = (position / 16f);
            maxDistance /= 16f;
            position /= 16f;

            Vector2 rayLength1d;
            Vector2i rayStep;

            if (direction.X < 0)
            {
                rayStep.X = -1;
                rayLength1d.X = (position.X - tilePoint.X) * unitRayStepSize.X;
            }
            else
            {
                rayStep.X = 1;
                rayLength1d.X = (tilePoint.X + 1f - position.X) * unitRayStepSize.X;
            }

            if (direction.Y < 0)
            {
                rayStep.Y = -1;
                rayLength1d.Y = (position.Y - tilePoint.Y) * unitRayStepSize.Y;
            }
            else
            {
                rayStep.Y = 1;
                rayLength1d.Y = (tilePoint.Y + 1 - position.Y) * unitRayStepSize.Y;
            }

            bool intersects = false;
            float distance = 0.0f;
            while (!intersects && distance < maxDistance)
            {
                if (rayLength1d.X < rayLength1d.Y)
                {
                    tilePoint.X += rayStep.X;
                    distance = rayLength1d.X;
                    rayLength1d.X += unitRayStepSize.X;
                }
                else
                {
                    tilePoint.Y += rayStep.Y;
                    distance = rayLength1d.Y;
                    rayLength1d.Y += unitRayStepSize.Y;
                }

                if (!Main.tile.InWorld(tilePoint))
                {
                    intersects = true;
                }
                else
                {
                    Tile tile = Main.tile[tilePoint];

                    bool solid = tile.TileSolid();
                    bool halfBrick = tile.halfBrick();
                    int slope = tile.slope();

                    if (solid)
                    {
                        if (halfBrick)
                        {
                            Vector2 currentPoint = position + direction * distance;
                            Vector2 nextPoint = currentPoint + direction * new Vector2(1f);
                            Vector2 rectMin = (Vector2)tilePoint + new Vector2(0f, 0.5f);
                            Vector2 rectMax = (Vector2)tilePoint + new Vector2(1f, 1f);

                            if (CollisionUtil.LineRect(currentPoint, nextPoint, rectMin, rectMax, out Vector2 point))
                            {
                                intersects = true;
                                distance = position.Distance(point);
                            }
                        }
                        else if (slope > 0)
                        {
                            Vector2 currentPoint = position + direction * distance;
                            Vector2 nextPoint = currentPoint + direction * new Vector2(1f);

                            Vector2 r1 = (Vector2)tilePoint + slopeOffsets[(slope - 1) * 3 + 0];
                            Vector2 r2 = (Vector2)tilePoint + slopeOffsets[(slope - 1) * 3 + 1];
                            Vector2 r3 = (Vector2)tilePoint + slopeOffsets[(slope - 1) * 3 + 2];

                            if (CollisionUtil.LineTri(currentPoint, nextPoint, r1, r2, r3, out Vector2 point))
                            {
                                intersects = true;
                                distance = position.Distance(point);
                            }
                        }
                        else
                        {
                            intersects = true;
                        }
                    }
                }
            }

            if (distance > maxDistance)
            {
                intersects = false;
                distance *= (maxDistance / distance);
            }
            distance *= 16f;
            Vector2 intersectionPoint = originalPosition + direction * distance;



            return new RaycastData(intersects, distance, originalPosition, intersectionPoint, direction);
        }
    }
}
