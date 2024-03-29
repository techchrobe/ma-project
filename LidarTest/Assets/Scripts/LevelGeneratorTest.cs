using System.Collections.Generic;
using UnityEngine;

public class LevelGeneratorTest : MonoBehaviour
{
    [SerializeField] GameObject simplePlatform;
    [SerializeField] GameObject debugObj;
    [SerializeField] GameObject debugObj2;
    [SerializeField] GameObject goal;
    [SerializeField] float stepDistance = 0.2f;
    [SerializeField] float distanceToWall = 0.2f;
    [SerializeField] LayerMask mask;

    private float maxHeight = 1.2f;
    private float minHeight = 0;

    public GameObject startPosition;
    public GameObject endPosition;


    public enum Direction {
        Top = 0,
	    Right = 1,
	    Bottom = 2,
	    Left = 3,
	    TopLeft = 4,
	    TopRight = 5,
	    BottomLeft = 6,
	    BottomRight = 7
    };

    public Direction[] All = { Direction.Top, Direction.Right, Direction.Bottom, Direction.Left, Direction.TopLeft, Direction.TopRight, Direction.BottomLeft, Direction.BottomRight };


    public void Start()
    {
        Vector3 end = FindEndPosition(startPosition.transform.position);
        if(end == startPosition.transform.position)
            return;
        minHeight = startPosition.transform.position.y - 0.1f;
        NodeRecord goal = AStar(startPosition.transform.position, end);
        BuildPath(goal, startPosition.transform.position, end);
    }

    NodeRecord AStar(Vector3 startPosition, Vector3 endPosition) {
        List<NodeRecord> open = new List<NodeRecord>();
        List<NodeRecord> closed = new List<NodeRecord>();

        open.Add(new NodeRecord(new Node(startPosition, null), null, 0, Vector3.Distance(startPosition, endPosition)));
        NodeRecord current = open[0];
        while(open.Count != 0) {
            current = open[0];
            //Instantiate(debugObj, new Vector3(current.Node.Position.x, current.CostSoFar / 10, current.Node.Position.z), debugObj.transform.rotation);

            // if current node is close enough to the goal stop
            if(Vector3.Distance(current.Node.Position, endPosition) <= stepDistance + distanceToWall) {
                break;
            }

            // add neighbours
            foreach(Direction d in All) {
                float cost = 0.1f;

                Node neighbour = GetNeighbour(d, current.Node);
                if(IsNodeValid(neighbour)) {

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
                        endNodeHeuristic = Vector3.Distance(neighbour.Position, endPosition);
                    }

                    // update node record
                    record.CostSoFar = endNodeCost;
                    record.Connection = current;
                    record.EstimatedTotalCost = endNodeCost + endNodeHeuristic;

                    // reinsert to trigger sort
                    open.Add(record);
                    open.Sort();

                }
            }
            open.Remove(current);
            closed.Add(current);
        }

