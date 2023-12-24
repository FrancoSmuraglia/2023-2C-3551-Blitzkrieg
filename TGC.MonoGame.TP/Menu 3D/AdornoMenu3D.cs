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

        // Te juro que me siento como el jugador de béisbol que el Sr. Burns le dice "CÓRTESE LAS PATILLAS" con esto del menú 3D

        public Model modelo;
        private Matrix mundo;
        private Effect efecto;

        public float Scale = 0.3f;
        private Texture2D Texture;

        public Vector2 Position { get; set; }


        public AdornoMenu3D(Texture2D texture, Model modelo, Effect efectoBasico)
        {
            Texture = texture;
            this.modelo = modelo;
            this.efecto = efectoBasico;
            mundo = Matrix.Identity;
        }

        float giro = 0;
        public void Draw(Vector3 positionCamera, Vector3 direction, Vector3 up, Vector3 right, Matrix View, Matrix Projection, float EjeX = 0, float EjeY = 0, float Distance = 5000)
        {
            giro += .1f;
            
            // Tanto la vista como la proyección vienen de la cámara por parámetro
            foreach (var mesh in modelo.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = efecto;
                }
            }

            efecto.Parameters["View"].SetValue(View);
            efecto.Parameters["Projection"].SetValue(Projection);
            efecto.Parameters["ModelTexture"]?.SetValue(Texture);


            var modelMeshesBaseTransforms = new Matrix[modelo.Bones.Count];
            modelo.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);

            var Position = positionCamera + direction * Distance - right * EjeX - up * EjeY;
            var Reposicionamiento = Matrix.CreateWorld(Position, up, Vector3.Up);
            var giroMatriz = Matrix.CreateRotationZ(this.giro);

            mundo = giroMatriz * Reposicionamiento;
            foreach (var mesh in modelo.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                efecto.Parameters["World"].SetValue(meshWorld * mundo);
                mesh.Draw();
            }
        }

    }
}