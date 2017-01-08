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
    public class LiveCreature
    {
        public Creature creature;
        public List<LiveCorner> corners;
        public Body body;
        private Vertices verts;
        private Vertices massVerts;
        public Vector2 offset;
        private float time;
        private int movementStep;
        private SpriteBatch batch;
        private Sprite sprite;
        private World world;
        private ScreenManager screenManager;
        private Vector2 position;
        private float speedMed;
        private float strengthMed;
        private float rangeMed;

        public LiveCreature(Creature _creature, World _world, ScreenManager _screenManager, Vector2 _position, bool[] _medicine)
        {
            creature = _creature;
            world = _world;
            screenManager = _screenManager;
            position = _position;
            batch = screenManager.SpriteBatch;

            if (_medicine[0])
                speedMed = 50f;
            else
                speedMed = 100f;
            if (_medicine[1])
                strengthMed = -0.8f;
            else
                strengthMed = 0.8f;
            if (_medicine[2])
                rangeMed = 2f;
            else
                rangeMed = 1f;

            corners = new List<LiveCorner>();
            verts = new Vertices();
            verts.Add(Vector2.Zero);
            foreach (Corner c in creature.corners)
            {
                LiveCorner lc = new LiveCorner(c, world, screenManager, position, _medicine);
                corners.Add(lc);
                verts.Add(lc.vector);
            }
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
            foreach (LiveCorner c in corners)
            {
                if (c.limb != null)
                {
                    Vector2 vecOff = c.vector + offset;
                    c.defAngle = (float)Math.Atan2(vecOff.Y, vecOff.X);
                    RevoluteJoint j = JointFactory.CreateRevoluteJoint(world, body, c.limb.creature.body, vecOff, c.limb.creature.offset);
                    c.limb.joint = JointFactory.CreateAngleJoint(world, body, c.limb.creature.body);
                    c.limb.joint.MaxImpulse = 3f;
                    c.limb.joint.TargetAngle = c.defAngle + c.limb.movement[0] * rangeMed;
                    c.limb.joint.Softness = strengthMed;
                }
            }
            AssetCreator creator = screenManager.Assets;
            sprite = new Sprite(creator.TextureFromShape(body.FixtureList[0].Shape, MaterialType.Blank, Color.Blue, 1f));
        }

        public void Update(GameTime gameTime)
        {
            time += gameTime.ElapsedGameTime.Milliseconds;
            if (time > speedMed)
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
            foreach (LiveCorner c in corners)
            {
                if (c.limb != null)
                {
                    c.limb.joint.TargetAngle = c.defAngle + c.limb.movement[movementStep] * rangeMed;
                    c.limb.creature.Update(gameTime);
                }
            }
        }

        public void Draw()
        {
            batch.Draw(sprite.Texture, ConvertUnits.ToDisplayUnits(body.Position), null, Color.White, body.Rotation, sprite.Origin, 1f, SpriteEffects.None, 0f);
            foreach (LiveCorner c in corners)
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

        public void Kill()
        {
            foreach (LiveCorner c in corners)
            {
                if (c.limb != null)
                {
                    c.limb.creature.Kill();
                }
            }
            body.Dispose();
        }
    }

    public class LiveCorner
    {
        public Corner corner;
        public LiveLimb limb;
        public Vector2 vector;
        public float defAngle;

        public LiveCorner(Corner _corner, World _world, ScreenManager _screenManager, Vector2 _position, bool[] _medicine)
        {
            corner = _corner;
            if (corner.limb != null)
                limb = new LiveLimb(corner.limb, _world, _screenManager, _position, _medicine);
            vector = new Vector2((float)Math.Cos(corner.angle) * corner.radius, (float)Math.Sin(corner.angle) * corner.radius);
        }
    }

    public class LiveLimb
    {
        public Limb limb;
        public LiveCreature creature;
        public float[] movement;
        public AngleJoint joint;

        public LiveLimb(Limb _limb, World _world, ScreenManager _screenManager, Vector2 _position, bool[] _medicine)
        {
            limb = _limb;
            creature = new LiveCreature(limb.creature, _world, _screenManager, _position, _medicine);
            movement = limb.movement;
        }
    }

    public class Creature
    {
        public List<Corner> corners;

        public Creature(List<Corner> corners)
        {
            this.corners = corners;
        }

        public static Creature[] CreateFirstGen(World _world, ScreenManager _screenManager, Vector2 _position, Random r)
        {
            Creature[] firstGen = new Creature[100];
            for (int i = 0; i < 100; i++)
            {
                Creature creature = new Creature(RandomCorners(r));
                creature.Mutate(r, 0);
                firstGen[i] = creature;
            }
            return firstGen;
        }

        public static Creature[] CreateNewGen(Creature[] fittestCreatures, Random r)
        {
            Creature[] offspring = new Creature[100];
            for (int i = 0; i < 10; i += 2)
            {
                for (int j = 0; j < 20; j++)
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
                if (c.limb != null)
                    limbs.Add(c.limb);
            foreach (Corner c in dad.corners)
                if (c.limb != null)
                    limbs.Add(c.limb);
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
                        c.limb = limbs[r.Next(limbs.Count)];
                        break;
                    default:
                        break;
                }
            }
            child = child.Clone();
            child.Mutate(r, 0);
            return child;
        }

        public Creature Clone()
        {
            List<Corner> newCorners = new List<Corner>();
            foreach (Corner c in corners)
            {
                Limb newLimb = null;
                if (c.limb != null)
                {
                    Creature newCreature = c.limb.creature.Clone();
                    float[] newMovement = c.limb.movement;
                    newLimb = new Limb(newCreature, newMovement);
                }
                newCorners.Add(new Corner(c.angle, c.radius, newLimb));
            }
            Creature clone = new Creature(newCorners);
            return clone;
        }

        public void Mutate(Random r, int depth)
        {
            foreach (Corner c in corners)
            {
                if(r.Next(3) == 0)
                {
                    c.angle += (float)r.NextDouble() * 0.4f - 0.2f;
                    c.radius += (float)r.NextDouble() - 0.5f;
                }
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
                        c.limb = new Limb(new Creature(RandomCorners(r)), RandomMovement(r));
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
            return new Corner(angle, radius, null);
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
        public float angle, radius;
        public Limb limb;

        public Corner(float angle, float radius, Limb limb)
        {
            this.angle = angle;
            this.radius = radius;
            this.limb = limb;
        }
    }

    public class Limb
    {
        public Creature creature;
        public float[] movement;

        public Limb(Creature creature, float[] movement)
        {
            this.creature = creature;
            this.movement = movement;
        }
    }
}
