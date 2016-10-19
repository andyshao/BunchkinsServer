﻿using System.Collections.Generic;
using System.Linq;
using Bunchkins.Domain.Cards;
using Bunchkins.Domain.Players;
using Bunchkins.Domain.Cards.Door.Monsters;
using static Bunchkins.Domain.Core.Input;
using Bunchkins.Hubs;

namespace Bunchkins.Domain.Core.GameStates
{
    public class CombatState : GameState
    {
        public List<MonsterCard> Monsters { get; private set; }
        public int MonsterCombatBonus { get; private set; }
        public int PlayerCombatBonus { get; private set; }
        public List<Player> PlayersPassed { get; set; }
        public int PileOfTreasures { get; private set; }

        public CombatState(Game game, MonsterCard monster) : base(game)
        {
            Monsters = new List<MonsterCard>();
            Monsters.Add(monster);

            PlayersPassed = new List<Player>();
            BunchkinsHub.UpdateCombatState(Game, this);
        }

        public override void HandleInput(Player player, Input input)
        {
            if (input == RUN)
            {
                foreach (MonsterCard monster in Monsters)
                {
                    int diceRoll = Game.RandomGenerator.Next(1, 7);
                    if (diceRoll < 5 && Game.ActivePlayer.Level >= monster.MinPursuitLevel)
                    {
                        // Bad Stuff!
                        monster.BadStuff(Game.ActivePlayer);
                    }
                    else
                    {
                        // Player escaped!

                    }
                }

                BunchkinsHub.EndCombatState(Game);
                Game.SetState(new EndState(Game));
            }
            else if (input == PASS && !PlayersPassed.Any(p => p.Name == player.Name))
            {
                PlayersPassed.Add(player);
                BunchkinsHub.UpdateCombatState(Game, this);
            }
            else if (input == PROCEED && PlayersPassed.Count() == Game.Players.Count() - 1)
            {
                if (Game.ActivePlayer.CombatPower + PlayerCombatBonus > Monsters.Sum(m => m.Level) + MonsterCombatBonus)
                {
                    int treasures = Monsters.Sum(m => m.TreasureGain) + PileOfTreasures;

                    BunchkinsHub.EndCombatState(Game);
                    player.IncreaseLevel(Monsters.Sum(m => m.LevelGain));
                    Game.LootTreasure(treasures);
                    Game.SetState(new EndState(Game));
                }
                // TODO: Error if player cannot defeat monster
            }
        }

        public override void PlayCard(Player player, Player target, Card card)
        {
            if (card is IAnytimeSpell)
            {
                ((IAnytimeSpell)card).Cast(target);
                player.Discard(card);
            }
            else if (card is ICombatSpell)
            {
                ((ICombatSpell)card).Cast(this);
                player.Discard(card);
            }
        }

        public void AddMonsterCombatBonus(int bonus)
        {
            MonsterCombatBonus += bonus;
            BunchkinsHub.UpdateCombatState(Game, this);
        }

        public void AddPlayerCombatBonus(int bonus)
        {
            PlayerCombatBonus += bonus;
            BunchkinsHub.UpdateCombatState(Game, this);
        }

        public void AddMonster()
        {
            MonsterCard monster = Game.DrawMonsterCard();
            Monsters.Add(monster);

            BunchkinsHub.UpdateCombatState(Game, this);
        }

        public void AddMonsterTreasureBonus(int bonus)
        {
            PileOfTreasures += bonus;
        }

        public void RemoveMonster(MonsterCard monster, bool isLootable)
        {
            if (isLootable)
            {
                PileOfTreasures += monster.TreasureGain;
            }

            Monsters.Remove(monster);

            if (Monsters.Count == 0 && isLootable)
            {
                Game.LootTreasure(PileOfTreasures);
                BunchkinsHub.EndCombatState(Game);
                Game.SetState(new EndState(Game));
            }
            else if (Monsters.Count == 0)
            {
                BunchkinsHub.EndCombatState(Game);
                Game.SetState(new EndState(Game));
            }
            else
            {
                BunchkinsHub.UpdateCombatState(Game, this);
            }

        }

        public bool CanPlayerWin()
        {
            int playerPower = Game.ActivePlayer.CombatPower + PlayerCombatBonus;
            int monsterPower = Monsters.Sum(m => m.Level) + MonsterCombatBonus;
            if (playerPower > monsterPower)
            {
                return true;
            }
            return false;
        }
    }
}
