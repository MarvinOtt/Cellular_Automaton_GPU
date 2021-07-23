using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using FormClosingEventArgs = System.Windows.Forms.FormClosingEventArgs;
using Form = System.Windows.Forms.Form;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Cellular_Automaton_GPU
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static ContentManager contmanager;
        List<Element> elements = new List<Element>();
        private SpriteFont font;
        private Effect logic_effect, render_effect;
        public static Random r = new Random();
        private RenderTarget2D target1, target2, finaltarget, originalsizetarget, originalsizetarget2;
        private int timer = 0;
        private int framespergeneration = 2;
        public static int worldsizex = 1000, worldsizey = 1000;
        private KeyboardState KB_currentstate, KB_oldstate;
        private MouseState M_currentstate, M_oldstate;
        private Vector2 worldpos;
        public int IsBreak = 1;
        public int Selection_type;
        public Segment segment;
        public int current_generation = 0;
        public float[] Selection_data, DATA_target1, Generation0_DATA;
        public bool STRG_C_pressed = false, STRG_V_pressed = false, STRG_ALT_C_pressed = false, STRG_ALT_V_pressed = false;
        public Vector2 Selection_click_pos;
        public int selection_start_X, selection_start_Y, selection_end_X, selection_end_Y, selection_size_X, selection_size_Y;
        private int worldzoom = 0;
        public int mouse_worldpos_X, mouse_worldpos_Y;
        public int selectedtype = 5;
        public bool IsDisplayHiglighted = false;

        #region UI

        private Texture2D button_partikel_top, button_partikel_right, button_partikel_bottom, button_partikel_left, button_partikel_multifunction, button_partikel_blue, button_play, button_break, button_reset, button_partikel_lowred, button_partikel_highred;

        #endregion


        public static int Screenwidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        public static int Screenheight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
                PreferredBackBufferWidth = Screenwidth,
                PreferredBackBufferHeight = Screenheight,
                IsFullScreen = false,
                SynchronizeWithVerticalRetrace = true

            };
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            Window.IsBorderless = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Form f = Form.FromHandle(Window.Handle) as Form;
            f.Location = new System.Drawing.Point(0, 0);
            if (f != null) { f.FormClosing += f_FormClosing; }

            base.Initialize();
        }

        private void f_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Exit();
            Thread.Sleep(100);
            base.Exit();
        }
        public void SetPixel<type>(int x, int y, type c, Texture2D tex) where type : struct
        {
            Rectangle r = new Rectangle(x, y, 1, 1);
            type[] color = new type[1];
            color[0] = c;

            tex.SetData<type>(0, r, color, 0, 1);
        }
        public void SetRectangle<type>(int x, int y, int x_size, int y_size, type c, Texture2D tex) where type : struct
        {
            Rectangle r = new Rectangle(x, y, x_size, y_size);
            type[] color = new type[x_size * y_size];
            color.Init(c);

            tex.SetData<type>(0, r, color, 0, color.Length);
        }

        public void screen2worldcoo(Vector2 screencoos, out int x, out int y)
        {
            x = (int)((screencoos.X - worldpos.X) / (float)Math.Pow(2, worldzoom));
            y = (int)((screencoos.Y - worldpos.Y) / (float)Math.Pow(2, worldzoom));
        }
        public Vector2 world2screencoo(int x, int y)
        {
            Vector2 OUT;
            OUT.X = worldpos.X + (float)Math.Pow(2, worldzoom) * x;
            OUT.Y = worldpos.Y + (float)Math.Pow(2, worldzoom) * y;
            return OUT;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            contmanager = Content;
            font = Content.Load<SpriteFont>("font");
            segment = new Segment();

            #region Loading UI

            button_partikel_bottom = Content.Load<Texture2D>("button_partikel_bottom");
            button_partikel_top = Content.Load<Texture2D>("button_partikel_top");
            button_partikel_left = Content.Load<Texture2D>("button_partikel_left");
            button_partikel_right = Content.Load<Texture2D>("button_partikel_right");
            button_partikel_multifunction = Content.Load<Texture2D>("button_partikel_multifunction");
            button_partikel_blue = Content.Load<Texture2D>("button_partikel_blue");
            button_partikel_highred = Content.Load<Texture2D>("button_partikel_highred");
            button_partikel_lowred = Content.Load<Texture2D>("button_partikel_lowred");
            button_break = Content.Load<Texture2D>("button_break");
            button_play = Content.Load<Texture2D>("button_play");
            button_reset = Content.Load<Texture2D>("button_reset");

            elements.Add(new Element("button_partikel_bottom", 3));
            elements.Add(new Element("button_partikel_top", 1));
            elements.Add(new Element("button_partikel_left", 4));
            elements.Add(new Element("button_partikel_right", 2));
            elements.Add(new Element("button_partikel_multifunction", 5));
            elements.Add(new Element("button_partikel_blue", 6));
            elements.Add(new Element("button_partikel_AND", 11));
            elements.Add(new Element("button_partikel_OR", 12));
            elements.Add(new Element("button_partikel_XOR", 13));
            elements.Add(new Element("button_partikel_highred", 9));
            elements.Add(new Element("button_partikel_lowred", 7));

            #endregion

            logic_effect = Content.Load<Effect>("logic_effect_2");
            render_effect = Content.Load<Effect>("render_effect");
            render_effect.Parameters["Screenwidth"].SetValue(Screenwidth);
            render_effect.Parameters["Screenheight"].SetValue(Screenheight);
            render_effect.Parameters["worldsizex"].SetValue(worldsizex);
            render_effect.Parameters["worldsizey"].SetValue(worldsizey);
            logic_effect.Parameters["worldsizex"].SetValue(worldsizex);
            logic_effect.Parameters["worldsizey"].SetValue(worldsizey);
            target1 = new RenderTarget2D(GraphicsDevice, worldsizex, worldsizey, false, SurfaceFormat.Single, DepthFormat.None);
            target2 = new RenderTarget2D(GraphicsDevice, worldsizex, worldsizey, false, SurfaceFormat.Single, DepthFormat.None);
            finaltarget = new RenderTarget2D(GraphicsDevice, Screenwidth, Screenheight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            originalsizetarget = new RenderTarget2D(GraphicsDevice, worldsizex, worldsizey, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            originalsizetarget2 = new RenderTarget2D(GraphicsDevice, worldsizex, worldsizey, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            GraphicsDevice.SetRenderTargets(target1, target2);
            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.SetRenderTarget(target1);
            SetPixel(100, 100, 5.0f, target1);
            SetPixel(100, 101, 5.0f, target1);
            SetPixel(101, 101, 5.0f, target1);

            SetPixel(200, 101, 5.0f, target1);
            SetPixel(201, 101, 5.0f, target1);
            SetPixel(201, 100, 5.0f, target1);

            SetPixel(100, 1, 5.0f, target1);
            SetPixel(100, 0, 5.0f, target1);
            SetPixel(101, 0, 5.0f, target1);

            SetPixel(200, 0, 5.0f, target1);
            SetPixel(201, 0, 5.0f, target1);
            SetPixel(201, 1, 5.0f, target1);

            SetPixel(150, 100, 4.0f, target1);
            SetPixel(151, 100, 2.0f, target1);
            Generation0_DATA = new float[target1.Width * target1.Height];
            target1.GetData(Generation0_DATA);

            KB_oldstate = Keyboard.GetState();
            M_oldstate = Mouse.GetState();
        }

        protected override void UnloadContent()
        {
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KB_currentstate = Keyboard.GetState();
            M_currentstate = Mouse.GetState();
            if (IsActive)
            {
                #region Updating Input

                #region Position and Zoom

                if (KB_currentstate.IsKeyDown(Keys.W))
                    worldpos.Y += 10;
                if (KB_currentstate.IsKeyDown(Keys.S))
                    worldpos.Y -= 10;
                if (KB_currentstate.IsKeyDown(Keys.A))
                    worldpos.X += 10;
                if (KB_currentstate.IsKeyDown(Keys.D))
                    worldpos.X -= 10;

                worldpos.X = (int)(worldpos.X);
                worldpos.Y = (int)(worldpos.Y);

                if (KB_currentstate.IsKeyDown(Keys.Add) && !KB_oldstate.IsKeyDown(Keys.Add))
                    framespergeneration -= 1;
                if (KB_currentstate.IsKeyDown(Keys.Subtract) && !KB_oldstate.IsKeyDown(Keys.Subtract))
                    framespergeneration += 1;

                if (M_currentstate.ScrollWheelValue != M_oldstate.ScrollWheelValue)
                {
                    if (M_currentstate.ScrollWheelValue < M_oldstate.ScrollWheelValue) // Zooming Out
                    {
                        worldzoom -= 1;
                        Vector2 diff = worldpos - M_currentstate.Position.ToVector2();
                        worldpos = M_currentstate.Position.ToVector2() + diff / 2;
                    }
                    else // Zooming In
                    {
                        worldzoom += 1;
                        Vector2 diff = worldpos - M_currentstate.Position.ToVector2();
                        worldpos += diff;
                    }
                }

                #endregion

                screen2worldcoo(M_currentstate.Position.ToVector2(), out mouse_worldpos_X, out mouse_worldpos_Y);

                if (KB_currentstate.IsKeyDown(Keys.H) && KB_oldstate.IsKeyUp(Keys.H))
                {
                    IsDisplayHiglighted = !IsDisplayHiglighted;
                }

                if (KB_currentstate.IsKeyDown(Keys.Space) && !KB_oldstate.IsKeyDown(Keys.Space))
                {
                    IsBreak = (IsBreak + 1) % 2;
                    if (IsBreak == 0 && current_generation == 0)
                        target1.GetData(Generation0_DATA);
                    Selection_type = 0;
                }

                if (Selection_type >= 0 && M_currentstate.LeftButton == ButtonState.Pressed && KB_currentstate.IsKeyDown(Keys.LeftControl))
                {
                    IsBreak = 1;
                    Selection_type = 1;
                    if (M_oldstate.LeftButton == ButtonState.Released)
                    {
                        selection_start_X = mouse_worldpos_X;
                        selection_start_Y = mouse_worldpos_Y;
                    }
                    selection_end_X = mouse_worldpos_X;
                    selection_end_Y = mouse_worldpos_Y;
                    selection_size_X = selection_end_X - selection_start_X;
                    selection_size_Y = selection_end_Y - selection_start_Y;
                }
                if (KB_currentstate.IsKeyDown(Keys.Escape))
                {
                    if (Selection_type < 0 && DATA_target1 != null)
                        target1.SetData(DATA_target1);
                    Selection_type = 0;
                }

                if (KB_currentstate.IsKeyDown(Keys.Enter) && Selection_type < 0)
                {
                    Selection_type = 0;
                }

                if (Selection_type == 1 && M_currentstate.LeftButton == ButtonState.Released)
                {
                    Selection_type = 2;
                    if (selection_end_X < selection_start_X)
                    {
                        selection_start_X = selection_end_X;
                        selection_size_X = -selection_size_X;
                        selection_end_X = selection_start_X + selection_size_X;
                    }
                    if (selection_end_Y < selection_start_Y)
                    {
                        selection_start_Y = selection_end_Y;
                        selection_size_Y = -selection_size_Y;
                        selection_end_Y = selection_start_Y + selection_size_Y;
                    }
                    if (selection_start_X < 0)
                        selection_start_X = 0;
                    if (selection_start_Y < 0)
                        selection_start_Y = 0;
                    if (selection_end_X >= target1.Width)
                        selection_end_X = target1.Width - 1;
                    if (selection_end_Y >= target1.Height)
                        selection_end_Y = target1.Height - 1;
                    selection_size_X = selection_end_X - selection_start_X;
                    selection_size_Y = selection_end_Y - selection_start_Y;
                }

                if (Selection_type > 0 && KB_currentstate.IsKeyDown(Keys.Delete) && KB_oldstate.IsKeyUp(Keys.Delete))
                {
                    SetRectangle(selection_start_X, selection_start_Y, selection_size_X + 1, selection_size_Y + 1, 0f, target1);
                }

                Vector2 start = world2screencoo(selection_start_X, selection_start_Y);
                Vector2 end = world2screencoo(selection_end_X + 1, selection_end_Y + 1);

                if (Selection_type > 1 && M_currentstate.LeftButton == ButtonState.Pressed)
                {
                    if (M_oldstate.LeftButton == ButtonState.Released && M_currentstate.Position.X > start.X && M_currentstate.Position.X < end.X && M_currentstate.Position.Y > start.Y && M_currentstate.Position.Y < end.Y)
                    {
                        Selection_click_pos = M_currentstate.Position.ToVector2() - world2screencoo(selection_start_X, selection_start_Y);
                        Selection_type = 3;
                    }

                    if (Selection_type == 3)
                    {
                        screen2worldcoo(M_currentstate.Position.ToVector2() - Selection_click_pos, out selection_start_X, out selection_start_Y);
                        if (selection_start_X < 0)
                            selection_start_X = 0;
                        if (selection_start_Y < 0)
                            selection_start_Y = 0;
                        if (selection_start_X + selection_size_X + 1 > target1.Width)
                            selection_start_X = target1.Width - selection_size_X - 1;
                        if (selection_start_Y + selection_size_Y + 1 > target1.Height)
                            selection_start_Y = target1.Height - selection_size_Y - 1;
                        selection_end_X = selection_start_X + selection_size_X;
                        selection_end_Y = selection_start_Y + selection_size_Y;
                    }
                }
                else if (Selection_type == 3)
                    Selection_type = 2;

                #region Rotating and Mirroring

                if (Selection_type < 0)
                {
                    if (KB_currentstate.IsKeyDown(Keys.R) && KB_oldstate.IsKeyUp(Keys.R))
                    {
                        if (segment.sizex < target1.Height && segment.sizey < target1.Width)
                        {
                            float[] newdata = new float[(segment.sizex + 1) * (segment.sizey + 1)];
                            int oldindex, newindex, oldx, oldy, newx, newy;
                            for (int i = 0; i < newdata.Length; i++)
                            {
                                newx = i % (segment.sizey + 1);
                                newy = i / (segment.sizey + 1);
                                oldx = newy;
                                oldy = segment.sizey - newx;
                                newdata[i] = segment.data[oldx + oldy * (segment.sizex + 1)];
                                if (newdata[i] > 0.5f && newdata[i] < 4.5f)
                                    newdata[i] = 1 + (newdata[i] % 4);
                            }

                            segment.data = newdata;
                            int buffer = segment.sizey;
                            segment.sizey = selection_size_Y = segment.sizex;
                            segment.sizex = selection_size_X = buffer;

                            if (selection_start_X < 0)
                                selection_start_X = 0;
                            if (selection_start_Y < 0)
                                selection_start_Y = 0;
                            if (selection_start_X + selection_size_X + 1 > target1.Width)
                                selection_start_X = target1.Width - selection_size_X - 1;
                            if (selection_start_Y + selection_size_Y + 1 > target1.Height)
                                selection_start_Y = target1.Height - selection_size_Y - 1;

                            selection_end_X = selection_start_X + selection_size_X;
                            selection_end_Y = selection_start_Y + selection_size_Y;
                            target1.SetData(DATA_target1);
                            target1.SetData(0, new Rectangle(selection_start_X, selection_start_Y, segment.sizex + 1, segment.sizey + 1), segment.data, 0, (segment.sizex + 1) * (segment.sizey + 1));
                        }
                    }

                    if (KB_currentstate.IsKeyDown(Keys.X) && KB_oldstate.IsKeyUp(Keys.X))
                    {
                        float[] newdata = new float[(segment.sizex + 1) * (segment.sizey + 1)];
                        int oldindex, newindex, oldx, oldy, newx, newy;
                        for (int i = 0; i < newdata.Length; i++)
                        {
                            newx = i % (segment.sizex + 1);
                            newy = i / (segment.sizex + 1);
                            oldx = segment.sizex - newx;
                            oldy = newy;
                            newdata[i] = segment.data[oldx + oldy * (segment.sizex + 1)];
                            if (newdata[i] > 1.5f && newdata[i] < 2.5f)
                                newdata[i] = 4;
                            else if (newdata[i] > 3.5f && newdata[i] < 4.5f)
                                newdata[i] = 2;
                        }
                        segment.data = newdata;
                        target1.SetData(0, new Rectangle(selection_start_X, selection_start_Y, segment.sizex + 1, segment.sizey + 1), segment.data, 0, (segment.sizex + 1) * (segment.sizey + 1));
                    }
                    if (KB_currentstate.IsKeyDown(Keys.Y) && KB_oldstate.IsKeyUp(Keys.Y))
                    {
                        float[] newdata = new float[(segment.sizex + 1) * (segment.sizey + 1)];
                        int oldindex, newindex, oldx, oldy, newx, newy;
                        for (int i = 0; i < newdata.Length; i++)
                        {
                            newx = i % (segment.sizex + 1);
                            newy = i / (segment.sizex + 1);
                            oldx = newx;
                            oldy = segment.sizey - newy;
                            newdata[i] = segment.data[oldx + oldy * (segment.sizex + 1)];
                            if (newdata[i] > 0.5f && newdata[i] < 1.5f)
                                newdata[i] = 3;
                            else if (newdata[i] > 2.5f && newdata[i] < 3.5f)
                                newdata[i] = 1;
                        }
                        segment.data = newdata;
                        target1.SetData(0, new Rectangle(selection_start_X, selection_start_Y, segment.sizex + 1, segment.sizey + 1), segment.data, 0, (segment.sizex + 1) * (segment.sizey + 1));
                    }

                    #endregion

                }

                #region Opening File

                if (KB_currentstate.IsKeyDown(Keys.LeftControl) && KB_currentstate.IsKeyDown(Keys.LeftShift) && KB_currentstate.IsKeyDown(Keys.O) && (KB_oldstate.IsKeyUp(Keys.LeftControl) || KB_oldstate.IsKeyUp(Keys.O) || KB_currentstate.IsKeyDown(Keys.LeftShift)))
                {
                    IsBreak = 1;
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.CheckFileExists = true;
                        dialog.CheckPathExists = true;
                        dialog.Title = "Select File to Open";
                        dialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
                        dialog.FilterIndex = 1;
                        dialog.RestoreDirectory = true;

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            string filename = dialog.FileName;
                            try
                            {
                                BinaryReader file = new BinaryReader(File.Open(filename, FileMode.Open), Encoding.Default, false);

                                byte[] DATA;
                                byte[] width = new byte[4], height = new byte[4];
                                int width_int, height_int;
                                file.Read(width, 0, 4);
                                file.Read(height, 0, 4);
                                width_int = BitConverter.ToInt32(width, 0);
                                height_int = BitConverter.ToInt32(height, 0);
                                //Reading the heights of the map
                                DATA = new byte[width_int * height_int * 4];
                                file.Read(DATA, 0, DATA.Length);
                                file.Close();
                                file.Dispose();
                                render_effect.Parameters["worldsizex"].SetValue(width_int);
                                render_effect.Parameters["worldsizey"].SetValue(height_int);
                                logic_effect.Parameters["worldsizex"].SetValue(width_int);
                                logic_effect.Parameters["worldsizey"].SetValue(height_int);
                                target1.Dispose();
                                target2.Dispose();
                                originalsizetarget.Dispose();
                                originalsizetarget2.Dispose();
                                target1 = new RenderTarget2D(GraphicsDevice, width_int, height_int, false, SurfaceFormat.Single, DepthFormat.None);
                                target2 = new RenderTarget2D(GraphicsDevice, width_int, height_int, false, SurfaceFormat.Single, DepthFormat.None);
                                originalsizetarget = new RenderTarget2D(GraphicsDevice, width_int, height_int, false, SurfaceFormat.Color, DepthFormat.None);
                                originalsizetarget2 = new RenderTarget2D(GraphicsDevice, width_int, height_int, false, SurfaceFormat.Color, DepthFormat.None);
                                target1.SetData(DATA);
                                Generation0_DATA = new float[width_int * height_int];
                                DATA_target1 = new float[width_int * height_int];
                                target1.GetData(Generation0_DATA);
                                if (DATA_target1 != null)
                                    target1.GetData(DATA_target1);
                                Selection_type = 0;
                                if (Selection_type < 0)
                                    //target1.SetData(0, new Rectangle(selection_start_X, selection_start_Y, segment.sizex + 1, segment.sizey + 1), segment.data, 0, (segment.sizex + 1) * (segment.sizey + 1));
                                    current_generation = 0;
                                Console.WriteLine("Loading suceeded. Filename: {0}", filename);

                            }
                            catch (Exception exp)
                            {
                                Console.WriteLine("Loading failed: {0}", exp);
                                System.Windows.Forms.MessageBox.Show("Loading failed", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                #endregion

                #region Saving File
                if (KB_currentstate.IsKeyDown(Keys.LeftControl) && KB_currentstate.IsKeyDown(Keys.LeftShift) && KB_currentstate.IsKeyDown(Keys.S) && (KB_oldstate.IsKeyUp(Keys.LeftControl) || KB_oldstate.IsKeyUp(Keys.S) || KB_currentstate.IsKeyDown(Keys.LeftShift)))
                {
                    IsBreak = 1;
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
                        dialog.Multiselect = false;
                        dialog.CheckPathExists = false;
                        dialog.CheckFileExists = false;
                        dialog.Title = "Select or Create File to Save";
                        dialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
                        dialog.FilterIndex = 1;
                        dialog.RestoreDirectory = true;

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            string filename = dialog.FileName;
                            try
                            {
                                BinaryReader file = new BinaryReader(File.Open(filename, FileMode.Create), Encoding.Default, false);
                                byte[] DATA = new byte[target1.Width * target1.Height * 4];
                                target1.GetData(DATA);
                                byte[] width, height;
                                // Writing mapsize
                                width = BitConverter.GetBytes(target1.Width);
                                height = BitConverter.GetBytes(target1.Height);
                                file.BaseStream.Write(width, 0, 4);
                                file.BaseStream.Write(height, 0, 4);
                                //Writing heights
                                file.BaseStream.Write(DATA, 0, DATA.Length);
                                file.Close();
                                file.Dispose();
                                Console.WriteLine("Saving suceeded. Filename: {0}", filename);
                            }
                            catch (Exception exp)
                            {
                                Console.WriteLine("Saving failed: {0}", exp);
                                System.Windows.Forms.MessageBox.Show("Saving failed", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                #endregion

                #region STRG_C
                if (Selection_type > 1 && KB_currentstate.IsKeyDown(Keys.LeftControl) && KB_currentstate.IsKeyDown(Keys.C) && KB_currentstate.IsKeyUp(Keys.LeftAlt) && STRG_C_pressed == false) // Copying Selection
                {
                    STRG_C_pressed = true;
                    segment.sizex = selection_size_X;
                    segment.sizey = selection_size_Y;
                    segment.data = new float[(segment.sizex + 1) * (segment.sizey + 1)];
                    target1.GetData(0, new Rectangle(selection_start_X, selection_start_Y, segment.sizex + 1, segment.sizey + 1), segment.data, 0, (segment.sizex + 1) * (segment.sizey + 1));
                }
                else if (STRG_C_pressed && (KB_currentstate.IsKeyUp(Keys.LeftControl) || KB_currentstate.IsKeyUp(Keys.C)))
                    STRG_C_pressed = false;
                #endregion

                #region STRG_V

                if (KB_currentstate.IsKeyDown(Keys.LeftControl) && KB_currentstate.IsKeyDown(Keys.V) && KB_currentstate.IsKeyUp(Keys.LeftAlt) && STRG_V_pressed == false && segment.data != null)
                {
                    if (segment.sizex < target1.Width && segment.sizey < target1.Height)
                    {
                        STRG_V_pressed = true;
                        Selection_type = -1;
                        IsBreak = 1;
                        DATA_target1 = new float[target1.Width * target1.Height];
                        target1.GetData(DATA_target1);
                        screen2worldcoo(M_currentstate.Position.ToVector2(), out selection_start_X, out selection_start_Y);
                        selection_start_X -= selection_size_X / 2;
                        selection_start_Y -= selection_size_Y / 2;
                        selection_size_X = segment.sizex;
                        selection_size_Y = segment.sizey;
                        if (selection_start_X < 0)
                            selection_start_X = 0;
                        if (selection_start_Y < 0)
                            selection_start_Y = 0;
                        if (selection_start_X + selection_size_X + 1 > target1.Width)
                            selection_start_X = target1.Width - selection_size_X - 1;
                        if (selection_start_Y + selection_size_Y + 1 > target1.Height)
                            selection_start_Y = target1.Height - selection_size_Y - 1;
                        selection_end_X = selection_start_X + selection_size_X;
                        selection_end_Y = selection_start_Y + selection_size_Y;
                        target1.SetData(0, new Rectangle(selection_start_X, selection_start_Y, segment.sizex + 1, segment.sizey + 1), segment.data, 0, (segment.sizex + 1) * (segment.sizey + 1));
                    }

                    //target1.SetData(0, new Rectangle(selection_start_X, selection_start_Y, segment.sizex, segment.sizey), segment.data, 0, segment.sizex * segment.sizey);
                }
                else if (STRG_V_pressed && (KB_currentstate.IsKeyUp(Keys.LeftControl) || KB_currentstate.IsKeyUp(Keys.V)))
                    STRG_V_pressed = false;

                if (Selection_type < 0 && M_currentstate.LeftButton == ButtonState.Pressed)
                {
                    if (M_oldstate.LeftButton == ButtonState.Released && M_currentstate.Position.X > start.X && M_currentstate.Position.X < end.X && M_currentstate.Position.Y > start.Y && M_currentstate.Position.Y < end.Y)
                    {
                        Selection_click_pos = M_currentstate.Position.ToVector2() - world2screencoo(selection_start_X, selection_start_Y);
                        Selection_type = -2;
                    }

                    if (Selection_type == -2)
                    {
                        screen2worldcoo(M_currentstate.Position.ToVector2() - Selection_click_pos, out selection_start_X, out selection_start_Y);
                        if (selection_start_X < 0)
                            selection_start_X = 0;
                        if (selection_start_Y < 0)
                            selection_start_Y = 0;
                        if (selection_start_X + selection_size_X + 1 > target1.Width)
                            selection_start_X = target1.Width - selection_size_X - 1;
                        if (selection_start_Y + selection_size_Y + 1 > target1.Height)
                            selection_start_Y = target1.Height - selection_size_Y - 1;
                        selection_end_X = selection_start_X + selection_size_X;
                        selection_end_Y = selection_start_Y + selection_size_Y;
                        target1.SetData(DATA_target1);
                        target1.SetData(0, new Rectangle(selection_start_X, selection_start_Y, segment.sizex + 1, segment.sizey + 1), segment.data, 0, (segment.sizex + 1) * (segment.sizey + 1));
                    }
                }
                else if (Selection_type == -2)
                    Selection_type = -1;
                #endregion

                #region STRG_ALT_C

                if (Selection_type > 1 && KB_currentstate.IsKeyDown(Keys.LeftControl) && KB_currentstate.IsKeyDown(Keys.LeftAlt) && KB_currentstate.IsKeyDown(Keys.C) && STRG_ALT_C_pressed == false)
                {
                    STRG_ALT_C_pressed = true;
                    try
                    {
                        string filename = System.IO.Directory.GetCurrentDirectory() + "\\STRG_SAVE";
                        BinaryReader file = new BinaryReader(File.Open(filename, FileMode.Create), Encoding.Default, false);
                        byte[] DATA = new byte[(selection_size_X + 1) * (selection_size_Y + 1) * 4];
                        target1.GetData(0, new Rectangle(selection_start_X, selection_start_Y, selection_size_X + 1, selection_size_Y + 1), DATA, 0, (selection_size_X + 1) * (selection_size_Y + 1) * 4);
                        byte[] width, height;
                        // Writing mapsize
                        width = BitConverter.GetBytes(selection_size_X + 1);
                        height = BitConverter.GetBytes(selection_size_Y + 1);
                        file.BaseStream.Write(width, 0, 4);
                        file.BaseStream.Write(height, 0, 4);
                        //Writing heights
                        file.BaseStream.Write(DATA, 0, DATA.Length);
                        file.Close();
                        file.Dispose();
                        Console.WriteLine("Saving suceeded. Filename: {0}", filename);
                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine("Saving failed: {0}", exp);
                        System.Windows.Forms.MessageBox.Show("Saving failed", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (STRG_ALT_C_pressed && (KB_currentstate.IsKeyUp(Keys.LeftControl) || KB_currentstate.IsKeyUp(Keys.LeftAlt) || KB_currentstate.IsKeyUp(Keys.C)))
                    STRG_ALT_C_pressed = false;

                #endregion

                #region STRG_ALT_V

                if (KB_currentstate.IsKeyDown(Keys.LeftControl) && KB_currentstate.IsKeyDown(Keys.LeftAlt) && KB_currentstate.IsKeyDown(Keys.V) && STRG_ALT_V_pressed == false)
                {
                    STRG_ALT_V_pressed = true;
                    STRG_V_pressed = true;
                    try
                    {
                        string filename = System.IO.Directory.GetCurrentDirectory() + "\\STRG_SAVE";

                        BinaryReader file = new BinaryReader(File.Open(filename, FileMode.Open), Encoding.Default, false);
                        byte[] DATA;
                        byte[] width = new byte[4], height = new byte[4];
                        int width_int, height_int;
                        file.Read(width, 0, 4);
                        file.Read(height, 0, 4);
                        width_int = BitConverter.ToInt32(width, 0);
                        height_int = BitConverter.ToInt32(height, 0);
                        //Reading the heights of the map
                        DATA = new byte[width_int * height_int * 4];
                        file.Read(DATA, 0, DATA.Length);
                        file.Close();
                        file.Dispose();
                        Console.WriteLine("Loading suceeded. Filename: {0}", filename);

                        segment.sizex = width_int - 1;
                        segment.sizey = height_int - 1;
                        if (segment.sizex < target1.Width && segment.sizey < target1.Height)
                        {
                            segment.data = new float[width_int * height_int];
                            Selection_type = -1;
                            IsBreak = 1;
                            DATA_target1 = new float[target1.Width * target1.Height];
                            target1.GetData(DATA_target1);
                            screen2worldcoo(M_currentstate.Position.ToVector2(), out selection_start_X, out selection_start_Y);
                            selection_start_X -= selection_size_X / 2;
                            selection_start_Y -= selection_size_Y / 2;
                            selection_size_X = segment.sizex;
                            selection_size_Y = segment.sizey;
                            if (selection_start_X < 0)
                                selection_start_X = 0;
                            if (selection_start_Y < 0)
                                selection_start_Y = 0;
                            if (selection_start_X + selection_size_X + 1 > target1.Width)
                                selection_start_X = target1.Width - selection_size_X - 1;
                            if (selection_start_Y + selection_size_Y + 1 > target1.Height)
                                selection_start_Y = target1.Height - selection_size_Y - 1;
                            selection_end_X = selection_start_X + selection_size_X;
                            selection_end_Y = selection_start_Y + selection_size_Y;
                            target1.SetData(0, new Rectangle(selection_start_X, selection_start_Y, segment.sizex + 1, segment.sizey + 1), DATA, 0, (segment.sizex + 1) * (segment.sizey + 1) * 4);

                            target1.GetData(0, new Rectangle(selection_start_X, selection_start_Y, segment.sizex + 1, segment.sizey + 1), segment.data, 0, (segment.sizex + 1) * (segment.sizey + 1));
                        }
                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine("Saving failed: {0}", exp);
                        System.Windows.Forms.MessageBox.Show("Loading Failed", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (STRG_ALT_V_pressed && (KB_currentstate.IsKeyUp(Keys.LeftControl) || KB_currentstate.IsKeyUp(Keys.LeftAlt) || KB_currentstate.IsKeyUp(Keys.V)))
                    STRG_ALT_V_pressed = false;

                #endregion

                #endregion

                render_effect.Parameters["zoom"].SetValue((float)Math.Pow(2, worldzoom));
                render_effect.Parameters["coos"].SetValue(worldpos);
                render_effect.Parameters["mousepos_X"].SetValue(mouse_worldpos_X);
                render_effect.Parameters["mousepos_Y"].SetValue(mouse_worldpos_Y);
                render_effect.Parameters["Selection_type"].SetValue(Selection_type);
                if (Selection_type > 0 || Selection_type < 0)
                {
                    if (selection_end_X >= selection_start_X)
                    {
                        render_effect.Parameters["selection_start_X"].SetValue(selection_start_X);
                        render_effect.Parameters["selection_end_X"].SetValue(selection_end_X);
                    }
                    else
                    {
                        render_effect.Parameters["selection_start_X"].SetValue(selection_end_X);
                        render_effect.Parameters["selection_end_X"].SetValue(selection_start_X);
                    }
                    if (selection_end_Y >= selection_start_Y)
                    {
                        render_effect.Parameters["selection_start_Y"].SetValue(selection_start_Y);
                        render_effect.Parameters["selection_end_Y"].SetValue(selection_end_Y);
                    }
                    else
                    {
                        render_effect.Parameters["selection_start_Y"].SetValue(selection_end_Y);
                        render_effect.Parameters["selection_end_Y"].SetValue(selection_start_Y);
                    }
                }
                #region Updating UI

                bool IsPressed = false;
                if (M_currentstate.Position.X > 20 && M_currentstate.Position.X < 55 && M_currentstate.Position.Y > 20 && M_currentstate.Position.Y < 55 && M_currentstate.LeftButton == ButtonState.Pressed)
                {
                    if (M_oldstate.LeftButton == ButtonState.Released)
                    {
                        IsBreak = (IsBreak + 1) % 2;
                        if (IsBreak == 0 && current_generation == 0)
                            target1.GetData(Generation0_DATA);
                        Selection_type = 0;
                    }
                    IsPressed = true;
                }
                else if (M_currentstate.Position.X > 20 && M_currentstate.Position.X < 55 && M_currentstate.Position.Y > 75 && M_currentstate.Position.Y < 110 && M_currentstate.LeftButton == ButtonState.Pressed)
                {
                    if (M_oldstate.LeftButton == ButtonState.Released && current_generation > 0)
                    {
                        current_generation = 0;
                        target1.SetData(Generation0_DATA);
                        IsBreak = 1;
                    }
                    IsPressed = true;
                }

                // Checking block buttons

                for (int i = 0; i < elements.Count; i++)
                {
                    if (M_currentstate.Position.X > 80 + 40 * i && M_currentstate.X < 100 + 40 * i && M_currentstate.Y > 20 && M_currentstate.Y < 40 && M_currentstate.LeftButton == ButtonState.Pressed)
                    {
                        selectedtype = elements[i].index;
                        IsPressed = true;
                    }
                }

                if (Selection_type == 0 && IsPressed == false)
                {
                    if (mouse_worldpos_X >= 0 && mouse_worldpos_X < target1.Width && mouse_worldpos_Y >= 0 && mouse_worldpos_Y < target1.Height && M_currentstate.LeftButton == ButtonState.Pressed)
                    {
                        SetPixel(mouse_worldpos_X, mouse_worldpos_Y, (float)selectedtype, target1);
                    }
                    else if (mouse_worldpos_X >= 0 && mouse_worldpos_X < target1.Width && mouse_worldpos_Y >= 0 && mouse_worldpos_Y < target1.Height && M_currentstate.RightButton == ButtonState.Pressed)
                    {
                        SetPixel(mouse_worldpos_X, mouse_worldpos_Y, 0f, target1);
                    }
                }

                #endregion
            }

            KB_oldstate = KB_currentstate;
            M_oldstate = M_currentstate;
            base.Update(gameTime);
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(finaltarget);
            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.SetRenderTarget(null);
            if (IsActive && IsBreak == 0)
            {
                timer++;
                if (timer + 0.5f >= Math.Pow(2, framespergeneration) && framespergeneration >= 0)
                {
                    timer = 0;
                    GraphicsDevice.SetRenderTarget(target2);
                    GraphicsDevice.Clear(Color.Black);
                    logic_effect.CurrentTechnique = logic_effect.Techniques[0];
                    spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, logic_effect, Matrix.Identity);
                    spriteBatch.Draw(target1, Vector2.Zero, Color.White);
                    spriteBatch.End();

                    logic_effect.CurrentTechnique = logic_effect.Techniques[1];
                    GraphicsDevice.SetRenderTarget(target1);
                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, logic_effect, Matrix.Identity);
                    spriteBatch.Draw(target2, Vector2.Zero, Color.White);
                    spriteBatch.End();
                    current_generation++;
                }
                else if (framespergeneration < 0)
                {
                    timer = 0;
                    for (int i = 0; i + 0.5f < Math.Pow(2, -framespergeneration); ++i)
                    {
                        GraphicsDevice.SetRenderTarget(target2);
                        logic_effect.CurrentTechnique = logic_effect.Techniques[0];
                        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, logic_effect, Matrix.Identity);
                        spriteBatch.Draw(target1, Vector2.Zero, Color.White);
                        spriteBatch.End();

                        logic_effect.CurrentTechnique = logic_effect.Techniques[1];
                        GraphicsDevice.SetRenderTarget(target1);
                        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, logic_effect, Matrix.Identity);
                        spriteBatch.Draw(target2, Vector2.Zero, Color.White);
                        spriteBatch.End();
                        current_generation++;
                    }
                }
            }
            render_effect.Parameters["IsDisplayHighlighted"].SetValue(IsDisplayHiglighted);
            if (worldzoom >= 0)
            {
                render_effect.Parameters["mode"].SetValue(0);
                render_effect.Parameters["logictex"].SetValue((Texture2D)target1);
                GraphicsDevice.SetRenderTarget(finaltarget);
                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, render_effect, Matrix.Identity);
                spriteBatch.Draw(finaltarget, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
            else
            {
                render_effect.Parameters["mode"].SetValue(1);
                render_effect.Parameters["logictex"].SetValue((Texture2D)target1);
                GraphicsDevice.SetRenderTarget(originalsizetarget);
                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, render_effect, Matrix.Identity);
                spriteBatch.Draw(originalsizetarget, Vector2.Zero, Color.White);
                spriteBatch.End();

                GraphicsDevice.SetRenderTarget(originalsizetarget2);
                spriteBatch.Begin();
                spriteBatch.Draw(originalsizetarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, (float)Math.Sqrt(Math.Pow(2, worldzoom)), SpriteEffects.None, 0);
                spriteBatch.End();

                GraphicsDevice.SetRenderTarget(originalsizetarget);
                spriteBatch.Begin();
                spriteBatch.Draw(originalsizetarget2, Vector2.Zero, null, Color.White, 0, Vector2.Zero, (float)Math.Sqrt(Math.Pow(2, worldzoom)), SpriteEffects.None, 0);
                spriteBatch.End();

                render_effect.Parameters["mode"].SetValue(2);
                render_effect.Parameters["logictex"].SetValue((Texture2D)originalsizetarget);
                GraphicsDevice.SetRenderTarget(finaltarget);
                spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, render_effect, Matrix.Identity);
                spriteBatch.Draw(finaltarget, Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin();
            spriteBatch.Draw(finaltarget, Vector2.Zero, Color.White);

            #region Drawing UI

            for (int i = 0; i < elements.Count; i++)
            {
                spriteBatch.Draw(elements[i].tex, new Vector2(80 + i * 40, 20), Color.White);
            }

            /*spriteBatch.Draw(button_partikel_top, new Vector2(80, 20), Color.White);
            spriteBatch.Draw(button_partikel_right, new Vector2(120, 20), Color.White);
            spriteBatch.Draw(button_partikel_bottom, new Vector2(160, 20), Color.White);
            spriteBatch.Draw(button_partikel_left, new Vector2(200, 20), Color.White);
            spriteBatch.Draw(button_partikel_multifunction, new Vector2(240, 20), Color.White);
            spriteBatch.Draw(button_partikel_blue, new Vector2(280, 20), Color.White);
            spriteBatch.Draw(button_partikel_lowred, new Vector2(320, 20), Color.White);
            spriteBatch.Draw(button_partikel_highred, new Vector2(360, 20), Color.White);*/
            if (IsBreak == 1)
                spriteBatch.Draw(button_play, new Vector2(20, 20), Color.White);
            else
                spriteBatch.Draw(button_break, new Vector2(20, 20), Color.White);
            if (current_generation > 0)
                spriteBatch.Draw(button_reset, new Vector2(20, 75), Color.White);
            else
                spriteBatch.Draw(button_reset, new Vector2(20, 75), Color.Gray);

            #endregion

            //spriteBatch.Draw(target1, Vector2.Zero, Color.White);
            spriteBatch.DrawString(font, "Zoom: " + Math.Pow(2, worldzoom).ToString(), new Vector2(20, 150), Color.Red);
            spriteBatch.DrawString(font, "Speed: 2^" + framespergeneration.ToString(), new Vector2(20, 170), Color.Red);
            spriteBatch.DrawString(font, "World position: (" + mouse_worldpos_X.ToString() + ", " + mouse_worldpos_Y.ToString() + ")", new Vector2(20, 190), Color.Red);
            spriteBatch.DrawString(font, "Generation: " + current_generation.ToString(), new Vector2(20, 210), Color.Red);
            if (mouse_worldpos_X >= 0 && mouse_worldpos_X < worldsizex && mouse_worldpos_Y >= 0 && mouse_worldpos_Y < worldsizey)
            {
                float[] type = new float[1];
                //target1.GetData(0, 0, new Rectangle(mouse_worldpos_X, mouse_worldpos_Y, 1, 1), type, 0, 1);
                spriteBatch.DrawString(font, "Type: " + ((int)(type[0] + 0.5f)).ToString(), new Vector2(20, 230), Color.Red);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
    public static class ArrayExtensions
    {
        public static void Init<T>(this T[] array, T defaultVaue)
        {
            if (array == null)
                return;
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = defaultVaue;
            }
        }
    }
}
