using UnityEngine;
using UnityEngine.UIElements;

public class GameEndScreen : MonoBehaviour
{
    public float fadeDuration = 2.0f;
    private VisualElement root;
    private VisualElement background;
    private Label messageLabel;
    private Button restartButton;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        background = root.Q<VisualElement>("Background");
        messageLabel = root.Q<Label>("Message");
        restartButton = root.Q<Button>("RestartButton");
        
        if (restartButton != null)
        {
            restartButton.clicked += OnRestartButtonClicked;
        }

        if (background != null)
        {
            background.style.opacity = 0;
        }
    }

    private System.Collections.IEnumerator FadeInCoroutine(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float easedAlpha = Mathf.SmoothStep(0f, 1f, t);
            background.style.opacity = easedAlpha;
            yield return null;
        }
    }

    public void ShowGameEndScreen(string message)
    {
        gameObject.SetActive(true);

        if (root != null)
        {
            root.style.display = DisplayStyle.Flex;
            if (messageLabel != null)
            {
                messageLabel.text = message;
            }
            StartCoroutine(FadeInCoroutine(fadeDuration));
        }
    }

    private void OnRestartButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
