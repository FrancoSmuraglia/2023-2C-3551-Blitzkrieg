using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Collisions;


namespace TGC.MonoGame.TP
{    
    public class Object
    {
        public Boolean Colisiono { get; set; }
        private Model Model { get; set; }
        public Matrix World { get; set; }

        public bool esVictima = false;
        public bool esEliminable = false;
        
        private Texture2D Texture { get; set; }
        public Vector3 Position{ get; set; }
        private float Rotation{ get; set; }
        public Effect Effect { get; set; }

        public OrientedBoundingBox Box { get; set; }
        
        private SoundEffect Colision {get; set;}
        public AudioEmitter Emitter {get;set;}


        public Object(Vector3 Position, Model modelo, Effect efecto, Texture2D textura, bool esDestruible, SoundEffect sonidoAlColisionar = null){
            this.Position = Position;

            World =  Matrix.CreateWorld(Position, Vector3.Forward, Vector3.Up);
            
            Model = modelo;

            Colision = sonidoAlColisionar;

            //Box = BoundingVolumesExtensions.FromMatrix(World);
            var AABB = BoundingVolumesExtensions.CreateAABBFrom(Model);
            AABB.Max.Z /= 3;
            AABB.Max.X /= 3;
            Box = OrientedBoundingBox.FromAABB(AABB);
             // Le sumo a la posición el eje Y máximo de la figura divido dos para que esté en en centro del modelo
            Box.Center = Position + Vector3.Up * (AABB.Max.Y/2); 
            Box.Orientation = Matrix.Identity;
            
            //Box = new OrientedBoundingBox(Position, new Vector3(25,0,25));
            
            Effect = efecto;

            Texture = textura;

            esEliminable = esDestruible;

            Colisiono = false;

            Emitter = new AudioEmitter
            {
                Position = this.Position
            };
        }

        public void LoadContent(){

            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.

            //Effect.Parameters["ModelTexture"].SetValue(Texture);

            // Al mesh le asigno el Effect (solo textura por ahora)
            Effect.CurrentTechnique = Effect.Techniques["Default"];
            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Effect;
                }
            }
        }

        public void Draw(GameTime gameTime, Matrix view, Matrix projection, Vector3 camaraPosition, RenderTarget2D ShadowMapRenderTarget, 
            Vector3 lightPosition, int ShadowmapSize, TargetCamera TargetLightCamera)
        {
            actualizarLuz(camaraPosition, ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            // Tanto la vista como la proyección vienen de la cámara por parámetro
            //Effect.Parameters["View"].SetValue(view);
            //Effect.Parameters["Projection"].SetValue(projection);
            //Effect.Parameters["ModelTexture"].SetValue(Texture);

            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            foreach (var mesh in Model.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                Effect.Parameters["World"].SetValue(meshWorld*World);
                Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(meshWorld * World)));
                Effect.Parameters["WorldViewProjection"].SetValue(meshWorld * World * view * projection);
                mesh.Draw();
            }
        }
        public void actualizarLuz(Vector3 camaraPosition, RenderTarget2D ShadowMapRenderTarget, Vector3 lightPosition, int ShadowmapSize, TargetCamera TargetLightCamera)
        {
            Effect.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f, 1f));
            Effect.Parameters["diffuseColor"].SetValue(new Vector3(0.1f, 0.1f, 0.6f));
            Effect.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));

            Effect.Parameters["KAmbient"].SetValue(1.0f);
            Effect.Parameters["KDiffuse"].SetValue(0.5f);
            Effect.Parameters["KSpecular"].SetValue(0.0f);
            Effect.Parameters["shininess"].SetValue(16.0f);
            Effect.Parameters["eyePosition"].SetValue(camaraPosition);

            Effect.Parameters["ModelTexture"].SetValue(Texture);
            Effect.Parameters["Tiling"].SetValue(Vector2.One);

            Effect.CurrentTechnique = Effect.Techniques["Default"];
            Effect.Parameters["shadowMap"].SetValue(ShadowMapRenderTarget);
            Effect.Parameters["lightPosition"].SetValue(lightPosition);
            Effect.Parameters["shadowMapSize"].SetValue(Vector2.One * ShadowmapSize);
            Effect.Parameters["LightViewProjection"].SetValue(TargetLightCamera.View * TargetLightCamera.Projection);
        }

        public void DrawShadows(GameTime gameTime, Matrix view, Matrix projection)
        {
            //actualizarLuz(camaraPosition, ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            Effect.CurrentTechnique = Effect.Techniques["DepthPass"];

            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            foreach (var mesh in Model.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                Effect.Parameters["World"].SetValue(meshWorld*World);
                Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(meshWorld * World)));
                Effect.Parameters["WorldViewProjection"].SetValue(meshWorld * World * view * projection);
                mesh.Draw();
            }
        }



        public void reproducirSonido(AudioListener listener)
        {
            var a = Colision?.CreateInstance();
            a?.Apply3D(listener,Emitter);
            Console.WriteLine(listener.Position);
            Console.WriteLine(Emitter.Position);
            a?.Play();
        }
    }

}