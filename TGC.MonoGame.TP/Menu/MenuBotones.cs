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
    public class MenuBotones
    {
        public List<Button> Botones {get; set;}
        public Vector2 PantallaTamanio {get; set;}
        public SpriteFont Font {get; set;}
        public MenuBotones(Vector2 pantalla, List<Button> botones, SpriteFont fuente = null){
            PantallaTamanio = pantalla;
            Botones = botones;
            Font = fuente;
        }

        public void Draw(SpriteBatch spriteBatch){
            var rectangulo = new Rectangle(0, 0, (int)PantallaTamanio.X, (int)PantallaTamanio.Y);
            Botones.ForEach(boton => boton.Draw(spriteBatch, Font));            
        }

        public void Update(MouseState currentMouseState, TGCGame juegoActual){
            foreach (var boton in Botones){
                boton.Update(currentMouseState, juegoActual);
                /*boton.IsSelected = false;
                if(boton.Rectangle.Contains(currentMouseState.X, currentMouseState.Y)){
                    if(currentMouseState.LeftButton.Equals(ButtonState.Pressed)){
                        Console.WriteLine("Click");
                        boton.Click?.Invoke(juegoActual);
                    }

                    boton.IsSelected = true;
                }*/
            }
            //mouse.Location.X = new Point(currentMouseState.Position.X, currentMouseState.Position.Y) ;
        }

    }
}