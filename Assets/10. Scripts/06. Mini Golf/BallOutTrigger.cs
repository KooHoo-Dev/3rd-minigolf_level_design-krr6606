using Study3D.MiniGolf;
using UnityEngine;

public class BallOutTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GolfBall"))
        {
            GolfBall ball = other.GetComponent<GolfBall>();
            ball.RestorePosition();
        }
    }
}
