using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Collisions;


namespace TGC.MonoGame.TP
{    
    public class Button
    {
        
        public float Scale = 0.3f;
        private Texture2D Texture;

        public Color Hover = Color.DarkGray;
        public Color NotHover = Color.White;
        public Color TextHover = Color.White;
        public Color NotTextHover = Color.Black;
        public Color SobreMouse = Color.White;
        public Color Fondo = Color.White;
        public Color Letras = Color.Black;

        public Action<TGCGame> Click;

        public bool Clicked { get; protected set; }

        public bool IsSelected { get; set; }

        public Vector2 Position { get; set; }

        private MouseState Anterior {get; set;}
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width * Scale), (int)(Texture.Height * Scale));
            }
        }
        public Button(Texture2D texture, Vector2 position, string texto = null, float escala = 1f)
        {
            Texture = texture;
            
            Scale = escala;

            Position = position - new Vector2(texture.Width, texture.Height)*Scale/2;

            Text = texto;

        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont fuente = null)
        {
            if (IsSelected)
                Fondo = Hover;
            else
                Fondo = NotHover;
            
            Rectangle a = this.Rectangle;
            
            spriteBatch.Draw(Texture, Position, null, Fondo, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0);

            DrawText(spriteBatch, fuente);
        }

        protected void DrawText(SpriteBatch spriteBatch, SpriteFont fuente = null)
        {
            if (string.IsNullOrEmpty(Text) || fuente  == null)
                return;

            if (IsSelected)
                PenColour = TextHover;
            else
                PenColour = NotTextHover;

            float x = ((Rectangle.X + (Rectangle.Width / 2)) - (fuente.MeasureString(Text).X / 2)) - Origin.X;
            float y = ((Rectangle.Y + (Rectangle.Height / 2)) - (fuente.MeasureString(Text).Y / 2)) - Origin.Y;


            spriteBatch.DrawString(fuente, Text, new Vector2(x, y), PenColour, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0.1f);
        }
        
        public void Update(MouseState currentMouseState, TGCGame juegoActual)
        {
            Fondo = Color.White;

            Clicked = false;
            IsSelected = false;
            if(Rectangle.Contains(currentMouseState.X, currentMouseState.Y)){
                if(currentMouseState.LeftButton.Equals(ButtonState.Released) && Anterior.LeftButton.Equals(ButtonState.Pressed)){
                    Click?.Invoke(juegoActual);
                }

                IsSelected = true;
            }
            Anterior = currentMouseState;
        }

        /*public virtual void OnClick()
        {
            Click?.Invoke();

            IsSelected = true;
        }*/


    public Vector2 Origin { get; set; }

    public string Text { get; set; }

    public Color PenColour { get; set; } = Color.Red;
    }
}