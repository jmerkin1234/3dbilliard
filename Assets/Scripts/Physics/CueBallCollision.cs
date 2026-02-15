using UnityEngine;
using Billiards.GameState;

namespace Billiards.Physics
{
    /// <summary>
    /// Attached to the Cue Ball to detect collisions with other balls.
    /// Reports the first contact to the RuleEngine for shot validation.
    /// </summary>
    public class CueBallCollision : MonoBehaviour
    {
        private RuleEngine ruleEngine;

        private void Awake()
        {
            ruleEngine = FindAnyObjectByType<RuleEngine>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Only care about collisions with other balls
            if (collision.gameObject.CompareTag("Ball"))
            {
                if (ruleEngine != null)
                {
                    ruleEngine.RecordFirstContact(collision.gameObject);
                }
            }
        }
    }
}
