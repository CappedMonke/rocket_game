using DG.Tweening;
using UnityEngine;

public class RocketHPCollider : MonoBehaviour
{
    public Rocket rocket;

    void OnCollisionEnter2D(Collision2D collision)
    {
        rocket.TakeDamage(10);
        rocket.impulseSource.GenerateImpulse(5f);
    }
}
