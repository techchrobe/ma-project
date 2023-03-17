using UnityEngine;

public class Platform : MonoBehaviour
{
    private int collisionCount = 0;
    private Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Destroy platform if it intersects two other platforms because it isn't needed then
        if (collision.collider.tag == "Platform")
        {
            Vector3 otherPosition = collision.collider.gameObject.transform.position;
            Quaternion otherRotation = collision.collider.gameObject.transform.rotation;

            Vector3 direction;
            float distance;

            Physics.ComputePenetration(col, transform.position, transform.rotation,
                collision.collider, otherPosition, otherRotation, out direction, out distance);

            if (distance > 0.06)
            {
                collisionCount++;
                if (collisionCount == 2)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}
