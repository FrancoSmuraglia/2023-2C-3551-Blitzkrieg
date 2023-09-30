using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace TGC.MonoGame.TP
{    
    public class Tanque
    {
        private float Speed { get; set; }
        
        Matrix RotationMatrix = Matrix.Identity;

        protected Model Model { get; set; }
        public Matrix World { get; set; }

        protected Texture2D Texture { get; set; }
        public Vector3 Position{ get; set; }
        protected float Rotation{ get; set; }
        protected Effect Effect { get; set; }

        public ModelBone Torreta;
        public ModelBone Cannon;
        private Matrix TorretaMatrix;
        private Matrix CannonMatrix;

        private float yaw = 0f;
        private float pitch = 0f;

        float anguloDelCañon = 0.0f;
        int mouseX = 0;
        float mouseY = 0f;

        private MouseState estadoAnteriorDelMouse;



        public Tanque(Vector3 Position, Model modelo, Effect efecto, Texture2D textura){
            this.Position = Position;

            World = Matrix.CreateWorld(Position, Vector3.Forward, Vector3.Up);
            
            Model = modelo;

            Effect = efecto;

            Texture = textura;

            Torreta = modelo.Bones["Turret"];
            TorretaMatrix = Torreta.Transform;

            Cannon = modelo.Bones["Cannon"];
            CannonMatrix = Cannon.Transform;

            
            TankVelocity = Vector3.Zero;
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

        
        private Vector3 TankVelocity  { get; set; }
        private Vector3 TankDirection  { get; set; }
        
        private Boolean Moving  { get; set; }
        int Sentido;
        float Acceleration = 7f;
        float CurrentAcceleration = 0;

        float anguloActual = 0;

        float primero = 0;
        float segundo = 0;
        float acumulacion = 0; 
        bool frenada = true;
        public void Update(GameTime gameTime, KeyboardState key){
            float deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            float moduloVelocidadXZ = new Vector3(TankVelocity.X, 0f, TankVelocity.Z).Length();
            
            if(key.IsKeyDown(Keys.W)){ //adeltante
                Moving = true;
                CurrentAcceleration = 1;
                Sentido = 1;
            }
            if(key.IsKeyDown(Keys.S)){ //reversa                
                Moving = true;
                CurrentAcceleration = -1;
                Sentido = -1;
            }
            if(key.IsKeyDown(Keys.A) && Moving){ 
                RotationMatrix *= Matrix.CreateRotationY(.03f);
                TankDirection = Vector3.Transform(Vector3.Forward, RotationMatrix); 
                TankVelocity = TankDirection * moduloVelocidadXZ * Sentido + new Vector3(0f, TankVelocity.Y, 0f); 
            }                
            if(key.IsKeyDown(Keys.D) && Moving){
                RotationMatrix *= Matrix.CreateRotationY(-.03f);
                TankDirection = Vector3.Transform(Vector3.Forward, RotationMatrix); 
                TankVelocity = TankDirection * moduloVelocidadXZ * Sentido + new Vector3(0f, TankVelocity.Y, 0f); 
            }

            Position += TankVelocity;                               // Actualizo la posición en función de la velocidad actual
            CurrentAcceleration *= Acceleration;                    // Decido la aceleración


            // **** Manejo de aceleración y renderizado del auto en el world **** //


            if(Moving && moduloVelocidadXZ < 20f){                  // aceleración sobre el plano XZ
                TankVelocity += TankDirection * CurrentAcceleration * deltaTime;
            }
            else if(TankVelocity.X != 0 || TankVelocity.Z != 0) {   // frenada del auto con desaceleración
                TankVelocity -= TankVelocity * 2f * deltaTime; 
                if(moduloVelocidadXZ < 0.1f){                       // analizo el módulo del vector velocidad del plano XZ
                    TankVelocity = new Vector3(0f, TankVelocity.Y, 0f);
                    Sentido = 0;
                    CurrentAcceleration = 0;
                } 
            }

            MouseState currentMouseState = Mouse.GetState();
    

            // Obtén la posición actual del mouse
            mouseX = currentMouseState.X;
            mouseY = currentMouseState.Y;

            // Calcula la diferencia entre la posición del mouse actual y la posición anterior
            float deltaX = mouseX - estadoAnteriorDelMouse.X;

            float deltaY = mouseY - estadoAnteriorDelMouse.Y;

            // Ajusta la velocidad de rotación según tus preferencias
            float rotationSpeed = 0.01f;
            Matrix torretaRotacion = Matrix.Identity;
            // Calcula la rotación en función de la diferencia del mouse
            if(currentMouseState.RightButton.Equals(ButtonState.Pressed))
                torretaRotacion = Matrix.CreateRotationZ(-deltaX * rotationSpeed);
            
                

            // Aplica la rotación a la torreta
            Torreta.Transform *= torretaRotacion;

            // Aplica la misma rotación al cañón
            Cannon.Transform *= torretaRotacion;

            // Actualiza la matriz de transformación relativa del cañón
            Matrix transformacionRelativaDelCañon = Cannon.Transform * Matrix.Invert(Torreta.Transform);

            // Aplica la transformación relativa al cañón
            Cannon.Transform = transformacionRelativaDelCañon * Torreta.Transform;

            // Actualiza el estado anterior del mouse para el próximo ciclo
            estadoAnteriorDelMouse = currentMouseState;

            CannonMatrix = Cannon.Transform;
            //deltaY = MathHelper.Clamp(deltaY, -0.25f, 0.25f);

            //anguloDelCañon += MathHelper.ToRadians(deltaY * rotationSpeed);

            //anguloDelCañon = Math.Clamp(anguloDelCañon,-MathHelper.PiOver4,MathHelper.PiOver4);

            primero = currentMouseState.ScrollWheelValue;
            var Dif = Math.Sign(primero - segundo);

            if(Dif != 0){
                acumulacion += Dif;
                Console.WriteLine("Dif " + Dif);
                Console.WriteLine(acumulacion);
                if(acumulacion >= -10 && acumulacion <= 0){    
                    var cannonRotacion = Matrix.CreateRotationX(Dif * rotationSpeed * 10f);
                    Cannon.Transform = cannonRotacion * Cannon.Transform;
                }
                acumulacion = Math.Clamp(acumulacion, -10, 0);
            }
            
            

            segundo = primero;

            //var cannonRotacion = Matrix.CreateRotationX(anguloActual * rotationSpeed);
            
            //Cannon.Transform = cannonRotacion * Cannon.Transform;


            /*anguloActual += deltaY * rotationSpeed;
            if(anguloActual >= 0 && anguloActual <= MathHelper.PiOver4) {
                var cannonRotacion = Matrix.CreateRotationX(-deltaY * rotationSpeed);
                Cannon.Transform = cannonRotacion * Cannon.Transform;
            }
            
            anguloActual = Math.Clamp(anguloActual, 0, MathHelper.PiOver4);
            Console.WriteLine("Angulo del cañón: " + anguloActual);
            */

            
            //if(radianes > MathHelper.PiOver4 && radianes < 0){

            //}

            
            





            //            MouseState estadoActualMouse = Mouse.GetState();
            //            Vector2 mouseDelta = new Vector2(estadoActualMouse.X - estadoAnteriorMouse.X, estadoActualMouse.Y - estadoAnteriorMouse.Y);
            //
            //            estadoAnteriorMouse = Mouse.GetState();
            //
            //            float velocidadDeRotacion = 0.01f;
            //            yaw += mouseDelta.X * velocidadDeRotacion;
            //            pitch += mouseDelta.Y * velocidadDeRotacion;
            //
            //
            //            TorretaMatrix = Matrix.CreateRotationZ(yaw) * TorretaMatrix;
            //
            //            Torreta.Transform = Matrix.CreateRotationZ(yaw) * TorretaMatrix;
            //
            //            Matrix transformacionRelativaDelCañon = Cannon.Transform * Matrix.Invert(Torreta.Transform);
            //            Cannon.Transform = transformacionRelativaDelCañon * Torreta.Transform;


            //            if(key.IsKeyDown(Keys.U)){
            //                Matrix transformacionRelativaDelCañon = Cannon.Transform * Matrix.Invert(Torreta.Transform);
            //                TorretaMatrix = Torreta.Transform;
            //                var torretaRotacion = Matrix.CreateRotationZ(0.03f);
            //                Torreta.Transform = torretaRotacion * TorretaMatrix;
            //                Cannon.Transform = transformacionRelativaDelCañon * Torreta.Transform;
            //            }
            //            if(key.IsKeyDown(Keys.I)){
            //                Matrix transformacionRelativaDelCañon = Cannon.Transform * Matrix.Invert(Torreta.Transform);
            //                TorretaMatrix = Torreta.Transform;
            //                var torretaRotacion = Matrix.CreateRotationZ(-0.03f);
            //                Torreta.Transform = torretaRotacion * TorretaMatrix;
            //                Cannon.Transform = transformacionRelativaDelCañon * Torreta.Transform;
            //            }
            //            if(key.IsKeyDown(Keys.O)){
            //                CannonMatrix = Cannon.Transform;
            //                var cañonRotacion = Matrix.CreateRotationX(-0.03f);
            //                Cannon.Transform = cañonRotacion * CannonMatrix;
            //            }
            //            if(key.IsKeyDown(Keys.L)){
            //                CannonMatrix = Cannon.Transform;
            //                var cañonRotacion = Matrix.CreateRotationX(0.03f);
            //                Cannon.Transform = cañonRotacion * CannonMatrix;
            //            } 

            Moving = false;

            System.Console.WriteLine(Position);

            //El auto en el world
            World = RotationMatrix * Matrix.CreateTranslation(Position) ;

        }
    }
}