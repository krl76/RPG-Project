using UnityEngine;
using UnityEngine.AI;

namespace Features.Enemy
{
    public partial class EnemyAI
    {
        private float _nextFleeRepathTime;
        private float _baseAgentSpeed;

        private bool _locomotionRequested;
        private bool _fleeLocomotionRequested;
        private bool _appliedMoveAnimation;
        private bool _appliedFleeAnimation;

        private NavMeshAgent _agent;

        public void MoveTowardsTarget(float speedMultiplier)
        {
            if (HasActiveTarget() == false)
            {
                StopMovement();
                return;
            }

            if (CanUseAgent() == false)
            {
                return;
            }

            _agent.speed = _baseAgentSpeed * Mathf.Max(0.01f, speedMultiplier);
            _agent.isStopped = false;
            _agent.SetDestination(_playerTransform.position);

            RequestLocomotion(isMoving: true, isFleeing: false);
        }

        public void MoveAwayFromTarget()
        {
            if (HasActiveTarget() == false)
            {
                StopMovement();
                return;
            }

            if (CanUseAgent() == false)
            {
                return;
            }

            if (_agent.hasPath && Time.time < _nextFleeRepathTime)
            {
                return;
            }

            Vector3 fleeDirection = (transform.position - _playerTransform.position).normalized;
            if (fleeDirection.sqrMagnitude < 0.001f)
            {
                fleeDirection = -transform.forward;
            }

            Vector3 desiredPoint = transform.position + fleeDirection * Config.FleeDistance;
            Vector3 fleeTarget = desiredPoint;

            if (NavMesh.SamplePosition(desiredPoint, out NavMeshHit hit, Config.FleeDistance, NavMesh.AllAreas))
            {
                fleeTarget = hit.position;
            }

            _agent.speed = _baseAgentSpeed * Mathf.Max(0.01f, Config.FleeSpeedMultiplier);
            _agent.isStopped = false;
            _agent.SetDestination(fleeTarget);
            _nextFleeRepathTime = Time.time + Config.FleeRepathInterval;

            RequestLocomotion(isMoving: true, isFleeing: true);
        }

        public bool HasReachedSafeDistance()
        {
            return HasActiveTarget() == false || DistanceToTarget() >= GetDisengageRange();
        }

        public void StopMovement()
        {
            if (CanUseAgent())
            {
                _agent.isStopped = true;
                if (_agent.hasPath)
                {
                    _agent.ResetPath();
                }

                _agent.speed = _baseAgentSpeed;
            }

            RequestLocomotion(isMoving: false, isFleeing: false);
        }

        public void LookAtTarget()
        {
            if (HasActiveTarget() == false)
            {
                return;
            }

            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                Config.RotationSpeed * Time.deltaTime);
        }

        private void SetMovementAnimation(bool isMoving, bool isFleeing)
        {
            _enemyAnimation.SetIsWalking(false);
            _enemyAnimation.SetIsRunning(isMoving);
            _enemyAnimation.SetIsFleeing(isFleeing);
            _enemyAnimation.SetRunSpeed(isMoving ? 1f : 0f);

            _enemyAnimation.SyncLocomotionState(isMoving);
        }

        private void RequestLocomotion(bool isMoving, bool isFleeing)
        {
            _locomotionRequested = isMoving;
            _fleeLocomotionRequested = isMoving && isFleeing;
        }

        private void RefreshLocomotionAnimation()
        {
            bool shouldAnimateMovement = ShouldAnimateMovement();
            bool shouldAnimateFlee = shouldAnimateMovement && _fleeLocomotionRequested;

            if (_appliedMoveAnimation == shouldAnimateMovement
                && _appliedFleeAnimation == shouldAnimateFlee)
            {
                return;
            }

            _appliedMoveAnimation = shouldAnimateMovement;
            _appliedFleeAnimation = shouldAnimateFlee;
            SetMovementAnimation(shouldAnimateMovement, shouldAnimateFlee);
        }

        private bool ShouldAnimateMovement()
        {
            if (_locomotionRequested == false
                || _enemyAnimation == null
                || IsActionInProgress
                || IsCurrentActionAnimationLocked())
            {
                return false;
            }

            if (CanUseAgent() == false)
            {
                return false;
            }

            if (_agent.isStopped)
            {
                return false;
            }

            if (_agent.velocity.sqrMagnitude > 0.01f)
            {
                return true;
            }

            if (_agent.pathPending || _agent.hasPath == false)
            {
                return false;
            }

            if (float.IsInfinity(_agent.remainingDistance))
            {
                return false;
            }

            return _agent.remainingDistance > Mathf.Max(_agent.stoppingDistance + 0.15f, 0.2f);
        }

        private bool CanUseAgent()
        {
            return _agent != null && _agent.enabled && _agent.isOnNavMesh;
        }

        private void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            if (CanUseAgent())
            {
                if (NavMesh.SamplePosition(position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    _agent.Warp(hit.position);
                }
                else
                {
                    transform.position = position;
                }
            }
            else
            {
                transform.position = position;
            }

            transform.rotation = rotation;
        }
    }
}
