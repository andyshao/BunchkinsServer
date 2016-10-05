﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunchkins.Domain.Core.GameStates
{
    class DoorLootState : LootState
    {
        public DoorLootState(Game game, int numTreasures) : base(game)
        {
            Game.ActivePlayer.Hand.Add(Game.DrawDoorCard());
            
            Game.SetState(new EndState(Game));
        }
    }
}
