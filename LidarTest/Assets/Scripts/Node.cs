using System;
using UnityEngine;

public class Node {
    Vector3 position;
    Node previousNode;
    bool visited;

    public Node(Vector3 position, Node previousNode) {
        this.position = position;
        this.previousNode = previousNode;
    }

    public Vector3 Position { get => position; set => position = value; }
    public Node getPreviousNode() { return previousNode; }
}

public class NodeRecord : IComparable<NodeRecord> {
    Node node;
    NodeRecord connection;
    float costSoFar;
    float estimatedTotalCost;

    public NodeRecord(Node node, NodeRecord connection, float costSoFar, float estimatedTotalCost) {
        this.node = node;
        this.connection = connection;
        this.costSoFar = costSoFar;
        this.estimatedTotalCost = estimatedTotalCost;
    }

    public Node Node { get => node; set => node = value; }
    public NodeRecord Connection { get => connection; set => connection = value; }
    public float CostSoFar { get => costSoFar; set => costSoFar = value; }
    public float EstimatedTotalCost { get => estimatedTotalCost; set => estimatedTotalCost = value; }

    public int CompareTo(NodeRecord obj) {
        if(estimatedTotalCost == obj.EstimatedTotalCost)
            return 0;
        else if(estimatedTotalCost > obj.EstimatedTotalCost)
            return 1;
        return -1;
    }
}
