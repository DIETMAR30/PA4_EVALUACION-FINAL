using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla el estado general de la partida, HUD, victoria, derrota y pausa.
/// </summary>
public class GameManagerPA4 : MonoBehaviour
{
    public Canvas canvas;
    public Text infoText;
    public Text messageText;
    public Text helpText;

    private int cells;
    private int lives = 3;
    private float time;
    private bool ended;
    private bool paused;
    private string lastMessage;

    public bool GameEnded => ended;
    public int Cells => cells;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !ended)
            TogglePause();

        if (paused)
            return;

        if (!ended)
        {
            time += Time.deltaTime;
            UpdateUI();
        }

        if (ended && Input.GetKeyDown(KeyCode.R))
            GameBootstrap.Instance.Restart();
    }

    public void ShowObjective()
    {
        lastMessage = "OBJETIVO: recupera las 3 runas ancestrales y cruza el PORTAL";
        if (messageText) messageText.text = lastMessage;
    }

    public void CollectCell()
    {
        cells++;

        if (cells >= 3)
            lastMessage = "RUNAS COMPLETAS: el portal de luz verde se ha despertado";
        else if (cells == 1)
            lastMessage = "Primera runa recuperada. Sube por la rampa hacia la torre del este.";
        else
            lastMessage = "Segunda runa recuperada. Cruza el puente de raíces: ¡salta el hueco!";

        if (messageText) messageText.text = lastMessage;
        UpdateUI();
    }

    public void LoseLife(string reason = "¡El espíritu del bosque te alcanzó!")
    {
        lives--;

        if (lives <= 0)
        {
            ended = true;
            if (messageText) messageText.text = "EL BOSQUE TE HA CONSUMIDO\nPulsa R para reiniciar";
            return;
        }

        lastMessage = reason + " Vidas restantes: " + lives;
        if (messageText) messageText.text = lastMessage;
        GameBootstrap.Instance.Player.Teleport(GameBootstrap.PlayerStart);
        UpdateUI();
    }

    public void Win()
    {
        if (cells < 3 || ended) return;

        ended = true;
        GameBootstrap.Instance.PlayOneShot(
            GameBootstrap.Instance.WinSound,
            GameBootstrap.Instance.Player.transform.position,
            0.8f);

        if (messageText)
            messageText.text = "¡HAS ESCAPADO DEL BOSQUE!\nTiempo: " + time.ToString("0.0") + " s\nPulsa R para volver a jugar";

        UpdateUI();
    }

    public void Alert(string text)
    {
        if (ended) return;
        lastMessage = text;
        if (messageText) messageText.text = text;
        if (GameBootstrap.Instance != null)
            GameBootstrap.Instance.PlayOneShot(
                GameBootstrap.Instance.AlarmSound,
                GameBootstrap.Instance.Player.transform.position,
                0.35f);
    }

    public void TogglePause()
    {
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;

        if (messageText)
            messageText.text = paused ? "PAUSA\nPulsa ESC para continuar" : lastMessage;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    void UpdateUI()
    {
        if (infoText)
        {
            infoText.text = string.Format(
                "RUNAS {0}/3     VIDAS {1}     TIEMPO {2:0.0}s",
                cells, lives, time);
        }
    }
}
