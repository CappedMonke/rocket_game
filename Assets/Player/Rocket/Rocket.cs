using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float thrustAcceleration = 1f;
    [SerializeField] private float maxSpeed = 1f;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float minThrustStrength = 1f;
    [SerializeField] private float thrustAdjustmentSpeed = 1f;
    [SerializeField] private float liftoffCooldown = 1f;

    [Header("Stats")]
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int fuel = 100;
    [SerializeField] private int maxFuel = 100;
    [SerializeField] private int oxygen = 100;
    [SerializeField] private int maxOxygen = 100;

    [Header("Collision Settings")]
    [SerializeField] private float impactForceDamageMultiplier = 1f;
    [SerializeField] private float impactAngleDamageMultiplier = 1f;
    [SerializeField] private float perfectLandingMaxAngle = 1f;
    [SerializeField] private float safeLandingMaxAngle = 1f;
    [SerializeField] private float safeLandingMaxImpactForce = 1f;

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

    private CinemachineImpulseSource impulseSource;


    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource flyingSoundSource;
    private Rigidbody2D rb;
    private Astronaut astronaut;
    private Controls controls;
    private bool isFlying = false;
    private bool canFly = true;
    private bool canRotate = true;
    private bool isThrusting = false;
    private bool wasThrusting = false;
    private float thrustStrength = 1f;
    private float thrustInput = 0f;
    private float rotateInput = 0f;
    private Vector2 landingNormal;

    private Coroutine flyingSoundFadeCoroutine;
    private float flyingSoundDefaultVolume = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        astronaut = FindFirstObjectByType<Astronaut>();

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
    }

    private void OnEnable()
    {
        controls.Rocket.Enable();
    }

    private void OnDisable()
    {
        controls.Rocket.Disable();
    }

    private void Update()
    {
        if (thrustInput != 0f)
        {
            thrustStrength += thrustInput * Time.deltaTime * thrustAdjustmentSpeed;
            thrustStrength = Mathf.Clamp(thrustStrength, minThrustStrength, 1f);
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

    private void Thrust()
    {
        Vector2 thrustForce = thrustAcceleration * thrustStrength * Time.fixedDeltaTime * transform.up;
        rb.AddForce(thrustForce);

        var thrustEffectMain = ThrustEffect.main;
        float thrustEffectLifetime = Mathf.Lerp(0, initialThrustEffectLifetime, thrustStrength);
        thrustEffectMain.startLifetime = thrustEffectLifetime;

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);

        isFlying = true;
        canRotate = true;
    }

    private void Rotate(float v)
    {
        float rotationAmount = v * rotationSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation + rotationAmount);
    }

    private void ExitRocket()
    {
        audioSource.PlayOneShot(ExitRocketSound);

        if (astronaut != null)
        {
            controls.Rocket.Disable();
            astronaut.OnExitRocket();
        }
    }

    public void OnEnterRocket()
    {
        audioSource.PlayOneShot(EnterRocketSound);

        controls.Rocket.Enable();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFlying)
        {
            return;
        }

        float impactForce = collision.relativeVelocity.magnitude;

        landingNormal = collision.contacts[0].normal;

        float angle = Vector2.Angle(landingNormal, transform.up);

        bool impactForceTooHigh = impactForce > safeLandingMaxImpactForce;

        if (angle < perfectLandingMaxAngle && !impactForceTooHigh && isFlying) // Perfect landing
        {
            impulseSource.GenerateImpulse(landingShakeStrength);
            audioSource.PlayOneShot(PerfectLandingSound);
            StartCoroutine(DisableAndEnableFlight(liftoffCooldown));
        }
        else if (angle < safeLandingMaxAngle && !impactForceTooHigh && isFlying) // Safe landing
        {
            impulseSource.GenerateImpulse(landingShakeStrength);
            audioSource.PlayOneShot(LandingSound);
            StartCoroutine(DisableAndEnableFlight(liftoffCooldown));
        }
        else // Crash
        {
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
        health -= damage;
        if (health <= 0)
        {
            controls.Rocket.Disable();
            audioSource.PlayOneShot(ExplosionSound);
            impulseSource.GenerateImpulse(explosionShakeStrength * damage * damageAmountShakeMultiplier);
        }
        else
        {
            impulseSource.GenerateImpulse(damageShakeStrength * damage * damageAmountShakeMultiplier);
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
}
