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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Box2D.XNA.TestBed.Tests
{
    public class RayCast : Test
    {
        static int e_maxBodies = 256;

        public RayCast()
        {
            // Ground body
		    {
			    BodyDef bd = new BodyDef();
			    Body ground = _world.CreateBody(bd);

			    PolygonShape shape = new PolygonShape();
                shape.SetAsEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
			    ground.CreateFixture(shape, 0.0f);
		    }

		    {
			    var vertices = new Vector2[3];
			    vertices[0] = new Vector2(-0.5f, 0.0f);
			    vertices[1] = new Vector2(0.5f, 0.0f);
			    vertices[2] = new Vector2(0.0f, 1.5f);
                _polygons[0] = new PolygonShape();
			    _polygons[0].Set(vertices, 3);
		    }

		    {
			    var vertices2 = new Vector2[3];
			    vertices2[0] = new Vector2(-0.1f, 0.0f);
			    vertices2[1] = new Vector2(0.1f, 0.0f);
			    vertices2[2] = new Vector2(0.0f, 1.5f);
                _polygons[1] = new PolygonShape();
			    _polygons[1].Set(vertices2, 3);
		    }

		    {
			    float w = 1.0f;
                float b = w / (2.0f + (float)Math.Sqrt(2.0));
			    float s = (float)Math.Sqrt(2.0) * b;

			    var vertices3 = new Vector2[8];
			    vertices3[0] = new Vector2(0.5f * s, 0.0f);
			    vertices3[1] = new Vector2(0.5f * w, b);
			    vertices3[2] = new Vector2(0.5f * w, b + s);
			    vertices3[3] = new Vector2(0.5f * s, w);
			    vertices3[4] = new Vector2(-0.5f * s, w);
			    vertices3[5] = new Vector2(-0.5f * w, b + s);
			    vertices3[6] = new Vector2(-0.5f * w, b);
			    vertices3[7] = new Vector2(-0.5f * s, 0.0f);
                _polygons[2] = new PolygonShape();
			    _polygons[2].Set(vertices3, 8);
		    }

		    {
                _polygons[3] = new PolygonShape();
			    _polygons[3].SetAsBox(0.5f, 0.5f);
		    }

		    {
                _circle = new CircleShape();
			    _circle._radius = 0.5f;
		    }

		    _bodyIndex = 0;
		    _angle = 0.0f;
            _mode = RayCastMode.Closest;
        }

        public override void Keyboard(KeyboardState state, KeyboardState oldState)
	    {
            if (state.IsKeyDown(Keys.D1) && oldState.IsKeyUp(Keys.D1))
            {
                Create(0);
            }
            if (state.IsKeyDown(Keys.D2) && oldState.IsKeyUp(Keys.D2))
            {
                Create(1);
            }
            if (state.IsKeyDown(Keys.D3) && oldState.IsKeyUp(Keys.D3))
            {
                Create(2);
            }
            if (state.IsKeyDown(Keys.D4) && oldState.IsKeyUp(Keys.D4))
            {
                Create(3);
            }
            if (state.IsKeyDown(Keys.D5) && oldState.IsKeyUp(Keys.D5))
            {
                Create(4);
            }
            if (state.IsKeyDown(Keys.D) && oldState.IsKeyUp(Keys.D))
            {
                DestroyBody();
            }
            if (state.IsKeyDown(Keys.M) && oldState.IsKeyUp(Keys.M))
            {
                switch (_mode)
                {
                    case RayCastMode.Closest:
                        _mode = RayCastMode.Any;
                        break;
                    case RayCastMode.Any:
                        _mode = RayCastMode.Multiple;
                        break;
                    case RayCastMode.Multiple:
                        _mode = RayCastMode.Closest;
                        break;
                    default:
                        break;
                }
            }
	    }

        public void DestroyBody()
	    {
		    for (int i = 0; i < e_maxBodies; ++i)
		    {
			    if (_bodies[i] != null)
			    {
				    _world.DestroyBody(_bodies[i]);
				    _bodies[i] = null;
				    return;
			    }
		    }
	    }

        public override void Step(Framework.Settings settings)
	    {
            bool advanceRay = settings.pause == 0 || settings.singleStep != 0;

		    base.Step(settings);
		    _debugDraw.DrawString(5, _textLine, "Press 1-5 to drop stuff, m to change the mode");
		    _textLine += 15;
            _debugDraw.DrawString(5, _textLine, string.Format("Mode = {0}", _mode));
            _textLine += 15;

		    float L = 11.0f;
		    Vector2 point1 = new Vector2(0.0f, 10.0f);
            Vector2 d = new Vector2(L * (float)Math.Cos(_angle), L * (float)Math.Sin(_angle));
		    Vector2 point2 = point1 + d;

            Vector2 point = Vector2.Zero, normal = Vector2.Zero;
            
            switch (_mode)
            {
                case RayCastMode.Closest:
                    bool hitClosest = false;
		            _world.RayCast((f, p, n, fr) => 
                        {
                            Body body = f.GetBody();
                            if (body.GetUserData() != null)
                            {
                                int index = (int)body.GetUserData();
                                if (index == 0)
                                {
                                    // filter
                                    return -1.0f;
                                }
                            }

                            hitClosest = true;
                            point = p;
                            normal = n;
                            return fr;
                        }, point1, point2);

                    if (hitClosest)                        
		            {
			            _debugDraw.DrawPoint(point, .5f, new Color(0.4f, 0.9f, 0.4f));

			            _debugDraw.DrawSegment(point1, point, new Color(0.8f, 0.8f, 0.8f));

			            Vector2 head = point + 0.5f * normal;
			            _debugDraw.DrawSegment(point, head, new Color(0.9f, 0.9f, 0.4f));
		            }
                    else
                    {
                        _debugDraw.DrawSegment(point1, point2, new Color(0.8f, 0.8f, 0.8f));
                    }

                    break;
                case RayCastMode.Any:
                    bool hitAny = false;
                    _world.RayCast((f, p, n, fr) =>
                    {
                        Body body = f.GetBody();
                        if (body.GetUserData() != null)
                        {
                            int index = (int)body.GetUserData();
                            if (index == 0)
                            {
                                // filter
                                return -1.0f;
                            }
                        }

                        hitAny = true;
                        point = p;
                        normal = n;
                        return 0;
                    }, point1, point2);

                    if (hitAny)
                    {
                        _debugDraw.DrawPoint(point, .5f, new Color(0.4f, 0.9f, 0.4f));

                        _debugDraw.DrawSegment(point1, point, new Color(0.8f, 0.8f, 0.8f));

                        Vector2 head = point + 0.5f * normal;
                        _debugDraw.DrawSegment(point, head, new Color(0.9f, 0.9f, 0.4f));
                    }
                    else
                    {
                        _debugDraw.DrawSegment(point1, point2, new Color(0.8f, 0.8f, 0.8f));
                    }
                    break;
                case RayCastMode.Multiple:
                    var points = new List<Vector2>();
                    var normals = new List<Vector2>();
                    _world.RayCast((f, p, n, fr) =>
                    {
                        Body body = f.GetBody();
                        if (body.GetUserData() != null)
                        {
                            int index = (int)body.GetUserData();
                            if (index == 0)
                            {
                                // filter
                                return -1.0f;
                            }
                        }

                        points.Add(p);
                        normals.Add(n);
                        return 1.0f;
                    }, point1, point2);

                    _debugDraw.DrawSegment(point1, point2, new Color(0.8f, 0.8f, 0.8f));

                    for (int i=0; i<points.Count; i++)
                    {
                        _debugDraw.DrawPoint(points[i], .5f, new Color(0.4f, 0.9f, 0.4f));

                        _debugDraw.DrawSegment(point1, points[i], new Color(0.8f, 0.8f, 0.8f));

                        Vector2 head = points[i] + 0.5f * normals[i];
                        _debugDraw.DrawSegment(points[i], head, new Color(0.9f, 0.9f, 0.4f));
                    }
                    break;
                default:
                    break;
            }

            if (advanceRay)
            {
                _angle += 0.25f * (float)Math.PI / 180.0f;
            }
	    }

        private void Create(int index)
	    {
		    if (_bodies[_bodyIndex] != null)
		    {
			    _world.DestroyBody(_bodies[_bodyIndex]);
			    _bodies[_bodyIndex] = null;
		    }

		    BodyDef bd = new BodyDef();

		    float x = Rand.RandomFloat(-10.0f, 10.0f);
            float y = Rand.RandomFloat(0.0f, 20.0f);
		    bd.position = new Vector2(x, y);
            bd.angle = Rand.RandomFloat(-(float)Math.PI, (float)Math.PI);

            bd.userData = index;

		    if (index == 4)
		    {
			    bd.angularDamping = 0.02f;
		    }

		    _bodies[_bodyIndex] = _world.CreateBody(bd);

		    if (index < 4)
		    {
			    FixtureDef fd = new FixtureDef();
			    fd.shape = _polygons[index];
			    fd.friction = 0.3f;
			    _bodies[_bodyIndex].CreateFixture(fd);
		    }
		    else
		    {
			    FixtureDef fd = new FixtureDef();
			    fd.shape = _circle;
			    fd.friction = 0.3f;

			    _bodies[_bodyIndex].CreateFixture(fd);
		    }

		    _bodyIndex = (_bodyIndex + 1) % e_maxBodies;
	    }

	    static internal Test Create()
	    {
		    return new RayCast();
	    }

	    int _bodyIndex;
	    Body[] _bodies = new Body[e_maxBodies];
        //int[] _userData = new int[e_maxBodies]; // this is not needed because we set it directly
	    PolygonShape[] _polygons = new PolygonShape[4];
	    CircleShape _circle;

	    float _angle;

        enum RayCastMode
        {
            Closest,
            Any,
            Multiple,
        }

        RayCastMode _mode;
    }
}
