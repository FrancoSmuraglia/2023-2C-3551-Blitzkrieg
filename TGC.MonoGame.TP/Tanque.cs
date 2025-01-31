using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Particle3DSample;
using TGC.MonoGame.Samples.Collisions;


namespace TGC.MonoGame.TP
{    
    public class Tanque
    {
        private enum EstadoRuedas{
            Quieto,
            Avance,
            Retroceso,
            GiroDerecha,
            GiroIzquierda
        }
        private List<Vector3> Colisiones;
        private Vector3[] ColisionesArray;
        int posArrayVictima = 0;
        int posLlena = 0;
        private EstadoRuedas EstadoActualDeRuedas = EstadoRuedas.Quieto;
        public ParticleSystem polvo;
        public ParticleSystem rastroBala;
        public const float VidaMaxima = 100;
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
        
        public Texture2D TreadmillTexture { get; set; }
        public Texture2D TreadmillNormalTexture { get; set; }
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

        // ruedas

        private Matrix[] RuedasMatriz {get; set; }
        private ModelBone[] RuedasBones {get; set;}

        public List<Vector3> Impactos { get; set;}
        public List<Vector3> DireccionImpactos { get; set;}


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

            var a = modelo.Bones["Treadmill1"];
            

            // obtengo las ruedas

            RuedasMatriz = new Matrix[16];
            RuedasBones = new ModelBone[16];
            for (int i = 1; i < 17; i++)
            {
                int posicionLogica = i - 1;
                string nombreRueda = "Wheel";
                nombreRueda += i;
                RuedasBones[posicionLogica] = modelo.Bones[nombreRueda];
                RuedasMatriz[posicionLogica] = RuedasBones[posicionLogica].Transform;
            }

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

            Colisiones = new List<Vector3>();
            ColisionesArray = new Vector3[10];

            Impactos = new List<Vector3>();
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
        public void agregarImpacto(Vector3 posicionBala){
            if(posLlena < 10){
                var meshes = Model.Meshes;
                
                //var hullTransform = Model.Meshes.First(m => m.Name == "Hull").ParentBone.Transform;
                Matrix[] hullTransform = new Matrix[Model.Bones.Count];
                this.Model.CopyAbsoluteBoneTransformsTo(hullTransform);
                var mundo = World;
                Impactos.Add(Vector3.Transform(posicionBala, Matrix.Invert(hullTransform[2] * World)));

                posLlena = (posLlena < 10) ? posLlena + 1 : 9;
            }
            


            /*if(Colisiones.Count() < 10)
                Colisiones.Add(posicionBala);*/
        }
                        
                
                
                /*var impactDir = Vector3.Transform(direccionBala, Matrix.CreateRotationY(anguloHorizontalTanque));
                var direction = new Vector3(impactDir.X * -1, 150, impactDir.Z * -1);
                direction.Normalize();
                DireccionImpactos.Add(direction);*/
                //posArrayVictima = (posArrayVictima < 10) ? posArrayVictima + 1 : 0;

