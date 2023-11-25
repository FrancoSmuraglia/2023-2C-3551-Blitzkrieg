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
    // WIP: A mejorar en gral
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
        public Vector2 PosicionTexto { get; set;}
        public Texture2D Logo;
        private Rectangle LogoRect {get; set;}
        
        string perdiste = "Y asi, el Tanque derroto a los de la URSS que le obligaron a meterse en las elecciones a candidato del Consejo Estudiantil.";
        string parte2 = "Es un periodo de guerra civil, los rebeldes se aliaron con los alemanes con el fin de poder derrocar al emperador de la\nnueva republica socialista Rusa.";
        string parte3 = "\nCon el poder de fuego de los alemanes, los rebeldes lograron acertar su primera victoria en el campo de batalla.";
        string parte4 = "\nMientras tanto, Stalin furioso por la traicion de los alemanes,se reunio con su equipo de elite en el Kremlin para \nlanzar una nueva ofensiva";
        string parte5 = "\n\nY aun asi, el tanque decidio combatir el resto de sus dias gracias al PODER DEL GENERA L STA LIN, NA DA   LE GA NA   A \nSTA LIN";
        string parte6 = "\n\n\nCON AMOR NINITA";
        string parteN = "\n\n\nNo sabias que tenia lore? Ahora lo tiene y lo vas a disfrutar";
        string parteFinal = "\n\n\n\nPresiona ESC para finalizar este sufrimiento de juego";

        public PantallaFinal(Vector2 pantalla,Texture2D fondo,SpriteFont fuente = null)
        {
            Font = fuente;
            //SeccionDeBotones = new MenuBotones(pantalla,botones, fuente);
            Estado = EstadoPantallaFinal.Apagado;
            Pantalla = pantalla;
            Fondo = fondo;
            PosicionTexto =  new Vector2(Pantalla.X/2, Pantalla.Y) - new Vector2((Font.MeasureString(perdiste).X / 2 * 1.5f), (Font.MeasureString(perdiste).Y / 2 * 1.5f));
            //perdiste += parte2 + parte3 + parte4 + parteN + parte5 + parte6 + parteFinal; 
        }

        public void Draw(SpriteBatch spriteBatch,int puntos)
        {
            spriteBatch.Begin();

            Color colorOscuro = new Color(255, 255, 255, 255);
            spriteBatch.Draw(Fondo, new Rectangle(0, 0, (int)Pantalla.X, (int)Pantalla.Y), colorOscuro);

            spriteBatch.DrawString(Font, "Felicitaciones", Pantalla/2, Color.White);
            spriteBatch.DrawString(Font, "Tu puntuacion fue: " + puntos, new Vector2(Pantalla.X / 2, (Pantalla.Y / 2) + 20), Color.White);
            spriteBatch.End();

        }

        public void DrawLost(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            Color colorOscuro = new Color(255, 255, 255, 255); // Color oscuro con un valor alfa de 128
            spriteBatch.Draw(Fondo, new Rectangle(0, 0, (int)Pantalla.X, (int)Pantalla.Y), colorOscuro);
            
            //float x = ((Rectangle.X + (Rectangle.Width / 2)) - (Font.MeasureString(perdiste).X / 2));
            //float y = ((Rectangle.Y + (Rectangle.Height / 2)) - (Font.MeasureString(perdiste).Y / 2));

            //spriteBatch.DrawString(Font, perdiste, PosicionTexto, Color.Yellow);
            
            //LogoRect = new Rectangle((int)PosicionTexto.X + Logo.Width / 3 / 2, Logo.Height / 3 / 2 - (int)-PosicionTexto.Y, Logo.Width / 3, Logo.Height / 3);
            LogoRect = new Rectangle(
                                    (int)Pantalla.X/2 + (int)PosicionTexto.X + Logo.Width / 3 / 2, 
                                    (int)Pantalla.Y/2 + Logo.Height / 3 / 2 - (int)-PosicionTexto.Y, 
                                    Logo.Width / 3, 
                                    Logo.Height / 3);
            var alturaY = Font.MeasureString(perdiste).Y * 1.5f;
            spriteBatch.DrawString(Font, perdiste, PosicionTexto, Color.Yellow, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Font, parte2, PosicionTexto + new Vector2(0, alturaY), Color.Yellow, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Font, parte3, PosicionTexto + new Vector2(0, alturaY) *2, Color.Yellow, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Font, parte4, PosicionTexto + new Vector2(0, alturaY) *3, Color.Yellow, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Font, parte5, PosicionTexto + new Vector2(0, alturaY) *4, Color.Yellow, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Font, parte6, PosicionTexto + new Vector2(0, alturaY) *5, Color.Yellow, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Font, parteN, PosicionTexto + new Vector2(0, alturaY) *6, Color.Yellow, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
            spriteBatch.DrawString(Font, parteFinal, PosicionTexto + new Vector2(0, alturaY) *7,  Color.Yellow, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
            
            spriteBatch.Draw(Logo, LogoRect, Color.White);
            spriteBatch.End();
            /*
            float x = ((Rectangle.X + (Rectangle.Width / 2)) - (fuente.MeasureString(Text).X / 2)) - Origin.X;
            float y = ((Rectangle.Y + (Rectangle.Height / 2)) - (fuente.MeasureString(Text).Y / 2)) - Origin.Y;

            spriteBatch.DrawString(fuente, Text, new Vector2(x, y), PenColour, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0.1f);
            
            */
        }

        public void Update(GameTime gameTime)
        {
            float ElapsedGameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(PosicionTexto.Y >= 0)
                PosicionTexto -= new Vector2(0,1);

        }
    }
}
