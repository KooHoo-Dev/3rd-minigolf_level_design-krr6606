using UnityEngine;

public class KinematicSpinner : MonoBehaviour
{
    public float rotationSpeed = 90f; // degrees per second
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
