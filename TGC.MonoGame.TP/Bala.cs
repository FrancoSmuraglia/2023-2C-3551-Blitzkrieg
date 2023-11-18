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

        public void Draw(GameTime gameTime, Matrix view, Matrix projection, Vector3 camaraPosition, RenderTarget2D ShadowMapRenderTarget, Vector3 lightPosition, int ShadowmapSize, TargetCamera TargetLightCamera)
        {
            actualizarEfecto(camaraPosition, ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);

            
            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            
            // forma de girar la torreta con cañón
            // Torreta.Transform = Torreta.Transform * Matrix.CreateRotationZ(0.006f);
            // Cannon.Transform = Cannon.Transform * Matrix.CreateRotationZ(0.006f);
            
            //World = Matrix.CreateScale(5f)  * Matrix.CreateTranslation(Position);
            foreach (var mesh in Model.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                Effect.Parameters["World"].SetValue(meshWorld*World);
                Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(meshWorld * World)));
                Effect.Parameters["WorldViewProjection"].SetValue(meshWorld * World * view * projection);
                mesh.Draw();
            }
        }
        public void actualizarEfecto(Vector3 camaraPosition, RenderTarget2D ShadowMapRenderTarget, Vector3 lightPosition, int ShadowmapSize, TargetCamera TargetLightCamera){
            Effect.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f, 1f));
            Effect.Parameters["diffuseColor"].SetValue(new Vector3(0.1f, 0.1f, 0.6f));
            Effect.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));
        
            Effect.Parameters["KAmbient"].SetValue(1f);
            Effect.Parameters["KDiffuse"].SetValue(1f);
            Effect.Parameters["KSpecular"].SetValue(0.8f);
            Effect.Parameters["shininess"].SetValue(100f);
            Effect.Parameters["eyePosition"].SetValue(camaraPosition);
        
            Effect.Parameters["ModelTexture"].SetValue(Texture);
            //Effect.Parameters["NormalTexture"].SetValue(NormalTexture);
            Effect.Parameters["Tiling"].SetValue(Vector2.One);


            Effect.CurrentTechnique = Effect.Techniques["Default"];
            Effect.Parameters["shadowMap"].SetValue(ShadowMapRenderTarget);
            Effect.Parameters["lightPosition"].SetValue(lightPosition);
            Effect.Parameters["shadowMapSize"].SetValue(Vector2.One * ShadowmapSize);
            Effect.Parameters["LightViewProjection"].SetValue(TargetLightCamera.View * TargetLightCamera.Projection);
        
        }
        public void DrawShadows(GameTime gameTime, Matrix view, Matrix projection)
        {
            Effect.CurrentTechnique = Effect.Techniques["DepthPass"];
            
            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            
            // forma de girar la torreta con cañón
            // Torreta.Transform = Torreta.Transform * Matrix.CreateRotationZ(0.006f);
            // Cannon.Transform = Cannon.Transform * Matrix.CreateRotationZ(0.006f);
            
            World = Matrix.CreateScale(5f)  * Matrix.CreateTranslation(Position);
            foreach (var mesh in Model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                    part.Effect = Effect;
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                Effect.Parameters["WorldViewProjection"]
                    .SetValue(meshWorld * World * view * projection);
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