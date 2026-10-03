using Study3D.MiniGolf;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    public float boostForce;
    Vector3 Diraction;
    
    private void Awake()
    {
        Diraction = transform.forward;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GolfBall"))
        {
            GolfBall ball = other.GetComponent<GolfBall>();

            ball.Boost(boostForce, Diraction);
        }
    }
}
