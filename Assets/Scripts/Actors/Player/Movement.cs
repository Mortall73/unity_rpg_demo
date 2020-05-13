using Actors.Base;
using Actors.Base.Interface;
using GameSystems;
using GameSystems.Input;
using UnityEngine;
using UnityEngine.AI;

namespace Actors.Player
{
    
    public class Movement : MonoBehaviour, IControlable
    {
        public float speedMultiplier = 1f;
        private BaseInput input;
        private Stats stats;
        private CharacterController characterController;
        private Camera cam;
        public Transform target { get; private set; }
        private bool stopped = false;


        public void Init(Stats actorStats, BaseInput input)
        {
            this.input = input;
            cam = GameController.instance.mainCamera.GetCamera();
            characterController = GetComponent<CharacterController>();
            stats = actorStats;
        }


        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;

            // no rigidbody
            if (body == null || body.isKinematic)
                return;
            
            if (hit.moveDirection.y < -0.3f)
                return;
            
            body.velocity = GetInputDirection() * 3;
        }
        
        
        private void FixedUpdate()
        {
            Vector3 direction = Vector3.zero;
            if (! IsCanMove())
            {
                characterController.SimpleMove(direction);
                return;
            }
            
            if (target != null)
            {
                direction = (transform.position - target.transform.position).normalized;
            }
            
            if (input.IsSomeDirection())
            {
                direction = GetInputDirection();
                
                direction = direction.normalized;

                direction *= stats.GetMovementSpeed() * speedMultiplier;
                
                float speedMultiply;
                
                if (Mathf.Abs(input.horizontal) >= .3f || Mathf.Abs(input.vertical) >= .3f)
                {
                    speedMultiply = 1f;
                }
                else
                {
                    speedMultiply = .4f;
                }
                direction *= speedMultiply;
                
                
                FaceDirection(direction);
            }
            
            Move(direction);
        }


        Vector3 GetInputDirection()
        {
            Vector3 direction = new Vector3(input.horizontal, 0f, input.vertical);
            direction = GameController.instance.mainCamera.GetCamera().transform.TransformDirection(direction);
            direction.y = 0;

            return direction;
        }
        
        public bool IsCanMove()
        {
            return characterController.isGrounded && !stopped;
        }

        public void Jump()
        {
            return;
        }

        public float GetSpeedMultiplier()
        {
            return speedMultiplier;
        }

        public float GetCurrentMagnitude()
        {
            return characterController.velocity.magnitude;
        }

        public void Move(Vector3 direction)
        {
            direction.y -= 500f * Time.deltaTime;
            characterController.Move(direction * Time.deltaTime);
        }

        public void MoveTo(Vector3 point)
        {
            return;
        }

        public void Follow(Transform newTarget, float stoppingDistance = 1f)
        {
            return;
        }

        public void StopFollow()
        {
            return;
        }

        public void Stop()
        {
            return;
        }
        
        public void FaceTarget(Vector3 target)
        {
            Vector3 direction = (target - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
                gameObject.transform.rotation =
                    Quaternion.Lerp(gameObject.transform.rotation, targetRotation, 15f * Time.deltaTime);
            }
        }
        
        
        public void FaceDirection(Vector3 direction)
        {
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
                gameObject.transform.rotation = 
                    Quaternion.Lerp(gameObject.transform.rotation, targetRotation, 15f * Time.deltaTime);
            }
        }


        public void SetTarget(Transform target)
        {
            throw new System.NotImplementedException();
        }

        public void Disable()
        {
            enabled = false;
        }

        public void Enable()
        {
            enabled = true;
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = multiplier;
        }
        
        public Transform GetTransform()
        {
            return transform;
        }
        
        public bool IsMoving()
        {
            return GetCurrentMagnitude() > 0;
        }
    }
}