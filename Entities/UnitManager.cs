using Basic_Wars_V2.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.Entities
{
    public class UnitManager : IGameEntity
    {
        public List<Unit> units = new List<Unit>();
        public List<Unit> unitsToRemove = new List<Unit>();

        private int ID = 0;
        private int TotalUnitsCreated = 0;

        public bool DrawUnits { get; set; }

        public int DrawOrder => 1;

        public UnitManager()
        {
            DrawUnits = false;
        }

        public void AddUnit(Unit unit)
        {
            units.Add(unit);
            ID++;
            TotalUnitsCreated++;
            units[TotalUnitsCreated - 1].ID = ID;
        }

        public void RemoveUnit(Unit unit)
        {
            unitsToRemove.Add(unit);
        }

        public void Draw(SpriteBatch _spriteBatch, GameTime gameTime, float Scale)
        {
            if (DrawUnits)
            {
                foreach (Unit unit in units)
                {
                    unit.Draw(_spriteBatch, gameTime, Scale);
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (Unit unit in units)
            {
                unit.Update(gameTime);
            }

            foreach (Unit unit in unitsToRemove)
            {
                units.Remove(unit);
            }
        }
    }
}
