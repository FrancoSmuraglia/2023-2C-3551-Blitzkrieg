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
    public class MenuPausa
    {
        public enum EstadoMenuPausa{
            Bajando,
            Quieto,
            Inicio
        }
        public EstadoMenuPausa Estado;
        public Texture2D Logo;
        public Texture2D Fondo {get; set;}
        public Vector2 PantallaTamanio {get; set;}
        public SpriteFont Font {get; set;}
        public MenuBotones SeccionDeBotones {get; set;}
        public TGCGame juego {get; set;}
        
        private Rectangle FondoRect {get; set;}
        private Rectangle LogoRect {get; set;}
        List<Vector2> posicionesOriginales;


        
        public SoundEffectInstance Cortina { get; set; }
        
        public SoundEffectInstance CortinaImpacto { get; set; }
        
        public MenuPausa(Texture2D fondo, Vector2 pantalla, List<Button> botones, SpriteFont fuente = null){
            Fondo = fondo;
            PantallaTamanio = pantalla;
            Font = fuente;
            SeccionDeBotones = new MenuBotones(pantalla, botones, fuente);
            posicionesOriginales = botones.Select(boton => boton.Position).ToList();
            Estado = EstadoMenuPausa.Bajando;            
        }

        public void Draw(SpriteBatch spriteBatch){
            spriteBatch.Begin();    

            switch (Estado)
            {
                case EstadoMenuPausa.Inicio:
                    IniciarCortina();
                    Estado = EstadoMenuPausa.Bajando;
                    break;
                case EstadoMenuPausa.Bajando:
                    BajarMenu();
                    break;
            }

            //FondoRect = new Rectangle(0, 0, (int)PantallaTamanio.X, (int)PantallaTamanio.Y);
            spriteBatch.Draw(Fondo, FondoRect, new Color(1,1,1,1f));
            spriteBatch.Draw(Logo, LogoRect, new Color(1,1,1,1f));
            SeccionDeBotones.Draw(spriteBatch);
            
            spriteBatch.End();
        }

        public void IniciarCortina()
        {
            FondoRect = new Rectangle(0, (int)-PantallaTamanio.Y, (int)PantallaTamanio.X, (int)PantallaTamanio.Y);
            LogoRect = new Rectangle((int)PantallaTamanio.X / 2 - Logo.Width / 3 / 2, Logo.Height / 3 / 2 - (int)PantallaTamanio.Y, Logo.Width / 3, Logo.Height / 3);
            for (int i = 0; i < SeccionDeBotones.Botones.Count; i++)
            {
                SeccionDeBotones.Botones[i].Position
                    = new Vector2(SeccionDeBotones.Botones[i].Position.X,
                                  posicionesOriginales[i].Y - PantallaTamanio.Y);
            }
        }

        public void BajarMenu(){
            Cortina.Play();
            if(FondoRect.Y < 0){
                FondoRect = new Rectangle(0, FondoRect.Y+10, (int)PantallaTamanio.X, (int)PantallaTamanio.Y);
                LogoRect = new Rectangle((int)PantallaTamanio.X/2 - Logo.Width/3/2, LogoRect.Y+10, Logo.Width/3, Logo.Height/3);
                SeccionDeBotones.Botones.ForEach(boton => boton.Position = new Vector2(boton.Position.X, boton.Position.Y+10));
            }
            else{
                Estado = EstadoMenuPausa.Quieto;
                CortinaImpacto.Play();
                Cortina.Stop();
            }
            Console.WriteLine(FondoRect.X + " " + FondoRect.Y);

            
        }

        public void Update(MouseState currentMouseState){
            SeccionDeBotones.Update(currentMouseState, juego);

            //mouse.Location.X = new Point(currentMouseState.Position.X, currentMouseState.Position.Y) ;
        }

    }
}