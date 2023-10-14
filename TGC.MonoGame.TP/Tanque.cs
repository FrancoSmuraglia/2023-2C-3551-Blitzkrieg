using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Collisions;


namespace TGC.MonoGame.TP
{    
    public class Tanque
    {
        // Colisión
        public OrientedBoundingBox TankBox { get; set; }
        private BoundingCylinder Cilindro { get; set; }
        Model Bullet;
        
        Texture2D BulletTexture;

        private float Speed { get; set; }

        private float PuntoMedio {get; set;}
        
        Matrix RotationMatrix = Matrix.Identity;

        protected Model Model { get; set; }
        public Matrix World { get; set; }

        protected Texture2D Texture { get; set; }
        public Vector3 Position{ get; set; }
        public Vector3 OldPosition{ get; set; }
        public Matrix OldRotation{ get; set; }
        protected float Rotation{ get; set; }
        protected Effect Effect { get; set; }

        public ModelBone Torreta;
        public ModelBone Cannon;
        private Matrix TorretaMatrix;
        private Matrix CannonMatrix;
        

        
        public Vector3 TankVelocity  { get; set; }
        private Vector3 TankDirection  { get; set; }
        
        private Boolean Moving  { get; set; }
        int Sentido;
        float Acceleration = 7f;
        float CurrentAcceleration = 0;


        private Matrix _initialCannon;
        private Matrix _initialTorret;

        private float mouseX = 0;
        private float mouseY = 0f;

        private float coolDownChoque = 0;

        private Vector2 estadoAnteriorDelMouse;

        private const float tiempoEntreDisparoLimite = 1.3f;

