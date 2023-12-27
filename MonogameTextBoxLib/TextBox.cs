using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FormElementsLib;
using System;
using MonoHelper;
using System.Timers;
using System.Text.RegularExpressions;
using System.Text;

namespace MonogameTextBoxLib
{
    public class TextBox : TextFE
    {
        Texture2D textureinit, textureon, texturepressed, textureline;
        public string regexcharfilter;
        SpriteFont font;
        int fontsize, fontinitsize;
        int lineposition;
        int pixellimit;
        public int charlimit = int.MaxValue;
        bool line=false;
        string pkey="no";
        public char passchar = '\n';
        Timer timer;
        Timer pkeytimer;
        public event EventHandler textchanged;
        Microsoft.Xna.Framework.Color linecolor = Microsoft.Xna.Framework.Color.Black;

        public TextBox(GraphicsDevice graphics, int x, int y, int width, SpriteFont _font, int _fontinitsize, int _fontsize)
        {
            TBCreate(graphics, x, y, width, -1, _font, _fontinitsize, _fontsize, ".");
        }

        public TextBox(GraphicsDevice graphics, int x, int y, int width, int height, SpriteFont _font, int _fontinitsize, int _fontsize)
        {
            TBCreate(graphics, x, y, width, height, _font, _fontinitsize, _fontsize, ".");
        }

        public TextBox(GraphicsDevice graphics, int x, int y, int width, int height, SpriteFont _font, int _fontinitsize, int _fontsize, string regexcharfilter)
        {
            TBCreate(graphics, x, y, width, height, _font, _fontinitsize, _fontsize, regexcharfilter);
        }

        private void TextBox_Click(object sender, ClickEventArgs e)
        {
            mode = pressmode;
            line = true;
            timer.Enabled = true;
            int pos = (int)e.mouseState.X - (int)Location.X-3;
            string output = text;
            if (passchar != '\n') output = new string(passchar, text.Length);
            for (int i=0; i< output.Length; i++)
            {
                int distance = pos-MHeleper.GetTextWidth(output.Substring(0, i+1), font, fontinitsize, fontsize);
                if (distance < 0)
                {
                    if (Math.Abs(pos - MHeleper.GetTextWidth(output.Substring(0, i + 1), font, fontinitsize, fontsize)) < Math.Abs(pos - MHeleper.GetTextWidth(output.Substring(0, i), font, fontinitsize, fontsize)))
                    {
                        lineposition = i+1;
                    }
                    else lineposition = i;
                    return;
                }
            }

            lineposition = output.Length;
        }

        public override void Dispose()
        {
            textureline.Dispose();
            textureon.Dispose();
            textureinit.Dispose();
            texturepressed.Dispose();
        }

        public void TBCreate(GraphicsDevice graphics, int x, int y, int width, int h, SpriteFont _font, int _fontinitsize, int _fontsize, string _regexcharfilter)
        {
            textcolor = Microsoft.Xna.Framework.Color.Black;
            color = Color.White;
            pkeytimer = new Timer();
            timer = new Timer(500);
            timer.Elapsed += Timer_Elapsed;
            regexcharfilter = _regexcharfilter;
            font = _font;
            fontinitsize = _fontinitsize;
            fontsize = _fontsize;
            lineposition = 0;
            text = "";
            int height = MHeleper.GetFontHeight(font, _fontinitsize, _fontsize)+7;
            Size = new System.Drawing.Size(width, height);
            if (h == -1) Location = new Vector2(x, y);
            else Location = new Vector2(x, y + (h - height) / 2);
            pixellimit = width-6;
            CreateGraphics(graphics);
            Click += TextBox_Click;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            line = !line;
        }

        public TextBox(GraphicsDevice graphics, int x, int y, int width, SpriteFont _font, int _fontinitsize, int _fontsize, string _regexcharfilter)
        {
            TBCreate(graphics, x, y, width, -1, _font, _fontinitsize, _fontsize, ".");
        }

