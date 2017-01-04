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
        private World world;
        private Body body;
        private List<Corner> corners;
        private Vertices verts;

        public Creature(World _world, List<Corner> _corners, ScreenManager screenManager, Vector2 position)
        {
            world = _world;
            corners = _corners;
            verts = new Vertices();
            verts.Add(Vector2.Zero);
            foreach (Corner c in corners)
            {
                verts.Add(c.GetVector());
            }
            body = BodyFactory.CreatePolygon(world, verts, 0.1f, position);
            body.BodyType = BodyType.Dynamic;
            foreach (Corner c in corners)
            {
                if (c.limb != null)
                {
                    JointFactory.CreateRevoluteJoint(world, body, c.limb.body, c.GetVector(), Vector2.Zero);
                    c.joint = JointFactory.CreateAngleJoint(world, body, c.limb.body);
                }
            }
        }
        
        public void Draw()
        {

        }
    }

    public class Corner
    {
        private float angle, radius;
        public Creature limb;
        public Movement move;
        public AngleJoint joint;

        public Corner(float _angle, float _radius, Creature _limb)
        {
            angle = _angle;
            radius = _radius;
            limb = _limb;
        }

        public Vector2 GetVector()
        {
            return new Vector2((float) Math.Cos(angle) * radius, (float) Math.Sin(angle) * radius);
        }
    }

    public class Movement
    {

    }
}
