using BepuPhysics.Constraints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     The quad is like a plane but its made by two triangle and the surface is oriented in the XY plane of the local
    ///     coordinate space.
    /// </summary>
    public class QuadPrimitive
    {
        public Texture2D Textura;
        public Texture2D Normal;
        /// <summary>
        ///     Create a textured quad.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        public QuadPrimitive(GraphicsDevice graphicsDevice, Texture2D textura, Texture2D normal)
        {
            CreateVertexBuffer(graphicsDevice);
            CreateIndexBuffer(graphicsDevice);

            Effect = new BasicEffect(graphicsDevice);
            Effect.TextureEnabled = true;
            Effect.EnableDefaultLighting();
            Effect.Texture = textura;
            Textura = textura;
            Normal = normal;
        }

        /// <summary>
        ///     Represents a list of 3D vertices to be streamed to the graphics device.
        /// </summary>
        private VertexBuffer Vertices { get; set; }

        /// <summary>
        ///     Describes the rendering order of the vertices in a vertex buffer, using counter-clockwise winding.
        /// </summary>
        private IndexBuffer Indices { get; set; }


        /// <summary>
        ///     Built-in effect that supports optional texturing, vertex coloring, fog, and lighting.
        /// </summary>
        public BasicEffect Effect { get; private set; }

        /// <summary>
        ///     Create a vertex buffer for the figure with the given information.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        private void CreateVertexBuffer(GraphicsDevice graphicsDevice)
        {
            // Set the position and texture coordinate for each vertex
            // Normals point Up as the Quad is originally XZ aligned

            var textureCoordinateLowerLeft = Vector2.Zero;
            var textureCoordinateLowerRight = Vector2.UnitX;
            var textureCoordinateUpperLeft = Vector2.UnitY;
            var textureCoordinateUpperRight = Vector2.One;

            var vertices = new[]
            {
                // Possitive X, Possitive Z (1,1) 0
                new VertexPositionNormalTexture((Vector3.UnitX + Vector3.UnitZ)*10000f, Vector3.Up, textureCoordinateUpperRight*20f),
                // Possitive X, Negative Z (1,-1) 1
                new VertexPositionNormalTexture((Vector3.UnitX - Vector3.UnitZ)*10000f, Vector3.Up, textureCoordinateLowerRight*20f),
                // Negative X, Possitive Z (-1,1) 2
                new VertexPositionNormalTexture((Vector3.UnitZ - Vector3.UnitX)*10000f, Vector3.Up, textureCoordinateUpperLeft*20f),
                // Negative X, Negative Z (-1,-1) 3
                new VertexPositionNormalTexture((-Vector3.UnitX - Vector3.UnitZ)*10000f, Vector3.Up, textureCoordinateLowerLeft*20f)
            };

            Vertices = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length,
                BufferUsage.WriteOnly);
            Vertices.SetData(vertices);
        }

        private void CreateIndexBuffer(GraphicsDevice graphicsDevice)
        {
            // Set the index buffer for each vertex, using clockwise winding
            var indices = new ushort[]
            {
                3, 1, 0, 
                3, 0, 2,
            };

            Indices = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Length,
                BufferUsage.WriteOnly);
            Indices.SetData(indices);
        }

        /// <summary>
        ///     Draw the Quad.
        /// </summary>
        /// <param name="world">The world matrix for this box.</param>
        /// <param name="view">The view matrix, normally from the camera.</param>
        /// <param name="projection">The projection matrix, normally from the application.</param>
        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            // Set BasicEffect parameters.
            Effect.World = world;
            Effect.View = view;
            Effect.Projection = projection;

            // Draw the model, using BasicEffect.
            Draw(Effect);
        }

        /// <summary>
        ///     Draws the primitive model, using the specified effect. Unlike the other Draw overload where you just specify the
        ///     world/view/projection matrices and color, this method does not set any render states, so you must make sure all
        ///     states are set to sensible values before you call it.
        /// </summary>
        /// <param name="effect">Used to set and query effects, and to choose techniques.</param>
        public void Draw(Effect effect)
        {
            var graphicsDevice = effect.GraphicsDevice;

            // Set our vertex declaration, vertex buffer, and index buffer.
            
            graphicsDevice.SetVertexBuffer(Vertices);
            graphicsDevice.Indices = Indices;

            foreach (var effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indices.IndexCount / 3);
            }
        }

        public void Draw(Effect effect, Matrix world, Matrix view, Matrix projection, Vector3 camaraPosition, RenderTarget2D ShadowMapRenderTarget, Vector3 lightPosition, int ShadowmapSize, TargetCamera TargetLightCamera)
        {
            //effect.Parameters["World"].SetValue(world);
            //effect.Parameters["View"].SetValue(view);
            //effect.Parameters["Projection"].SetValue(projection);
            //effect.Parameters["ModelTexture"]?.SetValue(Textura);
            //
            //var graphicsDevice = effect.GraphicsDevice;
            //
            //
            //// Set our vertex declaration, vertex buffer, and index buffer.
            //
            //graphicsDevice.SetVertexBuffer(Vertices);
            //graphicsDevice.Indices = Indices;
            //
            //foreach (var effectPass in effect.CurrentTechnique.Passes)
            //{
            //    effectPass.Apply();
            //    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indices.IndexCount / 3);
            //}

            actualizarLuz(camaraPosition, effect, ShadowMapRenderTarget, lightPosition, ShadowmapSize, TargetLightCamera);
            var graphicsDevice = effect.GraphicsDevice;
            graphicsDevice.SetVertexBuffer(Vertices);
            graphicsDevice.Indices = Indices;

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
            effect.Parameters["WorldViewProjection"].SetValue(world * view * projection);

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

            effect.Parameters["ModelTexture"].SetValue(Textura);
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