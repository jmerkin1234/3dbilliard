using System;
using System.Collections.Generic;
using UnityEngine;

namespace Billiards.Physics
{
    /// <summary>
    /// Monitors all balls and fires events when individual balls stop
    /// and when ALL balls have stopped (turn end signal).
    /// </summary>
    public class BallSleepMonitor : MonoBehaviour
    {
        // === Constants ===
        private const float SleepHorizontalVelocityThreshold = 0.005f;
        private const float SleepAngularThreshold = 0.01f;
        private const float SleepConfirmTime = 0.15f; // must be still for this duration to confirm sleep

        // === Events ===
        /// <summary>Fired when a single ball comes to rest.</summary>
        public static event Action<BallPhysics> OnBallStopped;

        /// <summary>Fired when ALL tracked balls have come to rest.</summary>
        public static event Action OnAllBallsStopped;

        // === State ===
        private readonly List<BallPhysics> trackedBalls = new List<BallPhysics>();
        private readonly Dictionary<BallPhysics, float> sleepTimers = new Dictionary<BallPhysics, float>();
        private readonly HashSet<BallPhysics> stoppedBalls = new HashSet<BallPhysics>();
        private bool allBallsWereMoving;

        /// <summary>Whether all tracked balls are currently at rest.</summary>
        public bool AllBallsStopped => stoppedBalls.Count == trackedBalls.Count && trackedBalls.Count > 0;

        /// <summary>Number of balls currently in motion.</summary>
        public int MovingBallCount => trackedBalls.Count - stoppedBalls.Count;

        private void Awake()
        {
            RefreshTrackedBalls();
        }

        /// <summary>
        /// Re-scan the scene for all BallPhysics components.
        /// Call this after spawning or removing balls.
        /// </summary>
        public void RefreshTrackedBalls()
        {
            trackedBalls.Clear();
            sleepTimers.Clear();
            stoppedBalls.Clear();

            BallPhysics[] balls = FindObjectsByType<BallPhysics>(FindObjectsSortMode.None);
            foreach (BallPhysics ball in balls)
            {
                trackedBalls.Add(ball);
                sleepTimers[ball] = 0f;
            }
        }

        /// <summary>
        /// Register a single ball for monitoring.
        /// </summary>
        public void RegisterBall(BallPhysics ball)
        {
            if (!trackedBalls.Contains(ball))
            {
                trackedBalls.Add(ball);
                sleepTimers[ball] = 0f;
            }
        }

        /// <summary>
        /// Unregister a ball (e.g., when pocketed).
        /// </summary>
        public void UnregisterBall(BallPhysics ball)
        {
            trackedBalls.Remove(ball);
            sleepTimers.Remove(ball);
            stoppedBalls.Remove(ball);
        }

        private void FixedUpdate()
        {
            if (trackedBalls.Count == 0)
                return;

            bool anyMoving = false;

            for (int i = trackedBalls.Count - 1; i >= 0; i--)
            {
                BallPhysics ball = trackedBalls[i];

                // Handle destroyed balls
                if (ball == null)
                {
                    sleepTimers.Remove(ball);
                    stoppedBalls.Remove(ball);
                    trackedBalls.RemoveAt(i);
                    continue;
                }

                Rigidbody rb = ball.Rb;
                if (rb == null)
                {
                    trackedBalls.RemoveAt(i);
                    sleepTimers.Remove(ball);
                    stoppedBalls.Remove(ball);
                    continue;
                }

                // Respect Unity's own sleep/kinematic states first.
                if (rb.isKinematic || rb.IsSleeping())
                {
                    sleepTimers[ball] = SleepConfirmTime;
                    if (!stoppedBalls.Contains(ball))
                    {
                        stoppedBalls.Add(ball);
                        OnBallStopped?.Invoke(ball);
                    }
                    continue;
                }

                Vector3 velocity = ball.Velocity;
                float horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;
                bool isBelowThreshold = horizontalSpeed < SleepHorizontalVelocityThreshold &&
                                        ball.AngularVelocity.magnitude < SleepAngularThreshold;

                if (isBelowThreshold)
                {
                    sleepTimers[ball] += Time.fixedDeltaTime;

                    // Confirm sleep after sustained stillness
                    if (sleepTimers[ball] >= SleepConfirmTime && !stoppedBalls.Contains(ball))
                    {
                        stoppedBalls.Add(ball);
                        ball.StopMotion(); // zero out any micro-velocities
                        OnBallStopped?.Invoke(ball);
                    }
                }
                else
                {
                    // Ball is moving — reset timer and remove from stopped set
                    sleepTimers[ball] = 0f;
                    stoppedBalls.Remove(ball);
                    anyMoving = true;
                }
            }

            bool allStoppedNow = trackedBalls.Count > 0 && !anyMoving && stoppedBalls.Count == trackedBalls.Count;

            // Fire OnAllBallsStopped once when transitioning from moving -> all stopped
            if (allStoppedNow && allBallsWereMoving)
            {
                OnAllBallsStopped?.Invoke();
            }

            allBallsWereMoving = anyMoving;
        }
    }
}
