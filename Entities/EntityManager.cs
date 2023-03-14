using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Basic_Wars_V2.Entities
{
    internal class EntityManager
    {
        private List<IGameEntity> entities = new();
        private readonly List<IGameEntity> entitiesToRemove = new();

        public void Update(GameTime gameTime)
        {
            foreach (IGameEntity entity in entities)
            {
                entity.Update(gameTime);
            }

            foreach (IGameEntity entity in entitiesToRemove)
            {
                entities.Remove(entity);
            }

            entitiesToRemove.Clear();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach (IGameEntity entity in entities.OrderBy(e => e.DrawOrder))
            {

                entity.Draw(spriteBatch, gameTime);

            }
        }

        public void AddEntity(IGameEntity entity)
        {
            entities.Add(entity);
        }

        public void RemoveEntity(IGameEntity entity)
        {
            entitiesToRemove.Add(entity);
        }

        public void Refresh()
        {
            List<IGameEntity> oldEntities = entities;
            ClearEntities();
            entities = oldEntities;
        }

        public void ClearEntities()
        {
            entities.Clear();
        }
    }
}
