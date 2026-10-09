using UnityEngine;

/// <summary>
/// IA del Espíritu Guardián del bosque. Estados:
///  - Patrol : flota despacio entre waypoints.
///  - Alert  : al detectar al jugador se detiene un instante (aviso justo antes de atacar).
///  - Chase  : persigue rápido; pierde el rastro si el jugador se aleja más que loseRange.
///  - Stunned: tras golpear, se disipa unos segundos y reaparece en su ruta de patrulla.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Alert, Chase, Stunned }

    [Header("Objetivos")]
    public Transform player;
    public Transform[] waypoints;

    [Header("Velocidades")]
    public float patrolSpeed = 1.1f;   // flota lento y misterioso
    public float chaseSpeed = 3.9f;    // más rápido que antes, pero más lento que el jugador (5.5)

    [Header("Detección")]
    public float detectionRange = 9f;  // radio amplio: el espíritu "siente" al jugador
    public float loseRange = 13f;      // histéresis: hay que alejarse más para perderlo
    public float attackRange = 1.4f;
    public float decisionInterval = 0.15f;

    [Header("Comportamiento del espíritu")]
    public float alertDelay = 0.7f;    // pausa de aviso antes de perseguir
    public float stunDuration = 2.5f;  // tiempo que se disipa tras golpear
    public float hoverAmplitude = 0.12f;

    private State state = State.Patrol;
    private int waypointIndex;
    private float decisionTimer;
    private float alertTimer;
    private float stunTimer;
    private float baseY;
    private float phase;

    public bool IsChasing => state == State.Chase;
    public State CurrentState => state;

    void Awake()
    {
        baseY = transform.position.y;
        phase = Random.value * 6.28f;
    }

    void Update()
    {
        if (player == null || GameBootstrap.Instance == null ||
            GameBootstrap.Instance.Manager == null ||
            GameBootstrap.Instance.Manager.GameEnded)
            return;

        float dt = Time.deltaTime;

        if (state == State.Stunned)
        {
            stunTimer -= dt;
            if (stunTimer <= 0f) Recover();
            return;
        }

        if (state == State.Alert) alertTimer -= dt;

        // Optimización: las decisiones de IA no se calculan en cada frame.
        decisionTimer -= dt;
        if (decisionTimer <= 0f)
        {
            decisionTimer = decisionInterval;
            UpdateState();
        }

        MoveAgent(dt);

        if (state == State.Chase) CheckAttack();
    }

    float FlatSqrDistanceToPlayer()
    {
        Vector3 d = player.position - transform.position;
        d.y = 0f;
        return d.sqrMagnitude;
    }

    void UpdateState()
    {
        float sqr = FlatSqrDistanceToPlayer();
        float detectSqr = detectionRange * detectionRange;

        switch (state)
        {
            case State.Patrol:
                if (sqr <= detectSqr) EnterAlert();
                break;

            case State.Alert:
                if (sqr > detectSqr) state = State.Patrol;       // el jugador se escabulló
                else if (alertTimer <= 0f) state = State.Chase;  // termina el aviso: persigue
                break;

            case State.Chase:
                if (sqr > loseRange * loseRange) state = State.Patrol; // pierde el rastro
                break;
        }
    }

    void EnterAlert()
    {
        state = State.Alert;
        alertTimer = alertDelay;
        GameBootstrap.Instance.Manager.Alert("Un espíritu del bosque te ha descubierto... ¡CORRE!");
    }

    void MoveAgent(float dt)
    {
        Vector3 pos = transform.position;

        if (state == State.Alert)
        {
            // Se queda quieto y mira fijamente al jugador.
            Face(player.position - pos, dt);
        }
        else
        {
            Vector3 target = state == State.Chase
                ? player.position
                : (waypoints != null && waypoints.Length > 0
                    ? waypoints[waypointIndex].position
                    : pos);

            Vector3 direction = target - pos;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0225f)
            {
                if (state == State.Patrol && waypoints != null && waypoints.Length > 0)
                    waypointIndex = (waypointIndex + 1) % waypoints.Length;
            }
            else
            {
                float speed = state == State.Chase ? chaseSpeed : patrolSpeed;
                pos += direction.normalized * speed * dt;
                Face(direction, dt);
            }
        }

        // Altura: sigue la altura del jugador al perseguir y flota suavemente.
        float targetY = state == State.Chase
            ? player.position.y + 0.3f
            : (waypoints != null && waypoints.Length > 0 ? waypoints[waypointIndex].position.y : baseY);
        baseY = Mathf.Lerp(baseY, targetY, Mathf.Clamp01(2.5f * dt));
        pos.y = baseY + Mathf.Sin(Time.time * 2f + phase) * hoverAmplitude;

        transform.position = pos;
    }

    void Face(Vector3 direction, float dt)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            7f * dt);
    }

    void CheckAttack()
    {
        if (FlatSqrDistanceToPlayer() > attackRange * attackRange) return;

        Player3D playerController = player.GetComponent<Player3D>();
        if (playerController != null)
            playerController.TakeHit();

        // Tras el golpe, el espíritu se disipa y reaparece en su ruta.
        BeginStun();
    }

    void BeginStun()
    {
        state = State.Stunned;
        stunTimer = stunDuration;

        if (waypoints != null && waypoints.Length > 0)
        {
            transform.position = waypoints[waypointIndex].position;
            baseY = transform.position.y;
        }
        SetVisible(false);
    }

    void Recover()
    {
        state = State.Patrol;
        SetVisible(true);
    }

    void SetVisible(bool visible)
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = visible;
        foreach (var l in GetComponentsInChildren<Light>(true)) l.enabled = visible;
    }
}
