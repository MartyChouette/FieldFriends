using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using FieldFriends.Data;
using FieldFriends.Party;

namespace FieldFriends.Battle
{
    /// <summary>
    /// Core battle loop.
    /// Turn order by SPD. Actions: MOVE (attack), WAIT (skip + friendship), BACK (flee).
    /// Typical battle: 4-6 turns. KO state is "rest," never "faint."
    /// No floating numbers, no flashing, no shake.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static event Action OnBattleStarted;
        public static event Action<bool> OnBattleEnded; // true = win, false = loss/flee

        [Header("References")]
        [SerializeField] BattleUI battleUI;

        BattleState _state;
        CreatureInstance _playerCreature;
        CreatureInstance _enemyCreature;
        PartyManager _partyManager;

        bool _waitingForInput;
        BattleAction _selectedAction;

        public BattleState State => _state;

        public void StartBattle(CreatureInstance enemy, PartyManager partyManager)
        {
            _partyManager = partyManager;
            _enemyCreature = enemy;
            _playerCreature = partyManager.GetLead();

            if (_playerCreature == null)
            {
                EndBattle(false);
                return;
            }

            _state = BattleState.Active;
            OnBattleStarted?.Invoke();

            if (battleUI != null)
            {
                battleUI.Initialize(_playerCreature, _enemyCreature);
                battleUI.ShowText(_enemyCreature.SpeciesName + " appeared.");
            }

            StartCoroutine(BattleLoop());
        }

        IEnumerator BattleLoop()
        {
            while (_state == BattleState.Active)
            {
                // Determine turn order by SPD
                bool playerFirst = _playerCreature.SPD >= _enemyCreature.SPD;

                if (playerFirst)
                {
                    yield return PlayerTurn();
                    if (_state != BattleState.Active) break;
                    yield return EnemyTurn();
                }
                else
                {
                    yield return EnemyTurn();
                    if (_state != BattleState.Active) break;
                    yield return PlayerTurn();
                }

                // Check end conditions
                if (_enemyCreature.IsResting)
                {
                    yield return HandleWin();
                    break;
                }
                if (_playerCreature.IsResting)
                {
                    yield return HandlePlayerCreatureResting();
                    if (_state != BattleState.Active) break;
                }
            }
        }

        IEnumerator PlayerTurn()
        {
            if (_playerCreature.IsResting) yield break;

            // Show menu and wait for input
            _waitingForInput = true;
            if (battleUI != null)
                battleUI.ShowMenu(true);

            while (_waitingForInput)
                yield return null;

            if (battleUI != null)
                battleUI.ShowMenu(false);

            switch (_selectedAction)
            {
                case BattleAction.Move:
                    yield return ExecuteMove(_playerCreature, _enemyCreature, true);
                    break;

                case BattleAction.Wait:
                    FriendshipTracker.OnWaitUsed(_playerCreature);

                    // Still's ability: chance to nullify enemy turn
                    bool nullify = _playerCreature.ActiveAbility == AbilityID.Wait
                                   && Random.value < 0.3f;

                    if (nullify)
                    {
                        if (battleUI != null)
                            battleUI.ShowText(_playerCreature.SpeciesName + " holds steady.");
                        _state = BattleState.WaitNullify;
                    }
                    else
                    {
                        if (battleUI != null)
                            battleUI.ShowText(_playerCreature.SpeciesName + " waits.");
                    }
                    yield return new WaitForSeconds(0.8f);
                    if (_state == BattleState.WaitNullify)
                        _state = BattleState.Active;
                    break;

                case BattleAction.Back:
                    yield return AttemptFlee();
                    break;
            }
        }

        IEnumerator EnemyTurn()
        {
            if (_enemyCreature.IsResting) yield break;
            if (_state == BattleState.WaitNullify) yield break;

            // Petalyn's Calm Field: reduce enemy aggression (chance enemy waits)
            if (_partyManager.PartyHasAbility(AbilityID.CalmField) && Random.value < 0.2f)
            {
                if (battleUI != null)
                    battleUI.ShowText(_enemyCreature.SpeciesName + " hesitates.");
                yield return new WaitForSeconds(0.8f);
                yield break;
            }

            yield return ExecuteMove(_enemyCreature, _playerCreature, false);
        }

