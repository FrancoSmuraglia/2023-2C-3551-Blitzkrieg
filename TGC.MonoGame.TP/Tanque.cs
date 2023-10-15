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
        Texture2D BulletSpecialTexture;

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

        public float Vida { get; set; }
        

        
        public Vector3 TankVelocity  { get; set; }
        private Vector3 TankDirection  { get; set; }
        public Vector3 TankAcceleration { get; set; } 
        
        private Boolean Moving  { get; set; }
        int Sentido;
        float Acceleration = 2f;
        float CurrentAcceleration = 0;


        private Matrix _initialCannon;
        private Matrix _initialTorret;

        private float mouseX = 0;
        private float mouseY = 0f;

        private float coolDownChoque = 0;

        private Vector2 estadoAnteriorDelMouse;

        private const float tiempoEntreDisparoLimite = 1.3f;

        public BoundingBox AABB;

        public bool balaEspecial { get ; set;}

        public Tanque(Vector3 Position, Model modelo, Effect efecto, Texture2D textura, Vector2 estadoInicialMouse){
            this.Position = Position + Vector3.Up * PuntoMedio;

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

            Vida = 10.0f;
            balaEspecial = false;
        }


        public void LoadContent(Model bala, Texture2D texturaBala,Texture2D texturaBalaEspecial){

            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.

            //Effect.Parameters["ModelTexture"].SetValue(Texture);

            // Al mesh le asigno el Effect (solo textura por ahora)
            Bullet = bala;

            BulletTexture = texturaBala;
            BulletSpecialTexture = texturaBalaEspecial;

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
        public void Update(GameTime gameTime, KeyboardState key, List<Object> ambiente, List<TanqueEnemigo> enemigos, List<Bala> balas)
        {
            float deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            float moduloVelocidadXZ = new Vector3(TankVelocity.X, 0f, TankVelocity.Z).Length();

            
            // ** Movimiento del tanque ** //

            if (key.IsKeyDown(Keys.W))
            { //adeltante
                Moving = true;
                CurrentAcceleration = 1f;
                Sentido = 1;
            }
            if (key.IsKeyDown(Keys.S))
            { //reversa                
                Moving = true;
                CurrentAcceleration = 1f;
                Sentido = -1;
            }
            if (key.IsKeyDown(Keys.A))
            { // Giro a la izquierda 
                RotationMatrix *= Matrix.CreateRotationY(.03f);
                TankDirection = RotationMatrix.Forward;
                TankBox.Rotate(Matrix.CreateRotationY(.03f));
                anguloHorizontalTanque += .03f;
            }
            if (key.IsKeyDown(Keys.D))
            { // Giro a la izquierda 
                RotationMatrix *= Matrix.CreateRotationY(-.03f);
                TankDirection = RotationMatrix.Forward;
                TankBox.Rotate(Matrix.CreateRotationY(-.03f));
                anguloHorizontalTanque -= .03f;
            }

            Position += TankVelocity;
            TankBox.Center = Position + Vector3.Up * PuntoMedio;
            CurrentAcceleration *= Acceleration;

            AgregarFriccion(deltaTime);

            if (Moving && moduloVelocidadXZ < 20f)
            {             
                TankVelocity += TankAcceleration * deltaTime;
            }

            // ** Disparo de balas ** //

            if (tiempoEntreDisparo < tiempoEntreDisparoLimite)
                            tiempoEntreDisparo += deltaTime;
            updateTurret(gameTime);
            Disparo(key, balas);

            // ** Análisis de colisión ** //

            if (!AnalisisDeColision(ambiente, enemigos, 1.5f))
            {
                OldPosition = Position;
                TankBox.Center = Position + Vector3.Up * PuntoMedio;
            }


            Moving = false;

            World = Matrix.CreateTranslation(Position) + RotationMatrix;
        }

        private void AgregarFriccion(float deltaTime)
        {
            var lateral_velocity = RotationMatrix.Right * Vector3.Dot(TankVelocity, RotationMatrix.Right);
            var lateral_friction = -lateral_velocity * 10f;
            var backwards_friction = -TankVelocity * 4f;
            TankVelocity += (backwards_friction + lateral_friction) * deltaTime;
            TankAcceleration = TankDirection * Sentido * CurrentAcceleration * 30;
        }

        private void Disparo(KeyboardState key, List<Bala> balas)
        {
            if (Mouse.GetState().LeftButton.Equals(ButtonState.Pressed) && tiempoEntreDisparo > tiempoEntreDisparoLimite)
            {
                var anguloHorizontaTotal = anguloHorizontalTorreta - anguloHorizontalTanque;
                var b = new Vector3(
                    (float)Math.Cos(anguloHorizontaTotal - MathHelper.PiOver2),
                    (float)Math.Sin(-anguloVertical),
                    (float)Math.Sin(anguloHorizontaTotal - MathHelper.PiOver2));

                b *= 5;

                if (balaEspecial)
                {
                    var a = new BalaEspecial(Position, b, Bullet, Effect, BulletSpecialTexture);
                    balas.Add(a);
                }
                else
                {
                    var a = new Bala(Position, b, Bullet, Effect, BulletTexture);
                    balas.Add(a);
                }
                tiempoEntreDisparo = 0;
            }
            if (key.IsKeyDown(Keys.D2))
            {
                balaEspecial = true;
            }
            else if (key.IsKeyDown(Keys.D1))
            {
                balaEspecial = false;
            }
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
                        
                        Position = OldPosition;
                        TankBox.Center = OldPosition + Vector3.Up * PuntoMedio;
                        if(TankBox.Intersects(itemEspecifico.Box))
                        {
                            TankVelocity = -TankVelocity*.5f;
                        }
                        //TankVelocity = Vector3.Zero;
                    }
                    return true;
                }
            }

            foreach (TanqueEnemigo enemigoEspecifico in enemigos){
                if(Intersecta(enemigoEspecifico)){
                    Position = OldPosition;
                    TankBox.Center = OldPosition + Vector3.Up * PuntoMedio;
                    TankVelocity = -TankVelocity*.5f;
                    var velocidadMain = TankVelocity;
                    /*if(velocidadMain.Equals(Vector3.Zero))
                        velocidadMain = -enemigoEspecifico.TankVelocity/2;*/
                    TankVelocity -= enemigoEspecifico.TankVelocity;
                    Console.WriteLine(TankVelocity.X + " y " + TankVelocity.Z);
                    enemigoEspecifico.agregarVelocidad(-velocidadMain);
                    TankVelocity /= 2; // El tanque enemigo al no tener velocidad en esta entrega simplemente se reduce la velocidad de nuestro tanque a la mitad
                    enemigoEspecifico.recibirDaño(0.5f);
                    recibirDaño(0.5f);
                    return true;
                }
            }
            
            TankBox.Center = Position + Vector3.Up * PuntoMedio;

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

        public void recibirDaño(float cantidad)
        {
            Vida -= cantidad;
        }
    }
}