        public void CreateGraphics(GraphicsDevice graphics)
        {
            maketextboxcolor(graphics, ref textureinit, new Microsoft.Xna.Framework.Color(255, 255, 255), new Microsoft.Xna.Framework.Color(122, 122, 122));
            maketextboxcolor(graphics, ref textureon, new Microsoft.Xna.Framework.Color(255, 255, 255), new Microsoft.Xna.Framework.Color(23, 23, 23));
            maketextboxcolor(graphics, ref texturepressed, new Microsoft.Xna.Framework.Color(255, 255, 255), new Microsoft.Xna.Framework.Color(0, 120, 215));
            textureline = new Texture2D(graphics, 1, Size.Height - 7);
            Microsoft.Xna.Framework.Color[] colordata = new Microsoft.Xna.Framework.Color[Size.Height - 7];
            for (int i = 0; i < Size.Height - 7; i++)
            {
                colordata[i] = new Microsoft.Xna.Framework.Color(0, 0, 0);
            }
            textureline.SetData<Microsoft.Xna.Framework.Color>(colordata);
        }

        private void maketextboxcolor(GraphicsDevice graphicsDevice, ref Texture2D texture, Microsoft.Xna.Framework.Color colorin, Microsoft.Xna.Framework.Color colorout)
        {
            texture = new Texture2D(graphicsDevice, Size.Width, Size.Height);
            Microsoft.Xna.Framework.Color[] colordata = new Microsoft.Xna.Framework.Color[Size.Width * Size.Height];
            for (int i = 0; i < Size.Width * Size.Height; i++) colordata[i] = colorin;
            for (int i = 0; i < Size.Width; i++) colordata[i] = colorout;
            for (int i = 0; i < Size.Width; i++) colordata[(Size.Height - 1) * Size.Width + i] = colorout;
            for (int i = 0; i < Size.Height; i++) colordata[i * Size.Width] = colorout;
            for (int i = 0; i < Size.Height; i++) colordata[i * Size.Width + Size.Width - 1] = colorout;
            texture.SetData<Microsoft.Xna.Framework.Color>(colordata);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (visible)
            {
                string output = text;
                if (passchar != '\n') output = new string(passchar, text.Length);
                switch (mode)
                {
                    case 0:
                        spriteBatch.Draw(textureinit, Location, color);
                        break;
                    case 1:
                        spriteBatch.Draw(textureon, Location, color);
                        break;
                    case 2:
                        spriteBatch.Draw(texturepressed, Location, color);
                        if (line) spriteBatch.Draw(textureline, new Vector2(MHeleper.GetTextWidth(output.Substring(0, lineposition), font, fontinitsize, fontsize) + 3 + Location.X, Location.Y + 3), linecolor);
                        break;
                }
                spriteBatch.DrawString(font, output, new Vector2(Location.X + 3, Location.Y + 3), textcolor, 0f, new Vector2(0, 0), ((float)fontsize / fontinitsize), SpriteEffects.None, 1f);
            }
        }

        public override void OnElement()
        {
            if (mode != pressmode) mode = onmode;
        }

        public override void Release()
        {
            if (mode != pressmode) mode = initmode;
            pressed = false;
        }

        public override void Check(MouseState mouse, KeyboardState keyboard)
        {
            if (visible)
            {
                CheckClick(mouse);
                if (mode == pressmode)
                {
                    char key = 'a';
                    if (TryConvertKeyboardInput(keyboard, ref key)&&(text.Length<charlimit))
                    {   
                        if (pkey != key.ToString())
                        {
                            string output = text;
                            if (passchar != '\n') output = new string(passchar, text.Length);
                            StringBuilder builder = new StringBuilder(text);
                            builder.Insert(lineposition, key);
                            StringBuilder builderout = new StringBuilder(output);
                            char outkey = key;
                            if (passchar != '\n') outkey = passchar;
                            builderout.Insert(lineposition, outkey);
                            if (MHeleper.GetTextWidth(builderout.ToString(), font, fontinitsize, fontsize) <= pixellimit)
                            {
                                text = builder.ToString();
                                lineposition++;
                                StartPKeyTimer(key.ToString());
                            }
                        }
                        else
                        {

                        }
                    }
                    else
                    {

                    }
                }
            }
        }

