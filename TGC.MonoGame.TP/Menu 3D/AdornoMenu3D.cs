using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Collisions;


namespace TGC.MonoGame.TP
{    
    public class AdornoMenu3D
    {

        // 3D
        public Model modelo;
        private Matrix mundo, vista, proyeccion;
        private Effect efectoBasico;

        public float Scale = 0.3f;
        private Texture2D Texture;

        
        public SoundEffect ClickSound { get; set; }
        public Action<TGCGame> Click;

        public bool Clicked { get; protected set; }

        public bool IsSelected;

        public Vector2 Position { get; set; }


        public AdornoMenu3D(Texture2D texture, Model modelo, Effect efectoBasico)
        {
            Texture = texture;
            this.modelo = modelo;
            this.efectoBasico = efectoBasico;
            mundo = Matrix.Identity;
            IsSelected = false;
        }

        float giro = 0;
        public void Draw(Vector3 positionCamera, Vector3 direction, Vector3 up, Vector3 right, Matrix View, Matrix Projection, float EjeX = 0, float EjeY = 0, float Distance = 2000)
        {
            if(!IsSelected)
                return; 
            giro += .1f;
            
            // Tanto la vista como la proyección vienen de la cámara por parámetro
            foreach (var mesh in modelo.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = efectoBasico;
                }
            }

            efectoBasico.Parameters["View"].SetValue(View);
            efectoBasico.Parameters["Projection"].SetValue(Projection);
            efectoBasico.Parameters["ModelTexture"]?.SetValue(Texture);


            var modelMeshesBaseTransforms = new Matrix[modelo.Bones.Count];
            modelo.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            //var Position = Vector3.Up * 300;
            var Position = positionCamera + direction * Distance - right * EjeX - up * EjeY;

            mundo = Matrix.CreateRotationY(giro) * Matrix.CreateScale(.5f) * Matrix.CreateWorld(Position, -direction, Vector3.Up);
            foreach (var mesh in modelo.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                efectoBasico.Parameters["World"].SetValue(meshWorld * mundo);
                mesh.Draw();
            }
    

        }

    public Vector2 Origin { get; set; }

    public string Text { get; set; }

    public Color PenColour { get; set; } = Color.Red;


    }
}