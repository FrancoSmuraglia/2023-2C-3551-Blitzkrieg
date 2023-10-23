using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BepuPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Collisions;
using static System.Formats.Asn1.AsnWriter;

namespace TGC.MonoGame.TP
{
    public class Hud
    {
        public float FPS {get; set;}
        public SpriteFont Font { get; set; }
        public Vector2 PositionVida { get; set; }
        public Vector2 PositionTiempo { get; set; }
        public MenuBotones SeccionDeBotones {get; set;}
        public List<Texture2D> RelojTexturas { get; set;}
        public Vector2 Pantalla { get;set; }
        public String TiempoRestante { get; set; }
        public Hud(Vector2 pantalla, List<Button> botones,SpriteFont fuente = null)
        {
            Font = fuente;
            PositionVida = new Vector2(((pantalla.X + 100) / 2) - 100, pantalla.Y - 50);
            PositionTiempo = new Vector2(((pantalla.X + 100) / 2) - 100, 10);
            SeccionDeBotones = new MenuBotones(pantalla, botones, fuente);
            Pantalla = pantalla;
        }

        int relojActual = 0;
        public void Draw(SpriteBatch spriteBatch, float Vida,float tiempoRestante)
        {
            

            spriteBatch.Begin();
            //spriteBatch.DrawString(Font, "Vida: " + Vida.ToString(), PositionVida, Color.White);
            //spriteBatch.DrawString(Font, minutes + ":" + seconds.ToString("0"), PositionTiempo, Color.White);
            spriteBatch.DrawString(Font, FPS.ToString("0.00") + " FPS", Vector2.Zero, Color.Yellow);
            SeccionDeBotones.Draw(spriteBatch);
            //spriteBatch.Draw(RelojTexturas[1], new Vector2(Pantalla.X /2, 40), Color.White);
            spriteBatch.Draw(RelojTexturas[relojActual], new Vector2((Pantalla.X / 2) - 75, 10), null, Color.White, 0f, Vector2.Zero, .2f, SpriteEffects.None, 0);
            //spriteBatch.DrawString(Font, TiempoRestante, new Vector2((Pantalla.X / 2) + 5, 35), Color.White);
            spriteBatch.DrawString(Font, TiempoRestante, new Vector2((Pantalla.X / 2), 35), Color.White, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
            spriteBatch.End();
        }
        public string ArreglarSegundos(float segundos){
            var segundoEntero = Math.Truncate(segundos);
            string segundoArreglado = "";
            if(segundoEntero == 60)
                segundoEntero--;
            
            if(segundoEntero < 10)
                segundoArreglado += "0";
            
            return segundoArreglado + segundoEntero;
            

        }

        private float elapsedTime;
        public void Update(float tiempoRestante, float Vida,bool balaEspecial,GameTime gameTime,List<TanqueEnemigo> Tanques){
            int minutes = (int)(tiempoRestante / 60);
            var seconds = (tiempoRestante % 60);
            TiempoRestante = "0" + minutes + ":" + ArreglarSegundos(seconds);

            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (elapsedTime >= 1.0f)
            {
                relojActual = (relojActual + 1) % RelojTexturas.Count;
                elapsedTime = 0f;
            }
            //SeccionDeBotones.Botones[0].Text = "0" + minutes + ":" + ArreglarSegundos(seconds);// seconds.ToString("0");
            SeccionDeBotones.Botones[0].Text = "Vida: " + Vida;
            if(balaEspecial){
                SeccionDeBotones.Botones[1].IsSelected = true;
                SeccionDeBotones.Botones[2].IsSelected = false;
            }
            else{
                SeccionDeBotones.Botones[1].IsSelected = false;
                SeccionDeBotones.Botones[2].IsSelected = true;
            }
            //SeccionDeBotones.Botones[4].Text = (1000f/(float)(gameTime.ElapsedGameTime.TotalMilliseconds)).ToString("0.00");
            FPS = 1000f/(float)(gameTime.ElapsedGameTime.TotalMilliseconds);
            for (int i = 0; i < Tanques.Count; i++)
            {
                int pos = i+1;
                SeccionDeBotones.Botones[i+3].Text = "Enemigo " + pos + ": " + Tanques[i].Vida;
            }
            /*SeccionDeBotones.Botones[6].Text = "Enemigo 2: " + Tanques[1].Vida;
            SeccionDeBotones.Botones[7].Text = "Enemigo 3: " + Tanques[2].Vida;
            SeccionDeBotones.Botones[8].Text = "Enemigo 4: " + Tanques[3].Vida;*/


        }
    }
}