        public void Draw(GameTime gameTime, Matrix view, Matrix projection, Vector3 camaraPosition, 
                            RenderTarget2D ShadowMapRenderTarget, Vector3 lightPosition, int ShadowmapSize, TargetCamera TargetLightCamera, bool estatico)
        {
            if(!estatico){
                AnimarRuedas();
            }
            updateTurret(gameTime,estatico);
            actualizarEfecto(camaraPosition, ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            // Tanto la vista como la proyección vienen de la cámara por parámetro
            //Effect.Parameters["View"].SetValue(view);
            //Effect.Parameters["Projection"].SetValue(projection);
            //Effect.Parameters["ModelTexture"]?.SetValue(Texture);

            
            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            

            World = RotationMatrix * Matrix.CreateTranslation(Position);
            efectoTanque.CurrentTechnique = efectoTanque.Techniques["Deformaciones"];
            foreach (var mesh in Model.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                var finalWorld = meshWorld * World;
                
                efectoTanque.Parameters["World"].SetValue(finalWorld);
                efectoTanque.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(finalWorld)));
                efectoTanque.Parameters["WorldViewProjection"].SetValue(finalWorld * view * projection);
                efectoTanque.Parameters["View"]?.SetValue(view);
                efectoTanque.Parameters["Projection"]?.SetValue(projection);
                
                if(mesh.ParentBone.Name.Contains("Treadmill")){
                    efectoTanque.Parameters["ModelTexture"].SetValue(TreadmillTexture);
                    efectoTanque.Parameters["NormalTexture"].SetValue(TreadmillNormalTexture);    
                    if(mesh.ParentBone.Name == "Treadmill1"){
                        efectoTanque.Parameters["Rapidez"]?.SetValue(anguloGiroDeRuedas + anguloGiro);
                    }
                    else{
                        efectoTanque.Parameters["Rapidez"]?.SetValue(anguloGiroDeRuedas - anguloGiro);
                    }
                }


                /*if(mesh.Name == "Hull" || mesh.Name == "Turret")
                {*/
                    efectoTanque.Parameters["impactos"]?.SetValue(this.Impactos.ToArray());
                    efectoTanque.Parameters["impactosCantidad"]?.SetValue(this.Impactos.Count);
                //}
                
                //efectoTanque.Parameters["direccionImpactos"]?.SetValue(this.DireccionImpactos.ToArray());

                mesh.Draw();
                efectoTanque.Parameters["Rapidez"]?.SetValue(0);    
                efectoTanque.Parameters["ModelTexture"].SetValue(Texture);
                efectoTanque.Parameters["NormalTexture"].SetValue(NormalTexture);
                Effect.Parameters["impactosCantidad"]?.SetValue(0);
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


            efectoTanque.CurrentTechnique = efectoTanque.Techniques["Deformaciones"];
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
        public void Update(GameTime gameTime, KeyboardState key, List<Object> ambiente, List<TanqueEnemigo> enemigos, List<Bala> balas, List<Muro> muros)
        {

            float deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            float moduloVelocidadXZ = new Vector3(TankVelocity.X, 0f, TankVelocity.Z).Length();
            
            // ** Movimiento del tanque ** //
            
            if(moduloVelocidadXZ <= .1)
                EstadoActualDeRuedas = EstadoRuedas.Quieto;

            if (key.IsKeyDown(Keys.W))
            { //adeltante
                Moving = true;
                CurrentAcceleration = 1f;
                Sentido = 1;
                polvo.AddParticle(Position, -TankVelocity/3);
                EstadoActualDeRuedas = EstadoRuedas.Avance;
            }
            if (key.IsKeyDown(Keys.S))
            { //reversa                
                Moving = true;
                CurrentAcceleration = 1f;
                Sentido = -1;
                EstadoActualDeRuedas = EstadoRuedas.Retroceso;
            }
            if (key.IsKeyDown(Keys.A))
            { // Giro a la izquierda 
                RotationMatrix *= Matrix.CreateRotationY(.03f);
                TankBox.Rotate(Matrix.CreateRotationY(.03f));
                if(!ChoqueConObjetosSinDestruccion(ambiente, enemigos, muros)){
                    EstadoActualDeRuedas = EstadoRuedas.GiroIzquierda;
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
                if(!ChoqueConObjetosSinDestruccion(ambiente, enemigos, muros)){
                    EstadoActualDeRuedas = EstadoRuedas.GiroDerecha;
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
            
            Disparo(key, balas);

            // ** Análisis de colisión ** //

            if (!ChoqueConObjetosSinDestruccion(ambiente, enemigos, muros))
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
            

            /**/
            //ColisionesArray = Colisiones.GetRange(0,10).ToArray();
            
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
                    var a = new BalaEspecial(Position, b, Bullet, Effect, BulletSpecialTexture, this)
                    {
                        Rastros = rastroBala
                    };
                    balas.Add(a);
                }
                else
                {
                    var a = new Bala(Position, b, Bullet, Effect, BulletTexture, this)
                    {
                        Rastros = rastroBala
                    };
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
        public bool Intersecta(Muro muro){
            return TankBox.Intersects(muro.Colision);
        }
        public void ChoqueDestructibles(List<Object> ambiente){
            foreach (Object itemEspecifico in ambiente.Where(x => x.esEliminable == true)){
                if(Intersecta(itemEspecifico)){
                    itemEspecifico.esVictima = true;
                    itemEspecifico.reproducirSonido(listener);
                }
            }
        }
        public bool ChoqueConObjetosSinDestruccion(List<Object> ambiente, List<TanqueEnemigo> enemigos, List<Muro> muros){
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

            foreach (Muro muro in muros)
            {
                if(Intersecta(muro)){
                    TankVelocity = -TankVelocity*.5f;
                    TankBox.Center = OldPosition + Vector3.Up * PuntoMedio;
                    return true;
                }
            }

            TankBox.Center = Position + Vector3.Up * PuntoMedio;
            return false;
        }

        public void updateTurret(GameTime gameTime, bool permitirMovimiento)
        {
            MouseState currentMouseState = Mouse.GetState();
            //updateVertical(currentMouseState, gameTime);
            Matrix anguloH = updateHorizontal(currentMouseState, gameTime, !permitirMovimiento);
            Matrix anguloV = updateVertical(currentMouseState,gameTime, !permitirMovimiento);


            Cannon.Transform = _initialCannon * anguloH;
            Cannon.Transform = anguloV * Cannon.Transform;
            Torreta.Transform = _initialTorret * anguloH;

            // Actualiza el estado anterior del mouse para el próximo ciclo
            estadoAnteriorDelMouse = new Vector2(currentMouseState.X,currentMouseState.Y);
        }

        public Matrix updateHorizontal(MouseState currentMouseState, GameTime gameTime, bool permitirMovimiento)
        {
            float rotationSpeed = 0.01f;
            mouseX = currentMouseState.X;
            var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            if (currentMouseState.RightButton.Equals(ButtonState.Pressed) && permitirMovimiento)
            {
                float deltaX = mouseX - estadoAnteriorDelMouse.X;

                anguloHorizontalTorreta += elapsedTime * deltaX;
            }
            return Matrix.CreateRotationZ(-anguloHorizontalTorreta);

        }

        public Matrix updateVertical(MouseState currentMouseState, GameTime gameTime, bool permitirMovimiento) {
            mouseY = currentMouseState.Y;
            var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            if(currentMouseState.RightButton.Equals(ButtonState.Pressed) && permitirMovimiento){
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

        float anguloGiroDeRuedas = 0;
        float anguloGiro = 0;
        float anguloGiroIzquierda = 0;
        private void AnimarRuedas(){
           
            switch (EstadoActualDeRuedas)
            {
                case EstadoRuedas.Avance:
                    anguloGiroDeRuedas += TankVelocity.Length()/100;
                    for (int i = 0; i < 16; i++)
                    {
                        RuedasBones[i].Transform = Matrix.CreateRotationX(anguloGiroDeRuedas) * RuedasMatriz[i];
                    }
                    break;
                case EstadoRuedas.Retroceso:
                    anguloGiroDeRuedas -= TankVelocity.Length()/100;
                    for (int i = 0; i < 16; i++)
                    {
                        RuedasBones[i].Transform = Matrix.CreateRotationX(anguloGiroDeRuedas) * RuedasMatriz[i];
                    }
                    break;
                case EstadoRuedas.GiroDerecha:                        
                    anguloGiro += 0.1f;
                    for (int i = 0; i < 9; i++)
                    {
                        RuedasBones[i].Transform = Matrix.CreateRotationX(anguloGiroDeRuedas + anguloGiro) * RuedasMatriz[i];
                    }
                    for (int i = 9; i < 15; i++)
                    {
                        RuedasBones[i].Transform = Matrix.CreateRotationX(anguloGiroDeRuedas - anguloGiro) * RuedasMatriz[i];
                    }
                    break;
                case EstadoRuedas.GiroIzquierda:                        
                    anguloGiro += 0.1f;
                    for (int i = 0; i < 9; i++)
                    {
                        RuedasBones[i].Transform = Matrix.CreateRotationX(anguloGiroDeRuedas - anguloGiro) * RuedasMatriz[i];
                    }
                    for (int i = 9; i < 15; i++)
                    {
                        RuedasBones[i].Transform = Matrix.CreateRotationX(anguloGiroDeRuedas + anguloGiro) * RuedasMatriz[i];
                    }
                    break;
            
                
                default:
                    break;
            }
        }
    }
}