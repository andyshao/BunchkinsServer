﻿using Bunchkins.Domain.Cards;
using Bunchkins.Domain.Cards.Door;
using Bunchkins.Domain.Cards.Door.Monsters;
using Bunchkins.Domain.Core.GameStates;
using Bunchkins.Domain.Players;
using Bunchkins.Hubs;
using Bunchkins.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bunchkins.Domain.Core
{
    public class Game
    {
        public Guid GameId { get; set; }
        public GameState State { get; private set; }
        public IEnumerator<Player> mPlayerIterator;
        public List<Player> Players { get; set; }
        public Random RandomGenerator = new Random();
        public Player ActivePlayer
        {
            get
            {
                return Players.SingleOrDefault(p => p.IsActive);
            }
        }

        public Game()
        {
            GameId = Guid.NewGuid();
            Players = new List<Player>();
        }

        public void Start()
        {
            // Host player is set to ActivePlayer when creating game
            mPlayerIterator = CreatePlayerIterator().GetEnumerator();

            // Draw hands for players
            foreach (Player player in Players)
            {
                player.Hand.Add(DrawDoorCardForHand());
                player.Hand.Add(DrawDoorCardForHand());
                player.Hand.Add(DrawTreasureCard());
                player.Hand.Add(DrawTreasureCard());
            }

            SetState(new StartState(this));
            
            foreach(Player player in Players)
            {
                BunchkinsHub.UpdatePlayer(this, player);
            }
        }

        public void NextPlayer()
        {
            ActivePlayer.IsActive = false;
            mPlayerIterator.MoveNext();
            mPlayerIterator.Current.IsActive = true;

            // Update frontend
            BunchkinsHub.UpdateActivePlayer(this);
        }

        IEnumerable<Player> CreatePlayerIterator()
        {
            while (true)
            {
                foreach (Player player in Players)
                {
                    yield return player;
                }
            }
        }

        public void HandleInput(Player player, Input input)
        {
            State.HandleInput(player, input);
        }

        public void PlayCard(Player player, Player target, Card card)
        {
            State.PlayCard(player, target, card);

            BunchkinsHub.CardPlayed(this, player, target, card);
        }

        public void SetState(GameState state)
        {
            State = state;
            // send update to clients
            BunchkinsHub.UpdateState(this, State);
        }

        public DoorCard DrawDoorCard()
        {
            using (var db = new BunchkinsDataContext())
            {
                bool isInHand = false;
                DoorCard card;

                do
                {
                    card = db.Cards.OfType<DoorCard>().GetRandomElement(c => c.CardId);
                    isInHand = false;

                    // check whether card already exists in players' hand/equips
                    if (Players.Any(p => p.Hand.Any(c => c.CardId == card.CardId)))
                    {
                        isInHand = true;
                    }
                } while (isInHand);

                return card;
            }
        }

        public TreasureCard DrawTreasureCard()
        {
            using (var db = new BunchkinsDataContext())
            {
                bool isInHand = false;
                TreasureCard card;

                do
                {
                    card = db.Cards.OfType<TreasureCard>().GetRandomElement(c => c.CardId);
                    isInHand = false;

                    // check whether card already exists in players' hand/equips
                    if (Players.Any(p => p.Hand.Any(c => c.CardId == card.CardId)) || Players.Any(p => p.EquippedCards.Any(c => c.CardId == card.CardId)))
                    {
                        isInHand = true;
                    }
                } while (isInHand);

                return card;
            }
        }

        // Draw door cards, excluding monsters
        public DoorCard DrawDoorCardForHand()
        {

            DoorCard card;
            bool isMonster = true;

            do
            {
                card = DrawDoorCard();

                if (!(card is MonsterCard))
                {
                    isMonster = false;
                }
            } while (isMonster);

            return card;
        }

        // Draw any cards, excluding monsters
        public Card DrawCardForHand()
        {
            using (var db = new BunchkinsDataContext())
            {
                bool isInHand = false;
                Card card;

                do
                {
                    card = db.Cards.OfType<TreasureCard>().GetRandomElement(c => c.CardId);
                    isInHand = false;

                    // check whether card already exists in players' hand/equips and if it is a monster
                    if ((Players.Any(p => p.Hand.Any(c => c.CardId == card.CardId)) || Players.Any(p => p.EquippedCards.Any(c => c.CardId == card.CardId))) && !(card is MonsterCard))
                    {
                        isInHand = true;
                    }
                } while (isInHand);

                return card;
            }
        }

        public MonsterCard DrawMonsterCard()
        {
            using (var db = new BunchkinsDataContext())
            {
                return db.Cards.OfType<MonsterCard>().GetRandomElement(c => c.CardId);
            }
        }

        public void LootTreasure(int numTreasures)
        {
            for (int i = 0; i < numTreasures; i++)
            {
                ActivePlayer.AddHandCard(DrawTreasureCard());
            }
        }

        public void LootDoor()
        {
            ActivePlayer.AddHandCard(DrawCardForHand());
        }

    }
}
