using UnityEngine;
using DG.Tweening;

/// <summary>
/// 공이 지나가는 길목의 장애물을 DOTween으로 재미있게 움직이는 스크립트.
/// 장애물 오브젝트에 붙이고 Inspector에서 Motion을 고르면 됩니다.
/// (DOTween이 프로젝트에 임포트되어 있어야 합니다)
/// </summary>
public class ObstacleTweener : MonoBehaviour
{
    public enum MotionType
    {
        PingPongMove,   // 1. 부드러운 왕복
        BounceMove,     // 2. 통통 튀며 도착 후 복귀
        ElasticSnap,    // 3. 고무줄처럼 튕겼다 돌아옴
        DashAndReturn,  // 4. 번개처럼 돌진, 천천히 복귀
        CircleMove,     // 5. 원형 궤도 이동
        ContinuousSpin, // 6. 풍차처럼 계속 회전
        StopAndGoSpin,  // 7. 한 바퀴 휙 돌고 멈칫 (반복)
        PendulumSwing,  // 8. 시계추 스윙
        GateOpenClose,  // 9. 문처럼 열렸다 닫힘
        PulseScale,     // 10. 숨쉬듯 커졌다 작아짐
        JellyPunch,     // 11. 젤리처럼 출렁
        CrazyCombo      // 12. 이동 + 회전 + 크기 변화 콤보
    }

    [Header("동작 선택")]
    [SerializeField] private MotionType motion = MotionType.PingPongMove;

    [Header("공통 설정")]
    [SerializeField] private float duration = 1.5f;       // 한 동작에 걸리는 시간
    [SerializeField] private float pauseTime = 0.5f;      // 동작 사이 멈추는 시간
    [SerializeField] private float startDelay = 0f;       // 시작 지연
    [SerializeField] private float randomDelay = 0f;      // 0 ~ 값 사이 랜덤 지연 (여러 장애물이 엇박자로 움직이게)

    [Header("이동 설정")]
    [SerializeField] private Vector3 moveOffset = new Vector3(3f, 0f, 0f); // 시작 위치 기준 이동량 (로컬)
    [SerializeField] private Vector3 circleNormal = Vector3.up;            // CircleMove: 원이 그려지는 평면의 법선

    [Header("회전 설정")]
    [SerializeField] private Vector3 rotateAxis = Vector3.up;
    [SerializeField] private float swingAngle = 60f;      // Pendulum / Gate 각도

    [Header("크기 설정")]
    [SerializeField] private float pulseScale = 1.5f;     // PulseScale / CrazyCombo 배율

    private Tween _tween;
    private Vector3 _startPos;
    private Vector3 _startEuler;
    private Vector3 _startScale;

    private void Start()
    {
        _startPos = transform.localPosition;
        _startEuler = transform.localEulerAngles;
        _startScale = transform.localScale;
        Play();
    }

    [ContextMenu("Replay")]
    public void Play()
    {
        KillTween();

        // 항상 초기 상태에서 시작
        transform.localPosition = _startPos;
        transform.localEulerAngles = _startEuler;
        transform.localScale = _startScale;

        _tween = BuildTween();
        if (_tween != null)
        {
            _tween.SetDelay(startDelay + Random.Range(0f, randomDelay));
        }
    }

    private Tween BuildTween()
    {
        switch (motion)
        {
            case MotionType.PingPongMove:   return PingPongMove();
            case MotionType.BounceMove:     return BounceMove();
            case MotionType.ElasticSnap:    return ElasticSnap();
            case MotionType.DashAndReturn:  return DashAndReturn();
            case MotionType.CircleMove:     return CircleMove();
            case MotionType.ContinuousSpin: return ContinuousSpin();
            case MotionType.StopAndGoSpin:  return StopAndGoSpin();
            case MotionType.PendulumSwing:  return PendulumSwing();
            case MotionType.GateOpenClose:  return GateOpenClose();
            case MotionType.PulseScale:     return PulseScale();
            case MotionType.JellyPunch:     return JellyPunch();
            case MotionType.CrazyCombo:     return CrazyCombo();
        }
        return null;
    }

