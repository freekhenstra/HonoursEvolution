using System.Text;
using FarseerPhysics.Samples.Demos.Prefabs;
using FarseerPhysics.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;

namespace FarseerPhysics.Samples.Demos
{
    public class SimpleDemo6 : PhysicsGameScreen, IDemoScreen
    {
        private Agent _agent;
        private Border _border;
        private Spider[] _spiders;
        private LiveCreature finalCreature;

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

            float[] movement = new float[] { 1f, 0f, -1f, -0f, 1f, 0f, -1f, -0f };
            float[] movement2 = new float[] { 0f, -1f, -0f, 1f, 0f, -1f, -0f, 1f };
            float[] movement3 = new float[] { -1f, -0f, 1f, 0f, -1f, -0f, 1f, 0f };
            float[] movement4 = new float[] { -0f, 1f, 0f, -1f, -0f, 1f, 0f, -1f };
            
            List<Corner> corners = new List<Corner>();
            corners.Add(new Corner(0.2f, 0.7f, null));
            corners.Add(new Corner(0f, 2f, null));
            corners.Add(new Corner(-0.2f, 0.7f, null));

            Creature creature = new Creature(corners);
            
            List<Corner> corners2 = new List<Corner>();
            corners2.Add(new Corner(0.1f, 2f, null));
            corners2.Add(new Corner(0f, 2.7f, new Limb(creature, movement)));
            corners2.Add(new Corner(-0.1f, 2f, null));

            Creature creature2 = new Creature(corners2);

            List<Corner> corners3 = new List<Corner>();
            corners3.Add(new Corner((float)Math.PI / 4, (float)Math.Sqrt(8), new Limb(creature2, movement2)));
            corners3.Add(new Corner(0f, 6f, null));
            corners3.Add(new Corner((float)-Math.PI / 4, (float)Math.Sqrt(8), new Limb(creature2.Clone(), movement2)));

            Creature creature3 = new Creature(corners3);

            List<Corner> corners4 = new List<Corner>();
            corners4.Add(new Corner(0.4f, 1.7f, null));
            corners4.Add(new Corner(0f, 2f, null));
            corners4.Add(new Corner(-0.4f, 1.7f, null));

            Creature creature4 = new Creature(corners4);

            List<Corner> corners5 = new List<Corner>();
            corners5.Add(new Corner(0.1f, 2f, null));
            corners5.Add(new Corner(0f, 4f, new Limb(creature4, movement3)));
            corners5.Add(new Corner(-0.1f, 2f, null));

            Creature creature5 = new Creature(corners5);

            List<Corner> corners6 = new List<Corner>();
            corners6.Add(new Corner((float)Math.PI / 4, (float)Math.Sqrt(8), new Limb(creature5, movement4)));
            corners6.Add(new Corner(0f, 4f, null));
            corners6.Add(new Corner((float)-Math.PI / 4, (float)Math.Sqrt(8), new Limb(creature5.Clone(), movement4)));

            Creature creature6 = new Creature(corners6);

            Creature creature7 = Creature.CreateOffspring(creature3, creature6, new Random());

            Creature creature8 = Creature.CreateFirstGen(World, ScreenManager, Vector2.Zero, new Random())[0];

            string outputJSON = JsonConvert.SerializeObject(creature8);
            File.WriteAllText("Output.json", outputJSON);
            string JSONstring = File.ReadAllText("Output.json");
            Creature creature9 = JsonConvert.DeserializeObject<Creature>(JSONstring);

            finalCreature = new LiveCreature(creature8, World, ScreenManager, Vector2.Zero);

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
                finalCreature.Update(gameTime);
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(0, null, null, null, null, null, Camera.View);
            //_agent.Draw();
            finalCreature.Draw();
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