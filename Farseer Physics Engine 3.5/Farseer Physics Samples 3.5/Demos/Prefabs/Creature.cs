using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.Samples.DrawingSystem;
using FarseerPhysics.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace FarseerPhysics.Samples.Demos.Prefabs
{
    public class Creature
    {
        private Body body;
        private List<Corner> corners;
        private Vertices verts;
        private Vertices massVerts;
        public Vector2 offset;
        public float defaultRotation;
        private float time;
        private int movementStep;

        private SpriteBatch batch;
        private Sprite sprite;

        public Creature(World world, List<Corner> _corners, ScreenManager screenManager, Vector2 position)
        {
            batch = screenManager.SpriteBatch;
            corners = _corners;
            verts = new Vertices();
            verts.Add(Vector2.Zero);
            foreach (Corner c in corners)
                verts.Add(c.vector);
            offset = Offset(verts);
            Vertices newVerts = new Vertices();
            foreach (Vector2 v in verts)
                newVerts.Add(v + offset);
            body = BodyFactory.CreatePolygon(world, newVerts, 0.05f);
            body.BodyType = BodyType.Dynamic;
            massVerts = new Vertices();
            foreach (Vector2 v in newVerts)
                massVerts.Add(v - body.LocalCenter);
            body.CollidesWith = Category.Cat1;
            body.CollisionCategories = Category.Cat2;
            body.LinearDamping = 0.4f;
            body.AngularDamping = 0.4f;
            /*body = BodyFactory.CreateBody(world, position);
            body.BodyType = BodyType.Dynamic;
            Shape shape = new PolygonShape(verts, 0.1f);
            fixture = body.CreateFixture(shape);//FixtureFactory.AttachPolygon(verts, 0.1f, body);*/
            foreach (Corner c in corners)
            {
                if (c.limb != null)
                {
                    Vector2 vecOff = c.vector + offset;
                    c.defAngle = (float)Math.Atan2(vecOff.Y, vecOff.X);
                    RevoluteJoint j = JointFactory.CreateRevoluteJoint(world, body, c.limb.body, vecOff, c.limb.offset);
                    c.joint = JointFactory.CreateAngleJoint(world, body, c.limb.body);
                    c.joint.MaxImpulse = 3f;
                    c.joint.TargetAngle = c.defAngle + c.movement[0];
                    c.joint.Softness = 0.5f;
                }
            }
            AssetCreator creator = screenManager.Assets;
            sprite = new Sprite(creator.TextureFromShape(body.FixtureList[0].Shape, MaterialType.Blank, Color.Blue, 1f));
        }

        public void Update(GameTime gameTime)
        {
            time += gameTime.ElapsedGameTime.Milliseconds;
            if (time > 500)
            {
                time = 0;
                if (movementStep == 3)
                    movementStep = 0;
                else
                    movementStep++;
            }
            Vector2 pointA;
            Vector2 pointB = Rotate(massVerts[massVerts.Count - 1], body.Rotation);
            Vector2 force = Vector2.Zero;
            foreach (Vector2 p in massVerts)
            {
                pointA = pointB;
                pointB = Rotate(p, body.Rotation);
                Vector2 side = pointB - pointA;
                Vector2 normal = new Vector2(side.Y, -side.X);
                Vector2 velocity = body.LinearVelocity;
                float velAngle = (float)Math.Atan2(velocity.Y, velocity.X);
                float normalAngle = (float)Math.Atan2(normal.Y, normal.X);
                float normalVelocity = (float)Math.Cos(velAngle - normalAngle) * velocity.Length();
                force += normal * normalVelocity * normalVelocity * 0.03f;
            }
            body.ApplyForce(force, body.LocalCenter);
            foreach (Corner c in corners)
            {
                if (c.limb != null)
                {
                    c.joint.TargetAngle = c.defAngle + c.movement[movementStep];
                    c.limb.Update(gameTime);
                }
            }
        }

        public void Draw()
        {
            batch.Draw(sprite.Texture, ConvertUnits.ToDisplayUnits(body.Position), null, Color.White, body.Rotation, sprite.Origin, 1f, SpriteEffects.None, 0f);
            foreach (Corner c in corners)
            {
                if (c.limb != null)
                {
                    c.limb.Draw();
                }
            }
        }

        public static Vector2 Offset(Vertices _verts)
        {
            float minX = 0;
            float maxX = 0;
            float minY = 0;
            float maxY = 0; 
            foreach (Vector2 v in _verts)
            {
                if (v.X < minX)
                    minX = v.X;
                if (v.X > maxX)
                    maxX = v.X;
                if (v.Y < minY)
                    minY = v.Y;
                if (v.Y > maxY)
                    maxY = v.Y;
            }
            return new Vector2(-((maxX + minX) / 2), -((maxY + minY) / 2));
            /*float dx = -((maxX + minX) / 2);
            float dy = -((maxY + minY) / 2);
            Vertices newVerts = new Vertices();
            foreach (Vector2 v in _verts)
            {
                newVerts.Add(new Vector2(v.X + dx, v.Y + dy));
            }
            return newVerts;*/
        }

        public static Vector2 Rotate(Vector2 vector, float angle)
        {
            float newX = vector.X * (float)Math.Cos(angle) - vector.Y * (float)Math.Sin(angle);
            float newY = vector.Y * (float)Math.Cos(angle) + vector.X * (float)Math.Sin(angle);
            return new Vector2(newX, newY);
        }
    }

    public class Corner
    {
        public float angle, radius, defAngle;
        public Creature limb;
        public float[] movement;
        public AngleJoint joint;
        public Vector2 vector;

        public Corner(float _angle, float _radius, Creature _limb, float[] _movement)
        {
            angle = _angle;
            radius = _radius;
            limb = _limb;
            movement = _movement;
            vector = new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
        }
    }
}
