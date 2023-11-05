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
        /// <summary>
        ///     Create a textured quad.
        /// </summary>
        /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
        public QuadPrimitive(GraphicsDevice graphicsDevice, Texture2D textura)
        {
            CreateVertexBuffer(graphicsDevice);
            CreateIndexBuffer(graphicsDevice);

            Effect = new BasicEffect(graphicsDevice);
            Effect.TextureEnabled = true;
            Effect.EnableDefaultLighting();
            Effect.Texture = textura;
            Textura = textura;
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
                new VertexPositionNormalTexture(Vector3.UnitX + Vector3.UnitZ, Vector3.Up, textureCoordinateUpperRight),
                // Possitive X, Negative Z (1,-1) 1
                new VertexPositionNormalTexture(Vector3.UnitX - Vector3.UnitZ, Vector3.Up, textureCoordinateLowerRight),
                // Negative X, Possitive Z (-1,1) 2
                new VertexPositionNormalTexture(Vector3.UnitZ - Vector3.UnitX, Vector3.Up, textureCoordinateUpperLeft),
                // Negative X, Negative Z (-1,-1) 3
                new VertexPositionNormalTexture(-Vector3.UnitX - Vector3.UnitZ, Vector3.Up, textureCoordinateLowerLeft)
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

        public void Draw(Effect effect, Matrix world, Matrix view, Matrix projection, Vector3 camaraPosition)
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

            actualizarLuz(camaraPosition, effect);
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

        public void actualizarLuz(Vector3 camaraPosition, Effect effect)
        {
            effect.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f, 1f));
            effect.Parameters["diffuseColor"].SetValue(new Vector3(0.1f, 0.1f, 0.6f));
            effect.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));

            effect.Parameters["KAmbient"].SetValue(1.0f);
            effect.Parameters["KDiffuse"].SetValue(1.0f);
            effect.Parameters["KSpecular"].SetValue(1.0f);
            effect.Parameters["shininess"].SetValue(16.0f);
            effect.Parameters["lightPosition"].SetValue(new Vector3(500, 500, 500));
            effect.Parameters["eyePosition"].SetValue(camaraPosition);

            effect.Parameters["ModelTexture"].SetValue(Textura);
            //Effect.Parameters["NormalTexture"].SetValue(NormalTexture);
            effect.Parameters["Tiling"].SetValue(Vector2.One);
        }
    }
}