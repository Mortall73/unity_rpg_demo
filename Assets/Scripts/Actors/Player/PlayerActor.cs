
using System.Collections;
using System.Collections.Generic;
using Actors.Base;
using Actors.Base.Interface;
using GameSystems;
using Managers.Player;
using Scriptable;
using UnityEngine;

namespace Actors.Player
{
    public class PlayerActor : Actor
    {
        public PlayerFX playerFx;
        private CameraController cameraController;
        private EquipmentManager equipment;
        
        
        private bool dashing = false;
        private float dashingColdownd = 1f;
        private float dashingTime;
        private bool isAiming;
        
        public override void Init()
        {
            base.Init();
            playerFx = GetComponent<PlayerFX>();
            playerFx.Init(combat);
            // turnoff automatic vision update
            vision.isEnabled = false;
            cameraController = GameController.instance.GetCameraController();
            equipment = GameController.instance.playerManager.equipmentManager;
            combat.OnAttackEnd += ShakeCamera;
            combat.OnAttackEnd += PushPhysicsObjects;
        }
        
        
        public override void MeleeAttack()
        {
            if (dashing)
            {
                return;
            }
            
            float oldView = vision.viewAngle;
            vision.viewAngle = 360f;
            vision.viewRadius *= 2f;
            List<Collider> objects = vision.FindVisibleColliders();

            vision.viewAngle = oldView;
            vision.viewRadius /= 2f;
            List<IHealthable> attack = new List<IHealthable>();
            
            for (int i = 0; i < objects.Count; i++)
            {
                IHealthable healthableObject = objects[i].GetComponent<IHealthable>();

                if (healthableObject != null)
                {
                    attack.Add(healthableObject);
                }
                
            }
            combat.MeleeAttack(attack);
        }


        public override void Dash()
        {
            if (CanDash())
            {
                StartCoroutine(Dashing());
            }
        }


        public override void Aim(Vector3 point)
        {
            if (equipment.GetRangeWeapon() == null)
            {
                return;
            }

            if (combat.IsRangeCooldown())
            {
                return;
            }
            
            if (combat.aimTime == .0f)
            {
                equipment.SetMainWeaponActive(false);
            }

            isAiming = true;
            float bodyRotationSpeed = 3f;
            movement.SetSpeedMultiplier(0.6f);
            combat.Aim();

            point = TransformJoystickPoint(point);
            point.y = animator.lookPoint.position.y;
//            point = Vector3.Slerp(animator.lookPoint.position, point, 0.25f);
            animator.lookPoint.position = point;
            animator.isLookAtEnabled = true;
            
            // Rotate actor
            if (animator.excessAngle != 0)
            {
                movement.Rotating(false);
                transform.Rotate(0, animator.excessAngle * Time.deltaTime * bodyRotationSpeed, 0);
            }
        }


        private Vector3 TransformJoystickPoint(Vector3 point)
        {
            Camera camera = GameController.instance.GetCameraController().GetCamera();
            return camera.transform.TransformPoint(- point) - (camera.transform.position - transform.position);
        }
        
        public override void RangeAttack()
        {
            combat.RangeAttack(animator.lookPoint.position);
        }

        public override void StopAim()
        {
            if (!isAiming)
            {
                return;
            }
            
            if (combat.aimTime > 0)
            {
                RangeAttack();
                movement.ResetSpeedMultiplier();
            }
            isAiming = false;
            movement.Rotating(true);
            animator.isLookAtEnabled = false;

            StartCoroutine(ShowMelee());
        }

        IEnumerator ShowMelee(float time = .3f)
        {
            yield return new WaitForSeconds(time);
            equipment.SetMainWeaponActive(true);
        }
        
        public IEnumerator Dashing()
        {
            // TODO remove hardcode dashing?
            if (!CanDash())
            {
                yield break;
            }
            dashingTime = Time.time;
            dashing = true;
            animator.Trigger("dash");
            movement.SetSpeedMultiplier(3.5f);
            yield return new WaitForSeconds(.15f);
            movement.SetSpeedMultiplier(2);
            yield return new WaitForSeconds(.3f);
            
            movement.SetSpeedMultiplier(1);
            dashing = false;
        }
        
        void PushPhysicsObjects()
        {
            List<Collider> objects = vision.FindVisibleColliders();
            for (int i = 0; i < objects.Count; i++)
            {
                Rigidbody rb = objects[i].attachedRigidbody;

                if (rb != null)
                {
                    if (combat.InMeleeRange(objects[i].transform.position))
                        rb.AddForce(transform.forward * 100, ForceMode.Impulse);
                }
            }
        }

        void ShakeCamera()
        {
            int current = combat.GetCurrentSuccessAttack();
            int multyplier = 1;
            if (0 == current)
            {
                multyplier *= 4;
            }

            StartCoroutine(cameraController.Shake(.25f * multyplier));
        }


        bool CanDash()
        {
            if (Time.time - dashingTime >= dashingColdownd)
            {
                if (combat.IsMeleeAttacking())
                {
                    return false;
                }

                if (combat.aimTime > 0)
                {
                    return false;
                }
                
                return true;
            }
            
            return false;
        }


        public override void PushBack(Vector3 pusherPos, float force = 1)
        {
            StartCoroutine(PushingBack(pusherPos, force));
        }

        IEnumerator PushingBack(Vector3 point, float force = 1)
        {
            float time = 0;
            
            Vector3 pos = transform.position;
            Vector3 endPos = pos - (GetDirection(point) * force);
            
            // Todo check nan
            if (endPos != endPos)
            {
                yield break;
            }
            
            while (time < 1)
            {
                time += Time.deltaTime * 5f;
                transform.position = Vector3.Lerp(pos, endPos, time);
                yield return null;
            }
            
        }
    }
}