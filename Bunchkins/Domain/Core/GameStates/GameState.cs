﻿using Bunchkins.Domain.Cards;
using Bunchkins.Domain.Cards.Treasure.Equipment;
using Bunchkins.Domain.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunchkins.Domain.Core.GameStates
{
    public abstract class GameState
    {
        protected Game Game { get; private set; }

        public GameState(Game game)
        {
            Game = game;
        }

        public abstract void HandleInput(Player player, Input input);
        public virtual void PlayCard(Player player, Player target, Card card)
        {
            // Anytime spell or equipment
            if (card is EquipmentCard)
            {
                // TODO: Check whether equipment is valid for user 
                target.EquippedCards.Add((EquipmentCard)card);
                player.Hand.Remove(card);
            }
            else if (card is IAnytimeSpell)
            {
                ((IAnytimeSpell)card).Cast(target);
                player.Hand.Remove(card);
            }
            else
            {
                Console.WriteLine("YOU DUN GOOFED, YOU CAN'T DO DAT YO!");
            }
        }
    }
}