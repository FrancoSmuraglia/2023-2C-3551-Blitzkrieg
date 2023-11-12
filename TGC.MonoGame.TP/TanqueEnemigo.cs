using System;
using System.Collections.Generic;
using System.Linq;
using BepuPhysics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Collisions;


namespace TGC.MonoGame.TP
{    
    public class TanqueEnemigo
    {
        // Colisión
        public OrientedBoundingBox TankBox { get; set; }
        private float Speed { get; set; }
        
        private float PuntoMedio {get; set;}

        Matrix RotationMatrix = Matrix.Identity;

        protected Model Model { get; set; }
        public Matrix World { get; set; }

        protected Texture2D Texture { get; set; }
        public Vector3 Position{ get; set; }
        public Vector3 OldPosition{ get; set; }
        protected float Rotation{ get; set; }
        public Effect Effect { get; set; }

        private ModelBone Torreta;
        private ModelBone Cannon;
        private Matrix TorretaMatrix;
        private Matrix CannonMatrix;

        public float Vida {  get; set; }
        public bool estaMuerto { get; set; }
        public AudioEmitter Emitter {get;set;}
        public SoundEffect SonidoColision {get;set;}
        public Texture2D NormalTexture { get; set; }
        


        public TanqueEnemigo(Vector3 Position, Model modelo, Effect efecto, Texture2D textura){
            this.Position = Position;

            Emitter = new AudioEmitter
            {
                Position = this.Position
            };
            OldPosition = Position;

            World = Matrix.CreateWorld(Position, Vector3.Forward, Vector3.Up);
            
            Model = modelo;
            
            
            var AABB = BoundingVolumesExtensions.CreateAABBFrom(Model);
            TankBox = OrientedBoundingBox.FromAABB(AABB);
            PuntoMedio = AABB.Max.Y / 3;
            TankBox.Center = Position;
            TankBox.Orientation = RotationMatrix;

            Effect = efecto;

            Texture = textura;

            TankDirection = Vector3.Forward;
            
            TankVelocity = Vector3.Zero;
            Vida = 10.0f;
            estaMuerto = false;
        }

        public void LoadContent(){

            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.

            //Effect.Parameters["ModelTexture"].SetValue(Texture);

            // Al mesh le asigno el Effect (solo textura por ahora)
            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Effect;
                }
            }
            

            Torreta = Model.Bones["Turret"];
            TorretaMatrix = Torreta.Transform;

            Cannon = Model.Bones["Cannon"];
            CannonMatrix = Cannon.Transform;
        }

        public void Draw(GameTime gameTime, Matrix view, Matrix projection, Vector3 camaraPosition)
        {
            actualizarLuz(camaraPosition);

           

            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            
            // forma de girar la torreta con cañón
            // Torreta.Transform = Torreta.Transform * Matrix.CreateRotationZ(0.006f);
            // Cannon.Transform = Cannon.Transform * Matrix.CreateRotationZ(0.006f);
            
            World = RotationMatrix * Matrix.CreateTranslation(Position);
            foreach (var mesh in Model.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                var finalWorld = meshWorld * World;
                Effect.Parameters["World"].SetValue(meshWorld * World);
                Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(meshWorld * World)));
                Effect.Parameters["WorldViewProjection"].SetValue(meshWorld * World * view * projection);
                mesh.Draw();
            }
        }

        public void actualizarLuz(Vector3 camaraPosition)
        {
            Effect.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f, 1f));
            Effect.Parameters["diffuseColor"].SetValue(new Vector3(0.1f, 0.1f, 0.6f));
            Effect.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));

            Effect.Parameters["KAmbient"].SetValue(1f);
            Effect.Parameters["KDiffuse"].SetValue(1f);
            Effect.Parameters["KSpecular"].SetValue(0.2f);
            Effect.Parameters["shininess"].SetValue(100f);
            //Effect.Parameters["lightPosition"].SetValue(new Vector3(500,500,500));
            Effect.Parameters["eyePosition"].SetValue(camaraPosition);

            Effect.Parameters["ModelTexture"].SetValue(Texture);
            Effect.Parameters["NormalTexture"].SetValue(NormalTexture);
            Effect.Parameters["Tiling"].SetValue(Vector2.One);
        }

        
        public Vector3 TankVelocity  { get; set; }
        private Vector3 TankDirection  { get; set; }
        
        private Boolean Moving  { get; set; }
        int Sentido;
        float Acceleration = 7f;
        float CurrentAcceleration = 0;

        public bool Intersecta(Object objeto){
            return TankBox.Intersects(objeto.Box);
        }

        public void Update(GameTime gameTime, List<Object> ambiente, AudioListener audioJugador){
            float deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            float moduloVelocidadXZ = new Vector3(TankVelocity.X, 0f, TankVelocity.Z).Length();
            
            Position += TankVelocity;                               // Actualizo la posición en función de la velocidad actual
            TankBox.Center = Position + Vector3.Up * PuntoMedio;
            
            TankBox.Orientation = RotationMatrix;
            
            if(TankVelocity.X != 0 || TankVelocity.Z != 0) {   // frenada del auto con desaceleración
                TankVelocity -= TankVelocity * deltaTime; 
                if(moduloVelocidadXZ < 0.1f){                       
                    TankVelocity = new Vector3(0f, TankVelocity.Y, 0f);
                    Sentido = 0;
                    CurrentAcceleration = 0;
                } 
            } 

            foreach (Object itemEspecifico in ambiente){
                if(Intersecta(itemEspecifico)){
                    if(itemEspecifico.esEliminable){
                        itemEspecifico.esVictima = true;
                    }                        
                    else{
                        Sentido *= -1;
                        Position = OldPosition;
                        TankBox.Center = OldPosition + Vector3.Up * PuntoMedio;
                        TankVelocity = -TankVelocity*.5f;
                    }
                    itemEspecifico.reproducirSonido(audioJugador);
                        
                }
            }

            if(Vida <= 0)
            {
                estaMuerto = true;
            }

            //System.Console.WriteLine("Enemigos rotatio: " + TankBox.Orientation.Equals(RotationMatrix));

 
            //System.Console.WriteLine(Position);

            //El auto en el world
            World = RotationMatrix * Matrix.CreateTranslation(Position) ;
            OldPosition = Position;

            Emitter.Position = Position;

        }
        public void agregarVelocidad(Vector3 velocidad){
            TankVelocity += velocidad;
        }
        public bool Intersecta(BoundingBox objectoAAnalizar){
            return TankBox.Intersects(objectoAAnalizar);
        }

        public bool Intersecta(List<Object> listaDeObjetos){

            foreach (var objeto in listaDeObjetos)
            {
                if(TankBox.Intersects(objeto.Box) && objeto.esEliminable){
                    objeto.esVictima = true;             
                    return true;
                }       
            }
            return false;
        }

        public void recibirDaño(float cantidad)
        {
            Vida -= cantidad;
        }

        public void reproducirSonido(AudioListener listener)
        {
            var a = SonidoColision.CreateInstance();
            a.Apply3D(listener,Emitter);
            //Console.WriteLine(listener.Position);
            //Console.WriteLine(Emitter.Position);
            a.Play();
        }
    }
}