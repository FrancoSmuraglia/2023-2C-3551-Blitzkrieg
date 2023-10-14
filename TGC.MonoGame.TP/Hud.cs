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
    public class Hud
    {
        public float FPS {get; set;}
        public SpriteFont Font { get; set; }
        public Vector2 PositionVida { get; set; }
        public Vector2 PositionTiempo { get; set; }
        public MenuBotones SeccionDeBotones {get; set;}
        public Hud(Vector2 Pantalla, List<Button> botones,SpriteFont fuente = null)
        {
            Font = fuente;
            PositionVida = new Vector2(((Pantalla.X + 100) / 2) - 100, Pantalla.Y - 50);
            PositionTiempo = new Vector2(((Pantalla.X + 100) / 2) - 100, 10);
            SeccionDeBotones = new MenuBotones(Pantalla, botones, fuente);
        }

        public void Draw(SpriteBatch spriteBatch, float Vida,float tiempoRestante)
        {
            

            spriteBatch.Begin();
            //spriteBatch.DrawString(Font, "Vida: " + Vida.ToString(), PositionVida, Color.White);
            //spriteBatch.DrawString(Font, minutes + ":" + seconds.ToString("0"), PositionTiempo, Color.White);
            spriteBatch.DrawString(Font, FPS.ToString("0.00"), Vector2.Zero, Color.Yellow);
            SeccionDeBotones.Draw(spriteBatch);
            spriteBatch.End();
        }

        public void Update(float tiempoRestante, float Vida,bool balaEspecial,GameTime gameTime,List<TanqueEnemigo> Tanques){
            int minutes = (int)(tiempoRestante / 60);
            var seconds = (tiempoRestante % 60);
            SeccionDeBotones.Botones[0].Text = minutes + ":" + seconds.ToString("0");
            SeccionDeBotones.Botones[1].Text = "Vida: " + Vida;
            if(balaEspecial){
                SeccionDeBotones.Botones[2].IsSelected = true;
                SeccionDeBotones.Botones[3].IsSelected = false;
            }
            else{
                SeccionDeBotones.Botones[2].IsSelected = false;
                SeccionDeBotones.Botones[3].IsSelected = true;
            }
            //SeccionDeBotones.Botones[4].Text = (1000f/(float)(gameTime.ElapsedGameTime.TotalMilliseconds)).ToString("0.00");
            FPS = 1000f/(float)(gameTime.ElapsedGameTime.TotalMilliseconds);
            for (int i = 0; i < Tanques.Count; i++)
            {
                int pos = i+1;
                SeccionDeBotones.Botones[i+4].Text = "Enemigo " + pos + ": " + Tanques[i].Vida;
            }
            /*SeccionDeBotones.Botones[6].Text = "Enemigo 2: " + Tanques[1].Vida;
            SeccionDeBotones.Botones[7].Text = "Enemigo 3: " + Tanques[2].Vida;
            SeccionDeBotones.Botones[8].Text = "Enemigo 4: " + Tanques[3].Vida;*/


        }
    }
}
