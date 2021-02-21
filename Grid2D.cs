
using System.Collections.Generic;
using UnityEngine;
public class Grid2D<T>
{
    Dictionary<Vector2Int, HashSet<T>> grid = new Dictionary<Vector2Int, HashSet<T>>();
    Vector2Int[] neighbourOffsets =
    {
        Vector2Int.up,
        Vector2Int.up + Vector2Int.left,
        Vector2Int.up + Vector2Int.right,
        Vector2Int.left,
        Vector2Int.zero,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.down + Vector2Int.left,
        Vector2Int.down + Vector2Int.right
    };
    public void Remove(Vector2Int position, T value)
    {
        if (grid.TryGetValue(position, out HashSet<T> hashSet))
        {
            hashSet.Remove(value);
            if (hashSet.Count == 0)
                grid.Remove(position);
        }
    }
    public void Add(Vector2Int position, T value)
    {
        if (!grid.TryGetValue(position, out HashSet<T> hashSet))
        {
            hashSet = new HashSet<T>();
            grid[position] = hashSet;
        }
        hashSet.Add(value);
    }
    void GetAt(Vector2Int position, HashSet<T> result)
    {
        if (grid.TryGetValue(position, out HashSet<T> hashSet))
        {
            foreach (T entry in hashSet)
                result.Add(entry);
        }
    }
    public void GetWithNeighbours(Vector2Int position, HashSet<T> result)
    {
        foreach (Vector2Int offset in neighbourOffsets)
            GetAt(position + offset, result);
    }
}