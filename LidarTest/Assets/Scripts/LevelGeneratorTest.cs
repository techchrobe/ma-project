using System.Collections.Generic;
using UnityEngine;

public class LevelGeneratorTest : MonoBehaviour
{
    [SerializeField] GameObject simplePlatform;
    [SerializeField] GameObject debugObj;
    [SerializeField] GameObject goal;
    [SerializeField] float stepDistance = 0.8f;
    [SerializeField] float distanceToWall = 0.2f;


    private List<GameObject> centers = new List<GameObject>();
    private float playerJumpHeight = 0.4f;

    public GameObject startPosition;
    public GameObject endPosition;


    enum Direction {
        Top = 0,
	    Right = 1,
	    Bottom = 2,
	    Left = 3,
	    TopLeft = 4,
	    TopRight = 5,
	    BottomLeft = 6,
	    BottomRight = 7
    };

    Direction[] All = { Direction.Top, Direction.Right, Direction.Bottom, Direction.Left, Direction.TopLeft, Direction.TopRight, Direction.BottomLeft, Direction.BottomRight };

    public void Start()
    {
        //Vector3 end = FindEndPosition(startPosition.transform.position);
        Vector3 end = endPosition.transform.position;

        // Find path between Start and End position
        List<NodeRecord> open = new List<NodeRecord>();
        List<NodeRecord> closed = new List<NodeRecord>();

        open.Add(new NodeRecord(new Node(startPosition.transform.position, null), null, 0, Vector3.Distance(startPosition.transform.position, end)));
        NodeRecord current = open[0];
        while (open.Count != 0)
        {
            current = open[0];

            // if current node is close enough to the goal stop
            if (Vector3.Distance(current.Node.Position, end) <= stepDistance) {
                break;
            }

            // add neighbours
            foreach(Direction d in All) {
                int cost = 1;
                if((int)d >= 4) {
                    cost = 2;
                }

                Node neighbour = GetNeighbour(d, current.Node);
                if (IsNodeValid(neighbour)) {

                    NodeRecord isInClosed = QueueContainsNode(closed, neighbour);
                    NodeRecord isInOpen = QueueContainsNode(open, neighbour);

                    float endNodeCost = current.CostSoFar + cost;
                    float endNodeHeuristic = 0.0f;

                    NodeRecord record;
                    if(isInClosed != null) {
                        record = isInClosed;
                        if(record.CostSoFar <= endNodeCost)
                            continue;

                        closed.Remove(record);
                        endNodeHeuristic = record.EstimatedTotalCost - record.CostSoFar;
                    }
                    else if(isInOpen != null) {
                        record = isInOpen;
                        if(record.CostSoFar <= endNodeCost)
                            continue;
                        endNodeHeuristic = record.EstimatedTotalCost - record.CostSoFar;
                    }
                    else {
                        record = new NodeRecord(neighbour, current, endNodeCost, endNodeHeuristic);
                        endNodeHeuristic = Vector3.Distance(neighbour.Position, endPosition.transform.position);
                    }

                    // update node record
                    record.CostSoFar = endNodeCost;
                    record.Connection = current;
                    record.EstimatedTotalCost = endNodeCost + endNodeHeuristic;

                    // reinsert to trigger sort
                    open.Add(record);
                    open.Sort();

                }
                open.Remove(current);
                closed.Add(current);
            }
        }

        // placce platforms on path
        Vector3 lastPosition = startPosition.transform.position;
        while(current.Node.Position != startPosition.transform.position) {
            Vector3 platformPosition = current.Node.Position;

            // set y position
            float yPos = Random.Range(-0.4f, 0.4f);
            platformPosition.y = lastPosition.y + yPos;

            float groundDistance = DistanceToGround(platformPosition);

            if(groundDistance == float.MaxValue) {
                platformPosition.y = lastPosition.y + (yPos * -1);
                groundDistance = DistanceToGround(platformPosition);
            }
            // Don't place to close to ground
            if(groundDistance < 0.1f) {
                platformPosition.y = platformPosition.y - (groundDistance - 0.1f);
            }

            Instantiate(simplePlatform, platformPosition, transform.rotation);
            lastPosition = platformPosition;
            current = current.Connection;
        }
    }

    Vector3 FindEndPosition(Vector3 startPosition) {
        Vector3 endPosition = startPosition;
        // ToDo
        return endPosition;
    }

    NodeRecord QueueContainsNode(List<NodeRecord> list, Node n) {
        foreach(NodeRecord nr in list) {
            if(nr.Node.Position == n.Position)
                return nr;
        }
        return null;
    }

    Node GetNeighbour(Direction direction, Node lastPosition) {
        switch(direction) {
            case Direction.Top:
                return new Node(lastPosition.Position + new Vector3(0, 0, stepDistance), lastPosition);
            case Direction.TopRight:
                return new Node(lastPosition.Position + new Vector3(stepDistance/2, 0, stepDistance / 2), lastPosition);
            case Direction.Right:
                return new Node(lastPosition.Position + new Vector3(stepDistance, 0, 0), lastPosition);
            case Direction.BottomRight:
                return new Node(lastPosition.Position + new Vector3(stepDistance / 2, 0, -stepDistance / 2), lastPosition);
            case Direction.Bottom:
                return new Node(lastPosition.Position + new Vector3(0, 0, -stepDistance), lastPosition);
            case Direction.BottomLeft:
                return new Node(lastPosition.Position + new Vector3(-stepDistance / 2, 0, -stepDistance / 2), lastPosition);
            case Direction.Left:
                return new Node(lastPosition.Position + new Vector3(-stepDistance, 0, 0), lastPosition);
            case Direction.TopLeft:
                return new Node(lastPosition.Position + new Vector3(-stepDistance / 2, 0, stepDistance / 2), lastPosition);
        }
        return lastPosition;
    }


    bool IsNodeValid(Node n) {
        RaycastHit hit;
        if(Physics.SphereCast(n.Position, 0.1f, Vector3.down, out hit)) {

            // move platform a bit to the side if it's to close to a wall
            if(Physics.Raycast(n.Position, Vector3.left, out hit, distanceToWall)) {
                n.Position += new Vector3(distanceToWall - hit.distance, 0, 0);
            }

            if(Physics.Raycast(n.Position, Vector3.right, out hit, distanceToWall)) {
                n.Position -= new Vector3(distanceToWall - hit.distance, 0, 0);
            }

            if(Physics.Raycast(n.Position, Vector3.forward, out hit, distanceToWall)) {
                n.Position -= new Vector3(0, 0, distanceToWall - hit.distance);
            }

            if(Physics.Raycast(n.Position, Vector3.back, out hit, distanceToWall)) {
                n.Position += new Vector3(0, 0, distanceToWall - hit.distance);
            }

            if(!Physics.SphereCast(n.Position, 0.1f, Vector3.down, out hit)) {
                return false;
            }
            return true;
        }
        return false;
    }

    private float DistanceToGround(Vector3 position) {
        RaycastHit hit;
        if(Physics.SphereCast(position, 0.1f, Vector3.down, out hit)) {
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
