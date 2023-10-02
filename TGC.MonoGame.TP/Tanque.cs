using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace TGC.MonoGame.TP
{    
    public class Tanque
    {
        Model Bullet;
        
        Texture2D BulletTexture;

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
        private Matrix _initialCannon;
        private Matrix _initialTorret;


        private float yaw = 0f;
        private float pitch = 0f;

        float anguloDelCañon = 0.0f;
        int mouseX = 0;
        float mouseY = 0f;

        private Vector2 estadoAnteriorDelMouse;

        private const float tiempoEntreDisparoLimite = 1.3f;



        public Tanque(Vector3 Position, Model modelo, Effect efecto, Texture2D textura, Vector2 estadoInicialMouse){
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

            estadoAnteriorDelMouse = estadoInicialMouse;

            _initialCannon = Cannon.Transform;
            _initialTorret = Torreta.Transform;
            TorretaMatrix = Torreta.Transform;
        }

        public void LoadContent(Model bala, Texture2D texturaBala){

            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.

            //Effect.Parameters["ModelTexture"].SetValue(Texture);

            // Al mesh le asigno el Effect (solo textura por ahora)
            Bullet = bala;

            BulletTexture = texturaBala;

            foreach (var mesh in Bullet.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Effect;
                }
            }

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

        float anguloVertical = 0;
        float anguloHorizontalTorreta = 0;
        float anguloHorizontalTanque = 0;

        float primero = 0;
        float segundo = 0;
        float acumulacion = 0; 
        bool frenada = true;
        float tiempoEntreDisparo = 0;
        public void Update(GameTime gameTime, KeyboardState key, List<Bala> balas){
            float deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            float moduloVelocidadXZ = new Vector3(TankVelocity.X, 0f, TankVelocity.Z).Length();
            
            if(tiempoEntreDisparo < tiempoEntreDisparoLimite)
                tiempoEntreDisparo += deltaTime;

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
                anguloHorizontalTanque += .03f;
            }                
            if(key.IsKeyDown(Keys.D) && Moving){
                RotationMatrix *= Matrix.CreateRotationY(-.03f);
                TankDirection = Vector3.Transform(Vector3.Forward, RotationMatrix); 
                TankVelocity = TankDirection * moduloVelocidadXZ * Sentido + new Vector3(0f, TankVelocity.Y, 0f); 
                anguloHorizontalTanque -= .03f;
            }
            if(key.IsKeyDown(Keys.Space) && tiempoEntreDisparo > tiempoEntreDisparoLimite){
                var anguloHorizontaTotal = anguloHorizontalTorreta - anguloHorizontalTanque;
                var b = new Vector3(
                    (float)Math.Cos(anguloHorizontaTotal - MathHelper.PiOver2), 
                    (float)Math.Sin(-anguloVertical), 
                    (float)Math.Sin(anguloHorizontaTotal - MathHelper.PiOver2));
                
                b *= 5;

                var a = new Bala(Position, b, Bullet, Effect, BulletTexture);
                tiempoEntreDisparo = 0;
                balas.Add(a);
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

            updateTurret(gameTime);
            

            Moving = false;

            System.Console.WriteLine(Position);

            //El auto en el world
            World = RotationMatrix * Matrix.CreateTranslation(Position) ;

        }

        public void updateTurret(GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();

            //updateVertical(currentMouseState, gameTime);
            Matrix anguloH = updateHorizontal(currentMouseState, gameTime);
            Matrix anguloV = updateVertical(currentMouseState,gameTime);


            Cannon.Transform = _initialCannon * anguloH;
            Cannon.Transform = anguloV * Cannon.Transform;
            Torreta.Transform = _initialTorret * anguloH;

            // Actualiza el estado anterior del mouse para el próximo ciclo
            estadoAnteriorDelMouse = new Vector2(currentMouseState.X,currentMouseState.Y);
        }

        public Matrix updateHorizontal(MouseState currentMouseState, GameTime gameTime)
        {



            // Obtén la posición actual del mouse
            //mouseX = currentMouseState.X;
            //
            //// Calcula la diferencia entre la posición del mouse actual y la posición anterior
            //float deltaX = mouseX - estadoAnteriorDelMouse.X;
            //
            //
            //// Ajusta la velocidad de rotación según tus preferencias
            //float rotationSpeed = 0.01f;
            //Matrix torretaRotacion = Matrix.Identity;
            //// Calcula la rotación en función de la diferencia del mouse
            //if (currentMouseState.RightButton.Equals(ButtonState.Pressed))
            //    torretaRotacion = Matrix.CreateRotationZ(-deltaX * rotationSpeed);
            //
            //
            //
            // Aplica la rotación a la torreta
            //Torreta.Transform *= torretaRotacion;
            //
            //// Aplica la misma rotación al cañón
            //Cannon.Transform *= torretaRotacion;
            //
            //TorretaMatrix = Torreta.Transform;
            //
            //// Actualiza la matriz de transformación relativa del cañón
            //Matrix transformacionRelativaDelCañon = Cannon.Transform * Matrix.Invert(Torreta.Transform);
            //
            //// Aplica la transformación relativa al cañón
            //Cannon.Transform = transformacionRelativaDelCañon * Torreta.Transform;

            float rotationSpeed = 0.01f;
            mouseX = currentMouseState.X;
            var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            Matrix torretaRotacion = Matrix.Identity;
            if (currentMouseState.RightButton.Equals(ButtonState.Pressed))
            {
                float deltaX = mouseX - estadoAnteriorDelMouse.X;

                anguloHorizontalTorreta += elapsedTime * deltaX;
            }
            return torretaRotacion = Matrix.CreateRotationZ(-anguloHorizontalTorreta);

            //Cannon.Transform = torretaRotacion * _initialCannon;
            //Torreta.Transform = torretaRotacion * _initialTorret;
        }

        public Matrix updateVertical(MouseState currentMouseState, GameTime gameTime) {

            float rotationSpeed = 0.01f;
            mouseY = currentMouseState.Y;
            var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            var cannonRotacion = Matrix.Identity;
            //Console.WriteLine("angulo: " + anguloActual);
            if(currentMouseState.RightButton.Equals(ButtonState.Pressed)){
                float deltaY = mouseY - estadoAnteriorDelMouse.Y;

                anguloVertical += elapsedTime * deltaY;
                anguloVertical = Math.Clamp(anguloVertical, -MathHelper.PiOver4, 0);

            }
            return Matrix.CreateRotationX(anguloVertical);

        }
    }
}