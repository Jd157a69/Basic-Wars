using Basic_Wars_V2.Enums;
using Basic_Wars_V2.Graphics;
using Basic_Wars_V2.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.Entities
{
    public class GameUI : IGameEntity
    {
        Tile Selected;

        Unit SelectedUnit;
        Tile SelectedTile;

        public Texture2D texture;
        public InputController _inputController;
        public ButtonManager _buttonManager;
        public MapManager _gameMap;
        public UnitManager _unitManager;

        public int DrawOrder { get; set; }

        public GameUI(Texture2D SpriteSheet, SpriteFont font, MapManager map, UnitManager unitManager)
        {
            texture = SpriteSheet;
            _gameMap = map;
            _unitManager = unitManager;

            _buttonManager = new ButtonManager();
            _inputController = new InputController(_unitManager, _buttonManager, _gameMap);

            /*Button TitleButton = new Button(texture, font, new Vector2(1080/2, 50), "Basic Wars", "Menu");
            Button NewGame = new Button(texture, font, new Vector2(1080/2, 270), "New Game", "Menu");
            Button LoadGame = new Button(texture, font, new Vector2(1080/2, 420), "Load Game", "Menu");
            Button Quit = new Button(texture, font, new Vector2(1080 / 2, 570), "Quit", "AltMenu");


            _buttonManager.AddButton(TitleButton);
            _buttonManager.AddButton(NewGame);
            _buttonManager.AddButton(LoadGame);
            _buttonManager.AddButton(Quit);
            */
            
            Selected = new Tile(new Vector2(0, 0), texture);
            Selected.CreateTile(0, 0, 1);

        }

        public void UpdateUI()
        {

            if (!((SelectedUnit = _unitManager.GetSelectedUnit()) == null))
            {
                Selected.Position = SelectedUnit.Position;
            }

            if (!((SelectedTile = _gameMap.GetSelectedTile()) == null))
            {
                Selected.Position = SelectedTile.Position;
            }
        }


        public void Update(GameTime gameTime)
        {
            _inputController.ProcessControls(gameTime);
            _buttonManager.Update(gameTime);
            UpdateUI();
        }

        public void Draw(SpriteBatch _spriteBatch, GameTime gameTime)
        {
            Selected.Draw(_spriteBatch, gameTime);

            foreach (Unit unit in _unitManager.units)
            {
                switch (unit.State)
                {
                    case UnitState.Moving:
                        //foreach (Tile tile in Overlay)
                        //{
                        //    tile.Draw(_spriteBatch, gameTime);
                        //}
                        break;
                }


            }

            _buttonManager.Draw(_spriteBatch, gameTime);
        }


    }
}
 