using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class Rocket : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private bool startWithControlsEnabled = true;

    [Header("Win Conditions")]
    [SerializeField] private float winAltitude = 1500f;

    [Header("Movement")]
    [SerializeField] private float thrustAcceleration = 1f;
    [SerializeField] private float maxSpeed = 1f;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float minThrustStrength = 1f;
    [SerializeField] private float thrustStrength = 1f;
    [SerializeField] private float thrustAdjustmentSpeed = 1f;
    [SerializeField] private float liftoffCooldown = 1f;
    [SerializeField] private float rotationTimeoutAfterLiftoff = 1f;
    private const float maxThrustStrength = 1f;

    [Header("Stats")]
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float fuel = 100;
    [SerializeField] private float maxFuel = 100;
    [SerializeField] private float fuelDepletionRate = 1f;
    [SerializeField] private float oxygen = 100;
    [SerializeField] private float maxOxygen = 100;
    [SerializeField] private float playerOxygenRefillSpeed = 1f;

    [Header("Collision Settings")]
    [SerializeField] private float impactForceDamageMultiplier = 1f;
    [SerializeField] private float impactAngleDamageMultiplier = 1f;
    [SerializeField] private float perfectLandingMaxAngle = 1f;
    [SerializeField] private float safeLandingMaxAngle = 1f;
    [SerializeField] private float safeLandingMaxImpactForce = 1f;
    [SerializeField] private Vector2 explosionStrength = new(1f, 1f);

    [Header("Sounds")]
    [SerializeField] private AudioClip ExitRocketSound;
    [SerializeField] private AudioClip EnterRocketSound;
    [SerializeField] private AudioClip LandingSound;
    [SerializeField] private AudioClip PerfectLandingSound;
    [SerializeField] private AudioClip DamageSound;
    [SerializeField] private AudioClip ExplosionSound;
    [SerializeField] private AudioClip DamagedEngineSound;
    [SerializeField] private AudioClip RepairSound;
    [SerializeField] private AudioClip DustSound;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem ThrustEffect;
    private float initialThrustEffectLifetime;
    [SerializeField] private ParticleSystem DustFrontEffect;
    [SerializeField] private ParticleSystem DustBackEffect;
    [SerializeField] private ParticleSystem LandingEffect;
    [SerializeField] private ParticleSystem ExplosionEffect;
    [SerializeField] private ParticleSystem DamageEffect;
    [SerializeField] private ParticleSystem RepairEffect;
    [SerializeField] private ParticleSystem EngineDamageEffect;
    [SerializeField] private ParticleSystem EngineRepairEffect;

    [Header("Camera")]
    [SerializeField] private float landingShakeStrength = 1f;
    [SerializeField] private float damageShakeStrength = 1f;
    [SerializeField] private float explosionShakeStrength = 1f;
    [SerializeField] private float damageAmountShakeMultiplier = 1f;

    [Header("Events")]
    public UnityEvent OnRocketDestroyed;

    [Header("References")]
    private CinemachineImpulseSource impulseSource;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource flyingSoundSource;
    [SerializeField] private GameObject doorPosition;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject Sprite;
    [SerializeField] private GameObject RocketExplosionPrefab;
    private HUD hud;
    private Rigidbody2D rb;
    private Astronaut astronaut;
    private Controls controls;
    private CameraManager cameraManager;

    private bool isFlying = true;
    private bool canFly = true;
    private bool canRotate = false;
    private bool isThrusting = false;
    private bool wasThrusting = false;
    private float thrustInput = 0f;
    private float rotateInput = 0f;
    private Vector2 landingNormal;

    private Coroutine flyingSoundFadeCoroutine;
    private float flyingSoundDefaultVolume = 1f;

    private bool isAstronautInside = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        astronaut = FindFirstObjectByType<Astronaut>();
        cameraManager = FindFirstObjectByType<CameraManager>();

        initialThrustEffectLifetime = ThrustEffect.main.startLifetime.constant;

        flyingSoundDefaultVolume = audioSource.volume;

        controls = new Controls();
        controls.Rocket.ExitRocket.performed += ctx => ExitRocket();
        controls.Rocket.Thrust.started += ctx => { isThrusting = true; StopFlyingSoundFadeout(); };
        controls.Rocket.Thrust.canceled += ctx => isThrusting = false;
        controls.Rocket.AdjustThrust.started += ctx => thrustInput = ctx.ReadValue<float>();
        controls.Rocket.AdjustThrust.canceled += ctx => thrustInput = 0f;
        controls.Rocket.Rotate.started += ctx => rotateInput = ctx.ReadValue<float>();
        controls.Rocket.Rotate.canceled += ctx => rotateInput = 0f;

        if (startWithControlsEnabled)
        {
            controls.Rocket.Enable();

            if (astronaut != null)
            {
                isAstronautInside = true;
            }

            if (cameraManager != null)
            {
                cameraManager.SwitchToRocketCam();
            }
        }
        else
        {
            controls.Rocket.Disable();
        }
    }

    private void Start()
    {

        astronaut.gameObject.SetActive(false);
        hud = FindFirstObjectByType<HUD>();

        if (hud != null && startWithControlsEnabled)
        {
            hud.EnableRocketUI();
        }

        InitializeHUD();
    }

    private void OnEnable()
    {
        InitializeHUD();
    }

    private void InitializeHUD()
    {
        if (hud != null)
        {
            hud.rocketUI.SetHealth(health, maxHealth);
            hud.rocketUI.SetFuel(fuel, maxFuel);
            hud.rocketUI.SetOxygen(oxygen, maxOxygen);
            hud.rocketUI.SetThrustStrength(thrustStrength, minThrustStrength, maxThrustStrength);
            hud.rocketUI.SetAltitude(transform.position.y, winAltitude);
        }
    }

    private void Update()
    {
        if (thrustInput != 0f)
        {
            float newThrustStrength = thrustStrength + thrustInput * Time.deltaTime * thrustAdjustmentSpeed;
            SetThrustStrength(newThrustStrength);
        }

        if (isFlying)
        {
            SetAltitude(transform.position.y);
        }

        if (isThrusting && canFly)
        {
            SetFuel(fuel - fuelDepletionRate * thrustStrength * Time.deltaTime);

            if (fuel <= 0)
            {
                isThrusting = false;
            }
        }

        if (isAstronautInside && astronaut != null)
        {
            float oxygenRefilled = playerOxygenRefillSpeed * Time.deltaTime;
            float oxygenTransferred = Mathf.Min(oxygenRefilled, oxygen);

            float astronautOxygenDepletion = astronaut.GetOxygenDepletionRate() * Time.deltaTime;

            if (oxygen > 0)
            {
                if (astronaut.IsOxygenFull())
                {
                    SetOxygen(oxygen - astronautOxygenDepletion);
                }
                else
                {
                    SetOxygen(oxygen - oxygenTransferred);
                    astronaut.RefillOxygen(oxygenTransferred);
                }
            }
            else
            {
                astronaut.RefillOxygen(-astronautOxygenDepletion);
            }
        }
    }

    private void FixedUpdate()
    {
        if (canFly)
        {
            if (isThrusting)
            {
                Thrust();
                if (!ThrustEffect.isPlaying)
                {
                    ThrustEffect.Play();
                }
            }
            else if (ThrustEffect.isPlaying)
            {
                ThrustEffect.Stop();
            }

            if (canRotate && rotateInput != 0f)
            {
                Rotate(rotateInput);
            }

            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
        }

        if (canFly && isThrusting && !wasThrusting)
        {
            StopFlyingSoundFadeout();
            flyingSoundSource.loop = true;
            flyingSoundSource.volume = flyingSoundDefaultVolume * thrustStrength;
            flyingSoundSource.Play();
        }
        else if (!isThrusting && wasThrusting)
        {
            if (flyingSoundSource.isPlaying)
            {
                StartFlyingSoundFadeout();
            }
        }

        if (isThrusting)
        {
            flyingSoundSource.volume = flyingSoundDefaultVolume * thrustStrength;
        }

        wasThrusting = isThrusting;
    }

    private IEnumerator DisableRotationTemporarily(float delay)
    {
        canRotate = false;
        yield return new WaitForSeconds(delay);
        canRotate = true;
    }

    private void Thrust()
    {
        if (fuel > 0)
        {
            Vector2 thrustForce = thrustAcceleration * thrustStrength * Time.fixedDeltaTime * transform.up;
            rb.AddForce(thrustForce);

            var thrustEffectMain = ThrustEffect.main;
            float thrustEffectLifetime = Mathf.Lerp(0, initialThrustEffectLifetime, thrustStrength);
            thrustEffectMain.startLifetime = thrustEffectLifetime;

            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);

            isFlying = true;
        }

        if (!canRotate)
        {
            StartCoroutine(DisableRotationTemporarily(rotationTimeoutAfterLiftoff));
        }
    }

    private void Rotate(float v)
    {
        if (fuel <= 0)
        {
            return;
        }

        float rotationAmount = v * rotationSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation + rotationAmount);
    }

    public void OnEnterRocket()
    {
        audioSource.PlayOneShot(EnterRocketSound);

        controls.Rocket.Enable();

        if (hud != null)
        {
            hud.EnableRocketUI();
        }

        InitializeHUD();
        isAstronautInside = true;
    }

    private void ExitRocket()
    {
        if (isFlying)
        {
            return;
        }

        if (hud != null)
        {
            hud.EnableAstronautUI();
        }

        audioSource.PlayOneShot(ExitRocketSound);

        if (astronaut != null)
        {
            controls.Rocket.Disable();
            astronaut.OnExitRocket(doorPosition.transform.position);

            if (cameraManager != null)
            {
                cameraManager.SwitchToAstronautCam();
            }
        }

        isAstronautInside = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFlying || health <= 0)
        {
            return;
        }

        float impactForce = collision.relativeVelocity.magnitude;

        landingNormal = collision.contacts[0].normal;

        float angle = Vector2.Angle(landingNormal, transform.up);

        bool impactForceTooHigh = impactForce > safeLandingMaxImpactForce;

        if (angle < perfectLandingMaxAngle && !impactForceTooHigh && isFlying) // Perfect landing
        {
            if (astronaut != null)
            {
                astronaut.ApplyPerfectLandingBoost();
            }
            Vector3 shakeDirection = new Vector3(Random.Range(-0.1f, 0.1f), 1f, Random.Range(-0.1f, 0.1f)).normalized;
            impulseSource.GenerateImpulse(landingShakeStrength * shakeDirection);
            audioSource.PlayOneShot(PerfectLandingSound);
            StartCoroutine(DisableAndEnableFlight(liftoffCooldown));
        }
        else if (angle < safeLandingMaxAngle && !impactForceTooHigh && isFlying) // Safe landing
        {
            Vector3 shakeDirection = new Vector3(Random.Range(-0.1f, 0.1f), 1f, Random.Range(-0.1f, 0.1f)).normalized;
            impulseSource.GenerateImpulse(landingShakeStrength * shakeDirection);
            audioSource.PlayOneShot(LandingSound);
            StartCoroutine(DisableAndEnableFlight(liftoffCooldown));
        }
        else if (angle < safeLandingMaxAngle && impactForceTooHigh && isFlying) // Crash landing
        {
            DamageEffect.transform.position = collision.contacts[0].point;
            DamageEffect.Play();
            audioSource.PlayOneShot(DamageSound);
            float forceDamage = impactForce * impactForceDamageMultiplier;
            float angleDamage = angle * impactAngleDamageMultiplier;
            int totalDamage = Mathf.RoundToInt(forceDamage + angleDamage);
            TakeDamage(totalDamage);
            StartCoroutine(DisableAndEnableFlight(liftoffCooldown));
        }
        else // Crash
        {
            DamageEffect.transform.position = collision.contacts[0].point;
            DamageEffect.Play();
            audioSource.PlayOneShot(DamageSound);
            float forceDamage = impactForce * impactForceDamageMultiplier;
            float angleDamage = angle * impactAngleDamageMultiplier;
            int totalDamage = Mathf.RoundToInt(forceDamage + angleDamage);
            TakeDamage(totalDamage);
        }
    }

    private IEnumerator DisableAndEnableFlight(float delay)
    {
        isFlying = false;
        canFly = false;
        canRotate = false;
        ThrustEffect.Stop();

        StartFlyingSoundFadeout();

        yield return new WaitForSeconds(delay);

        canFly = true;

        if (isThrusting)
        {
            isFlying = true;
            StopFlyingSoundFadeout();
            flyingSoundSource.loop = true;
            flyingSoundSource.volume = flyingSoundDefaultVolume;
            flyingSoundSource.Play();
        }
    }

    private void TakeDamage(int damage)
    {
        SetHealth(health - damage);

        if (health <= 0)
        {
            ExplosionEffect.Play();
            controls.Rocket.Disable();
            audioSource.PlayOneShot(ExplosionSound);
            impulseSource.GenerateImpulse(explosionShakeStrength * damage * damageAmountShakeMultiplier * Random.onUnitSphere);
            OnRocketDestroyed.Invoke();
            GameObject rocketExplosionInstance = Instantiate(RocketExplosionPrefab, transform.position, transform.rotation);
            rocketExplosionInstance.transform.localScale = transform.localScale;

            foreach (Transform child in rocketExplosionInstance.transform)
            {
                Rigidbody2D rb2d = child.GetComponent<Rigidbody2D>();
                Vector2 vChildParent = child.position - transform.position;
                if (rb2d != null)
                {
                    rb2d.AddForce(Random.Range(explosionStrength.x, explosionStrength.y) * rb.linearVelocity.magnitude * vChildParent.normalized, ForceMode2D.Impulse);
                }
            }
            Sprite.SetActive(false);
            rb.linearVelocity = Vector2.zero;
            hud.gameEndScreen.ShowGameEndScreen("Game Over! Rocket Destroyed!");
        }
        else
        {
            impulseSource.GenerateImpulse(damageShakeStrength * damage * damageAmountShakeMultiplier * Random.onUnitSphere);
        }
    }

    private void StartFlyingSoundFadeout()
    {
        StopFlyingSoundFadeout();
        flyingSoundFadeCoroutine = StartCoroutine(FadeOutFlyingSound(0.1f));
    }

    private void StopFlyingSoundFadeout()
    {
        if (flyingSoundFadeCoroutine != null)
        {
            StopCoroutine(flyingSoundFadeCoroutine);
            flyingSoundFadeCoroutine = null;
            flyingSoundSource.volume = flyingSoundDefaultVolume;
        }
    }

    private IEnumerator FadeOutFlyingSound(float duration)
    {
        float startVolume = flyingSoundSource.volume;
        float time = 0f;
        while (time < duration)
        {
            flyingSoundSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        flyingSoundSource.volume = 0f;
        flyingSoundSource.Stop();
        flyingSoundSource.loop = false;
        flyingSoundFadeCoroutine = null;
    }

    public void EnableOutline()
    {
        spriteRenderer.material.SetFloat("_OutlineEnabled", 1f);
    }

    public void DisableOutline()
    {
        spriteRenderer.material.SetFloat("_OutlineEnabled", 0f);
    }

    public Controls GetControls()
    {
        return controls;
    }

    public void ApplyStats(UpgradeBuff buff)
    {
        if (buff == null) return;

        SetMaxFuel(Mathf.Max(maxFuel, buff.maxFuelRocket));
        SetMaxOxygen(Mathf.Max(maxOxygen, buff.maxOxygenRocket));

        thrustAcceleration = Mathf.Max(thrustAcceleration, buff.accelerationRocket);

        maxSpeed = Mathf.Max(maxSpeed, buff.maxSpeedRocket);
    }

    public void RefillFuel(float amount)
    {
        SetFuel(fuel + amount);
    }

    public void RefillOxygen(float amount)
    {
        SetOxygen(oxygen + amount);
    }

    private void SetHealth(int value)
    {
        health = Mathf.Clamp(value, 0, maxHealth);
        if (hud != null)
        {
            hud.rocketUI.SetHealth(health, maxHealth);
        }
    }

    private void SetMaxHealth(int value)
    {
        maxHealth = Mathf.Max(value, 0);
        if (hud != null)
        {
            hud.rocketUI.SetHealth(health, maxHealth);
        }
    }

    private void SetFuel(float value)
    {
        fuel = Mathf.Clamp(value, 0, maxFuel);
        if (hud != null)
        {
            hud.rocketUI.SetFuel(fuel, maxFuel);
        }
    }

    private void SetMaxFuel(float value)
    {
        maxFuel = Mathf.Max(value, 0);
        if (hud != null)
        {
            hud.rocketUI.SetFuel(fuel, maxFuel);
        }
    }

    private void SetOxygen(float value)
    {
        oxygen = Mathf.Clamp(value, 0, maxOxygen);
        if (hud != null)
        {
            hud.rocketUI.SetOxygen(oxygen, maxOxygen);
        }
    }

    private void SetMaxOxygen(float value)
    {
        maxOxygen = Mathf.Max(value, 0);
        if (hud != null)
        {
            hud.rocketUI.SetOxygen(oxygen, maxOxygen);
        }
    }

    private void SetThrustStrength(float value)
    {
        thrustStrength = Mathf.Clamp(value, minThrustStrength, maxThrustStrength);
        if (hud != null)
        {
            hud.rocketUI.SetThrustStrength(thrustStrength, minThrustStrength, maxThrustStrength);
        }
    }

    private void SetAltitude(float value)
    {
        if (hud != null)
        {
            hud.rocketUI.SetAltitude(Mathf.Clamp(value, 0, winAltitude), winAltitude);
        }

        if (value >= winAltitude)
        {
            hud.gameEndScreen.ShowGameEndScreen("Game won! You reached the win altitude!");
        }
    }
}
