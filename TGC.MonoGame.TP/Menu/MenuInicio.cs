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
        public enum EstadoMenuInicio{
            Bajando,
            Quieto,
            Inicio
        }
        public EstadoMenuInicio Estado;
        public Texture2D Logo;
        public Texture2D Fondo {get; set;}
        public Vector2 PantallaTamanio {get; set;}
        public SpriteFont Font {get; set;}
        public MenuBotones SeccionDeBotones {get; set;}
        public TGCGame juego {get; set;}
        
        private Rectangle FondoRect {get; set;}
        private Rectangle LogoRect {get; set;}
        List<Vector2> posicionesOriginales;

        private BasicEffect giroLogo;
        
        public SoundEffectInstance Cortina { get; set; }
        
        public SoundEffectInstance CortinaImpacto { get; set; }
        
        public MenuInicio(GraphicsDevice graphicsDevice, Vector2 pantalla, List<Button> botones, SpriteFont fuente = null){
            int width = (int)pantalla.X;
            int height = (int)pantalla.Y;
            
            /*Fondo = new Texture2D(graphicsDevice, width/2, height/2);
            Color[] data = new Color[width/2 * height/2];
            for(int pixel = 0; pixel < data.Length; pixel++)
            {
                //the function applies the color according to the specified pixel
                data[pixel] = Color.Black;
            }
            Fondo.SetData(data);*/
            PantallaTamanio = pantalla;
            Font = fuente;
            SeccionDeBotones = new MenuBotones(pantalla, botones, fuente);
            posicionesOriginales = botones.Select(boton => boton.Position).ToList();
            
            FondoRect = new Rectangle(0, 0, (int)PantallaTamanio.X, (int)PantallaTamanio.Y);
            giroLogo = new BasicEffect(graphicsDevice);
            
        }

        float giro = 0f;
        public void Draw(SpriteBatch spriteBatch){
            
            spriteBatch.Begin();    
            spriteBatch.Draw(Fondo, FondoRect, new Color(1,1,1,1f));
            LogoRect = new Rectangle((int)PantallaTamanio.X / 2 - Logo.Width / 3 / 2, Logo.Height / 3 / 2, Logo.Width / 3, Logo.Height / 3);
            //spriteBatch.Draw(Logo, LogoRect, new Color(1,1,1,1f));
            
            spriteBatch.Draw(Logo, LogoRect, new Color(1,1,1,1f));
            SeccionDeBotones.Draw(spriteBatch);
            
            spriteBatch.End();
        }

        public void Update(MouseState currentMouseState){
            SeccionDeBotones.Update(currentMouseState, juego);

            //mouse.Location.X = new Point(currentMouseState.Position.X, currentMouseState.Position.Y) ;
        }

    }
}