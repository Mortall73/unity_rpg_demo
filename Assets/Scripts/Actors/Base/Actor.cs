using System.Collections.Generic;
using Actors.Base.Interface;
using GameInput;
using Scriptable;
using UnityEngine;

namespace Actors.Base
{
    [RequireComponent(typeof(Vision), typeof(CommonAnimator))]
    public abstract class Actor : MonoBehaviour
    {
        public GameActor actorScript;
        public GameObject target { get; protected set; }
        public Stats stats{ get; protected set; }
        public Combat combat{ get; protected set; }
        public CommonAnimator animator{ get; protected set; }
        public Vision vision{ get; protected set; }
        public BaseInput input{ get; protected set; }
        
        
        public IControlable movement { get; protected set; }

        private void Awake()
        {
            Init();
        }

        protected virtual void Init()
        {
            animator = GetComponent<CommonAnimator>();
            vision = GetComponent<Vision>();
            combat = GetComponent<Combat>();
            input = GetComponent<BaseInput>();
            stats = GetComponent<Stats>();
            movement = GetComponent<IControlable>();
            stats.Init();
            input.Init(this);
            
            combat.Init(stats, input);
            movement.Init(stats, input);
            animator.Init(combat, movement, stats);

            stats.onDied += Die;
        }

        protected virtual void Die(GameObject go)
        {
            Debug.Log("Transform die");
            enabled = false;
            animator.enabled = false;
            input.enabled = false;
        }
        
        public bool IsEnemy(Actor actor)
        {
            return actorScript.fraction.FractionInEnemies(actor.actorScript.fraction);
        }

        public bool IsFriend(Actor actor)
        {
            return actorScript.fraction.GetInstanceID() == actor.actorScript.fraction.GetInstanceID();
        }

        public bool InCombat()
        {
            return combat.InCombat();
        }

        public bool IsDead()
        {
            return stats.IsDead();
        }
        

    }
}