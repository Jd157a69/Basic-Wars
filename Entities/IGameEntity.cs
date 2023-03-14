using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Basic_Wars_V2.Entities
{
    public interface IGameEntity
    {
        int DrawOrder { get; }

        void Update(GameTime gameTime);

        void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    }
}
