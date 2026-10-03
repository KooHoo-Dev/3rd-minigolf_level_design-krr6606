using System;
using Study3D.MiniGolf;
using UnityEngine;

/// <summary>
/// 골프 게임의 전체 흐름을 관리합니다.
/// - StartGame() : 볼 위치 / 세이브 위치 / 타이머 / UI 초기화
/// - Clear()     : 어디서든 GameManager.Instance.Clear() 로 호출 가능
/// UI는 코드로 자동 생성되므로 씬에 따로 만들 필요가 없습니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Ref")]
    [SerializeField] private GolfBall ball;
    [SerializeField] private GolfPlayer player;

    [Header("Settings")]
    [Tooltip("공의 시작 위치. 비워두면 씬에 배치된 공의 위치를 사용합니다.")]
    [SerializeField] private Transform startPoint;
    [Tooltip("UI에 사용할 폰트. 비워두면 OS 폰트(한글 지원)를 자동으로 사용합니다.")]
    [SerializeField] private Font uiFont;

    /// <summary>클리어 시 호출되는 이벤트 (인자: 걸린 시간)</summary>
    public event Action<float> GameCleared;

    public float ElapsedTime { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsCleared { get; private set; }

    private GolfUI ui;
    private Vector3 startPosition;

    private CursorLockMode prevLockState;
    private bool prevCursorVisible;
    private bool cursorChanged;

    private void Awake()
    {
        Instance = this;

        startPosition = startPoint != null ? startPoint.position : ball.transform.position;

        // UI를 코드로 생성
        GameObject uiObject = new GameObject("GolfUI");
        ui = uiObject.AddComponent<GolfUI>();
        ui.Build(uiFont);
        ui.RestartClicked += StartGame;
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (!IsPlaying) return;

        ElapsedTime += Time.deltaTime;
        ui.SetTime(ElapsedTime);
    }

    private void OnDestroy()
    {
        if (ui != null)
            ui.RestartClicked -= StartGame;

        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// 게임 시작 (재시작에도 그대로 사용).
    /// 볼 위치 초기화, 세이브 초기화, 타이머 초기화, 클리어 UI 숨김.
    /// </summary>
    public void StartGame()
    {
        ElapsedTime = 0f;
        IsPlaying = true;
        IsCleared = false;

        // 볼 위치 + 세이브 위치 초기화
        ball.PlaceAt(startPosition);
        ball.SavePosition(startPosition);

        // 밟아서 초록색이 된 세이브 타일들도 원래대로
        foreach (SaveTile tile in FindObjectsByType<SaveTile>(FindObjectsSortMode.None))
            tile.ResetTile();

        // 입력 다시 허용
        if (player != null)
            player.enabled = true;

        // 클리어 때 바꿔둔 커서 상태 복구
        if (cursorChanged)
        {
            Cursor.lockState = prevLockState;
            Cursor.visible = prevCursorVisible;
            cursorChanged = false;
        }

        ui.HideClear();
        ui.SetTime(0f);
    }

    /// <summary>
    /// 게임 클리어. 홀 트리거 등 다른 곳에서
    /// GameManager.Instance.Clear(); 로 호출하세요. (중복 호출은 무시됩니다)
    /// </summary>
    public void Clear()
    {
        if (!IsPlaying) return;

        IsPlaying = false;
        IsCleared = true;

        // 클리어 후에는 공을 칠 수 없도록 입력 차단
        if (player != null)
        {
            player.enabled = false;
            if (player.aimIndicator != null)
                player.aimIndicator.Hide();
        }

        // 버튼을 누를 수 있도록 마우스 커서 표시
        prevLockState = Cursor.lockState;
        prevCursorVisible = Cursor.visible;
        cursorChanged = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ui.ShowClear(ElapsedTime);
        GameCleared?.Invoke(ElapsedTime);
    }
}
