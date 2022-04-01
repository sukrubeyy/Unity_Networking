using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectileId = 1;
    public int id;
    public Rigidbody rb;
    public int byPlayer;
    public Vector3 initializeForce;
    public float expRadius = 1.5f;
    public float expDamage = 75f;

    private void Start()
    {
        id = nextProjectileId;
        nextProjectileId++;

        projectiles.Add(id, this);
        ServerSend.SpawnProjectile(this, byPlayer);
        rb.AddForce(initializeForce);

        StartCoroutine(ExploAfterTime());
    }

    public void Initialize(Vector3 _initializeMoveDirection, float _initializeForce, int _byPlayer)
    {
        initializeForce = _initializeMoveDirection * _initializeForce;
        byPlayer = _byPlayer;
    }
    private void FixedUpdate()
    {
        ServerSend.ProjectilePosition(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Eðer bir objeye çarptýysa patlama gerçekleþsin. Player objesi olmasa dahil.
        Exp();
    }

    /// <summary>
    /// Atýlan Merminin expRadius çapýnda bir obje varsa ve bu obje player tag'ýna eþitse expDamage kadar Player.TakeDamage() hasar gönder.
    /// </summary>
    private void Exp()
    {

        ServerSend.ProjectileExp(this);

        Collider[] colliders = Physics.OverlapSphere(transform.position, expRadius);

        foreach (Collider item in colliders)
        {
            if (item.CompareTag("Player"))
            {
                item.GetComponent<Player>().TakeDamage(expDamage);
            }
        }
        projectiles.Remove(id);
        Destroy(gameObject);
    }
    /// <summary>
    /// Projectile Atýldýktan 5 saniye sonra patlama iþlemini gerçekleþtirir.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ExploAfterTime()
    {
        yield return new WaitForSeconds(5f);
        Exp();
    }
}
