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
    public class Bala
    {
        
        Matrix RotationMatrix = Matrix.Identity;

        public Boolean esVictima { get; set; }

        public OrientedBoundingBox BalaBox { get; set; }

        protected Model Model { get; set; }
        public Matrix World { get; set; }

        protected Texture2D Texture { get; set; }
        public Vector3 Position{ get; set; }
        public Vector3 Velocity{ get; set; }
        protected Effect Effect { get; set; }
        protected float Angulo{ get; set; }

        public float Daño { get; set; }


        private const float tiempoLimiteDeVida = 3000f;
        private float tiempoDeVida;

        float anguloDelCañon = 0.0f;
        int mouseX = 0;
        float mouseY = 0f;

        private Vector2 estadoAnteriorDelMouse;

        public Tanque Jugador {get; set;}


        public Bala(Vector3 Position, Vector3 velocidad, Model modelo, Effect efecto, Texture2D textura, Tanque Main){
            this.Position = Position;

            World = Matrix.CreateScale(5f) * Matrix.CreateWorld(Position, Vector3.Forward, Vector3.Up);
            
            Model = modelo;

            Jugador = Main;

            var AABB = BoundingVolumesExtensions.FromMatrix(World);
            BalaBox = OrientedBoundingBox.FromAABB(AABB);
            BalaBox.Center = Position + Vector3.Up * AABB.Max.Y / 2 ;

            Effect = efecto;

            Texture = textura;

            Velocity = velocidad;
            
            esVictima = false;

            Daño = 1f;
        }

        public void Draw(GameTime gameTime, Matrix view, Matrix projection)
        {
            // Tanto la vista como la proyección vienen de la cámara por parámetro
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(projection);
            Effect.Parameters["ModelTexture"]?.SetValue(Texture);

            
            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            
            // forma de girar la torreta con cañón
            // Torreta.Transform = Torreta.Transform * Matrix.CreateRotationZ(0.006f);
            // Cannon.Transform = Cannon.Transform * Matrix.CreateRotationZ(0.006f);
            
            World = Matrix.CreateScale(5f)  * Matrix.CreateTranslation(Position);
            foreach (var mesh in Model.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                Effect.Parameters["World"].SetValue(meshWorld*World);
                mesh.Draw();
            }
        }

        public bool recorridoCompleto(){
            return tiempoDeVida >= tiempoLimiteDeVida;
        }
        public void Update(GameTime gameTime, List<TanqueEnemigo> enemigos, List<Object> ambientaciones){
            //BalaBox = BoundingVolumesExtensions.FromMatrix(World);
            BalaBox.Center = Position;
            if(!recorridoCompleto() && !esVictima){
                var delta = (float)gameTime.ElapsedGameTime.Milliseconds;
                tiempoDeVida += delta;
                Position += Velocity * delta;
            }       

            foreach (var tanqueEnemigo in enemigos)
            {
                if(BalaBox.Intersects(tanqueEnemigo.TankBox)){
                    tanqueEnemigo.agregarVelocidad(new Vector3(Velocity.X, 0, Velocity.Z));
                    tanqueEnemigo.reproducirSonido(Jugador.listener);
                    esVictima = true;
                    tanqueEnemigo.recibirDaño(Daño);
                }
            }   

            foreach (var ambiente in ambientaciones){
                if(BalaBox.Intersects(ambiente.Box)){
                    if(ambiente.esEliminable){
                        ambiente.esVictima = true;
                    }
                    ambiente.reproducirSonido(Jugador.listener);
                    esVictima = true;   
                }
            }   

                
        }
        
    }
}