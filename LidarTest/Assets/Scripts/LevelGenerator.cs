using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class LevelGenerator : MonoBehaviour
{
    private GameObject arCam;
    private ARMeshManager meshManager;
    [SerializeField] GameObject simplePlatform;
    [SerializeField] GameObject debugObj;
    [SerializeField] GameObject goal;
    [SerializeField] float stepDistance = 0.8f;
    [SerializeField] float distanceToWall = 0.2f;
    [SerializeField] LayerMask mask;

    private List<GameObject> centers = new List<GameObject>();
    private float playerJumpHeight = 0f;

    private Vector3 startPosition;
    private Vector3 endPosition;

    public Vector3 StartPosition { get => startPosition; }

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

    public void Init(GameObject cam, ARMeshManager manager, float jumpHeight)
    {
        arCam = cam;
        meshManager = manager;
        playerJumpHeight = jumpHeight/2;
    }

    public void GenerateLevel()
    {
        foreach (MeshFilter i in meshManager.meshes)
        {
            GameObject instObj = Instantiate(debugObj, i.mesh.bounds.center, transform.rotation);
            centers.Add(instObj);
            if (!GameManager.Instance.GetDebug()) instObj.GetComponent<MeshRenderer>().enabled = false;
        }

        Bounds bounds = new Bounds(centers[0].transform.position, Vector3.zero);
        for (int i = 1; i < centers.Count; i++)
        {
            bounds.Encapsulate(centers[i].transform.position);
        }
        //centers.Add(Instantiate(level[Random.Range(0, level.Count-1)], new Vector3(bounds.center.x, bounds.min.y, bounds.center.z), new Quaternion(0, arCam.transform.rotation.y, 0, arCam.transform.rotation.w)));

        // Place start position
        Vector3 position = arCam.transform.position + arCam.transform.forward;
        position.y = position.y - (DistanceToGround(position) - 0.1f);

        GameObject start = Instantiate(simplePlatform, position, transform.rotation);
        startPosition = start.transform.position + new Vector3(0, 0.2f, 0);

        // Place end postion
        endPosition = FindEndPosition(startPosition);

        Instantiate(simplePlatform, endPosition, transform.rotation);
        Instantiate(goal, endPosition + new Vector3(0, 0.1f, 0), transform.rotation);

        // Place platforms between start and end position
        // Find path between Start and End position

        NodeRecord goalNode = AStar(startPosition, endPosition);
        BuildPath(goalNode, startPosition);
    }

    NodeRecord AStar(Vector3 startPosition, Vector3 endPosition) {
        List<NodeRecord> open = new List<NodeRecord>();
        List<NodeRecord> closed = new List<NodeRecord>();

        open.Add(new NodeRecord(new Node(startPosition, null), null, 0, Vector3.Distance(startPosition, endPosition)));
        NodeRecord current = open[0];
        while(open.Count != 0) {
            current = open[0];
            Instantiate(debugObj, new Vector3(current.Node.Position.x, current.CostSoFar / 10, current.Node.Position.z), debugObj.transform.rotation);

            // if current node is close enough to the goal stop
            if(Vector3.Distance(current.Node.Position, endPosition) <= stepDistance + distanceToWall) {
                break;
            }

            // add neighbours
            foreach(Direction d in All) {
                float cost = 0.1f;
                //if((int)d >= 4) {
                //    cost = 0.1f;
                //}

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

    void BuildPath(NodeRecord record, Vector3 startPosition) {
        // placce platforms on path
        Vector3 lastPosition = startPosition;
        while(record.Node.Position != startPosition) {
            Vector3 platformPosition = record.Node.FixedPosition;

            // set y position
            float yPos = Random.Range(-0.3f, 0.3f);
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
            record = record.Connection;
        }
    }

    bool NodePositionIsInList(List<Vector3> nodes, Vector3 item) {
        foreach(Vector3 n in nodes) {
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
                if(current.Cost > maxDistance) {
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

    Node GetNeighbour(Direction direction, Node lastPosition) {
        switch(direction) {
            case Direction.Top:
                return new Node(lastPosition.Position + new Vector3(0, 0, stepDistance), lastPosition);
            case Direction.TopRight:
                return new Node(lastPosition.Position + new Vector3(stepDistance, 0, stepDistance), lastPosition);
            case Direction.Right:
                return new Node(lastPosition.Position + new Vector3(stepDistance, 0, 0), lastPosition);
            case Direction.BottomRight:
                return new Node(lastPosition.Position + new Vector3(stepDistance, 0, -stepDistance), lastPosition);
            case Direction.Bottom:
                return new Node(lastPosition.Position + new Vector3(0, 0, -stepDistance), lastPosition);
            case Direction.BottomLeft:
                return new Node(lastPosition.Position + new Vector3(-stepDistance, 0, -stepDistance), lastPosition);
            case Direction.Left:
                return new Node(lastPosition.Position + new Vector3(-stepDistance, 0, 0), lastPosition);
            case Direction.TopLeft:
                return new Node(lastPosition.Position + new Vector3(-stepDistance, 0, stepDistance), lastPosition);
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

    private float DistanceToGround(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.SphereCast(position, 0.1f, Vector3.down, out hit))
        {
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
        foreach(GameObject i in gos)
        {
            float distance = Vector3.Distance(startPoint, i.transform.position);
            if(distance > maxDistance)
            {
                maxDistance = distance;
                farthest = i;
            }
        }
        return farthest;
    }
}
