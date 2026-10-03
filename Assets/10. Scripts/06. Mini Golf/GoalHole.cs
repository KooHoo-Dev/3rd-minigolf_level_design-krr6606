using UnityEngine;

/// <summary>
/// 홀(골인 지점)에 붙이는 트리거. Collider의 Is Trigger를 켜고 사용하세요.
/// 공이 들어오면 GameManager의 Clear()를 호출합니다.
/// (Clear()는 다른 스크립트 어디서든 GameManager.Instance.Clear()로 호출할 수 있습니다)
/// </summary>
public class GoalHole : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("GolfBall")) return;

        if (GameManager.Instance != null)
            GameManager.Instance.Clear();
    }
}
