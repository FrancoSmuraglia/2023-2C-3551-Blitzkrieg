using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Collisions;


namespace TGC.MonoGame.TP
{    
    public class MenuInicio
    {
        public Texture2D Logo;
        public Texture2D Fondo {get; set;}
        public Vector2 PantallaTamanio {get; set;}
        public SpriteFont Font {get; set;}
        public MenuBotones SeccionDeBotones {get; set;}
        public TGCGame juego {get; set;}
        
        private Rectangle LogoRect {get; set;}

        
        public SoundEffectInstance Cortina { get; set; }
        
        public SoundEffectInstance CortinaImpacto { get; set; }
        
        public MenuInicio(GraphicsDevice graphicsDevice, Vector2 pantalla, List<Button> botones, SpriteFont fuente = null){            
            PantallaTamanio = pantalla;
            Font = fuente;
            SeccionDeBotones = new MenuBotones(pantalla, botones, fuente);
                        
        }

        Matrix transform = Matrix.Identity;
        public void Draw(SpriteBatch spriteBatch){
            spriteBatch.Begin(0, null, null, null, null, null, transform);    
            LogoRect = new Rectangle((int)PantallaTamanio.X / 2 - Logo.Width / 3 / 2, Logo.Height / 3 / 2, Logo.Width / 3, Logo.Height / 3);
            
            spriteBatch.Draw(Logo, LogoRect, new Color(1,1,1,1f));
            SeccionDeBotones.Draw(spriteBatch);
            
            spriteBatch.End();
        }

        public void Update(MouseState currentMouseState){
            SeccionDeBotones.Update(currentMouseState, juego);
        }

    }
}