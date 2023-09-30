using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Collisions;

namespace TGC.MonoGame.TP
{
    
    public class Bullet
    {   
        private Model bullet { get; set; }
        private Tanque tanque{ get; set; }
        private TanqueEnemigo tanqueEnemigo { get; set; }
        public OrientedBoundingBox BulletBallBox { get; set; }
        private float Scale0;
        private Vector3 Position0 { get; set; }
        private Vector3 Position1 { get; set; }
        private Vector3 PositionActual { get; set; }
        private Vector3 PositionAnterior { get; set; }
        private Vector3 Velocidad { get; set; }
        protected Effect Effect { get; set; }

        public Bullet(Vector3 initialPosition, Vector3 endPosition, Model model, Tanque tanque, TanqueEnemigo tanqueEnemigo){
        
        Position0 = initialPosition;
        Scale0 = (float)0.5;
        Position1 = endPosition;
        PositionActual = Position0;
        PositionAnterior = PositionActual;
        var distancia = Position1 - Position0;
        var duracion = distancia.Length()*0.001;
            Velocidad = (distancia + new Vector3(0, (float) 9.8 + 1000, 0) * (float) duracion * (float) duracion) /
                          (float) duracion;

        var temporaryCubeAABB = BoundingVolumesExtensions.CreateAABBFrom(model);
            
            temporaryCubeAABB = BoundingVolumesExtensions.Scale(temporaryCubeAABB, Scale0);
            
            BulletBallBox = OrientedBoundingBox.FromAABB(temporaryCubeAABB);

            BulletBallBox.Center = PositionActual;
            this.tanque = tanque;
            this.tanqueEnemigo = tanqueEnemigo;

        }

        public void Draw(GameTime gameTime, Matrix view, Matrix projection){
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(projection);
        }

        public void Update(){

        }

    }
}