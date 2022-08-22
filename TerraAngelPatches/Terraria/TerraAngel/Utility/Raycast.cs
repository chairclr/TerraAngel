using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static RaycastData Cast(Vector2 position, Vector2 direction, float maxDistance, bool intersectWithBounds = true)
        {
            Vector2 unitRayStepSize = new Vector2((float)Math.Sqrt(1f + (direction.Y / direction.X) * (direction.Y / direction.X)), (float)Math.Sqrt(1f + (direction.X / direction.Y) * (direction.X / direction.Y)));
            Vector2 originalPosition = position;

            Vector2 rayLength1d;
            Vector2 rayStep;

            if (direction.X < 0)
            {
                rayStep.X = -1;
                rayLength1d.X = (position.X - position.X) * unitRayStepSize.X;
            }
            else
            {
                rayStep.X = 1;
                rayLength1d.X = (position.X + 1f - position.X) * unitRayStepSize.X;
            }

            if (direction.Y < 0)
            {
                rayStep.Y = -1;
                rayLength1d.Y = (position.Y - position.Y) * unitRayStepSize.Y;
            }
            else
            {
                rayStep.Y = 1;
                rayLength1d.Y = (position.Y + 1 - position.Y) * unitRayStepSize.Y;
            }

            int i = 0;
            bool intersects = false;
            float distance = 0.0f;
            while (!intersects && distance < maxDistance)
            {
                i++;
                Vector2 previousPosition = position;
                if (rayLength1d.X < rayLength1d.Y)
                {
                    position.X += rayStep.X;
                    distance = rayLength1d.X;
                    rayLength1d.X += unitRayStepSize.X;
                }
                else
                {
                    position.Y += rayStep.Y;
                    distance = rayLength1d.Y;
                    rayLength1d.Y += unitRayStepSize.Y;
                }


                Vector2 velocity = position - previousPosition;
                Vector2 tileVelocity = Collision.TileCollision(position, velocity, 1, 1);
                if (tileVelocity != velocity)
                    intersects = true;

                Vector4 slopeVelocity = Collision.SlopeCollision(position, velocity, 1, 1, 0f, fall: true);

                if (position.X != slopeVelocity.X || position.Y != slopeVelocity.Y ||
                    velocity.X != slopeVelocity.Z || velocity.Y != slopeVelocity.W)
                {
                    intersects = true;
                }
            }

            Vector2 intersection = originalPosition + direction * distance;

            return new RaycastData(intersects, distance, originalPosition, intersection, direction);
        }

        /// <summary>
        /// Is 16x faster but 16x less accurate
        /// </summary>
        public static RaycastData CastFast(Vector2 position, Vector2 direction, float maxDistance)
        {
            Vector2 unitRayStepSize = new Vector2((float)Math.Sqrt(1f + (direction.Y / direction.X) * (direction.Y / direction.X)), (float)Math.Sqrt(1f + (direction.X / direction.Y) * (direction.X / direction.Y)));
            Vector2 originalPosition = position;

            position /= 16f;

            Vector2 rayLength1d;
            Vector2 rayStep;

            if (direction.X < 0)
            {
                rayStep.X = -1;
                rayLength1d.X = (position.X - position.X) * unitRayStepSize.X;
            }
            else
            {
                rayStep.X = 1;
                rayLength1d.X = (position.X + 1f - position.X) * unitRayStepSize.X;
            }

            if (direction.Y < 0)
            {
                rayStep.Y = -1;
                rayLength1d.Y = (position.Y - position.Y) * unitRayStepSize.Y;
            }
            else
            {
                rayStep.Y = 1;
                rayLength1d.Y = (position.Y + 1 - position.Y) * unitRayStepSize.Y;
            }

            bool intersects = false;
            float distance = 0.0f;
            while (!intersects && (distance * 16f) < maxDistance)
            {
                Vector2 previousPosition = position;
                if (rayLength1d.X < rayLength1d.Y)
                {
                    position.X += rayStep.X;
                    distance = rayLength1d.X;
                    rayLength1d.X += unitRayStepSize.X;
                }
                else
                {
                    position.Y += rayStep.Y;
                    distance = rayLength1d.Y;
                    rayLength1d.Y += unitRayStepSize.Y;
                }

                Vector2 velocity = (position - previousPosition) * 16f;
                Vector2 tileVelocity = Collision.TileCollision(position * 16f, velocity, 1, 1);
                if (tileVelocity != velocity)
                    intersects = true;

                Vector4 slopeVelocity = Collision.SlopeCollision(position * 16f, velocity, 1, 1, 0f, fall: true);

                if (position.X * 16f != slopeVelocity.X || position.Y * 16f != slopeVelocity.Y ||
                    velocity.X != slopeVelocity.Z || velocity.Y != slopeVelocity.W)
                {
                    intersects = true;
                }
            }

            distance *= 16f;
            Vector2 intersection = originalPosition + direction * distance;

            return new RaycastData(intersects, distance, originalPosition, intersection, direction);
        }
    }
}
