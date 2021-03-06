/*
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
using Box2D.XNA.TestBed.Tests;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Box2D.XNA.TestBed
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferMultiSampling = true;
            this.IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            et = new EventTrace(this);
            TraceEvents.Register(et);

            testCount = 0;
	        while (TestEntries.g_testEntries[testCount].createFcn != null)
	        {
		        ++testCount;
	        }

            testIndex = MathUtils.Clamp(testIndex, 0, testCount - 1);
	        testSelection = testIndex;

	        entry = TestEntries.g_testEntries[testIndex];
	        test = entry.createFcn();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("font");
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.VertexColorEnabled = true;

            Framework.DebugDraw._device = GraphicsDevice;
            Framework.DebugDraw._batch = spriteBatch;
            Framework.DebugDraw._font = spriteFont;

            oldState = Keyboard.GetState();
            oldGamePad = GamePad.GetState(PlayerIndex.One);
            Resize(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            et.BeginTrace(TraceEvents.UpdateEventId);

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardState newState = Keyboard.GetState();
            GamePadState newGamePad = GamePad.GetState(PlayerIndex.One);
            MouseState newMouse = Mouse.GetState();

            HandleMouse(ref newMouse, ref oldMouse, newState.IsKeyDown(Keys.LeftShift));

            // Press 'z' to zoom out.
	        if (newState.IsKeyDown(Keys.Z) && oldState.IsKeyUp(Keys.Z))
            {
		        viewZoom = Math.Min(1.1f * viewZoom, 20.0f);
		        Resize(width, height);
            }
            // Press 'x' to zoom in.
            else if (newState.IsKeyDown(Keys.X) && oldState.IsKeyUp(Keys.X))
            {
		        viewZoom = Math.Max(0.9f * viewZoom, 0.02f);
		        Resize(width, height);
		    }
		    // Press 'r' to reset.
	        else if (newState.IsKeyDown(Keys.R) && oldState.IsKeyUp(Keys.R))
            {
		        test = entry.createFcn();
		    }
		    // Press space to launch a bomb.
            else if ((newState.IsKeyDown(Keys.Space) && oldState.IsKeyUp(Keys.Space)) ||
                      newGamePad.IsButtonDown(Buttons.B) && oldGamePad.IsButtonUp(Buttons.B))
            {
		        if (test != null)
		        {
			        test.LaunchBomb();
		        }
            }
            else if ((newState.IsKeyDown(Keys.P) && oldState.IsKeyUp(Keys.P)) ||
                      newGamePad.IsButtonDown(Buttons.Start) && oldGamePad.IsButtonUp(Buttons.Start))
            {
                settings.pause = settings.pause > (uint)0 ? (uint)1 : (uint)0;
            }
            // Press [ to prev test.
	        else if ((newState.IsKeyDown(Keys.OemOpenBrackets) && oldState.IsKeyUp(Keys.OemOpenBrackets)) ||
                      newGamePad.IsButtonDown(Buttons.LeftShoulder) && oldGamePad.IsButtonUp(Buttons.LeftShoulder))
            {
		        --testSelection;
		        if (testSelection < 0)
		        {
			        testSelection = testCount - 1;
		        }
            }
		    // Press ] to next test.
            else if ((newState.IsKeyDown(Keys.OemCloseBrackets) && oldState.IsKeyUp(Keys.OemCloseBrackets)) ||
                      newGamePad.IsButtonDown(Buttons.RightShoulder) && oldGamePad.IsButtonUp(Buttons.RightShoulder))
            {
		        ++testSelection;
		        if (testSelection == testCount)
		        {
			        testSelection = 0;
		        }
            }
		    // Press left to pan left.
            else if (newState.IsKeyDown(Keys.Left) && oldState.IsKeyUp(Keys.Left))
            {
                viewCenter.X -= 0.5f;
		        Resize(width, height);
            }
            // Press right to pan right.
	        else if (newState.IsKeyDown(Keys.Right) && oldState.IsKeyUp(Keys.Right))
            {
		        viewCenter.X += 0.5f;
		        Resize(width, height);
            }
            // Press down to pan down.
	        else if (newState.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Down))
            {
                viewCenter.Y -= 0.5f;
		        Resize(width, height);
		    }
            // Press up to pan up.
            else if (newState.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up))
            {
		        viewCenter.Y += 0.5f;
		        Resize(width, height);
		    }
            // Press home to reset the view.
            else if (newState.IsKeyDown(Keys.Home) && oldState.IsKeyUp(Keys.Home))
            {
		        viewZoom = 1.0f;
		        viewCenter = new Vector2(0.0f, 20.0f);
		        Resize(width, height);
            }
        	else
            {
		        if (test != null)
		        {
			        test.Keyboard(newState, oldState);
		        }
	        }

            base.Update(gameTime);

            oldState = newState;
            oldGamePad = newGamePad;
            oldMouse = newMouse;

            et.EndTrace(TraceEvents.UpdateEventId);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            et.BeginTrace(TraceEvents.DrawEventId);
            GraphicsDevice.Clear(Color.Black);

            basicEffect.Techniques[0].Passes[0].Apply();
            
	        test.SetTextLine(30);
	        settings.hz = settingsHz;

            et.BeginTrace(TraceEvents.PhysicsEventId);
	        test.Step(settings);
            et.EndTrace(TraceEvents.PhysicsEventId);

	        test.DrawTitle(50, 15, entry.name);

	        if (testSelection != testIndex)
	        {
		        testIndex = testSelection;
		        entry = TestEntries.g_testEntries[testIndex];
		        test = entry.createFcn();
		        viewZoom = 1.0f;
		        viewCenter = new Vector2(0.0f, 20.0f);
		        Resize(width, height);
	        }

            test._debugDraw.FinishDrawShapes();

            if (test != null)
            {
                spriteBatch.Begin();
                test._debugDraw.FinishDrawString();
                spriteBatch.End();
            }
            base.Draw(gameTime);

            et.EndTrace(TraceEvents.DrawEventId);
            et.EndTrace(TraceEvents.FrameEventId);
            et.ResetFrame();
            et.BeginTrace(TraceEvents.FrameEventId);
        }

        void Resize(int w, int h)
        {
	        width = w;
	        height = h;

	        tw = GraphicsDevice.Viewport.Width;
            th = GraphicsDevice.Viewport.Height;
            int x = GraphicsDevice.Viewport.X;
            int y = GraphicsDevice.Viewport.Y;

	        float ratio = (float)tw / (float)th;

	        Vector2 extents = new Vector2(ratio * 25.0f, 25.0f);
	        extents *= viewZoom;

	        Vector2 lower = viewCenter - extents;
	        Vector2 upper = viewCenter + extents;

	        // L/R/B/T
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(lower.X, upper.X, lower.Y, upper.Y, -1, 1);
        }

        Vector2 ConvertScreenToWorld(int x, int y)
        {
	        float u = x / (float)tw;
	        float v = (th - y) / (float)th;

	        float ratio = (float)tw / (float)th;
	        Vector2 extents = new Vector2(ratio * 25.0f, 25.0f);
	        extents *= viewZoom;

	        Vector2 lower = viewCenter - extents;
	        Vector2 upper = viewCenter + extents;

	        Vector2 p = new Vector2();
	        p.X = (1.0f - u) * lower.X + u * upper.X;
	        p.Y = (1.0f - v) * lower.Y + v * upper.Y;
	        return p;
        }

        void HandleMouse(ref MouseState state, ref MouseState oldState, bool shift)
        {
		    Vector2 p = ConvertScreenToWorld(state.X, state.Y);
            Vector2 oldp = ConvertScreenToWorld(oldState.X, oldState.Y);

            if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                if (shift)
                    test.ShiftMouseDown(p);
                else
                    test.MouseDown(p);
            }

            if (p != oldp)
                test.MouseMove(p);
            
            if (state.LeftButton == ButtonState.Released && oldState.LeftButton == ButtonState.Pressed)
            {
                test.MouseUp(p);
            }

            if (state.RightButton == ButtonState.Pressed && oldState.RightButton == ButtonState.Released)
            {
                lastp = p;
                rMouseDown = true;
            }
            else if (state.RightButton == ButtonState.Released && oldState.RightButton == ButtonState.Pressed)
            {
                rMouseDown = false;
            }

            if (rMouseDown)
	        {
		        Vector2 diff = p - lastp;
		        viewCenter.X -= diff.X;
		        viewCenter.Y -= diff.Y;
		        Resize(width, height);
	        }
            else if (state.ScrollWheelValue != oldState.ScrollWheelValue)
            {
                var direction = state.ScrollWheelValue - oldState.ScrollWheelValue;
	            if (direction > 0)
	            {
		            viewZoom /= 1.1f;
	            }
	            else
	            {
		            viewZoom *= 1.1f;
	            }
	            Resize(width, height);
            }

            lastp = ConvertScreenToWorld(state.X, state.Y);
        }

        void Restart()
        {
	        entry = TestEntries.g_testEntries[testIndex];
	        test = entry.createFcn();
            Resize(width, height);
        }

        void Pause()
        {
	        settings.pause = (uint)(settings.pause > 0 ? 0 : 1);
        }

        void SingleStep()
        {
	        settings.pause = 1;
	        settings.singleStep = 1;
        }

        int testIndex = 0;
	    int testSelection = 0;
	    int testCount = 0;
	    TestEntry entry;
	    Test test;
	    Framework.Settings settings = new Framework.Settings();
	    int width = 640;
	    int height = 480;
	    int framePeriod = 16;
	    float settingsHz = 60.0f;
	    float viewZoom = 1.0f;
        Vector2 viewCenter = new Vector2(0.0f, 20.0f);
	    int tx, ty, tw, th;
	    bool rMouseDown;
        Vector2 lastp;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        BasicEffect basicEffect;
        SpriteFont spriteFont;                
        KeyboardState oldState;
        GamePadState oldGamePad;
        MouseState oldMouse;
        IEventTrace et;
    }
}
