using Basic_Wars_V2.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Basic_Wars_V2.Entities
{
    public class UnitManager : IGameEntity
    {
        public List<Unit> units = new List<Unit>();
        public List<Unit> unitsToRemove = new List<Unit>();

        public bool DrawUnits { get; set; }

        public int DrawOrder => 1;

        public UnitManager()
        {
            DrawUnits = false;
        }

        public void AddUnit(Unit unit)
        {
            units.Add(unit);
        }

        public void RemoveUnit(Unit unit)
        {
            unitsToRemove.Add(unit);
        }

        public List<Vector2> GetUnitPositions()
        {
            List<Vector2> positions = new List<Vector2>();

            foreach (Unit unit in units)
            {
                positions.Add(unit.Position);
            }

            return positions;
        }

        public void ResetUnitStates(Player currentPlayer)
        {
            foreach (Unit unit in units)
            {
                if (unit.Team == currentPlayer.Team)
                {
                    unit.State = UnitState.None;
                }
            }
        }

        public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
        {
            if (DrawUnits)
            {
                foreach (Unit unit in units)
                {
                    unit.Draw(_spriteBatch, gameTime);
                }
            }
        }

        public void ClearUnits()
        {
            units.Clear();
        }

        public void Update(GameTime gameTime)
        {
            foreach (Unit unit in units)
            {
                unit.Update(gameTime);

                if (unit.State == UnitState.Dead)
                {
                    unitsToRemove.Add(unit);
                }
            }

            foreach (Unit unit in unitsToRemove)
            {
                units.Remove(unit);
            }
        }
    }
}
