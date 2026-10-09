using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player3D : MonoBehaviour
{
    [Header("Movimiento 3D")]
    public float speed = 5.5f;
    public float gravity = -20f;
    public float jumpHeight = 1.6f;

    public Vector3 CameraOffset { get; set; }

    // Si el jugador cae por debajo de esta altura, pierde una vida y reaparece.
    const float FallLimit = -3f;

    CharacterController controller;
    Vector3 velocity;
    float hitCooldown;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (GameBootstrap.Instance != null &&
            GameBootstrap.Instance.Manager != null &&
            GameBootstrap.Instance.Manager.GameEnded)
            return;

        if (transform.position.y < FallLimit)
        {
            if (GameBootstrap.Instance != null) GameBootstrap.Instance.PlayerFell();
            return;
        }

        // Lectura directa de teclas para que el movimiento funcione aunque
        // el proyecto cambie entre Input Manager antiguo y configuraciones nuevas.
        float x = 0f;
        float z = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z += 1f;

        Vector3 input = new Vector3(x, 0f, z);
        if (input.sqrMagnitude > 1f) input.Normalize();

        Vector3 move = input * speed;
        controller.Move(move * Time.deltaTime);

        if (input.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(input),
                12f * Time.deltaTime);
        }

        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (controller.isGrounded &&
            (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        hitCooldown -= Time.deltaTime;
    }

    /// <summary>Teletransporta al jugador desactivando el CharacterController (si no, Unity ignora el cambio de posición).</summary>
    public void Teleport(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        velocity = Vector3.zero;
        controller.enabled = true;
    }

    public void TakeHit()
    {
        if (hitCooldown > 0f) return;
        hitCooldown = 1.5f;
        if (GameBootstrap.Instance != null)
            GameBootstrap.Instance.PlayerHit();
    }
}
