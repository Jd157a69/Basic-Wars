using Basic_Wars_V2.Entities;
using Basic_Wars_V2.Enums;
using Basic_Wars_V2.Graphics;
using Basic_Wars_V2.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;

namespace Basic_Wars_V2
{
    public class BasicWarsGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        const string ASSET_NAME_IN_GAME_ASSETS = "InGameAssets";
        const string ASSET_NAME_GAMEFONT = "Font";

        Texture2D InGameAssets;
        SpriteFont Font;

        private const int WINDOW_WIDTH = 1920;
        private const int WINDOW_HEIGHT = 1080;

        public GameState gameState;


        //              TESTING
        private Unit unit;
        private UnitManager _unitManager;
        private EntityManager _entityManager;
        private MapManager _gameMap;
        private GameUI _gameUI;

        /*  
         *  TODO: Implementation of the movement point system for each unit type and displaying it with GameUI
         *  
         *  TODO: Ability to distinguish what team a unit is on and only allowing the current player to select units on their team
         *  
         *  TODO: Ability for units to attack each other 
         *  
         *  TODO: Attributes for both units and tiles should be displayed
         *      - Use console for now and implement UI version in the future
         *      
         *  TODO: User should eneter the number of players in the game (Max 4) 
         *      - Use console for this and implement the UI version in future
         *      
         *  TODO: Update method should loop through a list of players, going through each game state before moving onto the next player
         *      - This will introduce the Player class: potential use of built in Enum PlayerIndex?
         *  
         *  TODO: Code A* for use with the AI
         *      - Computerphile video on YouTube: https://www.youtube.com/watch?v=ySN5Wnu88nE
         *      - Simple A* Path Finding in Monogame: https://youtu.be/FflEY83irJo
         *  
         *  TODO: JSON or XML read and write to files
         *      - Serialization and deserialization of files: 
         *          https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to?pivots=dotnet-7-0
         *          https://www.c-sharpcorner.com/article/working-with-json-in-C-Sharp/
         *  
         */

        public BasicWarsGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            _graphics.ApplyChanges();


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            InGameAssets = Content.Load<Texture2D>(ASSET_NAME_IN_GAME_ASSETS);
            Font = Content.Load<SpriteFont>(ASSET_NAME_GAMEFONT);


            //      TESTING

            gameState = GameState.Initial;
            _entityManager = new EntityManager();
            _gameMap = new MapManager(InGameAssets, new Vector2(500, 0), 18, 18);
            _unitManager = new UnitManager();
            _gameUI = new GameUI(InGameAssets, Font, _gameMap, _unitManager);


            for (int i = 0; i < 4; i++)
            {
                int temp = 56 * i;
                unit = new Unit(InGameAssets, new Vector2(500 + temp, 0), i + 1, i + 1);
                _unitManager.AddUnit(unit);
            }

            _entityManager.AddEntity(_unitManager);
        }

        //Debug
        KeyboardState previousKeyboardState;
        KeyboardState currentKeyboardState;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);

            _entityManager.Update(gameTime);
            _gameMap.UpdateMap(gameTime);
            _gameUI.Update(gameTime);


            UpdateGameState(gameTime);

            //      TESTING

            //Regenerates map
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            if (currentKeyboardState.IsKeyDown(Keys.A) && previousKeyboardState.IsKeyUp(Keys.A))        
            {                                                                                           
                _gameMap = new MapManager(InGameAssets, new Vector2(500, 0), 18, 18);
                _gameUI = new GameUI(InGameAssets, Font,  _gameMap, _unitManager);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();


            //      TESTING
            _gameMap.DrawMap(_spriteBatch, gameTime);


            _entityManager.Draw(_spriteBatch, gameTime);

            _gameUI.Draw(_spriteBatch, gameTime);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public void UpdateGameState(GameTime gameTime)
        {
            switch (gameState)
            {
                case GameState.PlayerSelect:

                    break;
            }
        }
    }
}