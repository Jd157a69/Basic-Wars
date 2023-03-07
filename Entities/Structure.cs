using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.Entities
{
    public class Structure : Tile
    {
        public int Team { get; set; }

        public Structure(Vector2 position, Texture2D texture, int team) :base(position, texture)
        {
            Team = team;
        }
    }
}
