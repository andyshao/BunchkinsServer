﻿using Bunchkins.Domain.Cards.Door.Monsters;
using Bunchkins.Domain.Core.GameStates;
using Bunchkins.Domain.Players;
using System;

namespace Bunchkins.Domain.Cards.Treasure.Spells
{
    class EscapeSpellCard : TreasureSpellCard, ICombatSpell
    {
        public void Cast(CombatState combat)
        {
            throw new NotImplementedException();
            //Make monsters in combat go away
        }
    }
}
