using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Collisions;


namespace TGC.MonoGame.TP
{    
    public class Tanque
    {
        public const float VidaMaxima = 10;
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
        public Texture2D NormalTexture { get; set; }
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

        private const float tiempoEntreDisparoLimite = 4f;

        public BoundingBox AABB;

        public bool balaEspecial { get ; set;}
        public SoundEffect SonidoDisparo { get; set; }
        public SoundEffectInstance InstanciaSonidoDisparo { get; set;}
        public SoundEffect SonidoMovimiento { get; set; }
        public SoundEffectInstance InstanciaSonidoMovimiento { get; set; }
        public AudioListener listener {get;set;}
        public Effect efectoTanque {  get; set;}

        //bling
        //public Effect efectoTanque {get;set;}

        public Tanque(Vector3 Position, Model modelo, Effect efecto, Texture2D textura, Vector2 estadoInicialMouse,SoundEffect sonidoDisparo, SoundEffect sonidoMovimiento)
        {

            World = Matrix.CreateWorld(Position, Vector3.Forward, Vector3.Up);
            
            Model = modelo;

            AABB = BoundingVolumesExtensions.CreateAABBFrom(Model);
            TankBox = OrientedBoundingBox.FromAABB(AABB);
            PuntoMedio = AABB.Max.Y/2;
            TankBox.Center = Position + Vector3.Up * PuntoMedio;
            TankBox.Orientation = RotationMatrix;

            this.Position = Position; // + Vector3.Up * 150;
            OldPosition = Position;
            listener = new AudioListener()
            {
                Position = this.Position  
            };

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

            Vida = VidaMaxima;
            balaEspecial = false;
            SonidoDisparo = sonidoDisparo;
            InstanciaSonidoDisparo = sonidoDisparo.CreateInstance();
            SonidoMovimiento = sonidoMovimiento;
            
            InstanciaSonidoMovimiento = sonidoMovimiento.CreateInstance();
            InstanciaSonidoMovimiento.Volume = 0.05f;
        }


        public void LoadContent(Model bala, Texture2D texturaBala,Texture2D texturaBalaEspecial){

            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.

            //Effect.Parameters["ModelTexture"].SetValue(Texture);

            // Al mesh le asigno el Effect (solo textura por ahora)
            Bullet = bala;

            BulletTexture = texturaBala;
            BulletSpecialTexture = texturaBalaEspecial;
            efectoTanque.CurrentTechnique = efectoTanque.Techniques["NormalMapping"];
            foreach (var mesh in Bullet.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Effect;
                }
            }

