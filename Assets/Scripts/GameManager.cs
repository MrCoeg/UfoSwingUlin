using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Player player;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text scoreF;

    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject gameOver;

    public Spawner spawner;

    private int score;
    public int Score => score;

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(gameObject);
            Pause();
        }
    }

    public void Play()
    {
        score = 0;
        scoreText.text = score.ToString();
        scoreF.text = score.ToString();
        playButton.SetActive(false);
        gameOver.SetActive(false);

        Time.timeScale = 1f;
        player.enabled = true;
        Pipes[] pipes = FindObjectsOfType<Pipes>();

        for (int i = 0; i < pipes.Length; i++)
        {
            Destroy(pipes[i].gameObject);
        }

    }

    public void Log()
    {
        StartCoroutine(LogFile());
    }

    public IEnumerator LogFile()
    {

        var texture = ScreenCapture.CaptureScreenshotAsTexture();
        var imageByte = texture.EncodeToPNG();

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm("http://localhost:3000/LogUfoSwings", "POST"))
        {

            // Attach the form to the request
            www.uploadHandler = new UploadHandlerRaw(imageByte);
            www.uploadHandler.contentType = "multipart/form-data";
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Berhasil");
            }
            else
            {
                Debug.Log(www.error);
            }
        }
    }

    IEnumerator LoadJSON() {
        using (UnityWebRequest www = UnityWebRequest.Get("http://localhost:3000/ufoswing/configuration"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                var text = www.downloadHandler.text;
                var config = JsonUtility.FromJson<Configuration>(text);
                player.tilt = config.tilt;
                player.gravity = config.gravity;
                player.strength = config.strength;  

                spawner.speed = config.speed;
                spawner.spawnRate = config.spawnRate;
                spawner.maxHeight = config.maxHeight;
                spawner.minHeight = config.minHeight;

                Pipes[] pipes = FindObjectsOfType<Pipes>();

                for (int i = 0; i < pipes.Length; i++)
                {
                    Destroy(pipes[i].gameObject);
                }
            }
            else
            {
                Debug.Log(www.error);
            }
        }

    }

    IEnumerator AddScore()
    {
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm("http://localhost:3000/ufoswing/" + score, "POST"))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Success");

            }
            else
            {
                Debug.Log(www.error);
            }
        }

    }

    public void GameOver()
    {
        playButton.SetActive(true);
        gameOver.SetActive(true);
        StartCoroutine(AddScore());

        Pause();
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        player.enabled = false;
    }

    public void IncreaseScore()
    {
        score++;
        scoreText.text = score.ToString();
        scoreF.text = score.ToString();
    }

    

}

public struct Configuration
{
    public float spawnRate;
    public float minHeight;
    public float maxHeight;

    public float strength;
    public float gravity;
    public float tilt;

    public float speed;
}