        public BoundingBox AABB;
        Vector3 posicionAnterior;
        public Tanque(Vector3 Position, Model modelo, Effect efecto, Texture2D textura, Vector2 estadoInicialMouse){
            this.Position = Position;

            World = Matrix.CreateWorld(Position, Vector3.Forward, Vector3.Up);
            
            Model = modelo;

            AABB = BoundingVolumesExtensions.CreateAABBFrom(Model);
            TankBox = OrientedBoundingBox.FromAABB(AABB);
            PuntoMedio = AABB.Max.Y/2;
            TankBox.Center = Position + Vector3.Up * PuntoMedio;
            TankBox.Orientation = RotationMatrix;


            

            Effect = efecto;

            Texture = textura;

            Torreta = modelo.Bones["Turret"];
            TorretaMatrix = Torreta.Transform;

            Cannon = modelo.Bones["Cannon"];
            CannonMatrix = Cannon.Transform;

            TankDirection = Vector3.Forward;


            
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

        
        
        float anguloVertical = 0;
        float anguloHorizontalTorreta = 0;
        float anguloHorizontalTanque = 0;
        float tiempoEntreDisparo = 0;
        public void Update(GameTime gameTime, KeyboardState key, List<Object> ambiente, List<TanqueEnemigo> enemigos, List<Bala> balas){
            float deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            float moduloVelocidadXZ = new Vector3(TankVelocity.X, 0f, TankVelocity.Z).Length();
            //Console.WriteLine(coolDownChoque);
            if(coolDownChoque > 0)
                coolDownChoque -= deltaTime;
            
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
            if(key.IsKeyDown(Keys.A) && Moving && coolDownChoque <= 0){ 
                RotationMatrix *= Matrix.CreateRotationY(.03f);
                TankDirection = Vector3.Transform(Vector3.Forward, RotationMatrix); 
                TankVelocity = TankDirection * moduloVelocidadXZ * Sentido; 
                TankBox.Rotate(Matrix.CreateRotationY(.03f));
                anguloHorizontalTanque += .03f;
                OldRotation = Matrix.CreateRotationY(.03f);
            }                
            if(key.IsKeyDown(Keys.D) && Moving && coolDownChoque <= 0){
                RotationMatrix *= Matrix.CreateRotationY(-.03f);
                TankDirection = Vector3.Transform(Vector3.Forward, RotationMatrix); 
                TankVelocity = TankDirection * moduloVelocidadXZ * Sentido; 
                TankBox.Rotate(Matrix.CreateRotationY(-.03f));
                anguloHorizontalTanque -= .03f;
                OldRotation = Matrix.CreateRotationY(-.03f);
            }
            if(Mouse.GetState().LeftButton.Equals(ButtonState.Pressed) && tiempoEntreDisparo > tiempoEntreDisparoLimite){
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

            
            Position += TankVelocity;    
            TankBox.Center =  Position + Vector3.Up * PuntoMedio; 
            if(!AnalisisDeColision(ambiente, enemigos, moduloVelocidadXZ)){
                CurrentAcceleration *= Acceleration;                    // Decido la aceleración
                
                OldPosition = Position;
                OldRotation = RotationMatrix;

            }                          // Actualizo la posición en función de la velocidad actual


            //TankBox.Center =  Position + Vector3.Up * PuntoMedio;

            

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

            //System.Console.WriteLine(Position);

            //El auto en el world
            
            /*if (key.IsKeyDown(Keys.D))
            {
                RotationMatrix *= Matrix.CreateRotationY(-.03f);
                TankDirection = Vector3.Transform(Vector3.Forward, RotationMatrix);
            }
            else if (key.IsKeyDown(Keys.A))
            {
                RotationMatrix *= Matrix.CreateRotationY(.03f);
                TankDirection = Vector3.Transform(Vector3.Forward, RotationMatrix);
            }
            if(key.IsKeyDown(Keys.W)){ //adeltante
                TankVelocity += TankDirection * /*2 * Acceleration*  deltaTime * 5;
                Sentido = 1;
            }
            if(key.IsKeyDown(Keys.S)){ //reversa      
                TankVelocity -= TankDirection /** 2 * Acceleration* * deltaTime * 5;
                Sentido = -1;
            }
            if(key.IsKeyUp(Keys.S) && key.IsKeyUp(Keys.W)){
                TankVelocity -= Sentido * TankDirection * Acceleration * deltaTime;
                if(Math.Abs(TankVelocity.X) < 0.1f && Math.Abs(TankVelocity.Z) < 0.1f ){
                    TankVelocity = new Vector3(0f, TankVelocity.Y, 0f);
                    Sentido = 0;
                }
            }

            Console.WriteLine(TankVelocity);

            Position += TankVelocity;*/


            World = RotationMatrix * Matrix.CreateTranslation(Position);
        }
        public void agregarVelocidad(Vector3 velocidad){
            TankVelocity = velocidad;
        }

        public bool Intersecta(TanqueEnemigo tanqueEnemigo){
            return TankBox.Intersects(tanqueEnemigo.TankBox);
        }

        public bool Intersecta(Object objeto){
            return TankBox.Intersects(objeto.Box);
        }
        
        public bool AnalisisDeColision(List<Object> ambiente, List<TanqueEnemigo> enemigos, float rapidez){
            foreach (Object itemEspecifico in ambiente){
                if(Intersecta(itemEspecifico)){
                    if(itemEspecifico.esEliminable){
                        itemEspecifico.esVictima = true;
                    }                        
                    else{
                        var vector = Vector3.Normalize(TankVelocity);
                        var salto = vector * 15;
                        coolDownChoque = .3f; //segundos
                        Sentido *= -1;
                        TankVelocity = - vector * 20;
                        TankBox.Center = OldPosition + Vector3.Up * PuntoMedio;
                        /*if(Intersecta(itemEspecifico)){
                            Console.WriteLine("SIGUE INTERSECTANDO, FUCK");
                            itemEspecifico.esVictima = true;
                        }*/

                        Position = OldPosition;
                    }
                    return true;
                }
            }

            foreach (TanqueEnemigo enemigoEspecifico in enemigos){
                if(Intersecta(enemigoEspecifico)){
                    var velocidadMain = TankVelocity;
                    if(velocidadMain.Equals(Vector3.Zero))
                        velocidadMain = -enemigoEspecifico.TankVelocity/2;
                    TankVelocity -= enemigoEspecifico.TankVelocity;
                    Console.WriteLine(TankVelocity.X + " y " + TankVelocity.Z);
                    enemigoEspecifico.agregarVelocidad(velocidadMain);
                    TankVelocity /= 2; // El tanque enemigo al no tener velocidad en esta entrega simplemente se reduce la velocidad de nuestro tanque a la mitad
                    return true;
                }
            }

            return false;
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
            float rotationSpeed = 0.01f;
            mouseX = currentMouseState.X;
            var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            if (currentMouseState.RightButton.Equals(ButtonState.Pressed))
            {
                float deltaX = mouseX - estadoAnteriorDelMouse.X;

                anguloHorizontalTorreta += elapsedTime * deltaX;
            }
            return Matrix.CreateRotationZ(-anguloHorizontalTorreta);

        }

        public Matrix updateVertical(MouseState currentMouseState, GameTime gameTime) {
            mouseY = currentMouseState.Y;
            var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            if(currentMouseState.RightButton.Equals(ButtonState.Pressed)){
                float deltaY = mouseY - estadoAnteriorDelMouse.Y;

                anguloVertical += elapsedTime * deltaY;
                anguloVertical = Math.Clamp(anguloVertical, -MathHelper.PiOver4, 0);

            }
            return Matrix.CreateRotationX(anguloVertical);

        }
    }
}