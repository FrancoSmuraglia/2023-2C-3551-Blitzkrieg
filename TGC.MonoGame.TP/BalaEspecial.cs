using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Collisions;

namespace TGC.MonoGame.TP
{
    public class BalaEspecial : Bala
    {
        public BalaEspecial(Vector3 Position, Vector3 velocidad, Model modelo, Effect efecto, Texture2D textura, Tanque Main) : 
            base(Position, velocidad, modelo, efecto, textura, Main)
        {
            Daño = 2;
        }
    }
}