    // 1. 부드러운 왕복 (양 끝에서 천천히, 가운데서 빠르게)
    private Tween PingPongMove()
    {
        return transform
            .DOLocalMove(_startPos + moveOffset, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // 2. 통통 튀며 도착 → 잠깐 멈춤 → 부드럽게 복귀
    private Tween BounceMove()
    {
        return DOTween.Sequence()
            .Append(transform.DOLocalMove(_startPos + moveOffset, duration).SetEase(Ease.OutBounce))
            .AppendInterval(pauseTime)
            .Append(transform.DOLocalMove(_startPos, duration * 0.6f).SetEase(Ease.InOutQuad))
            .AppendInterval(pauseTime)
            .SetLoops(-1, LoopType.Restart);
    }

    // 3. 갈 때는 고무줄처럼 출렁, 올 때는 뒤로 살짝 당겼다가 슝
    private Tween ElasticSnap()
    {
        return DOTween.Sequence()
            .Append(transform.DOLocalMove(_startPos + moveOffset, duration).SetEase(Ease.OutElastic))
            .AppendInterval(pauseTime)
            .Append(transform.DOLocalMove(_startPos, duration * 0.6f).SetEase(Ease.InBack))
            .AppendInterval(pauseTime)
            .SetLoops(-1, LoopType.Restart);
    }

    // 4. 대기 → 번개처럼 돌진 → 멈춤 → 천천히 복귀
    private Tween DashAndReturn()
    {
        return DOTween.Sequence()
            .AppendInterval(pauseTime)
            .Append(transform.DOLocalMove(_startPos + moveOffset, duration * 0.2f).SetEase(Ease.OutExpo))
            .AppendInterval(pauseTime * 0.5f)
            .Append(transform.DOLocalMove(_startPos, duration).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Restart);
    }

    // 5. moveOffset을 지름으로 하는 원형 궤도 이동
    private Tween CircleMove()
    {
        Vector3 half = moveOffset * 0.5f;
        float radius = half.magnitude;
        Vector3 center = _startPos + half;
        Vector3 a = -half; // 중심에서 시작점까지의 벡터
        Vector3 b = Vector3.Cross(circleNormal.normalized, a.normalized) * radius;

        const int steps = 8;
        Vector3[] path = new Vector3[steps];
        for (int i = 1; i <= steps; i++)
        {
            float t = (360f / steps) * i * Mathf.Deg2Rad;
            path[i - 1] = center + a * Mathf.Cos(t) + b * Mathf.Sin(t);
        }

        return transform
            .DOLocalPath(path, duration * 2f, PathType.CatmullRom)
            .SetOptions(true) // 닫힌 경로
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    // 6. 풍차처럼 일정한 속도로 계속 회전
    private Tween ContinuousSpin()
    {
        return transform
            .DOLocalRotate(rotateAxis.normalized * 360f, duration, RotateMode.FastBeyond360)
            .SetRelative(true)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    // 7. 뒤로 살짝 감겼다가 한 바퀴 휙 → 멈칫 → 반복 (공이 타이밍 맞춰 지나가야 함)
    private Tween StopAndGoSpin()
    {
        return DOTween.Sequence()
            .Append(transform
                .DOLocalRotate(rotateAxis.normalized * 360f, duration, RotateMode.FastBeyond360)
                .SetRelative(true)
                .SetEase(Ease.InOutBack))
            .AppendInterval(pauseTime)
            .SetLoops(-1, LoopType.Restart);
    }

    // 8. 시계추 스윙 (-swingAngle ~ +swingAngle)
    private Tween PendulumSwing()
    {
        Vector3 axis = rotateAxis.normalized;
        transform.localEulerAngles = _startEuler - axis * swingAngle;

        return transform
            .DOLocalRotate(_startEuler + axis * swingAngle, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // 9. 문처럼 쾅 열렸다가 닫힘 (피벗을 경첩 위치에 두면 더 자연스러움)
    private Tween GateOpenClose()
    {
        Vector3 axis = rotateAxis.normalized;
        return DOTween.Sequence()
            .Append(transform.DOLocalRotate(_startEuler + axis * swingAngle, duration * 0.5f).SetEase(Ease.OutBack))
            .AppendInterval(pauseTime)
            .Append(transform.DOLocalRotate(_startEuler, duration * 0.5f).SetEase(Ease.InBack))
            .AppendInterval(pauseTime)
            .SetLoops(-1, LoopType.Restart);
    }

    // 10. 숨쉬듯 커졌다 작아졌다 (통로 크기가 계속 변함)
    private Tween PulseScale()
    {
        return transform
            .DOScale(_startScale * pulseScale, duration)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // 11. 가만히 있다가 젤리처럼 출렁 (크기 + 위치 펀치)
    private Tween JellyPunch()
    {
        return DOTween.Sequence()
            .AppendInterval(pauseTime)
            .Append(transform.DOPunchScale(Vector3.one * 0.5f, duration * 0.6f, 8, 0.5f))
            .Join(transform.DOPunchPosition(moveOffset * 0.3f, duration * 0.6f, 6, 0.5f))
            .SetLoops(-1, LoopType.Restart);
    }

    // 12. 이동 + 회전 + 크기 변화를 동시에 (가장 정신없는 장애물)
    private Tween CrazyCombo()
    {
        Vector3 axis = rotateAxis.normalized;
        return DOTween.Sequence()
            // 가기
            .Append(transform.DOLocalMove(_startPos + moveOffset, duration).SetEase(Ease.InOutBack))
            .Join(transform.DOLocalRotate(axis * 360f, duration, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.InOutCubic))
            .Join(transform.DOScale(_startScale * pulseScale, duration).SetEase(Ease.InOutQuad))
            .AppendInterval(pauseTime)
            // 돌아오기
            .Append(transform.DOLocalMove(_startPos, duration).SetEase(Ease.InOutBack))
            .Join(transform.DOLocalRotate(axis * -360f, duration, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.InOutCubic))
            .Join(transform.DOScale(_startScale, duration).SetEase(Ease.InOutQuad))
            .AppendInterval(pauseTime)
            .SetLoops(-1, LoopType.Restart);
    }

    private void KillTween()
    {
        if (_tween != null && _tween.IsActive())
            _tween.Kill();
        _tween = null;
    }

    private void OnDestroy()
    {
        KillTween();
    }

    // 씬 뷰에서 이동 목적지를 미리 확인
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;

        Vector3 from = transform.position;
        Vector3 to = transform.parent != null
            ? transform.parent.TransformPoint(transform.localPosition + moveOffset)
            : transform.position + moveOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawWireSphere(to, 0.2f);
    }
}
