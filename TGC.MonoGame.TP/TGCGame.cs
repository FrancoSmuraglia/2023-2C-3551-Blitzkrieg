﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using BepuPhysics.Collidables;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Particle3DSample;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Viewer.Gizmos;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Esta es la clase principal del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        public enum GameState
        {
            InitialMenu,
            Begin,
            Pause,
            Finished,
            Lost
        }
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";
        private const int DistanciaParaArboles = 500;
        private const int DistanciaParaArbustos = 15;
        private const int DistanciaParaFlores = 15;
        private const int DistanciaParaHongos = 15;
        private const int CantidadDeArboles = 75;
        private const int CantidadDeArbustos = 500;
        private const int CantidadDeFlores = 250;
        private const int CantidadDeHongos = 250;
        private const int CantidadDeRocas = 120;
        public SpriteFont Font {get ; set ;}

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
            Graphics.IsFullScreen = true;
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }
        public Gizmos Gizmos { get; set; }
        private Boolean GizmosActivado = false;
        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }

        private Model Model { get; set; }
        private Model T90 { get; set; }
        private Model T90A { get; set; }
        private Model T90B { get; set; }
        private Model T90C { get; set; }
        private Model Panzer { get; set; }
        private Effect Effect { get; set; }
        private float Rotation { get; set; }

        private Model BulletModel { get; set; }
        private List<Bala> bullets;
        
        private List<TanqueEnemigo> Tanques { get; set; }
        private List<Bala> BalasEnemigas { get; set; }
        private List<Bala> BalasMain { get; set; }

        private Object Prueba { get; set; }
        private Texture2D Textura { get; set; }
        private FollowCamera FollowCamera { get; set; }

        private Matrix vistaUsada {get; set;}
        private Matrix proyeccionUsada {get; set;}

        private Vector3 posicionUsada { get; set;}

        //private Suelo Suelo {get; set;}
        private QuadPrimitive Quad { get; set; }
        private Matrix FloorWorld { get; set; }

        //Menues
        public Vector2 PantallaResolucion {get; set;}
        public GameState EstadoActual {get; set;}
        public MenuPausa MenuPausa { get; set; }
        public MenuInicio MenuInicio { get; set; }

        //Frustum para delimitar dibujo
        private BoundingFrustum BoundingFrustum { get; set; }

        private Model roca { get; set; }
        private Object Roca { get; set; }
        private Effect EffectRoca { get; set; }
        private Texture2D TexturaRoca { get; set; }

        private List<Object> Ambiente { get; set; }

        public Tanque MainTanque { get; set; }

        private Vector2 estadoInicialMouse { get; set; }

        public float tiempoRestante = 300.0f; //5 minutos
        public bool juegoTerminado = false;
        public int puntos = 0;

        public Hud Hud { get; set; }
        public PantallaFinal PantallaFinal { get; set; }
        public Texture2D Fondo { get; set; }
        // Musica
        public Song Musica { get; set; }
        public Song MusicaFinal { get; set; }
        TimeSpan tiempoMusicaPrincipal = TimeSpan.Zero;
        public Song MusicaMenu { get; set; }
        
        bool SonidoActivado = true;
        internal FireParticleSystem fireParticles { get; private set; }

        TimeSpan tiempoMusicaMenu = TimeSpan.Zero;

        SkyBox SkyBox;


        //Blinn Phong
        private CubePrimitive LightBox;

        private Effect EffectLight { get; set; }
        private Matrix LightBoxWorld { get; set; } = Matrix.Identity;
        private Vector3 lightPosition = new Vector3(500f, 14000f, 500f);

        //Shadows
        private const int ShadowmapSize = 8192;

        private readonly float LightCameraFarPlaneDistance = 30000f;

        private readonly float LightCameraNearPlaneDistance = 50;
        private TargetCamera TargetLightCamera { get; set; }
        private RenderTarget2D ShadowMapRenderTarget;
        private RenderTarget2D pixel;


        // particula 
        
        List<Projectile> projectiles = new List<Projectile>();
        ParticleSystem explosionParticles;
        ParticleSystem explosionSmokeParticles;
        ParticleSystem projectileTrailParticles;

        SmokePlumeParticleSystem smokePlumeParticles { get; set; }

        //Muro
        List<Muro> Muros {get; set;}
        private Muro Muro1 { get; set; }
        private Muro Muro2 { get; set; }
        private Muro Muro3 { get; set; }
        private Muro Muro4 { get; set; }
        private Muro Muro5 { get; set; }
        private Muro Muro6 { get; set; }
        private Muro Muro7 { get; set; }
        private Muro Muro8 { get; set; }

        //FreeCamera
        FreeCamera FreeCamera { get; set; }



        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {

            explosionParticles = new ExplosionParticleSystem(this, Content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(this, Content);
            projectileTrailParticles = new ProjectileTrailParticleSystem(this, Content);
            smokePlumeParticles = new SmokePlumeParticleSystem(this, Content);
            fireParticles = new FireParticleSystem(this, Content);
            smokePlumeParticles = new SmokePlumeParticleSystem(this, Content);

            
            explosionParticles.DrawOrder = 400;
            explosionSmokeParticles.DrawOrder = 200;
            projectileTrailParticles.DrawOrder = 300;
            smokePlumeParticles.DrawOrder = 100;

            Components.Add(explosionParticles);
            Components.Add(explosionSmokeParticles);
            Components.Add(projectileTrailParticles);
            Components.Add(smokePlumeParticles);
            
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

            // Apago el backface culling.
            // Esto se hace por un problema en el diseno del modelo del logo de la materia.
            // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.CullClockwiseFace;
            GraphicsDevice.RasterizerState = rasterizerState;

            // Seria hasta aca.
            PantallaResolucion = new Vector2
            {
                X = (int)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                Y = (int)GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
            };

            Graphics.PreferredBackBufferWidth = (int)PantallaResolucion.X;
            Graphics.PreferredBackBufferHeight = (int)PantallaResolucion.Y;
            Graphics.ApplyChanges();
            Gizmos = new Gizmos
            {
                Enabled = true
            };

            Font = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "File"); 

            Mouse.SetPosition((int)PantallaResolucion.X  / 2, (int)PantallaResolucion.Y  / 2);
            estadoInicialMouse = PantallaResolucion/2;

            /*Mouse.SetPosition(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2);
            estadoInicialMouse = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2);*/

            EstadoActual = GameState.InitialMenu;



            Tanques = new List<TanqueEnemigo>();

            // Configuramos nuestras matrices de la escena, en este caso se realiza en el objeto FollowCamara
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);

            // Configuración del frustum
            BoundingFrustum = new BoundingFrustum(FollowCamera.View * FollowCamera.Projection);

            FloorWorld = Matrix.Identity; //Matrix.CreateScale(10000f, 1f, 10000f);

            Ambiente = new List<Object>();

            TargetLightCamera = new TargetCamera(1f, lightPosition, Vector3.Zero);
            TargetLightCamera.BuildProjection(1f, LightCameraNearPlaneDistance, LightCameraFarPlaneDistance,
                MathHelper.PiOver2);

            FreeCamera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0,1000f,0));
            
            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Gizmos.LoadContent(GraphicsDevice, Content);



            // Cargo el modelo, efecto y textura del tanque que controla el jugador.
            T90 = Content.Load<Model>(ContentFolder3D + "T90");
            
            Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            Textura = Content.Load<Texture2D>(ContentFolder3D + "textures_mod/hullA");
            SoundEffect sonidoDisparo = Content.Load<SoundEffect>(ContentFolderMusic + "SFX/Tank/TankShooting");
            SoundEffect sonidoMovimiento = Content.Load<SoundEffect>(ContentFolderMusic + "SFX/Tank/TankMoving2");
            SoundEffect sonidoTorreta = Content.Load<SoundEffect>(ContentFolderMusic + "SFX/Tank/TankTurretMoving");

            EffectLight = Content.Load<Effect>(ContentFolderEffects + "BlinnPhong");
            //BulletModel = Content.Load<Model>(ContentFolder3D + "bullet");
            MainTanque = new Tanque(
                    new Vector3(0f, 150, 0f),
                    T90,
                    EffectLight,
                    Content.Load<Texture2D>(ContentFolder3D + "textures_mod/hullA"),
                    estadoInicialMouse,
                    sonidoDisparo,
                    sonidoMovimiento
                    )
            {
                efectoTanque = EffectLight,
                NormalTexture = Content.Load<Texture2D>(ContentFolder3D + "textures_mod/normal"),
                polvo = smokePlumeParticles,
                rastroBala = explosionSmokeParticles,
                TreadmillTexture = Content.Load<Texture2D>(ContentFolder3D + "textures_mod/treadmills"),
                TreadmillNormalTexture = Content.Load<Texture2D>(ContentFolder3D + "textures_mod/treadmills_normal")
            };

            MainTanque.LoadContent(Content.Load<Model>(ContentFolder3D + "Bullet/Bullet"), null, Content.Load<Texture2D>(ContentFolderTextures + "gold"));

            Quad = new QuadPrimitive(GraphicsDevice, Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Gravel_001_BaseColor"), Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Gravel_001_Normal"));


            roca = Content.Load<Model>(ContentFolder3D + "Rock/rock");
            EffectRoca = Content.Load<Effect>(ContentFolderEffects + "BasicShaderRock");

            // Menu 
            InitializeMenus();

            InitializeTanks();

            InitializeHUD();

            Fondo = Content.Load<Texture2D>(ContentFolderTextures + "PantallaFinal/fondoEstrellas");
            PantallaFinal = new PantallaFinal(PantallaResolucion, Fondo, Font)
            {
                Logo = Content.Load<Texture2D>(ContentFolderTextures + "Menu/Blitzkrieg")
            };

            BalasMain = new List<Bala>();
            BalasEnemigas = new List<Bala>();

            Musica = Content.Load<Song>(ContentFolderMusic + "Ambient/MainGame");
            MusicaMenu = Content.Load<Song>(ContentFolderMusic + "Ambient/MenuPause");
            MusicaFinal = Content.Load<Song>(ContentFolderMusic + "Ambient/FinalScene");
            InitializeAmbient();

            Tanques.ForEach(o => o.LoadContent(Content.Load<Model>(ContentFolder3D + "Bullet/Bullet"), null, Content.Load<Effect>(ContentFolderEffects + "BlinnPhong")));
            Ambiente.ForEach(o => o.LoadContent());

            MediaPlayer.Volume = 0.2f;
            MediaPlayer.Play(Musica);
            MediaPlayer.IsRepeating = true;

            //SkyBox
            Model modeloSkyBox = Content.Load<Model>(ContentFolder3D + "SkyBox/cube");
            TextureCube textureCube = Content.Load<TextureCube>(ContentFolderTextures + "SkyBox/skybox");
            Effect efectoSkyBox = Content.Load<Effect>(ContentFolderEffects + "Skybox");
            SkyBox = new SkyBox(modeloSkyBox, textureCube, efectoSkyBox, 25000f);

            //light
            EffectLight.Parameters["lightPosition"].SetValue(lightPosition);
            LightBoxWorld = Matrix.CreateScale(3f) * Matrix.CreateTranslation(lightPosition);
            LightBox = new CubePrimitive(GraphicsDevice, 100, Color.Yellow);

            //Shadows
            ShadowMapRenderTarget = new RenderTarget2D(GraphicsDevice, ShadowmapSize, ShadowmapSize, false,
                SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

            pixel =  new RenderTarget2D(GraphicsDevice, (int)(PantallaResolucion.X/2.5), (int)(PantallaResolucion.Y/2.5), true,
            GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8,
            GraphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.PreserveContents);
            //pixel = new RenderTarget2D(GraphicsDevice, (int)PantallaResolucion.X/2, (int)PantallaResolucion.Y/2);


            //Muro

            Muro1 = new Muro(GraphicsDevice, Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_basecolor"), 
                Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_normal"),0);

            Muro2 = new Muro(GraphicsDevice, Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_basecolor"),
                Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_normal"),1);

            Muro3 = new Muro(GraphicsDevice, Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_basecolor"),
                Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_normal"),2);

            Muro4 = new Muro(GraphicsDevice, Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_basecolor"),
                Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_normal"),3);
            Muro5 = new Muro(GraphicsDevice, Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_basecolor"),
                Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_normal"), 4);
            Muro6 = new Muro(GraphicsDevice, Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_basecolor"),
                Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_normal"), 5);
            Muro7 = new Muro(GraphicsDevice, Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_basecolor"),
                Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_normal"), 6);
            Muro8 = new Muro(GraphicsDevice, Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_basecolor"),
                Content.Load<Texture2D>(ContentFolderTextures + "ParedPiso/Catacomb_Wall_001_normal"), 7);
            Muros = new List<Muro>(){
                Muro1,
                Muro2,
                Muro3,
                Muro4
            };
            base.LoadContent();
        }

        private void InitializeMenus()
        {
            Texture2D boton = Content.Load<Texture2D>(ContentFolderTextures + "Menu/Boton");
            var clickSound = Content.Load<SoundEffect>(ContentFolderMusic + "SFX/MenuPause/ButtonIsClicked");
            var continuar = new Button(boton, PantallaResolucion / 2, "Continuar", .3f)
            {
                Click = x => x.EstadoActual = GameState.Begin,
                ClickSound = clickSound
            };
            var salir = new Button(boton, PantallaResolucion / 2 + Vector2.UnitY * 200, "Salir", .3f)
            {
                Click = x => {
                            Thread.Sleep(clickSound.Duration);
                            x.Exit();
                            },
                ClickSound = clickSound
            };
            string mensaje = "Desactivar sonido";
            
            var sonido = new Button(boton, 
                                    PantallaResolucion / 2 + Vector2.UnitY * 100, 
                                    mensaje, 
                                    .3f)
            {
                ClickSound = clickSound
            };
            Action<TGCGame> function = x => {
                        SonidoActivado = !SonidoActivado;
                        SoundEffect.MasterVolume = SonidoActivado ? 1 : 0;
                        sonido.Text = SonidoActivado ? "Desactivar sonido" : "Activar sonido";
                        MediaPlayer.IsMuted = !SonidoActivado;
                            };
            sonido.Click = function;
            // no me juzguen, funca (que dios bendiga los punteros) :(

            var godSound = Content.Load<SoundEffect>(ContentFolderMusic + "SFX/MenuPause/Grito");
            var botonAngelical = Content.Load<Texture2D>(ContentFolderTextures + "Menu/BotonAngelical");
            var god = new Button(botonAngelical, 
                                new Vector2(botonAngelical.Width, botonAngelical.Height) * .2f / 2, 
                                "I'M GOD", .2f)
            {
                Click = x => x.isGod = !x.isGod,
                ClickSound = godSound
            };

            var arribaIzquierda = new Button(
                boton,
                new Vector2(boton.Width, boton.Height) * .3f / 2,
                "arribaIzquierda",
                .3f);

            var arribaDerecha = new Button(
                boton,
                new Vector2(PantallaResolucion.X, 0) + new Vector2(-boton.Width, boton.Height) * .3f / 2,
                "arribaDerecha",
                .3f);

            var abajoIzquierda = new Button(
                boton,
                new Vector2(0, PantallaResolucion.Y) + new Vector2(boton.Width, -boton.Height) * .3f / 2,
                "abajoIzquierda",
                .3f);

            var abajoDerecha = new Button(
                boton,
                PantallaResolucion - new Vector2(boton.Width, boton.Height) * .3f / 2,
                "abajoDerecha",
                .3f);

            List<Button> botonesPausa = new(){
                continuar,
                salir,
                god,
                sonido/*,
                arribaIzquierda,
                arribaDerecha,
                abajoIzquierda,
                abajoDerecha*/
            };

            MenuPausa = new MenuPausa(Content.Load<Texture2D>(ContentFolderTextures + "Menu/Reja"), PantallaResolucion, botonesPausa, Font)
            {
                juego = this,
                Logo = Content.Load<Texture2D>(ContentFolderTextures + "Menu/Blitzkrieg"),
                Cortina = Content.Load<SoundEffect>(ContentFolderMusic + "SFX/MenuPause/Chains").CreateInstance(),
                CortinaImpacto = Content.Load<SoundEffect>(ContentFolderMusic + "SFX/MenuPause/MetalImpact").CreateInstance()
            };
            
            // Pantalla de inicio

            var iniciar = new Button(boton, PantallaResolucion / 2, "Comenzar", .3f)
            {
                Click = x => x.EstadoActual = GameState.Begin,
                ClickSound = clickSound
            };
            var salida = new Button(boton, PantallaResolucion / 2 + Vector2.UnitY * 125, "Salir", .3f)
            {
                Click = x => {
                            Thread.Sleep(clickSound.Duration);
                            x.Exit();
                            },
                ClickSound = clickSound
            };

            var apagado = Content.Load<Texture2D>(ContentFolderTextures + "Menu/SonidoApagado");
            var encendido = Content.Load<Texture2D>(ContentFolderTextures + "Menu/SonidoEncendido");
            var sonidoInicio = new Button(encendido,new Vector2(PantallaResolucion.X, 0) + new Vector2(-encendido.Width, encendido.Height) * .15f / 2,null,.15f){
                ClickSound = clickSound
            };
            Action<TGCGame> funcionSonido = x => {
                SonidoActivado = !SonidoActivado;
                SoundEffect.MasterVolume = SonidoActivado ? 1 : 0;
                MediaPlayer.IsMuted = !SonidoActivado;
                var boton = SonidoActivado ? encendido : apagado;
                sonidoInicio.cambiarTextura(boton);
            };
            sonidoInicio.Click = funcionSonido;
            List<Button> botonesInicio = new(){
                iniciar,
                salida,
                sonidoInicio
            };

            MenuInicio = new MenuInicio(GraphicsDevice, PantallaResolucion, botonesInicio, Font)
            {
                juego = this,
                Logo = Content.Load<Texture2D>(ContentFolderTextures + "Menu/Blitzkrieg"),
                Fondo = Content.Load<Texture2D>(ContentFolderTextures + "Menu/WallpaperInicio")
            };

            MenuPausa.IniciarCortina();
        }

        private void InitializeHUD()
        {
            //var tiempo = new Button(
                //null, new Vector2(PantallaResolucion.X / 2, 40),
                 //null, 
                 //.17f);
            var texturaVida = Content.Load<Texture2D>(ContentFolderTextures + "Hud/Placa2");
            
            /*var vida = new Button(texturaVida, new Vector2(PantallaResolucion.X / 2, PantallaResolucion.Y/2),
                 null,
                 .2f)
            { 
                NotHover = new Color(0,0,0,0.2f)
            };*/
            var vida = new Button(texturaVida, new Vector2(PantallaResolucion.X/2, PantallaResolucion.Y - texturaVida.Height*.4f/2),
                 null,
                 .4f)
            { 
                NotHover = new Color(1,1,1,1f)
            };
            var bala = Content.Load<Texture2D>(ContentFolderTextures + "Hud/Bala");
            var botonBalaNormal = new Button(
                bala, 
                new Vector2(0, PantallaResolucion.Y) + new Vector2(bala.Width, -bala.Height) * .2f/2, 
                "Bala Normal",
                .2f){
                    TextHover = Color.DarkGray
                };
            var botonBalaEspecial = new Button(
                bala, 
                new Vector2(0, PantallaResolucion.Y) + new Vector2(bala.Width, -bala.Height - 900) * .2f/2, 
                "Bala Especial",
                .2f){
                    TextHover = Color.DarkGray
                };
            //var fps = new Button(Content.Load<Texture2D>(ContentFolderTextures + "Menu/Boton"), new Vector2(80, 30));
            //vida.NotTextHover = Color.White;
           
            List<Button> botonesHud = new(){
                //tiempo,
                vida,
                botonBalaNormal,
                botonBalaEspecial
                //fps
            };
            for (int i = 0; i < Tanques.Count; i++)
            {
                var nombre = "Tanque3";
                //int random = new Random().Next(0,1);
                //if(random == 1)
                //    nombre += "1";

                var a = new Button(
                    Content.Load<Texture2D>(ContentFolderTextures + "Hud/" + nombre),
                    new Vector2(PantallaResolucion.X - 80, PantallaResolucion.Y - 30 - 60 * i),
                    null,
                    .5f
                    )
                {
                    NotHover = new Color(.7f,.7f,.7f,.8f),
                    NotTextHover = Color.White
                };                
                botonesHud.Add(a);
            }

            var texturaReloj1 = Content.Load<Texture2D>(ContentFolderTextures + "Reloj/reloj-1");
            var texturaReloj2 = Content.Load<Texture2D>(ContentFolderTextures + "Reloj/reloj-2");
            var texturaReloj3 = Content.Load<Texture2D>(ContentFolderTextures + "Reloj/reloj-3");
            var texturaReloj4 = Content.Load<Texture2D>(ContentFolderTextures + "Reloj/reloj-4");

            List<Texture2D> texturasReloj = new()
            {
                texturaReloj1,
                texturaReloj2,
                texturaReloj3,
                texturaReloj4
            };

            Hud = new Hud(PantallaResolucion, botonesHud, Font)
            {
                RelojTexturas = texturasReloj,
                Vida = MainTanque.Vida,
                FuenteVida = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "File Vida")
            };
        }

        private void InitializeAmbient()
        {
            List<String> posiblesArboles = new(){
                "BigTree","BigTree2", "Tree", "SmallTree"
            };

            // textura de árboles se usa tanto para árboles como arbustos
            List<String> posiblesTexturasArboles = new(){
                "Tree", "Tree2", "Tree3", "Tree4", "Tree5", "Tree6", "Tree7"
            };
            
            List<String> posiblesFlores = new(){
                "Flower","Flower2", "Flower3"
            };

            List<String> posiblesTexturasFlores = new(){
                "Flower","Flower2", "Flower3", "Flower4"
            };

            List<String> posiblesArbustos = new(){
                "Bush","BigBush", "BiggerBush"
            };

            List<String> posiblesRocas = new(){
                "roca1", "roca2", "roca3", "roca4", "roca5", "roca6", "roca7", "roca8"
            };

            List<String> posiblesTexturasRocas = new(){
                "TexturaRoca1", "TexturaRoca2"
            };
            
            Vector3 posicionAmbiente;
            

            var arbolSonido = Content.Load<SoundEffect>(ContentFolderMusic + "SFX/Ambiente/TreeImpact");
            // Árboles
            for (int i = 0; i < CantidadDeArboles; i++)
            {
                posicionAmbiente = SelectNewPosition(DistanciaParaArboles, 3000);
                string arbol = posiblesArboles[new Random().Next(0, 3)];
                Ambiente.Add(
                    new Object(
                        posicionAmbiente,
                        Content.Load<Model>(ContentFolder3D + "Ambiente/" + arbol),
                        Content.Load<Effect>(ContentFolderEffects + "BlinnPhong"),
                        Content.Load<Texture2D>(ContentFolderTextures + posiblesTexturasArboles[new Random().Next(0, 6)]),
                        arbol.Equals("Tree"),
                        arbolSonido
                        )
                    );
            }
            
            // Anillo de árboles
            /*for (int i = 0; i < 500; i++)
            {
                float angle = new Random().Next(0, 360);
                float delta = new Random().Next(-150, 150);
                float dist = 7000 + delta;
                posicionAmbiente = new Vector3((dist * MathF.Cos(angle)), 0, (dist * MathF.Sin(angle)));
                String arbol = posiblesArboles[new Random().Next(0, 3)];
                Ambiente.Add(
                    new Object(
                        posicionAmbiente,
                        Content.Load<Model>(ContentFolder3D + "Ambiente/" + arbol),
                        Content.Load<Effect>(ContentFolderEffects + "BasicShader"),
                        Content.Load<Texture2D>(ContentFolderTextures + posiblesTexturasArboles[new Random().Next(0, 6)]),
                        false
                        )
                    );
            }*/
            
            var plantasNoArbolSonido = Content.Load<SoundEffect>(ContentFolderMusic + "SFX/Ambiente/NotTreeImpact");
            // Arbustos
            for (int i = 0; i < CantidadDeArbustos; i++)
            {
                posicionAmbiente = SelectNewPosition(DistanciaParaArbustos, 1);

                Ambiente.Add(
                    new Object(
                        posicionAmbiente,
                        Content.Load<Model>(ContentFolder3D + "Ambiente/" + posiblesArbustos[new Random().Next(0, 3)]),
                        Content.Load<Effect>(ContentFolderEffects + "BlinnPhong"),
                        Content.Load<Texture2D>(ContentFolderTextures + posiblesTexturasArboles[new Random().Next(0, 6)]),
                        true,
                        plantasNoArbolSonido
                        )
                    );
            }

            // Flores
            for (int i = 0; i < CantidadDeFlores; i++)
            {
                posicionAmbiente = SelectNewPosition(DistanciaParaFlores, 1);

                Ambiente.Add(
                    new Object(
                        posicionAmbiente,
                        Content.Load<Model>(ContentFolder3D + "Ambiente/" + posiblesFlores[new Random().Next(0, 2)]),
                        Content.Load<Effect>(ContentFolderEffects + "BlinnPhong"),
                        Content.Load<Texture2D>(ContentFolderTextures + posiblesTexturasFlores[new Random().Next(0, 3)]), 
                        true,
                        plantasNoArbolSonido
                        )
                    );
            }

            // Hongos
            for (int i = 0; i < CantidadDeHongos; i++)
            {
                posicionAmbiente = SelectNewPosition(DistanciaParaHongos, 1);

                Ambiente.Add(
                    new Object(
                        posicionAmbiente,
                        Content.Load<Model>(ContentFolder3D + "Ambiente/Mushroom"),
                        Content.Load<Effect>(ContentFolderEffects + "BlinnPhong"),
                        Content.Load<Texture2D>(ContentFolderTextures + "Mushroom"),
                        true,
                        plantasNoArbolSonido)
                    );
            }

            // Rocas
            for (int i = 0; i < CantidadDeRocas; i++)
            {
                posicionAmbiente = SelectNewPosition(100, 5000);
                posicionAmbiente += Vector3.Up * 30;

                Ambiente.Add(
                    new Object(
                        posicionAmbiente,
                        Content.Load<Model>(ContentFolder3D + "Rocas/" + posiblesRocas[new Random().Next(0, 7)]),
                        Content.Load<Effect>(ContentFolderEffects + "BlinnPhong"),
                        Content.Load<Texture2D>(ContentFolderTextures + posiblesTexturasRocas[new Random().Next(0, 1)]),
                        false)
                    );
                Ambiente.Last().World = Matrix.CreateScale(300f) * Ambiente.Last().World;
            }
        }

        private Vector3 SelectNewPosition(int distanciaMinimaEntreObjetos, int radio)
        {
            Vector3 posicionAmbiente = new Vector3();
            int X;
            int Z;
            do
            { //que genere posiciones hasta que esté a más de lo establecido por parámetro
                X = new Random().Next(-9500, 9500);
                Z = new Random().Next(-9500, 9500);
                posicionAmbiente = new Vector3(X, 0f, Z);
            }
            while (
                Ambiente.Exists( arbol => Vector3.Distance(arbol.Position, posicionAmbiente) < distanciaMinimaEntreObjetos ) ||
                Vector3.Distance(Vector3.Zero, posicionAmbiente) < radio
                //Tanques.Exists( tanqueEnemigo => Vector3.Distance(tanqueEnemigo.Position, posicionAmbiente) < 6000 ) ||
                //Vector3.Distance(MainTanque.Position, posicionAmbiente) < 6000
                );
            return posicionAmbiente;
        }

        private void InitializeTanks()
        {
            /*for (int i = 0; i < 10; i++)
            {
                objetos3D.Add(new Object(
                    new Vector3(1000f*i, 150, 0), 
                    T90, 
                    Content.Load<Effect>(ContentFolderEffects + "BasicShader"), 
                    Content.Load<Texture2D>(ContentFolder3D + "textures_mod/hullA")));

                objetos3D.Add(new Object(
                    new Vector3(1000f*i, 150, -1000f), 
                    T90, 
                    Content.Load<Effect>(ContentFolderEffects + "BasicShader"), 
                    Content.Load<Texture2D>(ContentFolder3D + "textures_mod/hullB")));
            }*/
            var sonidoDeColision = Content.Load<SoundEffect>(ContentFolderMusic + "SFX/Tank/TankBeingFired_1");
            var tanque1 = new TanqueEnemigo(new Vector3(8000f, 300, 8000f), Content.Load<Model>(ContentFolder3D + "T90"), Content.Load<Effect>(ContentFolderEffects + "BlinnPhong"), Content.Load<Texture2D>(ContentFolder3D + "textures_mod/hullA"))
            {
                NormalTexture = Content.Load<Texture2D>(ContentFolder3D + "textures_mod/normal"),
                SonidoColision = sonidoDeColision,
                polvo = smokePlumeParticles,
                rastroBala = explosionSmokeParticles
            };
            var tanque2 = new TanqueEnemigo(new Vector3(-8000, 300, -8000), Content.Load<Model>(ContentFolder3D + "T90"), Content.Load<Effect>(ContentFolderEffects + "BlinnPhong"), Content.Load<Texture2D>(ContentFolder3D + "textures_mod/hullB"))
            {
                NormalTexture = Content.Load<Texture2D>(ContentFolder3D + "textures_mod/normal"),
                SonidoColision = sonidoDeColision,
                polvo = smokePlumeParticles,
                rastroBala = explosionSmokeParticles
            };
            var tanque3 = new TanqueEnemigo(new Vector3(8000f, 300, -8000), Content.Load<Model>(ContentFolder3D + "T90"), Content.Load<Effect>(ContentFolderEffects + "BlinnPhong"), Content.Load<Texture2D>(ContentFolder3D + "textures_mod/hullC"))
            {
                NormalTexture = Content.Load<Texture2D>(ContentFolder3D + "textures_mod/normal"),
                SonidoColision = sonidoDeColision,
                polvo = smokePlumeParticles,
                rastroBala = explosionSmokeParticles
            };
            var tanque4 = new TanqueEnemigo(new Vector3(-8000, 300, 8000), Content.Load<Model>(ContentFolder3D + "T90"), Content.Load<Effect>(ContentFolderEffects + "BlinnPhong"), Content.Load<Texture2D>(ContentFolder3D + "textures_mod/mask"))
            {
                NormalTexture = Content.Load<Texture2D>(ContentFolder3D + "textures_mod/normal"),
                SonidoColision = sonidoDeColision,
                polvo = smokePlumeParticles,
                rastroBala = explosionSmokeParticles
            };
            Tanques.Add(tanque1);
            Tanques.Add(tanque2);
            Tanques.Add(tanque3);
            Tanques.Add(tanque4);
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        
        int frames = 0;
        float tiempo = 0;
        KeyboardState estadoAnterior;
        TimeSpan timeToNextProjectile;
        void UpdateExplosions(GameTime gameTime)
        {
            timeToNextProjectile -= gameTime.ElapsedGameTime;

            if (timeToNextProjectile <= TimeSpan.Zero)
            {
                // Create a new projectile once per second. The real work of moving
                // and creating particles is handled inside the Projectile class.
                projectileTrailParticles.AddParticle(MainTanque.Position, Vector3.Zero);
                projectiles.Add(new Projectile(explosionParticles,
                                               explosionSmokeParticles,
                                               projectileTrailParticles));

                timeToNextProjectile += TimeSpan.FromSeconds(1);
            }
        }
        void UpdateProjectiles(GameTime gameTime)
        {
            int i = 0;

            while (i < projectiles.Count)
            {
                if (!projectiles[i].Update(gameTime))
                {
                    // Remove projectiles at the end of their life.
                    projectiles.RemoveAt(i);
                }
                else
                {
                    // Advance to the next projectile.
                    i++;
                }
            }
        }
        /*void UpdateFire()
        {
            const int fireParticlesPerFrame = 20;

            // Create a number of fire particles, randomly positioned around a circle.
            for (int i = 0; i < fireParticlesPerFrame; i++)
            {
                fireParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);
            }

            // Create one smoke particle per frmae, too.
            smokePlumeParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);
        }*/
        float Timer;
        private bool isGod = false;

        protected override void Update(GameTime gameTime)
        {
            TargetLightCamera.BuildView();
            //Mouse.SetPosition((int)PantallaResolucion.X  / 2, (int)PantallaResolucion.Y  / 2);
            /*tiempo += (float)(gameTime.ElapsedGameTime.TotalSeconds);
            frames++;
            Console.WriteLine("Frames: " + 1000f/(float)(gameTime.ElapsedGameTime.TotalMilliseconds));*/

            //Console.WriteLine(EstadoActual);
            UpdateExplosions(gameTime);
            //UpdateProjectiles(gameTime);
            if(isGod){
                FreeCamera.Update(gameTime);
                vistaUsada = FreeCamera.View;
                proyeccionUsada = FreeCamera.Projection;
                posicionUsada = FreeCamera.Position;
            }
            else{
                FollowCamera.Update(gameTime, MainTanque.World);
                vistaUsada = FollowCamera.View;
                proyeccionUsada = FollowCamera.Projection;
                posicionUsada = FollowCamera.CamaraPosition;
            }
            smokePlumeParticles.SetCamera(vistaUsada, proyeccionUsada);
            explosionParticles.SetCamera(vistaUsada, proyeccionUsada);
            explosionSmokeParticles.SetCamera(vistaUsada, proyeccionUsada);
            projectileTrailParticles.SetCamera(vistaUsada, proyeccionUsada);
            BoundingFrustum.Matrix = FollowCamera.View * FollowCamera.Projection;

            switch (EstadoActual)
            {   
                case GameState.InitialMenu:
                    
                    MenuInicio.Update(Mouse.GetState());
                    break;
                case GameState.Begin:
                    //HUD.update(gametime);
                    if(FollowCamera.Frenado)
                    {
                        MediaPlayer.Stop();
                        MenuPausa.IniciarCortina();
                        MenuPausa.Cortina.Stop();
                        FollowCamera.FrenarCamara();

                        
                        tiempoMusicaMenu = MediaPlayer.PlayPosition;
                        MediaPlayer.Play(Musica, tiempoMusicaPrincipal);
                    }
                    //smokePlumeParticles.Draw(gameTime);
                    if(BotonPresionado(Keys.G)){
                        GizmosActivado = !GizmosActivado;
                    }
                    if(!isGod)
                        MainGame(gameTime);

                    
                    if (BotonPresionado(Keys.Escape))
                    {
                        //Exit();
                        EstadoActual = GameState.Pause;
                        //return;
                    }

                    break;
                case GameState.Pause:
                    if (!FollowCamera.Frenado)
                    {
                        MenuPausa.Estado = MenuPausa.EstadoMenuPausa.Inicio;
                        FollowCamera.FrenarCamara();
                        
                        tiempoMusicaPrincipal = MediaPlayer.PlayPosition;
                        MediaPlayer.Stop();
                        MediaPlayer.Play(MusicaMenu);
                    }
                    

                    MenuPausa.Update(Mouse.GetState());
                    if (BotonPresionado(Keys.Escape))
                    {
                        //Salgo del juego.
                        //Exit();
                        EstadoActual = GameState.Begin;
                        MediaPlayer.Resume();
                    }
                    break;

                case GameState.Finished:
                case GameState.Lost:
                    if (!FollowCamera.Frenado)
                    {
                        FollowCamera.FrenarCamara();    
                        MediaPlayer.Stop();
                        MediaPlayer.Play(MusicaFinal);
                    }
                    PantallaFinal.Update(gameTime);
                    if(BotonPresionado(Keys.Escape))
                        Exit();
                    break;
            }
            Gizmos.UpdateViewProjection(vistaUsada, proyeccionUsada);
            estadoAnterior = Keyboard.GetState();

            Hud.Update(tiempoRestante, MainTanque.Vida, MainTanque.balaEspecial, gameTime, Tanques);

            LightBoxWorld = Matrix.CreateTranslation(lightPosition);


            if(EstadoActual.Equals(GameState.Begin) && !isGod)
                base.Update(gameTime);
        }

        public bool BotonPresionado(Keys tecla)
        {
            return Keyboard.GetState().IsKeyUp(tecla) && estadoAnterior.IsKeyDown(tecla);
        }

        private void MainGame(GameTime gameTime)
        {
            
            tiempoRestante -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (tiempoRestante <= 0)
            {
                juegoTerminado = true;
                EstadoActual = GameState.Lost;
            }

            if(BotonPresionado(Keys.M))
            {
                EstadoActual = GameState.Lost;
            }
            if(BotonPresionado(Keys.N))
            {
                EstadoActual = GameState.Finished;
            }

            

            Tanques.ForEach(TanqueEnemigoDeLista =>
            {
                TanqueEnemigoDeLista.Update(gameTime, Ambiente, MainTanque, BalasEnemigas);
            });

            // Control del jugador
            if(!isGod)
                MainTanque.Update(gameTime, Keyboard.GetState(), Ambiente, Tanques, BalasMain, Muros);

            BalasMain.ForEach(o => o.Update(gameTime, Tanques, Ambiente));

            BalasEnemigas.ForEach(O => O.Update(gameTime, MainTanque, Ambiente));

            BalasMain.RemoveAll(O => O.esVictima || O.recorridoCompleto());
            BalasEnemigas.RemoveAll(O => O.esVictima || O.recorridoCompleto());

            if (Tanques.Exists(O => O.estaMuerto))
            {
                Tanques.RemoveAll(O => O.estaMuerto);
                InitializeHUD();
                Hud.Update(tiempoRestante, MainTanque.Vida, MainTanque.balaEspecial, gameTime, Tanques);
                puntos += 100;
            }

            if(Tanques.Count == 0)
            {
                EstadoActual = GameState.Finished;
                puntos += (int)tiempoRestante * 25;
            }

            if(MainTanque.Vida == 0)
            {
                EstadoActual = GameState.Lost;
            }



            if (Ambiente.Exists(O => O.esVictima))
                Ambiente.RemoveAll(O => O.esVictima);

            Muro1.Update(BalasEnemigas, BalasMain);
            Muro2.Update(BalasEnemigas, BalasMain);
            Muro3.Update(BalasEnemigas, BalasMain);
            Muro4.Update(BalasEnemigas, BalasMain);

        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        /// 

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.BlueViolet);
            DrawShadowMap(gameTime);
            DrawScene(gameTime);
        }

        private void DrawShadowMap(GameTime gameTime)
        {
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Set the render target as our shadow map, we are drawing the depth into this texture
            GraphicsDevice.SetRenderTarget(ShadowMapRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            //EffectLight.Parameters["lightPosition"].SetValue(lightPosition);
            Quad.DrawShadows(EffectLight, FloorWorld, TargetLightCamera.View, TargetLightCamera.Projection);

            //Muros
            Muro1.DrawShadows(EffectLight, Matrix.Identity, TargetLightCamera.View, TargetLightCamera.Projection);
            Muro2.DrawShadows(EffectLight, Matrix.Identity, TargetLightCamera.View, TargetLightCamera.Projection);
            Muro3.DrawShadows(EffectLight, Matrix.Identity, TargetLightCamera.View, TargetLightCamera.Projection);
            Muro4.DrawShadows(EffectLight, Matrix.Identity, TargetLightCamera.View, TargetLightCamera.Projection);


            //MainTanque.efectoTanque.Parameters["lightPosition"].SetValue(lightPosition);
            MainTanque.DrawShadows(gameTime, TargetLightCamera.View, TargetLightCamera.Projection);

            Ambiente.ForEach(ambientes =>
            {
                if (ambientes.Box.Intersects(BoundingFrustum))
                    ambientes.DrawShadows(gameTime, TargetLightCamera.View, TargetLightCamera.Projection);
                //Gizmos.DrawCube(ambientes.Box.Center, ambientes.Box.Extents * 2f, ambientes.Colisiono ? Color.Blue : Color.Red);
                //ambientes.Colisiono = false;

            });

            Tanques.ForEach(tanquesEnemigos => {
                if (tanquesEnemigos.TankBox.Intersects(BoundingFrustum))
                {
                    tanquesEnemigos.DrawShadows(gameTime, TargetLightCamera.View, TargetLightCamera.Projection);
                }
                //Gizmos.DrawCube(tanquesEnemigos.TankBox.Center, tanquesEnemigos.TankBox.Extents * 2f, Color.Black);
            });

            BalasMain.ForEach(balas => {
                if (balas.BalaBox.Intersects(BoundingFrustum))
                    balas.DrawShadows(gameTime, TargetLightCamera.View, TargetLightCamera.Projection);
                //Gizmos.DrawCube(balas.BalaBox.Center, balas.BalaBox.Extents * 2f, Color.White);
            });

            BalasEnemigas.ForEach(balas => {
                if (balas.BalaBox.Intersects(BoundingFrustum))
                    balas.DrawShadows(gameTime, TargetLightCamera.View, TargetLightCamera.Projection);
                //Gizmos.DrawCube(balas.BalaBox.Center, balas.BalaBox.Extents * 2f, Color.White);
            });
        }

        private void DrawScene(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(null);
            
            GraphicsDevice.SetRenderTarget(pixel);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

            var originalRasterizerState = GraphicsDevice.RasterizerState;
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            Graphics.GraphicsDevice.RasterizerState = rasterizerState;
            SkyBox.Draw(FollowCamera.View, FollowCamera.Projection, posicionUsada);
            GraphicsDevice.RasterizerState = originalRasterizerState;


            bool quedarseQuieto = isGod || EstadoActual == GameState.Pause;
            MainTanque.Draw(gameTime, vistaUsada, proyeccionUsada, posicionUsada, ShadowMapRenderTarget, 
                lightPosition, ShadowmapSize, TargetLightCamera, quedarseQuieto);

            var tankOBBToWorld = Matrix.CreateScale(MainTanque.TankBox.Extents * 2f) *
                 MainTanque.TankBox.Orientation *
                 Matrix.CreateTranslation(MainTanque.Position);
            //Gizmos.DrawCube(tankOBBToWorld, Color.YellowGreen);

            Tanques.ForEach(tanquesEnemigos => {
                if (tanquesEnemigos.TankBox.Intersects(BoundingFrustum))
                {
                    tanquesEnemigos.Draw(gameTime, vistaUsada, proyeccionUsada, posicionUsada, ShadowMapRenderTarget, 
                        lightPosition, ShadowmapSize, TargetLightCamera);
                }
                //Gizmos.DrawCube(tanquesEnemigos.TankBox.Center, tanquesEnemigos.TankBox.Extents * 2f, Color.Black);
            });


            Ambiente.ForEach(ambientes =>
            {
                if (ambientes.Box.Intersects(BoundingFrustum))
                    ambientes.Draw(gameTime, vistaUsada, proyeccionUsada, posicionUsada, ShadowMapRenderTarget, 
                        lightPosition, ShadowmapSize, TargetLightCamera);
                /*Gizmos.DrawCube(ambientes.Box.Center, ambientes.Box.Extents * 2f, ambientes.Colisiono ? Color.Blue : Color.Red);
                ambientes.Colisiono = false;*/

            });


            BalasMain.ForEach(balas => {
                if (balas.BalaBox.Intersects(BoundingFrustum))
                    balas.Draw(gameTime, vistaUsada, proyeccionUsada, posicionUsada, ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
                //Gizmos.DrawCube(balas.BalaBox.Center, balas.BalaBox.Extents * 2f, Color.White);
            });

            BalasEnemigas.ForEach(balas => {
                if(balas.BalaBox.Intersects(BoundingFrustum))
                    balas.Draw(gameTime, vistaUsada, proyeccionUsada, posicionUsada, ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            });


            if (GizmosActivado)
                Gizmos.Draw();


            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Quad.Draw(EffectLight, FloorWorld, vistaUsada, proyeccionUsada, posicionUsada, ShadowMapRenderTarget, 
                lightPosition, ShadowmapSize, TargetLightCamera);

            //dibujo los muros
            Muro1.Draw(EffectLight, Matrix.Identity, vistaUsada, proyeccionUsada, posicionUsada, 
                ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            Muro2.Draw(EffectLight, Matrix.Identity, vistaUsada, proyeccionUsada, posicionUsada,
                ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            Muro3.Draw(EffectLight, Matrix.Identity, vistaUsada, proyeccionUsada, posicionUsada,
                ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            Muro4.Draw(EffectLight, Matrix.Identity, vistaUsada, proyeccionUsada, posicionUsada,
                ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            Muro5.Draw(EffectLight, Matrix.Identity, vistaUsada, proyeccionUsada, posicionUsada,
                ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            Muro6.Draw(EffectLight, Matrix.Identity, vistaUsada, proyeccionUsada, posicionUsada,
                ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            Muro7.Draw(EffectLight, Matrix.Identity, vistaUsada, proyeccionUsada, posicionUsada,
                ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            Muro8.Draw(EffectLight, Matrix.Identity, vistaUsada, proyeccionUsada, posicionUsada,
                ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
                var box = Muro1.Colision;
                Gizmos.DrawCube((box.Max+box.Min)/2f,(box.Max-box.Min), Color.Red);
                box = Muro2.Colision;
                Gizmos.DrawCube((box.Max+box.Min)/2f,(box.Max-box.Min), Color.Yellow);
                box = Muro3.Colision;
                Gizmos.DrawCube((box.Max+box.Min)/2f,(box.Max-box.Min), Color.Black);
                box = Muro4.Colision;
                Gizmos.DrawCube((box.Max+box.Min)/2f,(box.Max-box.Min), Color.Green);
            LightBox.Draw(LightBoxWorld, FollowCamera.View, FollowCamera.Projection);
            base.Draw(gameTime);
            
            GraphicsDevice.SetRenderTarget(null);

            SpriteBatch.Begin(samplerState: SamplerState.PointWrap);
            SpriteBatch.Draw(pixel, new Rectangle(0,0, (int)PantallaResolucion.X, (int)PantallaResolucion.Y), new Color(1,1,1,.2f));
            SpriteBatch.End();
            switch (EstadoActual)
            {
                case GameState.InitialMenu:
                    MenuInicio.Draw(SpriteBatch);
                break;
                case GameState.Pause:
                    MenuPausa.Draw(SpriteBatch);
                break;
                case GameState.Begin:
                    Hud.Draw(SpriteBatch, MainTanque.Vida, tiempoRestante);
                break;
                case GameState.Finished:
                    // wip
                    PantallaFinal.Draw(SpriteBatch, puntos);
                break;
                case GameState.Lost:
                    // wip
                    PantallaFinal.DrawLost(SpriteBatch);
                break;
                default:
                break;
            }



            GraphicsDevice.BlendState = BlendState.Opaque;
        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
            ShadowMapRenderTarget.Dispose();
        }

    }
}