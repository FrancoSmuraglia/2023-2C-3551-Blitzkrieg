using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
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

        private const int DistanciaMaximaDeVision = 5000;
        Matrix RotationMatrix = Matrix.Identity;

        protected Model Model { get; set; }
        public Matrix World { get; set; }

        protected Texture2D Texture { get; set; }
        public Vector3 Position{ get; set; }
        public Vector3 OldPosition{ get; set; }
        protected float Rotation{ get; set; }
        protected Effect Effect { get; set; }

        private ModelBone Torreta;
        private ModelBone Cannon;
        private Matrix TorretaMatrix;
        private Matrix CannonMatrix;

        public float Vida {  get; set; }
        public bool estaMuerto { get; set; }
        public AudioEmitter Emitter {get;set;}
        public SoundEffect SonidoColision {get;set;}

        private float velocidadGiro {get; set;} 
        

        // bala 

        private Model ModelBala {get; set;}
        private Texture2D TexturaBala {get; set;}
        private Effect EfectoBala{get; set;}


        public TanqueEnemigo(Vector3 Position, Model modelo, Effect efecto, Texture2D textura){
            this.Position = Position;

            Emitter = new AudioEmitter
            {
                Position = this.Position
            };
            OldPosition = Position;

            World = Matrix.CreateWorld(Position, Vector3.Forward, Vector3.Up);
            
            Model = modelo;
            RotationMatrix = Matrix.CreateRotationY(0f);
            
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

            velocidadGiro = MathHelper.Lerp(.05f,.1f, (float)new Random().NextDouble());
        }

        public void LoadContent(Model bala, Texture2D texturaBala, Effect efectoBala){

            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.
            ModelBala = bala;

            TexturaBala = texturaBala;

            EfectoBala = efectoBala;

            foreach (var mesh in ModelBala.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Effect;
                }
            }

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
            
            World = RotationMatrix * Matrix.CreateTranslation(Position);
            foreach (var mesh in Model.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                Effect.Parameters["World"].SetValue(meshWorld*World);
                mesh.Draw();
            }
        }

        
        public Vector3 TankVelocity  { get; set; }
        private Vector3 TankDirection  { get; set; }
        public Vector3 TankAcceleration { get; set; } 
        
        private Boolean Moving  { get; set; }
        int Sentido;
        float Acceleration = 7f;
        float CurrentAcceleration = 0;

        private bool Intersecta(Object objeto){
            return TankBox.Intersects(objeto.Box);
        }
        private double VectorAngle(Vector3 v1, Vector3 v2)
        {
            // Calculate the vector lengths.
            double len1 = Math.Sqrt(v1.X * v1.X + v1.Z * v1.Z);
            double len2 = Math.Sqrt(v2.X * v2.X + v2.Z * v2.Z);

            // Use the dot product to get the cosine.
            double dot_product = v1.X * v2.X + v1.Z * v2.Z;
            double cos = dot_product / len1 / len2;

            // Use the cross product to get the sine.
            double cross_product = v1.X * v2.Z - v1.Z * v2.X;
            double sin = cross_product / len1 / len2;

            // Find the angle.
            double angle = Math.Acos(cos);
            if (sin < 0) angle = -angle;
            return angle;
        }

        float tiempoEntreDisparo;
        private void BusquedaYPersecucion(Vector3 playerposition, List<Bala> balas, float deltaTime){
            Vector3 distance = playerposition - Position;
            var distanceLongitud = distance.Length();
            tiempoEntreDisparo += deltaTime;
            if(distanceLongitud < DistanciaMaximaDeVision)
            { // radio de visión del tanqueEnemigo
                var directionActual = RotationMatrix.Forward;
                
                var angulo = VectorAngle(distance, directionActual);

                if(Math.Abs(angulo) > 0.2){
                    if(angulo > 0)
                        RotationMatrix *= Matrix.CreateRotationY(velocidadGiro);
                    else if (angulo < 0)
                        RotationMatrix *= Matrix.CreateRotationY(-velocidadGiro);
                }
                
                if(distanceLongitud > 2000)
                    TankVelocity += deltaTime * Vector3.Normalize(distance) * 100;
                
                if(distanceLongitud < DistanciaMaximaDeVision/2 && tiempoEntreDisparo >= 5)
                    Disparar(balas, Vector3.Normalize(distance));
            }
            


        }
        private void Disparar(List<Bala> balas, Vector3 velocidadDisparo)
        {
            var balaObjeto = new Bala(Position, velocidadDisparo * 5, ModelBala, EfectoBala, TexturaBala, this);
            balas.Add(balaObjeto);
            tiempoEntreDisparo = 0;
        }

        private void AgregarFriccion(float deltaTime)
        {
            var lateral_velocity = RotationMatrix.Right * Vector3.Dot(TankVelocity, RotationMatrix.Right);
            var lateral_friction = -lateral_velocity * 10f;
            var backwards_friction = -TankVelocity * 8f;
            TankVelocity += (backwards_friction + lateral_friction) * deltaTime;
            TankAcceleration = TankDirection * CurrentAcceleration * 30;
        }

        public void Update(GameTime gameTime, List<Object> ambiente, Tanque jugador, List<Bala> balasEnemigas){
            float deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            float moduloVelocidadXZ = new Vector3(TankVelocity.X, 0f, TankVelocity.Z).Length();
            
            Position += TankVelocity;                               // Actualizo la posición en función de la velocidad actual
            TankBox.Center = Position + Vector3.Up * PuntoMedio;
            
            TankBox.Orientation = RotationMatrix;
            
            //BusquedaYPersecucion(jugador.Position, balasEnemigas, deltaTime);
            
            AgregarFriccion(deltaTime);

            if (Moving && moduloVelocidadXZ < 30f){             
                TankVelocity += TankAcceleration * deltaTime;
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
                    itemEspecifico.reproducirSonido(jugador.listener);
                }
            }

            if(Vida <= 0)
            {
                estaMuerto = true;
            }

            //System.Console.WriteLine("Enemigos rotatio: " + TankBox.Orientation.Equals(RotationMatrix));

 
            //System.Console.WriteLine(Position);

            //El auto en el world
            //World = RotationMatrix * Matrix.CreateTranslation(Position) ;
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