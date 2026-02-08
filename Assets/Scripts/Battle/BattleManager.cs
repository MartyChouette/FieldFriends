using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using FieldFriends.Core;
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
                battleUI.ShowText(BattleTextProvider.EncounterStart(_enemyCreature.SpeciesName));
            }

            StartCoroutine(BattleLoop());
        }

        IEnumerator BattleLoop()
        {
            yield return new WaitForSeconds(1.2f);

            while (_state == BattleState.Active)
            {
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

                    bool nullify = _playerCreature.ActiveAbility == AbilityID.Wait
                                   && Random.value < GameConstants.StillWaitNullifyChance;

                    if (nullify)
                    {
                        if (battleUI != null)
                            battleUI.ShowText(BattleTextProvider.StillNullifies());
                        _state = BattleState.WaitNullify;
                    }
                    else
                    {
                        if (battleUI != null)
                            battleUI.ShowText(BattleTextProvider.WaitAction(_playerCreature.SpeciesName));
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

            // Petalyn's Calm Field: reduce enemy aggression
            if (_partyManager.PartyHasAbility(AbilityID.CalmField)
                && Random.value < (1f - GameConstants.PetalynCalmFieldModifier))
            {
                if (battleUI != null)
                    battleUI.ShowText(BattleTextProvider.CalmFieldActive(_enemyCreature.SpeciesName));
                yield return new WaitForSeconds(0.8f);
                yield break;
            }

            yield return ExecuteMove(_enemyCreature, _playerCreature, false);
        }

        IEnumerator ExecuteMove(CreatureInstance attacker, CreatureInstance defender, bool isPlayer)
        {
            int damage = CalculateDamage(attacker, defender);
            float typeMult = TypeChart.GetMultiplier(attacker.Type, defender.Type);

            string attackText = isPlayer
                ? BattleTextProvider.MoveAttack(attacker.SpeciesName)
                : BattleTextProvider.EnemyAttack(attacker.SpeciesName);

            if (battleUI != null)
                battleUI.ShowText(attackText);

            if (Audio.AudioManager.Instance != null)
                Audio.AudioManager.Instance.PlayHit();

            yield return new WaitForSeconds(0.6f);

            defender.TakeDamage(damage);

            if (typeMult > 1f)
            {
                if (battleUI != null)
                    battleUI.ShowText(BattleTextProvider.StrongHit(defender.SpeciesName));
            }
            else if (typeMult < 1f)
            {
                if (battleUI != null)
                    battleUI.ShowText(BattleTextProvider.WeakHit(defender.SpeciesName));
            }
            else
            {
                if (battleUI != null)
                    battleUI.ShowText(BattleTextProvider.TakeDamage(defender.SpeciesName));
            }

            if (battleUI != null)
                battleUI.UpdateHP();

            yield return new WaitForSeconds(0.6f);

            // Bramblet's Snare Step: chance to slow
            if (isPlayer && attacker.HasAbility(AbilityID.SnareStep) && Random.value < 0.25f)
            {
                if (battleUI != null)
                    battleUI.ShowText(BattleTextProvider.SnareStepSlow());
                yield return new WaitForSeconds(0.5f);
            }

            if (defender.IsResting)
            {
                if (battleUI != null)
                    battleUI.ShowText(BattleTextProvider.NeedsRest(defender.SpeciesName));

                if (Audio.AudioManager.Instance != null)
                    Audio.AudioManager.Instance.PlayRest();

                yield return new WaitForSeconds(0.8f);
            }
        }

        /// <summary>
        /// Damage: BaseDamage + ATK * type_multiplier - DEF/2, minimum 1.
        /// </summary>
        int CalculateDamage(CreatureInstance attacker, CreatureInstance defender)
        {
            float typeMult = TypeChart.GetMultiplier(attacker.Type, defender.Type);
            int raw = Mathf.RoundToInt((GameConstants.BaseDamage + attacker.ATK) * typeMult);
            int reduced = raw - (defender.DEF / 2);
            return Mathf.Max(1, reduced);
        }

        IEnumerator AttemptFlee()
        {
            if (battleUI != null)
                battleUI.ShowText(BattleTextProvider.BackAttempt());

            yield return new WaitForSeconds(0.5f);

            bool canFlee = _playerCreature.ActiveAbility == AbilityID.SlipAway;

            if (!canFlee && _playerCreature.ActiveAbility == AbilityID.QuickReturn)
            {
                var abilityEffects = FindFirstObjectByType<World.AbilityEffects>();
                if (abilityEffects != null)
                    canFlee = abilityEffects.TryQuickReturn();
            }

            if (!canFlee)
                canFlee = Random.value < GameConstants.BaseFleeChance;

            if (!canFlee && _partyManager.PartyHasAbility(AbilityID.GentleGust))
            {
                canFlee = Random.value < GameConstants.WispinGustEvadeChance;
                if (canFlee && battleUI != null)
                {
                    battleUI.ShowText(BattleTextProvider.GentleGustEvade());
                    yield return new WaitForSeconds(0.5f);
                }
            }

            if (canFlee)
            {
                if (battleUI != null)
                    battleUI.ShowText(BattleTextProvider.BackSuccess());
                yield return new WaitForSeconds(0.8f);
                EndBattle(false);
            }
            else
            {
                if (battleUI != null)
                    battleUI.ShowText(BattleTextProvider.BackFail());
                yield return new WaitForSeconds(0.8f);
            }
        }

        IEnumerator HandleWin()
        {
            if (battleUI != null)
                battleUI.ShowText(BattleTextProvider.Win());

            // Ripplet's Clear Pool: post-battle heal
            if (_partyManager.PartyHasAbility(AbilityID.ClearPool))
            {
                yield return new WaitForSeconds(0.5f);
                int healAmt = Mathf.Max(1, _playerCreature.MaxHP / 6);
                _playerCreature.Heal(healAmt);
                if (battleUI != null)
                {
                    battleUI.ShowText(BattleTextProvider.ClearPoolHeal(_playerCreature.SpeciesName));
                    battleUI.UpdateHP();
                }
            }

            // Check friendship milestone
            if (FriendshipTracker.HasReachedUpgrade(_playerCreature)
                && _playerCreature.UpgradeAbility != AbilityID.None)
            {
                yield return new WaitForSeconds(0.5f);
                string abilityName = _playerCreature.UpgradeAbility.ToString();
                if (battleUI != null)
                    battleUI.ShowText(BattleTextProvider.AbilityUpgrade(
                        _playerCreature.SpeciesName, abilityName));
            }

            yield return new WaitForSeconds(1.0f);
            EndBattle(true);
        }

        IEnumerator HandlePlayerCreatureResting()
        {
            var nextActive = _partyManager.GetActiveParty();
            if (nextActive.Count == 0)
            {
                if (battleUI != null)
                    battleUI.ShowText(BattleTextProvider.AllResting());
                yield return new WaitForSeconds(0.5f);
                if (battleUI != null)
                    battleUI.ShowText(BattleTextProvider.Lose());
                yield return new WaitForSeconds(1.0f);
                EndBattle(false);
                yield break;
            }

            _playerCreature = nextActive[0];
            if (battleUI != null)
            {
                battleUI.ShowText(BattleTextProvider.SwapIn(_playerCreature.SpeciesName));
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

            if (Audio.AudioManager.Instance != null)
                Audio.AudioManager.Instance.PlayMenuSelect();
        }
    }

    public enum BattleState
    {
        Active,
        WaitNullify,
        Ended
    }
}
