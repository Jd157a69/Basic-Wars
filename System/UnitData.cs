using Basic_Wars_V2.Entities;
using Basic_Wars_V2.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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
            Unit unit = new(Texture, Position, UnitTypeInt, Team)
            {
                Health = Health,
                Ammo = Ammo,
                Fuel = Fuel,
                Defence = Defence,
                MovementPoints = MovementPoints,
                CostToProduce = CostToProduce
            };
            unit.CreateUnitSprite(UnitTypeInt);

            return unit;
        }
    }
}