            //efectoTanque.CurrentTechnique = Effect.Techniques["Default"];
            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = efectoTanque;
                }
            }
        }

        //public void Draw(GameTime gameTime, Matrix view, Matrix projection)
        //{
        //    // Tanto la vista como la proyección vienen de la cámara por parámetro
        //    Effect.Parameters["View"].SetValue(view);
        //    Effect.Parameters["Projection"].SetValue(projection);
        //    Effect.Parameters["ModelTexture"]?.SetValue(Texture);
        //
        //
        //    var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
        //    Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
        //
        //    // forma de girar la torreta con cañón
        //    // Torreta.Transform = Torreta.Transform * Matrix.CreateRotationZ(0.006f);
        //    // Cannon.Transform = Cannon.Transform * Matrix.CreateRotationZ(0.006f);
        //
        //    World = RotationMatrix * Matrix.CreateTranslation(Position);
        //    foreach (var mesh in Model.Meshes)
        //    {
        //        var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
        //        Effect.Parameters["World"].SetValue(meshWorld * World);
        //        mesh.Draw();
        //    }
        //}

        public void Draw(GameTime gameTime, Matrix view, Matrix projection, Vector3 camaraPosition, RenderTarget2D ShadowMapRenderTarget, Vector3 lightPosition, int ShadowmapSize, TargetCamera TargetLightCamera)
        {
            actualizarEfecto(camaraPosition, ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            // Tanto la vista como la proyección vienen de la cámara por parámetro
            
            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            
        
            World = RotationMatrix * Matrix.CreateTranslation(Position);
            foreach (var mesh in Model.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                var finalWorld = meshWorld * World;
                efectoTanque.Parameters["World"].SetValue(finalWorld);
                efectoTanque.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(finalWorld)));
                efectoTanque.Parameters["WorldViewProjection"].SetValue(finalWorld * view * projection);
                mesh.Draw();
            }
            
        }
        
        public void actualizarEfecto(Vector3 camaraPosition, RenderTarget2D ShadowMapRenderTarget, Vector3 lightPosition, int ShadowmapSize, TargetCamera TargetLightCamera){
            efectoTanque.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f, 1f));
            efectoTanque.Parameters["diffuseColor"].SetValue(new Vector3(0.1f, 0.1f, 0.6f));
            efectoTanque.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));
        
            efectoTanque.Parameters["KAmbient"].SetValue(1f);
            efectoTanque.Parameters["KDiffuse"].SetValue(1f);
            efectoTanque.Parameters["KSpecular"].SetValue(0.8f);
            efectoTanque.Parameters["shininess"].SetValue(100f);
            efectoTanque.Parameters["eyePosition"].SetValue(camaraPosition);
        
            efectoTanque.Parameters["ModelTexture"].SetValue(Texture);
            efectoTanque.Parameters["NormalTexture"].SetValue(NormalTexture);
            efectoTanque.Parameters["Tiling"].SetValue(Vector2.One);


            efectoTanque.CurrentTechnique = efectoTanque.Techniques["NormalMapping"];
            efectoTanque.Parameters["shadowMap"].SetValue(ShadowMapRenderTarget);
            efectoTanque.Parameters["lightPosition"].SetValue(lightPosition);
            efectoTanque.Parameters["shadowMapSize"].SetValue(Vector2.One * ShadowmapSize);
            efectoTanque.Parameters["LightViewProjection"].SetValue(TargetLightCamera.View * TargetLightCamera.Projection);
        
        }

        public void DrawShadows(GameTime gameTime, Matrix view, Matrix projection)
        {
            //actualizarLuz(camaraPosition);
            // Tanto la vista como la proyección vienen de la cámara por parámetro
            efectoTanque.CurrentTechnique = efectoTanque.Techniques["DepthPass"];

            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            //World = RotationMatrix * Matrix.CreateTranslation(Position);
            foreach (var modelMesh in Model.Meshes)
            {
                foreach (var part in modelMesh.MeshParts)
                    part.Effect = efectoTanque;

                // We set the main matrices for each mesh to draw
                var worldMatrix = modelMeshesBaseTransforms[modelMesh.ParentBone.Index];

                // WorldViewProjection is used to transform from model space to clip space
                efectoTanque.Parameters["WorldViewProjection"]
                    .SetValue(worldMatrix * World * view * projection);

                // Once we set these matrices we draw
                modelMesh.Draw();
            }

        }



        float anguloVertical = 0;
        float anguloHorizontalTorreta = 0;
        float anguloHorizontalTanque = 0;
        float tiempoEntreDisparo = 3;
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
                TankBox.Rotate(Matrix.CreateRotationY(.03f));
                if(!ChoqueConObjetosSinDestruccion(ambiente, enemigos)){
                    TankDirection = RotationMatrix.Forward;
                    anguloHorizontalTanque += .03f;
                }
                else{
                    RotationMatrix *= Matrix.CreateRotationY(-.03f);
                    TankBox.Rotate(Matrix.CreateRotationY(-.03f));
                }
            }
            if (key.IsKeyDown(Keys.D))
            { // Giro a la izquierda 
                RotationMatrix *= Matrix.CreateRotationY(-.03f);
                TankBox.Rotate(Matrix.CreateRotationY(-.03f));
                if(!ChoqueConObjetosSinDestruccion(ambiente, enemigos)){
                    TankDirection = RotationMatrix.Forward;
                    anguloHorizontalTanque -= .03f;
                }
                else{
                    RotationMatrix *= Matrix.CreateRotationY(.03f);
                    TankBox.Rotate(Matrix.CreateRotationY(.03f));
                }
            }

            Position += TankVelocity;
            TankBox.Center = Position + Vector3.Up * PuntoMedio;
            CurrentAcceleration *= Acceleration;

            AgregarFriccion(deltaTime);

            if (Moving && moduloVelocidadXZ < 30f)
            {             
                TankVelocity += TankAcceleration * deltaTime;
            }

            // ** Disparo de balas ** //

            if (tiempoEntreDisparo < tiempoEntreDisparoLimite)
                tiempoEntreDisparo += deltaTime;
            updateTurret(gameTime);
            Disparo(key, balas);

            // ** Análisis de colisión ** //

            if (!ChoqueConObjetosSinDestruccion(ambiente, enemigos))
            {
                OldPosition = Position;
                //TankBox.Center = Position + Vector3.Up * PuntoMedio;
            }
            else{
                Position = OldPosition;
                TankBox.Center = OldPosition + Vector3.Up * PuntoMedio;
            }
            
            ChoqueDestructibles(ambiente);
            reproducirSonidoMovimiento(key.IsKeyDown(Keys.W) || key.IsKeyDown(Keys.S), gameTime.ElapsedGameTime);
            if (key.IsKeyDown(Keys.Escape))
            {
                InstanciaSonidoMovimiento.Stop();
            }
            Moving = false;

            World = RotationMatrix * Matrix.CreateTranslation(Position);

            listener.Position = Position;
            
        }

        TimeSpan progreso = TimeSpan.Zero;
        private void reproducirSonidoMovimiento(bool moving, TimeSpan delta)
        {
            progreso += delta;
            if (moving)
            {
                InstanciaSonidoMovimiento.Play();
                if(progreso.TotalMilliseconds >= SonidoMovimiento.Duration.TotalMilliseconds * .85){
                    var NuevaInstancia = SonidoMovimiento.CreateInstance();
                    NuevaInstancia.Volume = .05f;
                    NuevaInstancia.Play();
                    progreso = TimeSpan.Zero;
                }
            }
            else
            {
                InstanciaSonidoMovimiento.Stop();
            }
        }

        private void AgregarFriccion(float deltaTime)
        {
            var lateral_velocity = RotationMatrix.Right * Vector3.Dot(TankVelocity, RotationMatrix.Right);
            var lateral_friction = -lateral_velocity * 10f;
            var backwards_friction = -TankVelocity * 2f;
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
                    var a = new BalaEspecial(Position, b, Bullet, Effect, BulletSpecialTexture, this);
                    balas.Add(a);
                }
                else
                {
                    var a = new Bala(Position, b, Bullet, Effect, BulletTexture, this);
                    balas.Add(a);
                }
                tiempoEntreDisparo = 0;
                InstanciaSonidoDisparo.Play();
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
        public void ChoqueDestructibles(List<Object> ambiente){
            foreach (Object itemEspecifico in ambiente.Where(x => x.esEliminable == true)){
                if(Intersecta(itemEspecifico)){
                    itemEspecifico.esVictima = true;
                    itemEspecifico.reproducirSonido(listener);
                }
            }
        }
        public bool ChoqueConObjetosSinDestruccion(List<Object> ambiente, List<TanqueEnemigo> enemigos){
            foreach (Object itemEspecifico in ambiente.Where(x => x.esEliminable == false)){
                if(Intersecta(itemEspecifico)){
                    if(TankVelocity.Length() > 5f)
                        itemEspecifico.reproducirSonido(listener);
                    TankVelocity = -TankVelocity*.5f;
                    itemEspecifico.Colisiono = true;
                    TankBox.Center = OldPosition + Vector3.Up * PuntoMedio;
                    return true;
                }
            }

            foreach (TanqueEnemigo enemigoEspecifico in enemigos){
                if(Intersecta(enemigoEspecifico)){
                    var velocidadMain = TankVelocity;
                    TankVelocity = -TankVelocity*.5f;
                    
                    if(TankVelocity.Length() > 5f)
                        enemigoEspecifico.reproducirSonido(listener);
                    /*if(Math.Abs(velocidadMain.X) < 0.1 && Math.Abs(velocidadMain.Z) < 0.1)
                        velocidadMain -= enemigoEspecifico.TankVelocity/2;
                    TankVelocity -= enemigoEspecifico.TankVelocity;
                    enemigoEspecifico.agregarVelocidad(velocidadMain);*/
                    //TankVelocity /= 2; // El tanque enemigo al no tener velocidad en esta entrega simplemente se reduce la velocidad de nuestro tanque a la mitad
                    
                    //recibirDaño(.5f);
                    TankBox.Center = OldPosition + Vector3.Up * PuntoMedio;
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
            Vida = Math.Clamp(Vida, 0, VidaMaxima);
        }
    }
}