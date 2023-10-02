using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace TGC.MonoGame.TP
{    
    public class Bala
    {
        
        Matrix RotationMatrix = Matrix.Identity;

        protected Model Model { get; set; }
        public Matrix World { get; set; }

        protected Texture2D Texture { get; set; }
        public Vector3 Position{ get; set; }
        public Vector3 Velocity{ get; set; }
        protected Effect Effect { get; set; }
        protected float Angulo{ get; set; }


        private const float tiempoLimiteDeVida = 3000f;
        private float tiempoDeVida;

        float anguloDelCañon = 0.0f;
        int mouseX = 0;
        float mouseY = 0f;

        private Vector2 estadoAnteriorDelMouse;



        public Bala(Vector3 Position, Vector3 velocidad, Model modelo, Effect efecto, Texture2D textura){
            this.Position = Position;

            World = Matrix.CreateScale(5f) * Matrix.CreateWorld(Position, Vector3.Forward, Vector3.Up);
            
            Model = modelo;

            Effect = efecto;

            Texture = textura;

            Velocity = velocidad;
        }

        public void LoadContent(){

            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.

            //Effect.Parameters["ModelTexture"].SetValue(Texture);

            // Al mesh le asigno el Effect (solo textura por ahora)
            tiempoDeVida = 0;
            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Effect;
                }
            }
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
            
            World = Matrix.CreateScale(5f) * Matrix.CreateTranslation(Position);
            foreach (var mesh in Model.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                Effect.Parameters["World"].SetValue(meshWorld*World);
                mesh.Draw();
            }
        }


        public void Update(GameTime gameTime){
            if(tiempoDeVida <= tiempoLimiteDeVida){
                tiempoDeVida += (float)gameTime.ElapsedGameTime.Milliseconds;
                Position += Velocity * (float)gameTime.ElapsedGameTime.Milliseconds;
            }
                
        }
        
    }
}