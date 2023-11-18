using BepuPhysics.Constraints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    public class Muro
    {
        public Texture2D Texture;
        public Texture2D Normal;
        private VertexBuffer Vertices { get; set; }
        private IndexBuffer Indices { get; set; }

        ////muro 2
        //private VertexBuffer Vertices2 { get; set; }
        //private IndexBuffer Indices2 { get; set; }
        //
        ////muro 3
        //private VertexBuffer Vertices3 { get; set; }
        //private IndexBuffer Indices3 { get; set; }
        //
        ////muro 4
        //private VertexBuffer Vertices4 { get; set; }
        //private IndexBuffer Indices4 { get; set; }

        private int NroMuro {  get; set; }

        public Muro(GraphicsDevice graphicsDevice, Texture2D textura, Texture2D normal, int nroMuro)
        {
            Texture = textura;
            Normal = normal;

            NroMuro = nroMuro;

            CreateVertexBuffer(graphicsDevice);
            CreateIndexBuffer(graphicsDevice);
        }

        private void CreateVertexBuffer(GraphicsDevice graphicsDevice)
        {
            var textureCoordinateLowerLeft = Vector2.Zero;
            var textureCoordinateLowerRight = Vector2.UnitX;
            var textureCoordinateUpperLeft = Vector2.UnitY;
            var textureCoordinateUpperRight = Vector2.One;

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[0]; ; 
            switch(NroMuro)
            {
                case 0:
                    vertices = new[]
                    {
                        //adelante, abajo, derecha 0 (1,1,0)
                        new VertexPositionNormalTexture((Vector3.UnitX + Vector3.UnitZ)*10000f,Vector3.Up, textureCoordinateUpperRight*3f),
                        //1
                        new VertexPositionNormalTexture((-Vector3.UnitX + Vector3.UnitZ) * 10000f, Vector3.Up, textureCoordinateUpperLeft*3f),
                        //2
                        new VertexPositionNormalTexture((Vector3.UnitX + Vector3.UnitY/3 + Vector3.UnitZ)* 10000f, Vector3.Up, textureCoordinateLowerRight * 3f),
                        //3
                        new VertexPositionNormalTexture((-Vector3.UnitX + Vector3.UnitY/3 + Vector3.UnitZ)*10000f, Vector3.Up, textureCoordinateLowerLeft*3f)
                    };
                    break;
                case 1:
                    vertices = new[]
                    {
                        //0
                        new VertexPositionNormalTexture((-Vector3.UnitX - Vector3.UnitZ) * 10000f, Vector3.Up, textureCoordinateUpperRight * 3f),
                        //1
                        new VertexPositionNormalTexture((-Vector3.UnitX + Vector3.UnitY/3 - Vector3.UnitZ) * 10000f, Vector3.Up, textureCoordinateLowerRight * 3f),
                        //2
                        new VertexPositionNormalTexture((-Vector3.UnitX + Vector3.UnitZ)* 10000f, Vector3.Up, textureCoordinateUpperLeft * 3),
                        //3
                        new VertexPositionNormalTexture((-Vector3.UnitX + Vector3.UnitY/3 + Vector3.UnitZ) * 10000f, Vector3.Up, textureCoordinateLowerLeft * 3f)
                    };
                    break;
                case 2:
                    vertices = new[]
                    {
                        //0
                        new VertexPositionNormalTexture((-Vector3.UnitX - Vector3.UnitZ)* 10000f, Vector3.Up, textureCoordinateUpperRight * 3f),
                        //1
                        new VertexPositionNormalTexture((-Vector3.UnitX + Vector3.UnitY/3 - Vector3.UnitZ) * 10000f, Vector3.Up, textureCoordinateLowerRight * 3f),
                        //2
                        new VertexPositionNormalTexture((Vector3.UnitX - Vector3.UnitZ)* 10000f, Vector3.Up, textureCoordinateUpperLeft *3f),
                        //3
                        new VertexPositionNormalTexture((Vector3.UnitX + Vector3.UnitY/3 - Vector3.UnitZ)* 10000f, Vector3.Up, textureCoordinateLowerLeft * 3f)
                    };
                    break;
                case 3:
                    vertices = new[]
                    {
                        //0
                        new VertexPositionNormalTexture((Vector3.UnitX + Vector3.UnitZ)* 10000f, Vector3.Up, textureCoordinateUpperRight * 3f),
                        //1
                        new VertexPositionNormalTexture((Vector3.UnitX + Vector3.UnitY/3 + Vector3.UnitZ) * 10000f, Vector3.Up, textureCoordinateLowerRight * 3f),
                        //2
                        new VertexPositionNormalTexture((Vector3.UnitX - Vector3.UnitZ)* 10000f, Vector3.Up, textureCoordinateUpperLeft *3f),
                        //3
                        new VertexPositionNormalTexture((Vector3.UnitX + Vector3.UnitY/3 - Vector3.UnitZ)* 10000f, Vector3.Up, textureCoordinateLowerLeft * 3f)
                    };
                    break;
            }
            Vertices = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length,
                BufferUsage.WriteOnly);
            Vertices.SetData(vertices);
        }

        private void CreateIndexBuffer(GraphicsDevice graphicsDevice)
        {
            // Set the index buffer for each vertex, using clockwise winding
            var indices = new ushort[0];

            if(NroMuro == 2)
            {
                indices = new ushort[]
                {
                    0, 1, 2,
                    2, 1, 3,
                };
            }
            else
            {
                indices = new ushort[]
                {
                    1, 0, 2,
                    1, 2, 3,
                };
            }

            Indices = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Length,
                BufferUsage.WriteOnly);
            Indices.SetData(indices);
        }

        public void Draw(Effect effect)
        {
            var graphicsDevice = effect.GraphicsDevice;

            graphicsDevice.SetVertexBuffer(Vertices);
            graphicsDevice.Indices = Indices;

            foreach (var effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indices.IndexCount / 3);
            }
        }

        public void Draw(Effect effect, Matrix world, Matrix view, Matrix projection, Vector3 camaraPosition, 
            RenderTarget2D ShadowMapRenderTarget, Vector3 lightPosition, int ShadowmapSize, TargetCamera TargetLightCamera)
        {
            actualizarLuz(camaraPosition, effect, ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            var graphicsDevice = effect.GraphicsDevice;
            graphicsDevice.SetVertexBuffer(Vertices);
            graphicsDevice.Indices = Indices;

            foreach (var effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indices.IndexCount / 3);
            }
        }
        public void actualizarLuz(Vector3 camaraPosition, Effect effect, RenderTarget2D ShadowMapRenderTarget, Vector3 lightPosition,
            int ShadowmapSize, TargetCamera TargetLightCamera)
        {
            effect.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f, 1f));
            effect.Parameters["diffuseColor"].SetValue(new Vector3(0.1f, 0.1f, 0.6f));
            effect.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));

            effect.Parameters["KAmbient"].SetValue(1.0f);
            effect.Parameters["KDiffuse"].SetValue(1.0f);
            effect.Parameters["KSpecular"].SetValue(0.0f);
            effect.Parameters["shininess"].SetValue(32.0f);
            effect.Parameters["eyePosition"].SetValue(camaraPosition);

            effect.Parameters["ModelTexture"].SetValue(Texture);
            effect.Parameters["NormalTexture"].SetValue(Normal);
            effect.Parameters["Tiling"].SetValue(Vector2.One);

            effect.CurrentTechnique = effect.Techniques["NormalMapping"];
            effect.Parameters["shadowMap"].SetValue(ShadowMapRenderTarget);
            effect.Parameters["lightPosition"].SetValue(lightPosition);
            effect.Parameters["shadowMapSize"].SetValue(Vector2.One * ShadowmapSize);
            effect.Parameters["LightViewProjection"].SetValue(TargetLightCamera.View * TargetLightCamera.Projection);
        }

        public void DrawShadows(Effect effect, Matrix world, Matrix view, Matrix projection)
        {
            var graphicsDevice = effect.GraphicsDevice;
            graphicsDevice.SetVertexBuffer(Vertices);
            graphicsDevice.Indices = Indices;

            effect.CurrentTechnique = effect.Techniques["DepthPass"];

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
            effect.Parameters["WorldViewProjection"].SetValue(world * view * projection);

            foreach (var effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indices.IndexCount / 3);
            }
        }
    }
}