using UnityEngine;
using UnityEngine.UIElements;

public class Compass : MonoBehaviour
{
    public Astronaut astronaut;
    public Rocket rocket;


    private UIDocument uiDocument;
    private VisualElement _compassContainer;
    private VisualElement _triangleElement;
    private Label _distanceLabel;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument component not found.");
            return;
        }

        var root = uiDocument.rootVisualElement;
        _compassContainer = root.Q<VisualElement>("compass-container");
        _distanceLabel = root.Q<Label>("distance-label");

        _triangleElement = new VisualElement();
        _triangleElement.style.width = 40;
        _triangleElement.style.height = 40;
        _triangleElement.style.backgroundColor = new Color(1, 1, 1, 0); // transparent

        _triangleElement.style.backgroundImage = new StyleBackground(CreateTriangleTexture(40, 40, 0.0f));
        _compassContainer.Add(_triangleElement);
    }

    Texture2D CreateTriangleTexture(int width, int height, float angleInDegrees)
    {
        Texture2D original = Resources.Load<Texture2D>("triangle");
        if (original == null)
        {
            Debug.LogError("triangle.png not found in Resources.");
            return null;
        }

        // Create a new Texture2D for the rotated image
        Texture2D rotated = new(width, height, TextureFormat.ARGB32, false);

        float angleRad = angleInDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        int x0 = width / 2;
        int y0 = height / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Translate coordinates to center
                int xt = x - x0;
                int yt = y - y0;

                // Rotate coordinates
                int xs = Mathf.RoundToInt(cos * xt + sin * yt) + original.width / 2;
                int ys = Mathf.RoundToInt(-sin * xt + cos * yt) + original.height / 2;

                Color color = new(0, 0, 0, 0);
                if (xs >= 0 && xs < original.width && ys >= 0 && ys < original.height)
                {
                    color = original.GetPixel(xs, ys);
                }
                rotated.SetPixel(x, y, color);
            }
        }
        rotated.Apply();
        return rotated;
    }

    void Start()
    {
        if (astronaut == null)
        {
            Debug.LogError("Astronaut not set.");
        }

        if (rocket == null)
        {
            Debug.LogError("Rocket not set.");
        }
    }

    void Update()
    {
        Vector2 distanceToRocket = rocket.transform.position - astronaut.transform.position;
        float angle = Mathf.Atan2(distanceToRocket.y, distanceToRocket.x) * Mathf.Rad2Deg - 90.0f;

        _triangleElement.style.backgroundImage = new StyleBackground(CreateTriangleTexture(100, 100, angle));
        _compassContainer.Add(_triangleElement);

        _distanceLabel.text = $"{Mathf.FloorToInt(distanceToRocket.magnitude)} m";
    }
}