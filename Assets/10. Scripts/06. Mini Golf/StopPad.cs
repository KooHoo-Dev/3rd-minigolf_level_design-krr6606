using Study3D.MiniGolf;
using UnityEngine;

public class StopPad : MonoBehaviour
{
    public GameObject Pad;
    Material mat;

    private void Awake()
    {
        mat = Pad.GetComponent<Renderer>().material;
        mat.color = Color.red;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GolfBall"))
        {
            GolfBall ball = other.GetComponent<GolfBall>();
            ball.StopBall();
            mat.color = Color.green;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GolfBall"))
        {
            mat.color = Color.red;
        }
    }


}
