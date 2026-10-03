using Study3D.MiniGolf;
using UnityEngine;

public class SaveTile : MonoBehaviour
{
    private Vector3 SavePosition;
    public Color NoSaveColor = Color.aliceBlue;
    public Color SaveColor = Color.green;
    private Material material;
    private void Awake()
    {
        SavePosition = transform.position + Vector3.up;
        material = GetComponent<Renderer>().material;
        material.color = NoSaveColor;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("GolfBall"))
        {
            GolfBall ball = collision.gameObject.GetComponent<GolfBall>();
            ball.SavePosition(SavePosition);
            material.color = SaveColor;
        }
    }

    // [추가] 게임 재시작 시 GameManager가 호출 - 밟은 표시(초록색)를 원래대로 되돌립니다.
    public void ResetTile()
    {
        material.color = NoSaveColor;
    }
}
