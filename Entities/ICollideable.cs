using Microsoft.Xna.Framework;

namespace Basic_Wars_V2.Entities
{
    public interface ICollideable
    {
        Rectangle Collider { get; }
    }
}
