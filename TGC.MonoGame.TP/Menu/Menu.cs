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
    public class Menu
    {
        public List<Button> Botones {get; set;}
        public Texture2D Fondo {get; set;}
        public Vector2 PantallaTamanio {get; set;}
        public SpriteFont Font {get; set;}
        public Menu(Texture2D fondo, Vector2 pantalla, List<Button> botones, SpriteFont fuente = null){
            Fondo = fondo;
            PantallaTamanio = pantalla;
            Botones = botones;
            Font = fuente;
        }

        public void Draw(SpriteBatch spriteBatch){
            var rectangulo = new Rectangle(0, 0, (int)PantallaTamanio.X, (int)PantallaTamanio.Y);
            spriteBatch.Begin();


            
            spriteBatch.Draw(Fondo, rectangulo, new Color(1,1,1,1f));
            Botones.ForEach(boton => boton.Draw(spriteBatch, Font));
            
            spriteBatch.End();
        }

        public void Update(MouseState currentMouseState){
            foreach (var boton in Botones){
                boton.IsSelected = false;
                if(boton.Rectangle.Contains(currentMouseState.X, currentMouseState.Y)){
                    if(currentMouseState.RightButton.Equals(ButtonState.Pressed)){}

                    boton.IsSelected = true;
                    Console.WriteLine("Está sobre el botón '" + boton.Text + "' y está en " + currentMouseState.X + " " + currentMouseState.Y);
                }
            }
            //mouse.Location.X = new Point(currentMouseState.Position.X, currentMouseState.Position.Y) ;
        }

    }
}