        public void StartPKeyTimer(string str)
        {
            pkeytimer.Stop();
            pkeytimer.Start();
            pkeytimer.Interval = 100;
            pkeytimer.Elapsed += Pkeytimer_Elapsed;
            pkey = str;
            pkeytimer.Enabled = true;
            textchanged?.Invoke(null, null);
        }

        private void Pkeytimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            pkeytimer.Enabled = false;
            pkey = "no";
        }

        public Action changecolor_action;
        public Action changecolor_actionfinally;



        public void MakeAction()
        {
            try
            {
                changecolor_action();
                textcolor = Color.Black;
            }
            catch (Exception)
            {
                textcolor = Color.Red;
            }
            finally 
            {
                if (changecolor_actionfinally != null) changecolor_actionfinally();
            }
        }

        public bool MakeAction(Action _changecolor_action, Action _changecolor_actionfinally)
        {
            try
            {
                _changecolor_action();
                textcolor = Color.Black;
                return true;
            }
            catch (Exception)
            {
                textcolor = Color.Red;
                return false;
            }
            finally
            {
                if (changecolor_actionfinally != null) _changecolor_actionfinally();
            }
        }

        public bool TryConvertKeyboardInput(KeyboardState keyboard, ref char key)
        {
            Keys[] keys = keyboard.GetPressedKeys();
            bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            bool capslock = keyboard.CapsLock;
            bool bigletter;
            if (capslock)
            {
                bigletter = !shift;
            }
            else
            {
                bigletter = shift;
            }

            if (keys.Length > 0+Convert.ToInt32(shift))
            {
                Keys opkey;
                bool psh = keys[0] == Keys.LeftShift || keys[0] == Keys.RightShift;
                if (psh) { opkey = keys[1]; }
                else opkey = keys[0];
                timer.Stop(); timer.Start(); line = true;
                switch (opkey)
                {
                    //Alphabet keys
                    case Keys.A: if (bigletter) { key = 'A'; } else { key = 'a'; } return true;
                    case Keys.B: if (bigletter) { key = 'B'; } else { key = 'b'; } return true;
                    case Keys.C: if (bigletter) { key = 'C'; } else { key = 'c'; } return true;
                    case Keys.D: if (bigletter) { key = 'D'; } else { key = 'd'; } return true;
                    case Keys.E: if (bigletter) { key = 'E'; } else { key = 'e'; } return true;
                    case Keys.F: if (bigletter) { key = 'F'; } else { key = 'f'; } return true;
                    case Keys.G: if (bigletter) { key = 'G'; } else { key = 'g'; } return true;
                    case Keys.H: if (bigletter) { key = 'H'; } else { key = 'h'; } return true;
                    case Keys.I: if (bigletter) { key = 'I'; } else { key = 'i'; } return true;
                    case Keys.J: if (bigletter) { key = 'J'; } else { key = 'j'; } return true;
                    case Keys.K: if (bigletter) { key = 'K'; } else { key = 'k'; } return true;
                    case Keys.L: if (bigletter) { key = 'L'; } else { key = 'l'; } return true;
                    case Keys.M: if (bigletter) { key = 'M'; } else { key = 'm'; } return true;
                    case Keys.N: if (bigletter) { key = 'N'; } else { key = 'n'; } return true;
                    case Keys.O: if (bigletter) { key = 'O'; } else { key = 'o'; } return true;
                    case Keys.P: if (bigletter) { key = 'P'; } else { key = 'p'; } return true;
                    case Keys.Q: if (bigletter) { key = 'Q'; } else { key = 'q'; } return true;
                    case Keys.R: if (bigletter) { key = 'R'; } else { key = 'r'; } return true;
                    case Keys.S: if (bigletter) { key = 'S'; } else { key = 's'; } return true;
                    case Keys.T: if (bigletter) { key = 'T'; } else { key = 't'; } return true;
                    case Keys.U: if (bigletter) { key = 'U'; } else { key = 'u'; } return true;
                    case Keys.V: if (bigletter) { key = 'V'; } else { key = 'v'; } return true;
                    case Keys.W: if (bigletter) { key = 'W'; } else { key = 'w'; } return true;
                    case Keys.X: if (bigletter) { key = 'X'; } else { key = 'x'; } return true;
                    case Keys.Y: if (bigletter) { key = 'Y'; } else { key = 'y'; } return true;
                    case Keys.Z: if (bigletter) { key = 'Z'; } else { key = 'z'; } return true;

                    //Decimal keys
                    case Keys.D0: if (shift) { key = ')'; } else { key = '0'; } return true;
                    case Keys.D1: if (shift) { key = '!'; } else { key = '1'; } return true;
                    case Keys.D2: if (shift) { key = '@'; } else { key = '2'; } return true;
                    case Keys.D3: if (shift) { key = '#'; } else { key = '3'; } return true;
                    case Keys.D4: if (shift) { key = '$'; } else { key = '4'; } return true;
                    case Keys.D5: if (shift) { key = '%'; } else { key = '5'; } return true;
                    case Keys.D6: if (shift) { key = '^'; } else { key = '6'; } return true;
                    case Keys.D7: if (shift) { key = '&'; } else { key = '7'; } return true;
                    case Keys.D8: if (shift) { key = '*'; } else { key = '8'; } return true;
                    case Keys.D9: if (shift) { key = '('; } else { key = '9'; } return true;

                    //Decimal numpad keys
                    case Keys.NumPad0: key = '0'; return true;
                    case Keys.NumPad1: key = '1'; return true;
                    case Keys.NumPad2: key = '2'; return true;
                    case Keys.NumPad3: key = '3'; return true;
                    case Keys.NumPad4: key = '4'; return true;
                    case Keys.NumPad5: key = '5'; return true;
                    case Keys.NumPad6: key = '6'; return true;
                    case Keys.NumPad7: key = '7'; return true;
                    case Keys.NumPad8: key = '8'; return true;
                    case Keys.NumPad9: key = '9'; return true;

                    //Special keys
                    case Keys.OemTilde: if (shift) { key = '~'; } else { key = '`'; } return true;
                    case Keys.OemSemicolon: if (shift) { key = ':'; } else { key = ';'; } return true;
                    case Keys.OemQuotes: if (shift) { key = '"'; } else { key = '\''; } return true;
                    case Keys.OemQuestion: if (shift) { key = '?'; } else { key = '/'; } return true;
                    case Keys.OemPlus: if (shift) { key = '+'; } else { key = '='; } return true;
                    case Keys.OemPipe: if (shift) { key = '|'; } else { key = '\\'; } return true;
                    case Keys.OemPeriod: if (shift) { key = '>'; } else { key = '.'; } return true;
                    case Keys.OemOpenBrackets: if (shift) { key = '{'; } else { key = '['; } return true;
                    case Keys.OemCloseBrackets: if (shift) { key = '}'; } else { key = ']'; } return true;
                    case Keys.OemMinus: if (shift) { key = '_'; } else { key = '-'; } return true;
                    case Keys.OemComma: if (shift) { key = '<'; } else { key = ','; } return true;
                    case Keys.Space: key = ' '; return true;
                    case Keys.Back: 
                        if ((lineposition != 0)&&(pkey!="back")) 
                        {
                            text = text.Remove(lineposition - 1, 1); 
                            lineposition--;
                            StartPKeyTimer("back");
                        } 
                        return false;
                    case Keys.Delete:
                        if ((lineposition != text.Length) && (pkey != "del"))
                        {
                            text = text.Remove(lineposition, 1);
                            StartPKeyTimer("del");
                        }
                        return false;
                    case Keys.Left:
                        if ((lineposition != 0) && (pkey != "left")) { lineposition--; timer.Stop(); timer.Start(); line = true; StartPKeyTimer("left");  }
                        return false;
                    case Keys.Right:
                        if ((lineposition != text.Length) && (pkey != "right"))
                        {
                            lineposition++;
                            StartPKeyTimer("right");
                            timer.Stop(); timer.Start(); line = true;
                        }
                        return false;
                    case Keys.End:
                        lineposition = text.Length;
                        timer.Stop(); timer.Start(); line = true;
                        break;
                    case Keys.Home:
                        lineposition = 0;
                        timer.Stop(); timer.Start(); line = true;
                        break;
                }
            }

            key = (char)0;
            return false;
        }
    }
}
