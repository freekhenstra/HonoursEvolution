using System.Text;
using FarseerPhysics.Samples.Demos.Prefabs;
using FarseerPhysics.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace FarseerPhysics.Samples.Demos
{
    public class SimpleDemo6 : PhysicsGameScreen, IDemoScreen
    {
        private Agent _agent;
        private Border _border;
        private Spider[] _spiders;
        private Creature creature, creature2, creature3, creature4, creature5;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Dynamic Angle Joints";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo demonstrates the use of revolute joints combined");
            sb.AppendLine("with angle joints that have a dynamic target angle.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Rotate agent: left and right triggers");
            sb.AppendLine("  - Move agent: right thumbstick");
            sb.AppendLine("  - Move cursor: left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: left thumbstick");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Rotate agent: left and right arrows");
            sb.AppendLine("  - Move agent: A,S,D,W");
            sb.AppendLine("  - Exit to menu: Escape");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse / Touchscreen");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: move mouse / finger");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = Vector2.Zero;

            //_border = new Border(World, ScreenManager, Camera);
            //_agent = new Agent(World, ScreenManager, new Vector2(0f, 10f));
            //_spiders = new Spider[8];

            /*List<Corner> corners = new List<Corner>();
            corners.Add(new Corner((float)Math.PI / 4, 1f, null));
            corners.Add(new Corner((float)Math.PI * 3 / 4, 1f, null));
            corners.Add(new Corner((float)-Math.PI * 3 / 4, 1f, null));
            corners.Add(new Corner((float)-Math.PI / 4, 1f, null));
            creature = new Creature(World, corners, ScreenManager, new Vector2(2f, 6f));*/

            /*List<Corner> corners = new List<Corner>();
            corners.Add(new Corner((float)Math.PI / 4, 3f, null));
            corners.Add(new Corner(0f, (float)Math.Sqrt(2), null));
            corners.Add(new Corner((float)-Math.PI / 4, 3f, null));
            
            creature = new Creature(World, corners, ScreenManager, new Vector2(2f, 6f));*/

            float[] movement = new float[] { 1f, -1f, 1f, -1f };
            float[] movement2 = new float[] { -0.5f, 0.5f, -1.2f, 1.2f };

            List<Corner> corners = new List<Corner>();
            corners.Add(new Corner(0.5f, 2f, null, null));
            corners.Add(new Corner(0f, 5f, null, null));
            corners.Add(new Corner(-0.5f, 3f, null, null));

            creature = new Creature(World, corners, ScreenManager, new Vector2(2f, 6f));

            List<Corner> corners2 = new List<Corner>();
            corners2.Add(new Corner(0.5f, 2f, null, null));
            corners2.Add(new Corner(-0.5f, 3f, creature, movement));

            creature2 = new Creature(World, corners2, ScreenManager, new Vector2(2f, 6f));

            List<Corner> corners3 = new List<Corner>();
            corners3.Add(new Corner(0.5f, 2f, null, null));
            corners3.Add(new Corner(0f, 5f, null, null));
            corners3.Add(new Corner(-0.5f, 3f, null, null));

            creature3 = new Creature(World, corners3, ScreenManager, new Vector2(2f, 6f));

            List<Corner> corners4 = new List<Corner>();
            corners4.Add(new Corner(0.5f, 2f, null, null));
            corners4.Add(new Corner(-0.5f, 3f, creature3, movement));

            creature4 = new Creature(World, corners4, ScreenManager, new Vector2(2f, 6f));

            List<Corner> corners5 = new List<Corner>();
            corners5.Add(new Corner(0.5f, 2f, null, null));
            corners5.Add(new Corner(0.1f, 6f, creature2, movement2));
            corners5.Add(new Corner(-0.2f, 5f, creature4, movement2));
            corners5.Add(new Corner(-0.5f, 3f, null, null));

            creature5 = new Creature(World, corners5, ScreenManager, new Vector2(2f, 6f));

            //for (int i = 0; i < _spiders.Length; i++)
            //{
            //    _spiders[i] = new Spider(World, ScreenManager, new Vector2(0f, 8f - (i + 1) * 2f));
            //}

            //SetUserAgent(_agent.Body, 1000f, 400f);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (IsActive)
            {
                //for (int i = 0; i < _spiders.Length; i++)
                //{
                //    _spiders[i].Update(gameTime);
                //}
                creature5.Update(gameTime);
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(0, null, null, null, null, null, Camera.View);
            //_agent.Draw();
            creature5.Draw();
            //for (int i = 0; i < _spiders.Length; i++)
            //{
            //    _spiders[i].Draw();
            //}
            ScreenManager.SpriteBatch.End();
            //_border.Draw();
            base.Draw(gameTime);
        }
    }
}