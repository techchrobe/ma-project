using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGeneratorTest : MonoBehaviour
{
    [SerializeField] GameObject simplePlatform;
    [SerializeField] GameObject debugObj;
    [SerializeField] GameObject goal;

    private List<GameObject> centers = new List<GameObject>();
    private float playerJumpHeight = 0.8f;

    public GameObject startPosition;
    public GameObject endPosition;

    public void Start()
    {
        // Place platforms between start and end position
        int platformCount = 0;
        Vector3 lastPlatformPosition = startPosition.transform.position;
        while (Vector3.Distance(lastPlatformPosition, endPosition.transform.position) > playerJumpHeight && platformCount < 20)
        {
            Vector3 dirToGoal = (endPosition.transform.position - lastPlatformPosition).normalized;

            float xPos = Random.Range(-playerJumpHeight, playerJumpHeight);
            float yPos = Random.Range(-playerJumpHeight/2, playerJumpHeight/2);
            float zPos = Random.Range(-playerJumpHeight, playerJumpHeight);

            if ((dirToGoal.x < 0 && xPos > 0)
                || (dirToGoal.x > 0 && xPos < 0))
            {
                xPos *= -1;
            }
            if ((dirToGoal.y < 0 && yPos > 0)
                || (dirToGoal.y > 0 && yPos < 0))
            {
                yPos *= -1;
            }
            if ((dirToGoal.z < 0 && zPos > 0)
                || (dirToGoal.z > 0 && zPos < 0))
            {
                zPos *= -1;
            }

            Vector3 newPlatformPosition = new Vector3(xPos, yPos, zPos) + lastPlatformPosition + (dirToGoal / 50);
            // Don't place to close to ceiling
            float boundDistance = DistanceToCeiling(newPlatformPosition);
            if (boundDistance < 0.7f)
            {
                newPlatformPosition.y = newPlatformPosition.y - (0.7f - boundDistance);
            }

            // Don't place to close to ground
            boundDistance = DistanceToGround(newPlatformPosition);
            if (boundDistance < 0.1f)
            {
                newPlatformPosition.y = newPlatformPosition.y - (boundDistance - 0.1f);
            }

            Instantiate(simplePlatform, newPlatformPosition, transform.rotation);
            lastPlatformPosition = newPlatformPosition;
            platformCount++;
        }
    }

    private float DistanceToGround(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.SphereCast(position, 0.1f, Vector3.down, out hit))
        {
            Debug.Log(hit.distance);
            Debug.Log(hit.collider.tag);
            return hit.distance;
        }
        return float.MaxValue;
    }

    private float DistanceToCeiling(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.SphereCast(position, 0.1f, Vector3.up, out hit))
        {
            Debug.Log(hit.distance);
            Debug.Log(hit.collider.tag);
            return hit.distance;
        }
        return float.MaxValue;
    }

    private GameObject GetFarthest(Vector3 startPoint, List<GameObject> gos)
    {
        GameObject farthest = gos[0];
        float maxDistance = 0;
        foreach (GameObject i in gos)
        {
            float distance = Vector3.Distance(startPoint, i.transform.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthest = i;
            }
        }
        return farthest;
    }
}