        IEnumerator ExecuteMove(CreatureInstance attacker, CreatureInstance defender, bool isPlayer)
        {
            int damage = CalculateDamage(attacker, defender);
            defender.TakeDamage(damage);

            string text = attacker.SpeciesName + " nudges forward.";
            if (battleUI != null)
            {
                battleUI.ShowText(text);
                battleUI.UpdateHP();
            }

            yield return new WaitForSeconds(1.0f);

            if (defender.IsResting)
            {
                string restText = defender.SpeciesName + " needs to rest.";
                if (battleUI != null)
                    battleUI.ShowText(restText);
                yield return new WaitForSeconds(0.8f);
            }
        }

        /// <summary>
        /// Damage formula: ATK * type_multiplier - DEF/2, minimum 1.
        /// Kept simple and low-variance for calm battles.
        /// </summary>
        int CalculateDamage(CreatureInstance attacker, CreatureInstance defender)
        {
            float typeMult = TypeChart.GetMultiplier(attacker.Type, defender.Type);
            int raw = Mathf.RoundToInt(attacker.ATK * typeMult);
            int reduced = raw - (defender.DEF / 2);
            return Mathf.Max(1, reduced);
        }

        IEnumerator AttemptFlee()
        {
            // Base 50% chance. Drift's Slip Away: guaranteed.
            // Skirl's Quick Return: guaranteed once per area.
            bool canFlee = _playerCreature.ActiveAbility == AbilityID.SlipAway
                        || _playerCreature.ActiveAbility == AbilityID.QuickReturn
                        || Random.value < 0.5f;

            // Wispin's Gentle Gust: bonus flee chance
            if (!canFlee && _partyManager.PartyHasAbility(AbilityID.GentleGust))
                canFlee = Random.value < 0.3f;

            if (canFlee)
            {
                if (battleUI != null)
                    battleUI.ShowText("You head back for now.");
                yield return new WaitForSeconds(0.8f);
                EndBattle(false);
            }
            else
            {
                if (battleUI != null)
                    battleUI.ShowText("You can't get away.");
                yield return new WaitForSeconds(0.8f);
            }
        }

        IEnumerator HandleWin()
        {
            if (battleUI != null)
                battleUI.ShowText("You keep moving.");

            // Ripplet's Clear Pool: post-battle heal
            if (_partyManager.PartyHasAbility(AbilityID.ClearPool))
            {
                int healAmt = 3;
                _playerCreature.Heal(healAmt);
                yield return new WaitForSeconds(0.5f);
                if (battleUI != null)
                {
                    battleUI.ShowText(_playerCreature.SpeciesName + " feels a bit better.");
                    battleUI.UpdateHP();
                }
            }

            yield return new WaitForSeconds(1.0f);
            EndBattle(true);
        }

        IEnumerator HandlePlayerCreatureResting()
        {
            // Try to swap in the next active creature
            var nextActive = _partyManager.GetActiveParty();
            if (nextActive.Count == 0)
            {
                if (battleUI != null)
                    battleUI.ShowText("You head back for now.");
                yield return new WaitForSeconds(1.0f);
                EndBattle(false);
                yield break;
            }

            _playerCreature = nextActive[0];
            if (battleUI != null)
            {
                battleUI.ShowText(_playerCreature.SpeciesName + " steps forward.");
                battleUI.SetPlayerCreature(_playerCreature);
            }
            yield return new WaitForSeconds(0.8f);
        }

        void EndBattle(bool won)
        {
            _state = BattleState.Ended;
            OnBattleEnded?.Invoke(won);
        }

        /// <summary>
        /// Called by BattleUI when the player selects an action.
        /// </summary>
        public void OnActionSelected(BattleAction action)
        {
            if (!_waitingForInput) return;
            _selectedAction = action;
            _waitingForInput = false;
        }
    }

    public enum BattleState
    {
        Active,
        WaitNullify,
        Ended
    }
}
