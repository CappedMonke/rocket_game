using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;

public class Rocket : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference rotateAction;
    public InputActionReference thrustAction;
    public InputActionReference exitRocketAction;

    [Header("Interaction")]

    [Header("Movement")]
    public float rotationSpeed = 10f;
    public float thrustSpeed = 10f;

    [Header("References")]
    public Transform doorTransform;
    private Player player;
    public CinemachineImpulseSource impulseSource;

    [Header("Stats")]
    public int health = 100;
    public int maxHealth = 100;
    public float oxygen = 100f;
    public float maxOxygen = 100f;
    public float fuel = 100f;
    public float maxFuel = 100f;
    public float fuelLossRate = 1f;

    [Header("Sounds")]
    public AudioClip perfectLandingSound;
    public AudioClip exitRocketSound;
    public AudioClip thrustSound;
    public AudioClip launchSound;
    public AudioClip explosionSound;
    public AudioClip damageSound;
    public AudioClip landingSound;

    [Header("Landing")]
    public float perfectLandingVelocityThreshold = 1f;
    public float perfectLandingAngleThreshold = 5f;

    private Rigidbody2D rb;
    private bool hasFlown = false;
    private const float minVelocityThreshold = 0.1f;
    private const float speedBoostDuration = 5f;
    private const float speedBoostMultiplier = 2f;
    private float flightTime = 0f;
    private bool hasRotated = false;
    private bool canThrust = true;
    private AudioSource thrustAudioSource;

    void Awake()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        player = FindFirstObjectByType<Player>();
        thrustAudioSource = gameObject.AddComponent<AudioSource>();
        thrustAudioSource.clip = thrustSound;
        thrustAudioSource.loop = true;
        thrustAudioSource.playOnAwake = false;
    }

    void Update()
    {
        HandleExitRocket();
        UiManager.Instance.UpdateRocketUi(health, maxHealth, Mathf.RoundToInt(oxygen), Mathf.RoundToInt(maxOxygen), Mathf.RoundToInt(fuel), Mathf.RoundToInt(maxFuel));
        if (hasFlown)
        {
            flightTime += Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        HandleRotation();
        HandleThrust();
    }

    private void HandleRotation()
    {
        float rotateInput = rotateAction.action.ReadValue<float>();
        if (Mathf.Abs(rotateInput) > 0.1f)
        {
            hasRotated = true;
        }
        rb.MoveRotation(rb.rotation - rotateInput * rotationSpeed * Time.fixedDeltaTime);
    }

    private void HandleThrust()
    {
        if (!canThrust) return;

        float thrustInput = thrustAction.action.ReadValue<float>();
        if (thrustInput > 0 && fuel > 0)
        {
            if (!thrustAudioSource.isPlaying || thrustAudioSource.volume < 1f)
            {
                thrustAudioSource.volume = 1f; // Reset volume in case it was fading out
                thrustAudioSource.Play();
            }
            hasFlown = true;
            Vector2 thrustVector = thrustInput * thrustSpeed * rb.transform.up;
            rb.AddForce(thrustVector);
            fuel -= fuelLossRate * thrustInput * Time.fixedDeltaTime;
            fuel = Mathf.Clamp(fuel, 0f, maxFuel);
        }
        else if (thrustAudioSource.isPlaying)
        {
            StartCoroutine(FadeOutThrustSound());
        }
    }

    private IEnumerator FadeOutThrustSound()
    {
        float startVolume = thrustAudioSource.volume;

        for (float t = 0; t < 0.2f; t += Time.deltaTime)
        {
            thrustAudioSource.volume = Mathf.Lerp(startVolume, 0, t / 0.2f);
            yield return null;
        }

        thrustAudioSource.Stop();
        thrustAudioSource.volume = startVolume; // Reset volume for next use
    }

    private void HandleExitRocket()
    {
        if (player != null && exitRocketAction.action.WasPressedThisFrame())
        {
            InputManager.Instance.EnablePlayerControls();
            player.gameObject.SetActive(true);
            player.transform.position = doorTransform.position;
            AudioSource.PlayClipAtPoint(exitRocketSound, transform.position);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && hasFlown)
        {
            float landingVelocity = rb.linearVelocity.magnitude;
            float landingAngle = Mathf.Abs(rb.rotation % 360);

            if (landingAngle > 180) landingAngle = 360 - landingAngle;

            bool isPerfectLanding = landingVelocity <= perfectLandingVelocityThreshold &&
                                    landingAngle <= perfectLandingAngleThreshold &&
                                    flightTime >= 2f &&
                                    hasRotated;

            if (isPerfectLanding)
            {
                AudioSource.PlayClipAtPoint(perfectLandingSound, transform.position);
                StartCoroutine(ApplyPlayerSpeedBoost());
            }
            else
            {
                if (hasFlown)
                {
                    impulseSource.GenerateImpulse();
                    AudioSource.PlayClipAtPoint(landingSound, transform.position);
                }
            }

            StartCoroutine(DisableThrustTemporarily());
        }
    }

    private IEnumerator DisableThrustTemporarily()
    {
        canThrust = false;
        yield return new WaitForSeconds(0.5f);
        canThrust = true;
    }

    private IEnumerator ApplyPlayerSpeedBoost()
    {
        if (player != null)
        {
            player.SetSpeedMultiplier(speedBoostMultiplier);
            yield return new WaitForSeconds(speedBoostDuration);
            player.SetSpeedMultiplier(1f);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        AudioSource.PlayClipAtPoint(damageSound, transform.position);
        if (health <= 0)
        {
            Explode();
        }
    }
    
    private void Explode()
    {
        AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        // Add explosion effect here
        Destroy(gameObject);
    }
}
