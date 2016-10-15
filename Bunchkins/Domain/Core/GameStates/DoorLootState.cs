﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunchkins.Domain.Core.GameStates
{
    class DoorLootState : LootState
    {
        public DoorLootState(Game game) : base(game)
        {
            Game.ActivePlayer.AddHandCard(Game.DrawDoorCardForHand());
            
            new EndState(Game);
        }
    }
}
