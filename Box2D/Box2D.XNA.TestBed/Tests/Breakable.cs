﻿/*
* Box2D.XNA port of Box2D:
* Copyright (c) 2009 Brandon Furtwangler, Nathan Furtwangler
*
* Original source Box2D:
* Copyright (c) 2006-2009 Erin Catto http://www.gphysics.com 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using Box2D.XNA.TestBed.Framework;
using Microsoft.Xna.Framework;

namespace Box2D.XNA.TestBed.Tests
{
    public class Breakable : Test
    {
        static int e_count = 7;

        public Breakable()
        {
            // Ground body
            {
                BodyDef bd = new BodyDef();
                Body ground = _world.CreateBody(bd);

                PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
                ground.CreateFixture(shape, 0.0f);
            }

            // Breakable dynamic body
            {
                BodyDef bd = new BodyDef();
                bd.type = BodyType.Dynamic;
                bd.position = new Vector2(0.0f, 40.0f);
                bd.angle = 0.25f * Settings.b2_pi;
                _body1 = _world.CreateBody(bd);

                _shape1 = new PolygonShape();
                _shape1.SetAsBox(0.5f, 0.5f, new Vector2(-0.5f, 0.0f), 0.0f);
                _piece1 = _body1.CreateFixture(_shape1, 1.0f);

                _shape2 = new PolygonShape();
                _shape2.SetAsBox(0.5f, 0.5f, new Vector2(0.5f, 0.0f), 0.0f);
                _piece2 = _body1.CreateFixture(_shape2, 1.0f);
            }

            _break = false;
            _broke = false;
        }

        public override void PostSolve(Contact contact, ref ContactImpulse impulse)
	    {
		    if (_broke)
		    {
			    // The body already broke.
			    return;
		    }

		    // Should the body break?
            Manifold manifold;
            contact.GetManifold(out manifold);
            int count = manifold._pointCount;

		    float maxImpulse = 0.0f;
		    for (int i = 0; i < count; ++i)
		    {
			    maxImpulse = Math.Max(maxImpulse, impulse.normalImpulses[i]);
		    }

		    if (maxImpulse > 40.0f)
		    {
			    // Flag the body for breaking.
			    _break = true;
		    }
	    }

        void Break()
        {
            // Create two bodies from one.
            Body body1 = _piece1.GetBody();
            Vector2 center = body1.GetWorldCenter();

            body1.DestroyFixture(_piece2);
            _piece2 = null;

            BodyDef bd = new BodyDef();
            bd.type = BodyType.Dynamic;
            bd.position = body1.GetPosition();
            bd.angle = body1.GetAngle();

            Body body2 = _world.CreateBody(bd);
            _piece2 = body2.CreateFixture(_shape2, 1.0f);

            // Compute consistent velocities for new bodies based on
            // cached velocity.
            Vector2 center1 = body1.GetWorldCenter();
            Vector2 center2 = body2.GetWorldCenter();

            Vector2 velocity1 = _velocity + MathUtils.Cross(_angularVelocity, center1 - center);
            Vector2 velocity2 = _velocity + MathUtils.Cross(_angularVelocity, center2 - center);

            body1.SetAngularVelocity(_angularVelocity);
            body1.SetLinearVelocity(velocity1);

            body2.SetAngularVelocity(_angularVelocity);
            body2.SetLinearVelocity(velocity2);
        }

        public override void Step(Framework.Settings settings)
	    {
		    if (_break)
		    {
			    Break();
			    _broke = true;
			    _break = false;
		    }

		    // Cache velocities to improve movement on breakage.
		    if (_broke == false)
		    {
			    _velocity = _body1.GetLinearVelocity();
			    _angularVelocity = _body1.GetAngularVelocity();
		    }

		    base.Step(settings);
	    }

	    static internal Test Create()
	    {
		    return new Breakable();
	    }

	    Body _body1;
	    Vector2 _velocity;
	    float _angularVelocity;
	    PolygonShape _shape1;
	    PolygonShape _shape2;
	    Fixture _piece1;
	    Fixture _piece2;

        bool _broke;
        bool _break;
    }
}
