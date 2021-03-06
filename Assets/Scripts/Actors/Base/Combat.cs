using System.Collections;
using System.Collections.Generic;
using Actors.Base.Interface;
using Actors.Base.StatsStuff;
using Gameplay.Projectile;
using GameSystems.Input;
using UnityEngine;

namespace Actors.Base
{
    
    [RequireComponent(typeof(Stats))]
    public class Combat : MonoBehaviour
    {
        [SerializeField]
        private float combatCooldown = 6;

        
        public float meleeAttackSpeed = 1f;
        public float meleeAttackDelay = 0f;
        public float meleeAttackRaduis = 2f;
        public float meleeAttackDamageMultiplier = 1f;
        public float commonCombatSpeedMultiplier = 1f;
        public float rangeAttackCooldown = .4f;
        
        public float aimTime;
        
        protected float lastAttackTime;
        protected float lastRangeAttackTime;
        protected int successAttackInRow = 0;
        protected int maxSuccessAttackInRow = 3;
        protected float successAttackRowTime = 1f;
        
        protected float curMAttackSpeed;
        protected float curMAttackDelay;
        protected float curMAttackRadius;
        protected float curMAttackDamageMultiplier;
        
        protected Actor targetActor;
        protected Actor actor;
        protected Stats stats;
        private bool inCombat = false;
        private bool proccessAttack = false;
        
        
        public event System.Action OnAttack;
        public event System.Action OnAttackEnd;
        public delegate void OnAimStart();
        public delegate void OnAimEnd();
        public delegate void OnAimBreak();

        public OnAimStart onAimStart;
        public OnAimEnd onAimEnd;
        public OnAimBreak onAimBreak;

        
        public delegate void OnTargetChange(Actor target);

        private void Awake()
        {
            enabled = false;
        }

        public virtual void Init(Stats actorStats, BaseInput baseInput)
        {
            stats = actorStats;
            curMAttackSpeed = meleeAttackSpeed;
            curMAttackDelay = meleeAttackDelay;
            curMAttackRadius = meleeAttackRaduis;
            curMAttackDamageMultiplier = meleeAttackDamageMultiplier;
            OnAttack = null;
            OnAttackEnd = null;
            // TODO Rework 
            actor = GetComponent<Actor>();
            enabled = true;
        }
        
        protected virtual void FixedUpdate()
        {
            // TODO blend cooldowns ??
            float lastAttackDelta = Time.time - lastAttackTime;
            float lastRangeAttackDelta = Time.time - lastRangeAttackTime;
            
            if (lastAttackDelta > combatCooldown && lastRangeAttackDelta > rangeAttackCooldown && inCombat)
            {
                ExitCombat();
            }
            
            if (lastAttackDelta > (successAttackRowTime + GetCurrentMeleeAttackSpeed()))
            {
                successAttackInRow = 0;
            }
        }
        
        public virtual void MeleeAttack(List<IHealthable> targetStats)
        {
            if (Time.time - lastAttackTime < GetCurrentMeleeAttackSpeed() && proccessAttack)
            {
                return;
            }
            proccessAttack = true;

            EnterCombat();

            lastAttackTime = Time.time;
            InvokeOnAttack();
            StartCoroutine(DoMeleeDamage(targetStats));
        }

        public virtual void Aim()
        {
            if (aimTime == 0)
            {
                onAimStart?.Invoke();
            }
            aimTime += Time.deltaTime;
        }
        
        public virtual void RangeAttack(Vector3 point)
        {
            if (Time.time - lastRangeAttackTime < rangeAttackCooldown)
            {
                return;
            }
            onAimEnd?.Invoke();
        }

        
        
        protected virtual IEnumerator DoMeleeDamage(List<IHealthable> targetStats)
        {
            yield return new WaitForSeconds(curMAttackDelay / commonCombatSpeedMultiplier);
            
            for (int i = 0; i < targetStats.Count; i++)
            {
                if (! InMeleeZone(targetStats[i].GetTransform()))
                {
                    continue;
                }
                if (!stats.IsDead())
                {
                    targetStats[i].TakeDamage(stats.GetDamageValue(true, true, curMAttackDamageMultiplier));
                }
            }

            successAttackInRow++;

            if (successAttackInRow == maxSuccessAttackInRow)
            {
                successAttackInRow = 0;
            }
            
            OnAttackEnd?.Invoke();
            proccessAttack = false;
        }

        protected bool InMeleeZone(Transform transform)
        {
            return InMeleeRange(transform) && actor.vision.IsInViewAngle(transform);
        }
        
        protected void InvokeOnAttack()
        {
            OnAttack?.Invoke();
        }
        
        protected void EnterCombat()
        {
            inCombat = true;
        }
        
        protected void ExitCombat()
        {
            inCombat = false;
        }

        public int GetCurrentSuccessAttack()
        {
            return successAttackInRow;
        }
        
        public int GetMaxSuccessAttack()
        {
            return maxSuccessAttackInRow;
        }

        public bool IsLastCombatAttack()
        {
            return successAttackInRow == maxSuccessAttackInRow - 1;
        }
        
        public float GetCurrentMeleeAttackSpeed()
        {
            return curMAttackSpeed / commonCombatSpeedMultiplier;
        }

        public bool InMeleeRange(Vector3 targetPosition)
        {
            return Vector3.Distance(transform.position, targetPosition) <= curMAttackRadius;
        }
        
        public bool InMeleeRange(Transform targetTransform)
        {
            return InMeleeRange(targetTransform.position);
        }
        
        public bool IsMeleeAttacking()
        {
            return Time.time - lastAttackTime <= GetCurrentMeleeAttackSpeed();
        }

        public bool IsRangeCooldown()
        {
            return Time.time - lastRangeAttackTime <= rangeAttackCooldown;
        }
        
        public void SetTarget(Actor target)
        {
            targetActor = target;
        }
        
        public Actor GetTarget()
        {
            return targetActor;
        }
        
        public bool InCombat()
        {
            return inCombat;
        }


        public float GetRangeCooldown()
        {
            return Time.time - lastRangeAttackTime;
        }
    }
}