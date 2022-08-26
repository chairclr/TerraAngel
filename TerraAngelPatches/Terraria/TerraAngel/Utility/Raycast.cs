using System;

namespace TerraAngel.Utility
{
    public struct RaycastData
    {
        public readonly bool Hit;
        public readonly float Distance;
        public readonly float MaxDistance;
        public readonly bool HitMaxDistance;
        public readonly Vector2 HitNormal;
        public readonly Vector2 Origin;
        public readonly Vector2 Direction;
        public readonly Vector2 End;

        public RaycastData()
        {
            Hit = false;
            Distance = 0f;
            MaxDistance = 0f;
            HitMaxDistance = false;
            Origin = Vector2.Zero;
            Direction = Vector2.Zero;
            End = Vector2.Zero;
            HitNormal = Vector2.Zero;
        }
        public RaycastData(bool hit, float distance, float maxDistance, Vector2 origin, Vector2 direction, Vector2 hitNormal)
        {
            Hit = hit;
            Distance = distance;
            MaxDistance = maxDistance;
            Origin = origin;
            Direction = direction;
            HitNormal = hitNormal;
            End = Origin + Direction * Distance;
            HitMaxDistance = Distance >= MaxDistance;
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


        public static RaycastData Cast(Vector2 position, Vector2 direction, float maxDistance)
        {
            Vector2 unitRayStepSize = new Vector2(MathF.Sqrt(1f + (direction.Y / direction.X) * (direction.Y / direction.X)), MathF.Sqrt(1f + (direction.X / direction.Y) * (direction.X / direction.Y)));
            Vector2 originalPosition = position;
            Vector2i tilePoint = position / 16f;


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
            Vector2 hitNormal = Vector2.Zero;

            while (!intersects && distance < maxDistance)
            {


                // step the ray forward
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
                // collision checks
                if (Main.tile.InWorld(tilePoint))
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
                            Vector2 nextPoint = currentPoint + direction * 2f;
                            Vector2 rectMin = (Vector2)tilePoint + new Vector2(0f, 0.5f);
                            Vector2 rectMax = (Vector2)tilePoint + new Vector2(1f, 1f);

                            if (CollisionUtil.LineRect(currentPoint, nextPoint, rectMin, rectMax, out Vector2 point, out Vector2 normal))
                            {
                                intersects = true;
                                distance = position.Distance(point);
                                hitNormal = normal;
                            }
                        }
                        else if (slope > 0)
                        {
                            Vector2 currentPoint = position + direction * distance;
                            Vector2 nextPoint = currentPoint + direction * 2f;

                            Vector2 r1 = (Vector2)tilePoint + slopeOffsets[(slope - 1) * 3 + 0];
                            Vector2 r2 = (Vector2)tilePoint + slopeOffsets[(slope - 1) * 3 + 1];
                            Vector2 r3 = (Vector2)tilePoint + slopeOffsets[(slope - 1) * 3 + 2];

                            if (CollisionUtil.LineTri(currentPoint, nextPoint, r1, r2, r3, out Vector2 point, out Vector2 normal))
                            {
                                intersects = true;
                                distance = position.Distance(point);
                                hitNormal = normal;
                            }
                        }
                        else
                        {
                            intersects = true;

                            Vector2 currentPoint = position + direction * distance;
                            Vector2 nextPoint = currentPoint + direction * 2f;
                            Vector2 rectMin = (Vector2)tilePoint;
                            Vector2 rectMax = (Vector2)tilePoint + new Vector2(1f, 1f);

                            CollisionUtil.LineRect(currentPoint, nextPoint, rectMin, rectMax, out Vector2 point, out hitNormal);
                        }
                    }
                }
                else
                {
                    intersects = true;
                }
            }

            if (distance > maxDistance)
            {
                intersects = false;
                distance = maxDistance;
            }
            distance *= 16f;
            return new RaycastData(intersects, distance, maxDistance, originalPosition, direction, hitNormal);
        }

        /// <summary>
        /// Same as normal Raycast.Cast, but it does not perform advanced slope checking or produce a normal vector
        /// </summary>
        public static RaycastData CastNoSlopeCheck(Vector2 position, Vector2 direction, float maxDistance)
        {
            Vector2 unitRayStepSize = new Vector2(MathF.Sqrt(1f + (direction.Y / direction.X) * (direction.Y / direction.X)), MathF.Sqrt(1f + (direction.X / direction.Y) * (direction.X / direction.Y)));
            Vector2 originalPosition = position;
            Vector2i tilePoint = position / 16f;
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

                if (Main.tile.InWorld(tilePoint))
                {
                    Tile tile = Main.tile[tilePoint];
                    if (tile.TileSolid())
                    {
                        intersects = true;
                    }
                }
                else
                {
                    intersects = true;
                }
            }

            if (distance > maxDistance)
            {
                intersects = false;
                distance = maxDistance;
            }
            distance *= 16f;
            return new RaycastData(intersects, distance, maxDistance, originalPosition, direction, Vector2.Zero);
        }
    }
}
