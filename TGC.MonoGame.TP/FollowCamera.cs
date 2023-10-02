﻿using Microsoft.Xna.Framework;
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
        private const float AxisDistanceToTarget = 3000f;

        private const float AngleFollowSpeed = 0.015f;

        private const float AngleThreshold = 0.85f;

        public Matrix Projection { get; private set; }

        public Matrix View { get; private set; }

        private Vector3 CurrentRightVector { get; set; } = Vector3.Backward;

        private float RightVectorInterpolator { get; set; } = 0f;

        private Vector3 PastRightVector { get; set; } = Vector3.Backward;

        private float yaw = 0f;
        private float pitch = 0f;

        private MouseState estadoAnteriorMouse;

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
        public void Update(GameTime gameTime, Matrix followedWorld)
        {
            
            // Obtengo la posicion de la matriz de mundo que estoy siguiendo
            var followedPosition = followedWorld.Translation;


            


            MouseState estadoActualMouse = Mouse.GetState();
            Vector2 mouseDelta = new Vector2(estadoActualMouse.X - estadoAnteriorMouse.X, estadoActualMouse.Y - estadoAnteriorMouse.Y);

            //guardo la posicion actual del mouse para el siguiente
            estadoAnteriorMouse = estadoActualMouse;

            float velocidadDeRotacion = 0.01f;
            yaw += mouseDelta.X * velocidadDeRotacion;
            pitch += mouseDelta.Y * velocidadDeRotacion;

            //limitacion de la camara
            pitch = MathHelper.Clamp(pitch, -MathHelper.PiOver2 + 0.01f, 0.05f);

            //vector direccion de la camara
            Vector3 direccionCamara = new Vector3(MathF.Cos(yaw) * MathF.Cos(pitch), MathF.Sin(pitch),MathF.Sin(yaw) * MathF.Cos(pitch));

            //calculo la posicion con la direccion de la camara
            Vector3 posicionCamara = followedPosition - direccionCamara * AxisDistanceToTarget;

            //



            // Obtengo el vector Derecha asumiendo que la camara tiene el vector Arriba apuntando hacia arriba
            // y no esta rotada en el eje X (Roll)

            View = Matrix.CreateLookAt(posicionCamara, followedPosition, Vector3.Up);

            //Console.WriteLine("pitch: " + pitch);


        }
    }
}
