namespace Forge.Sync;

/// <summary>Defines the safe structural phase order for applying a topology transition.</summary>
public enum TopologyPhase
{
    RemoveEdges,
    RemoveNodes,
    AddNodes,
    UpdateNodes,
    UpdateEdges,
    AddEdges
}
