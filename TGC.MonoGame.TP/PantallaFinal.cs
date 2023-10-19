using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BepuPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Collisions;

namespace TGC.MonoGame.TP
{
    public class PantallaFinal
    {
        public enum EstadoPantallaFinal
        {
            Encendido,
            Apagado
        }
        public EstadoPantallaFinal Estado;
        MenuBotones SeccionDeBotones { get; set; }
        public SpriteFont Font { get; set; }
        public Vector2 Pantalla {  get; set; }
        public Texture2D Fondo { get; set; }
        public PantallaFinal(Vector2 pantalla,Texture2D fondo,SpriteFont fuente = null)
        {
            Font = fuente;
            //SeccionDeBotones = new MenuBotones(pantalla,botones, fuente);
            Estado = EstadoPantallaFinal.Apagado;
            Pantalla = pantalla;
            Fondo = fondo;
        }

        public void Draw(SpriteBatch spriteBatch,int puntos)
        {
            spriteBatch.Begin();

            Color colorOscuro = new Color(0, 0, 0, 128);
            spriteBatch.Draw(Fondo, new Rectangle(0, 0, (int)Pantalla.X, (int)Pantalla.Y), colorOscuro);

            spriteBatch.DrawString(Font, "Felicitaciones", new Vector2(Pantalla.X/2, Pantalla.Y/2), Color.White);
            spriteBatch.DrawString(Font, "Tu puntuacion fue: " + puntos, new Vector2(Pantalla.X / 2, (Pantalla.Y / 2) + 20), Color.White);
            spriteBatch.End();
        }
        public void DrawLost(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            Color colorOscuro = new Color(0, 0, 0, 128); // Color oscuro con un valor alfa de 128
            spriteBatch.Draw(Fondo, new Rectangle(0, 0, (int)Pantalla.X, (int)Pantalla.Y), colorOscuro);

            spriteBatch.DrawString(Font, "Perdiste", new Vector2(Pantalla.X / 2, Pantalla.Y / 2), Color.Red);
            spriteBatch.End();
        }
    }
}
