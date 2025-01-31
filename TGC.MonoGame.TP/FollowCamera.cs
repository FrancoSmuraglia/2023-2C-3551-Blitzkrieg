﻿﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP
{
    /// <summary>
    /// Una camara que sigue objetos
    /// </summary>
    class FollowCamera
    {
        private const float AxisDistanceToTarget = /*1000f;*/3000f;

        private const float AngleFollowSpeed = 0.015f;

        private const float AngleThreshold = 0.85f;

        public Matrix Projection { get; private set; }

        public Matrix View { get; private set; }

        private Vector3 CurrentRightVector { get; set; } = Vector3.Backward;

        private float RightVectorInterpolator { get; set; } = 0f;

        private Vector3 PastRightVector { get; set; } = Vector3.Backward;

        private float yaw = 0f;
        private float pitch = -MathHelper.PiOver4;

        private MouseState estadoAnteriorMouse;

        public Vector3 CamaraPosition { get; set; }

        /// <summary>
        /// Crea una FollowCamera que sigue a una matriz de mundo
        /// </summary>
        /// <param name="aspectRatio"></param>
        public FollowCamera(float aspectRatio)
        {
            // Orthographic camera
            // Projection = Matrix.CreateOrthographic(screenWidth, screenHeight, 0.01f, 10000f);

            // Perspective camera
            // Uso 60° como FOV, aspect ratio, pongo las distancias a near plane y far plane en 0.1 y 100000 (mucho) respectivamente
            Projection = Matrix.CreatePerspectiveFieldOfView(MathF.PI / 3f, aspectRatio, 0.1f, 100000f);
        }

        /// <summary>
        /// Actualiza la Camara usando una matriz de mundo actualizada para seguirla
        /// </summary>
        /// <param name="gameTime">The Game Time to calculate framerate-independent movement</param>
        /// <param name="followedWorld">The World matrix to follow</param>
        public bool Frenado = false;
        public Vector3 posicionCamara;
        public Vector3 direccionCamara, upDirection, rightDirection;

        public void GiroDeCamara(GameTime gameTime){
            yaw += 0.5f * Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
        }
        public void Update(GameTime gameTime, Matrix followedWorld)
        {
            
            // Obtengo la posicion de la matriz de mundo que estoy siguiendo
            var followedPosition = followedWorld.Translation;   
            
            if(!Frenado){
                MouseState estadoActualMouse = Mouse.GetState();
                Vector2 mouseDelta = new Vector2(estadoActualMouse.X - estadoAnteriorMouse.X, estadoActualMouse.Y - estadoAnteriorMouse.Y);
                //guardo la posicion actual del mouse para el siguiente
                estadoAnteriorMouse = estadoActualMouse;

                float velocidadDeRotacion = 0.01f;
                yaw += mouseDelta.X * velocidadDeRotacion;
                pitch += mouseDelta.Y * velocidadDeRotacion;

                //limitacion de la camara
                pitch = MathHelper.Clamp(pitch, -MathHelper.PiOver2 + 0.01f, 0.05f);
            }


            //vector direccion de la camara
            direccionCamara = new Vector3(MathF.Cos(yaw) * MathF.Cos(pitch), MathF.Sin(pitch),MathF.Sin(yaw) * MathF.Cos(pitch));
            rightDirection = Vector3.Normalize(Vector3.Cross(direccionCamara, Vector3.Up));
            upDirection = Vector3.Normalize(Vector3.Cross(rightDirection, direccionCamara));
            //calculo la posicion con la direccion de la camara
            CamaraPosition = followedPosition - direccionCamara * AxisDistanceToTarget;

            View = Matrix.CreateLookAt(CamaraPosition, followedPosition, Vector3.Up);
            //Console.WriteLine("pitch: " + pitch);
        }

        public void FrenarCamara(){
            Frenado = !Frenado;
            estadoAnteriorMouse = Mouse.GetState();
        }
    }
}
