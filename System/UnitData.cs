using Basic_Wars_V2.Entities;
using Basic_Wars_V2.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.System
{
    [Serializable]
    public class UnitData
    {
        public Vector2 Position { get; set; }
        public UnitState State { get; set; }

        public int Team { get; set; }
        public int UnitTypeInt { get; set; }

        public int Health { get; set; }
        public int Ammo { get; set; }
        public int Fuel { get; set; }
        public float Defence { get; set; }
        public int MovementPoints { get; set; }
        public int CostToProduce { get; set; }

        public UnitData() { }

        public UnitData(Unit unit)
        {
            Position = unit.Position;
            State = unit.State;
            Team = unit.Team;
            UnitTypeInt = unit.UnitTypeInt;
            Health = unit.Health;
            Ammo = unit.Ammo;
            Fuel = unit.Fuel;
            MovementPoints = unit.MovementPoints;
            CostToProduce = unit.CostToProduce;
        }

        public Unit FromUnitData(Texture2D Texture)
        {
            Unit unit = new Unit(Texture, Position, UnitTypeInt, Team);
            unit.Health = Health;
            unit.Ammo = Ammo;
            unit.Fuel = Fuel;
            unit.Defence = Defence;
            unit.MovementPoints = MovementPoints;
            unit.CostToProduce = CostToProduce;
            unit.CreateUnitSprite(UnitTypeInt);

            return unit;
        }
    }
}