        return current;
    }

    void BuildPath(NodeRecord record, Vector3 startPosition, Vector3 endPosition) {
        // placce platforms on path
        Stack<Vector3> positions = new Stack<Vector3>();
        (bool[] walk, bool[] jump) = GenerateRhythm(record.EstimatedTotalCost/2);

        bool odd = true;

        while(record.Node.Position != startPosition) {
            if(odd) {
                positions.Push(record.Node.Position);
            }
            odd = !odd;
            record = record.Connection;
        }

        float yPos;
        float groundDistance;
        float ceilingDistance;
        int count = 0;
        int missedPlatforms = 0;

        // ignore first platform
        Vector3 lastPosition = positions.Peek();
        while(positions.Count > 0) {
            Vector3 platformPosition = positions.Pop();
            // when rhythm walk and jump don't place a platform
            if(count != 0 && missedPlatforms < 1 && !walk[count] && jump[count]) {
                missedPlatforms++;
                count++;
                continue;
            }

            // set y position
            yPos = Random.Range(0.05f, 0.2f) * (Random.Range(0, 2) == 0 ? 1 : -1);

            // when only walking and not jumping don't change y
            if(walk[count] && !jump[count]) {
                yPos = 0;
            }
            platformPosition.y = lastPosition.y + yPos;

            if(platformPosition.y > maxHeight) {
                platformPosition.y -= yPos;
            }
            else if (platformPosition.y < minHeight)
            {
                platformPosition.y += minHeight - platformPosition.y;
            }

            // Don't place to close to ground
            groundDistance = DistanceToGround(platformPosition);

            // Below ground
            if(groundDistance == float.MaxValue) {
                platformPosition.y = lastPosition.y + (yPos * -1);
                groundDistance = DistanceToGround(platformPosition);
            }

            if(groundDistance < 0.1f) {
                platformPosition.y = platformPosition.y - (groundDistance - 0.1f);
            }

            // Don't place to close to ceiling
            ceilingDistance = DistanceToCeiling(platformPosition);

            if(ceilingDistance < 0.7f) {
                platformPosition.y = platformPosition.y - (0.7f - ceilingDistance);
            }

            Instantiate(simplePlatform, platformPosition, simplePlatform.transform.rotation)/*transform.LookAt(lastPosition)*/;
            lastPosition = platformPosition;
            count++;
            missedPlatforms = 0;
        }
        // set y position
        yPos = Random.Range(-0.3f, 0.3f);
        endPosition.y = lastPosition.y + yPos;

        // Don't place to close to ground
        groundDistance = DistanceToGround(endPosition);

        // Below ground
        if(groundDistance == float.MaxValue) {
            endPosition.y = lastPosition.y + (yPos * -1);
            groundDistance = DistanceToGround(endPosition);
        }

        if(groundDistance < 0.1f) {
            endPosition.y = endPosition.y - (groundDistance - 0.1f);
        }

        // Don't place to close to ceiling
        ceilingDistance = DistanceToCeiling(endPosition);

        if(ceilingDistance < 0.7f) {
            endPosition.y = endPosition.y - (0.7f - ceilingDistance);
        }

        Instantiate(simplePlatform, endPosition, transform.rotation);
        Instantiate(goal, endPosition + new Vector3(0, 0.1f, 0), transform.rotation);
    }

    (bool[], bool[]) GenerateRhythm(float distance) {
        bool[] walk = new bool[(int)(distance * 10)];
        bool[] jump = new bool[(int)(distance * 10)];
        int walkLength = 0;
        int jumpLength = 0;

        bool newRange = false;
        int walkingRangeMin = -1;
        int walkingRangeMax = 4;
        int jumpingRangeMin = -2;
        int jumpinRangeMax = 3;
        for (int p = 0; p < walk.Length; p += 1) {
            if (!newRange && p > walk.Length / 2) {
                walkingRangeMin = -3;
                walkingRangeMax = 2;
                jumpingRangeMin = -1;
                jumpinRangeMax = 4;
                newRange = true;
            }
            int walking = Random.Range(walkingRangeMin, walkingRangeMax);
            if(walking > 0 && walkLength < 3) {
                walk[p] = true;
                walkLength++;
            }
            else {
                walkLength = 0;
            }

            int jumping = Random.Range(jumpingRangeMin, jumpinRangeMax);
            if(jumping > 0 && jumpLength < 4) {
                jump[p] = true;
                jumpLength++;
            }
            else {
                jumpLength = 0;
            }
        }
        return (walk, jump);
    }

    Vector3 FindEndPosition(Vector3 startPosition) {
        Vector3 endPosition = startPosition;
        // Do Flood fill and take last position as end position
        Queue<FloodFillNode> positions = new Queue<FloodFillNode>();
        List<Vector3> visited = new List<Vector3>();
        float maxDistance = 0;
        positions.Enqueue(new FloodFillNode(startPosition, maxDistance));
        while(positions.Count != 0) {
            FloodFillNode current = positions.Dequeue();

            RaycastHit hit;
            if(Physics.SphereCast(current.Position, 0.1f, Vector3.down, out hit) && !NodePositionIsInList(visited, current.Position)
                && !(Physics.SphereCast(current.Position, 0.05f, Vector3.left, out hit, distanceToWall, mask)
                    || Physics.SphereCast(current.Position, 0.05f, Vector3.right, out hit, distanceToWall, mask)
                    || Physics.SphereCast(current.Position, 0.05f, Vector3.forward, out hit, distanceToWall, mask)
                    || Physics.SphereCast(current.Position, 0.05f, Vector3.back, out hit, distanceToWall, mask))) {
                if (current.Cost > maxDistance) {
                    endPosition = current.Position;
                    maxDistance = current.Cost;
                }
                //    Instantiate(debugObj, new Vector3(current.Position.x, current.Cost/10, current.Position.z), debugObj.transform.rotation);

                positions.Enqueue(new FloodFillNode(current.Position + new Vector3(0, 0, 0.2f), current.Cost + 1));
                positions.Enqueue(new FloodFillNode(current.Position + new Vector3(0, 0, -0.2f), current.Cost + 1));
                positions.Enqueue(new FloodFillNode(current.Position + new Vector3(0.2f, 0, 0), current.Cost + 1));
                positions.Enqueue(new FloodFillNode(current.Position + new Vector3(-0.2f, 0, 0), current.Cost + 1));
                visited.Add(current.Position);
            }
        }
        Debug.Log(maxDistance);
        return endPosition;
    }

    bool NodePositionIsInList(List<Vector3> nodes, Vector3 item) {
        foreach (Vector3 n in nodes) {
            if(n == item) {
                return true;
            }
        }
        return false;
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
            case Direction.Right:
                return new Node(lastPosition.Position + new Vector3(stepDistance, 0, 0), lastPosition);
            case Direction.Bottom:
                return new Node(lastPosition.Position + new Vector3(0, 0, -stepDistance), lastPosition);
            case Direction.Left:
                return new Node(lastPosition.Position + new Vector3(-stepDistance, 0, 0), lastPosition);
        }
        return lastPosition;
    }


    bool IsNodeValid(Node n) {
        RaycastHit hit;
        if(Physics.SphereCast(n.Position, 0.1f, Vector3.down, out hit)) {

            // move platform a bit to the side if it's to close to a wall
            if(Physics.SphereCast(n.Position, 0.05f, Vector3.left, out hit, distanceToWall, mask)
                || Physics.SphereCast(n.Position, 0.05f, Vector3.right, out hit, distanceToWall, mask)
                || Physics.SphereCast(n.Position, 0.05f, Vector3.forward, out hit, distanceToWall, mask)
                || Physics.SphereCast(n.Position, 0.05f, Vector3.back, out hit, distanceToWall, mask)) {
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
        if (Physics.SphereCast(position, 0.05f, Vector3.up, out hit))
        {
            return hit.distance;
        }
        return float.MaxValue;
    }

    private float DistanceToWall(Vector3 position) {
        float distance = 0;
        float punishment = 2;
        RaycastHit hit;
        if(Physics.SphereCast(position, 0.05f, Vector3.left, out hit, distanceToWall, mask)) {
            distance += distanceToWall - hit.distance + punishment;
        }

        if(Physics.SphereCast(position, 0.05f, Vector3.right, out hit, distanceToWall, mask)) {
            distance += distanceToWall - hit.distance + punishment;
        }

        if(Physics.SphereCast(position, 0.05f, Vector3.forward, out hit, distanceToWall, mask)) {
            distance += distanceToWall - hit.distance + punishment;
        }

        if(Physics.SphereCast(position, 0.05f, Vector3.back, out hit, distanceToWall, mask)) {
            distance += distanceToWall - hit.distance + punishment;
        }
        return distance;
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
