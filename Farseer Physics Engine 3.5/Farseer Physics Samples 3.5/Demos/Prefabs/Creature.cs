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
using System.Diagnostics;

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
        private World world;
        private ScreenManager screenManager;
        private Vector2 position;

        public Creature(World _world, List<Corner> _corners, ScreenManager _screenManager, Vector2 _position)
        {
            world = _world;
            screenManager = _screenManager;
            position = _position;
            batch = screenManager.SpriteBatch;
            corners = _corners;
            verts = new Vertices();
            verts.Add(Vector2.Zero);
            foreach (Corner c in corners)
                verts.Add(c.vector);
            offset = Offset(verts);
            Vertices newVerts = new Vertices();
            foreach (Vector2 v in verts)
            {
                newVerts.Add(v + offset);
            }
            body = BodyFactory.CreatePolygon(world, newVerts, 0.05f);
            body.BodyType = BodyType.Dynamic;
            massVerts = new Vertices();
            foreach (Vector2 v in newVerts)
            {
                massVerts.Add(v - body.LocalCenter);
            }
            body.CollidesWith = Category.Cat1;
            body.CollisionCategories = Category.Cat2;
            foreach (Corner c in corners)
            {
                if (c.limb != null)
                {
                    Vector2 vecOff = c.vector + offset;
                    c.defAngle = (float)Math.Atan2(vecOff.Y, vecOff.X);
                    RevoluteJoint j = JointFactory.CreateRevoluteJoint(world, body, c.limb.creature.body, vecOff, c.limb.creature.offset);
                    c.limb.joint = JointFactory.CreateAngleJoint(world, body, c.limb.creature.body);
                    c.limb.joint.MaxImpulse = 3f;
                    c.limb.joint.TargetAngle = c.defAngle + c.limb.movement[0];
                    c.limb.joint.Softness = 0.5f;
                }
            }
            AssetCreator creator = screenManager.Assets;
            sprite = new Sprite(creator.TextureFromShape(body.FixtureList[0].Shape, MaterialType.Blank, Color.Blue, 1f));
        }

        public void Update(GameTime gameTime)
        {
            time += gameTime.ElapsedGameTime.Milliseconds;
            if (time > 200)
            {
                time = 0;
                if (movementStep == 7)
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
                float normalAngle = (float)Math.Atan2(-normal.Y, -normal.X);
                float normalVelocity = (float)Math.Cos(velAngle - normalAngle) * velocity.Length();
                body.ApplyForce(body.AngularVelocity * new Vector2(pointA.Y, -pointA.X) * 0.5f, body.Position + pointA);
                if (normalVelocity > 0)
                    force += normal * normalVelocity * normalVelocity * 0.01f;
            }
            body.ApplyForce(force);
            foreach (Corner c in corners)
            {
                if (c.limb != null)
                {
                    c.limb.joint.TargetAngle = c.defAngle + c.limb.movement[movementStep];
                    c.limb.creature.Update(gameTime);
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
                    c.limb.creature.Draw();
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
        }

        public static Vector2 Rotate(Vector2 vector, float angle)
        {
            float newX = vector.X * (float)Math.Cos(angle) - vector.Y * (float)Math.Sin(angle);
            float newY = vector.Y * (float)Math.Cos(angle) + vector.X * (float)Math.Sin(angle);
            return new Vector2(newX, newY);
        }

        public static Creature[] CreateFirstGen(World _world, ScreenManager _screenManager, Vector2 _position, Random r)
        {
            Creature[] firstGen = new Creature[36];
            for (int i = 0; i < 36; i++)
            {
                Creature creature = new Creature(_world, RandomCorners(r), _screenManager, _position);
                creature.Mutate(r, 0);
                firstGen[i] = creature.Clone();
            }
            return firstGen;
        }

        public static Creature[] CreateNewGen(Creature[] fittestCreatures, Random r)
        {
            Creature[] offspring = new Creature[36];
            for (int i = 0; i < 6; i += 2)
            {
                for (int j = 0; j < 12; j++)
                {
                    offspring[j + 20 * i] = CreateOffspring(fittestCreatures[i], fittestCreatures[i + 1], r);
                }
            }
            return offspring;
        }

        public static Creature CreateOffspring(Creature mom, Creature dad, Random r)
        {
            List<Limb> limbs = new List<Limb>();
            foreach (Corner c in mom.corners)
            {
                if (c.limb != null)
                {
                    limbs.Add(c.limb);
                }
            }
            foreach (Corner c in dad.corners)
            {
                if (c.limb != null)
                {
                    limbs.Add(c.limb);
                }
            }
            Creature child;
            if (r.Next(2) == 0)
                child = mom;
            else
                child = dad;
            foreach (Corner c in child.corners)
            {
                switch (r.Next(6))
                {
                    case 0:
                        c.limb = null;
                        break;
                    case 1:
                        c.AddLimb(limbs[r.Next(limbs.Count)]);
                        break;
                    default:
                        break;
                }
            }
            child.Mutate(r, 0);
            return child.Clone();
        }

        public Creature Clone()
        {
            List<Corner> newCorners = new List<Corner>();
            foreach (Corner c in corners)
            {
                Creature newLimb = null;
                float[] newMovement = null;
                if (c.limb != null)
                {
                    newLimb = c.limb.creature.Clone();
                    newMovement = c.limb.movement;
                }
                newCorners.Add(new Corner(c.angle, c.radius, newLimb, newMovement));
            }
            Creature clone = new Creature(world, newCorners, screenManager, position);
            return clone;
        }

        public void Mutate(Random r, int depth)
        {
            foreach (Corner c in corners)
            {
                c.angle += (float)r.NextDouble() * 0.4f - 0.2f;
                c.radius += (float)r.NextDouble() - 0.5f;
                if (c.limb != null)
                {
                    c.limb.creature.Mutate(r, depth + 1);
                    float[] newMovement = new float[8];
                    c.limb.movement.CopyTo(newMovement, 0);
                    for (int i = 0; i < r.Next(9); i++)
                    {
                        int step = r.Next(8);
                        c.limb.movement[step] += (float)r.NextDouble() * 0.4f - 0.2f;
                    }
                    float overallChange = (float)r.NextDouble() * 1.2f - 0.6f;
                    for (int i = 0; i < r.Next(2) * 8; i++)
                    {
                        c.limb.movement[i] += overallChange;
                        if (c.limb.movement[i] < -(float)Math.PI / 2)
                            c.limb.movement[i] = -(float)Math.PI / 2;
                        if (c.limb.movement[i] > (float)Math.PI / 2)
                            c.limb.movement[i] = (float)Math.PI / 2;
                    }
                    if (r.Next(3) == 0)
                    {
                        for (int i = 0; i < 8; i++)
                            c.limb.movement[i] *= -1;
                    }
                    if (r.Next(3) == 0)
                    {
                        int sign = r.Next(2) * 2 - 1;
                        float first = c.limb.movement[0];
                        for (int i = 0; i != (8 - sign) % 8; i = (8 + i + sign) % 8)
                        {
                            c.limb.movement[i] = c.limb.movement[(8 + i + sign) % 8];
                        }
                        c.limb.movement[(8 - sign) % 8] = first;
                    }
                }
                else
                {
                    if (r.Next(40) == 0 && depth < 5)
                    {
                        c.limb = new Limb(new Creature(world, RandomCorners(r), screenManager, position), RandomMovement(r));
                    }
                }
            }
            if (corners.Count < 5 && r.Next(10) == 0)
                corners.Add(RandomCorner(r));
            CheckCorners(corners);
        }

        public static float[] RandomMovement(Random r)
        {
            float[] movement = new float[8];
            for (int i = 0; i < 8; i++)
                movement[i] = (float)r.NextDouble() * (float)Math.PI - (float)Math.PI / 2;
            return movement;
        }

        public static List<Corner> RandomCorners(Random r)
        {
            List<Corner> corners = new List<Corner>();
            for (int i = 0; i < r.Next(2, 5); i++)
                corners.Add(RandomCorner(r));
            return CheckCorners(corners);
        }

        public static Corner RandomCorner(Random r)
        {
            float angle = (float)r.NextDouble() * (float)Math.PI - (float)Math.PI / 2;
            float radius = (float)r.NextDouble() * 6f + 1f;
            return new Corner(angle, radius, null, null);
        }

        public static List<Corner> CheckCorners(List<Corner> _corners)
        {
            _corners.Sort((x, y) => x.angle.CompareTo(y.angle));
            float lastAngle = -(float)Math.PI / 2 - 0.2f;
            foreach (Corner c in _corners)
            {
                if (c.angle - lastAngle < 0.2f)
                    c.angle = lastAngle + 0.2f;
                if (c.angle > (float)Math.PI / 2)
                    c.angle = (float)Math.PI / 2;
                lastAngle = c.angle;
                if (c.radius < 1f)
                    c.radius = 1f;
                if (c.radius > 10f)
                    c.radius = 10f;
                if (c.limb != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (c.limb.movement[i] < -(float)Math.PI / 2)
                            c.limb.movement[i] = -(float)Math.PI / 2;
                        if (c.limb.movement[i] > (float)Math.PI / 2)
                            c.limb.movement[i] = (float)Math.PI / 2;
                    }
                }
            }
            return _corners;
        }
    }

    public class Corner
    {
        public float angle, radius, defAngle;
        public Limb limb;
        public Vector2 vector;

        public Corner(float _angle, float _radius, Creature _limb, float[] _movement)
        {
            angle = _angle;
            radius = _radius;
            if (_limb != null)
                limb = new Limb(_limb, _movement);
            vector = new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
        }

        public void AddLimb(Limb _limb)
        {
            Debug.WriteLine(_limb.movement.ToString());
            if (_limb != null)
                limb = new Limb(_limb.creature.Clone(), _limb.movement);
        }
    }

    public class Limb
    {
        public Creature creature;
        public float[] movement;
        public AngleJoint joint;

        public Limb(Creature _creature, float[] _movement)
        {
            creature = _creature;
            movement = _movement;
        }
    }
}
