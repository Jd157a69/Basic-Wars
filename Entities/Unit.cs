using Basic_Wars.Graphics;
using Basic_Wars_V2.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.Entities
{
    public class Unit : IGameEntity, ICollideable
    {
        public const int UNIT_WIDTH = 56;
        public const int UNIT_HEIGHT = 56;
        public const int X_SPRITE_SHEET_START_POS = 0;
        public const int Y_SPRITE_SHEET_START_POS = 0;
        public const int SPRITE_SHEET_TEAM_SHIFT = 168;
        public const int SPRITE_SHEET_UNIT_SHIFT = 56;

        public Sprite unitSprite;

        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public UnitState State { get; set; }
      
        public int Team { get; set; }
        public int UnitType { get; set; }


        public int Health { get; set; }
        public int RemainingMoves { get; set; }
        public int MovementRange { get; set; }

        
        public int ID { get; set; }
        public int DrawOrder { get; set; }
        public Color UnitUsed = Color.White * 0.5f;

        public Unit(Texture2D texture, Vector2 position, int unitType, int team)
        {
            this.Texture = texture;
            this.Position = position;
            this.Team = team;
            this.UnitType = unitType;

            State = UnitState.Idle;
            CreateUnitSprite();

            //Temporary
            Health = 10;
        }

        public void CreateUnitSprite()
        {
            int teamShift = (Team - 1) * SPRITE_SHEET_TEAM_SHIFT;
            int unitShift = (UnitType - 1) * SPRITE_SHEET_UNIT_SHIFT;
            unitSprite = new Sprite(Texture, X_SPRITE_SHEET_START_POS + teamShift, Y_SPRITE_SHEET_START_POS + unitShift, UNIT_WIDTH, UNIT_HEIGHT);
        }

        public Rectangle Collider
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, UNIT_WIDTH, UNIT_HEIGHT);
            }
        }

        public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
        {
            unitSprite.Draw(_spriteBatch, Position);
        }

        public void CheckHealth()
        {
            if (Health <= 0)
            {
                State = UnitState.Dead;
            }
        }

        public void CheckState()
        {

            //Use of Unit.State should be elsewhere instead of inside the unit class

            switch (State)
            {
                case UnitState.Idle:

                    CheckHealth();

                    break;

                case UnitState.Selected:

                    //Display options to user of what unit can do               

                    break;

                case UnitState.Moving:

                    //State = UnitState.Idle;

                    //Display tiles unit can move to
                    //In InputController find the position of the mouse and relate it to the tile grid -> (MousePos - Offset of map position) % 56 
                    //Change Position of selected unit to match Position of selected tile
                    //unitSprite.TintColour = UnitUsed;

                    break;

                case UnitState.Attacking:

                    //Show potential targets that the unit can attack

                    unitSprite.TintColour = UnitUsed;

                    break;

                default:
                    break;
            }
        }


        public void Update(GameTime gameTime)
        {
            CheckState();
        }

    }
}
