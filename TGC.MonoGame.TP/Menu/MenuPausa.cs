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
    public class MenuPausa
    {
        public Texture2D Fondo {get; set;}
        public Vector2 PantallaTamanio {get; set;}
        public SpriteFont Font {get; set;}
        public MenuBotones SeccionDeBotones {get; set;}
        public TGCGame juego {get; set;}

        public MenuPausa(Texture2D fondo, Vector2 pantalla, List<Button> botones, SpriteFont fuente = null){
            Fondo = fondo;
            PantallaTamanio = pantalla;
            Font = fuente;
            SeccionDeBotones = new MenuBotones(pantalla, botones, fuente);
        }

        public void Draw(SpriteBatch spriteBatch){
            var rectangulo = new Rectangle(0, 0, (int)PantallaTamanio.X, (int)PantallaTamanio.Y);
            spriteBatch.Begin();


            
            spriteBatch.Draw(Fondo, rectangulo, new Color(1,1,1,1f));
            SeccionDeBotones.Draw(spriteBatch);
            
            spriteBatch.End();
        }

        public void Update(MouseState currentMouseState){
            SeccionDeBotones.Update(currentMouseState, juego);
            //mouse.Location.X = new Point(currentMouseState.Position.X, currentMouseState.Position.Y) ;
        }

    }
}