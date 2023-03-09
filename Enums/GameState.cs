﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Wars_V2.Enums
{
    public enum GameState
    {
        PlayerSelect,
        SelectAction,
        PlayerMove,
        PlayerAttack,
        PlayerCapture,
        PlayerProduceUnit,
        EnemyTurn,
        PauseGame,
        GameOver,
    }
}
