using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RelEcs;

public sealed class TableEdge
{
    public Table Add;
    public Table Remove;
}

public sealed class Table
{
    const int StartCapacity = 4;

    public readonly int Id;

    public readonly SortedSet<StorageType> Types;

    public Identity[] Entities => _entities;
    public Array[] Storages => _storages;

    public int Count { get; private set; }
    public bool IsEmpty => Count == 0;

    readonly World _world;

    Identity[] _entities;
    readonly Array[] _storages;

    readonly Dictionary<StorageType, TableEdge> _edges = new();
    readonly Dictionary<StorageType, int> _indices = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Table(int id, World world, SortedSet<StorageType> types)
    {
        _world = world;

        Id = id;
        Types = types;

        _entities = new Identity[StartCapacity];

        var i = 0;
        foreach (var type in types)
        {
            if (type.IsTag) continue;
            _indices.Add(type, i++);
        }

        _storages = new Array[_indices.Count];

        foreach (var (type, index) in _indices)
        {
            _storages[index] = Array.CreateInstance(type.Type, StartCapacity);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Add(Identity identity)
    {
        EnsureCapacity(Count + 1);
        _entities[Count] = identity;
        return Count++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(int row)
    {
        if (row >= Count) throw new ArgumentOutOfRangeException(nameof(row), "row cannot be greater or equal to count");

        Count--;

        if (row < Count)
        {
            _entities[row] = _entities[Count];

            foreach (var storage in _storages)
            {
                Array.Copy(storage, Count, storage, row, 1);
            }

            _world.GetEntityMeta(_entities[row]).Row = row;
        }

        _entities[Count] = default;

        foreach (var storage in _storages)
        {
            Array.Clear(storage, Count, 1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TableEdge GetTableEdge(StorageType type)
    {
        if (_edges.TryGetValue(type, out var edge)) return edge;

        edge = new TableEdge();
        _edges[type] = edge;

        return edge;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] GetStorage<T>(Identity target)
    {
        var type = StorageType.Create<T>(target);
        return (T[])GetStorage(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Array GetStorage(StorageType type)
    {
        if (type.IsTag) throw new Exception("Cannot get Storage of Tag Component");
        return _storages[_indices[type]];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void EnsureCapacity(int capacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity), "minCapacity must be positive");
        if (capacity <= _entities.Length) return;

        Resize(Math.Max(capacity, StartCapacity) << 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Resize(int length)
    {
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length), "length cannot be negative");
        if (length < Count)
            throw new ArgumentOutOfRangeException(nameof(length), "length cannot be smaller than Count");

        Array.Resize(ref _entities, length);

        for (var i = 0; i < _storages.Length; i++)
        {
            var elementType = _storages[i].GetType().GetElementType()!;
            var newStorage = Array.CreateInstance(elementType, length);
            Array.Copy(_storages[i], newStorage, Math.Min(_storages[i].Length, length));
            _storages[i] = newStorage;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int MoveEntry(Identity identity, int oldRow, Table oldTable, Table newTable)
    {
        var newRow = newTable.Add(identity);

        foreach (var (type, oldIndex) in oldTable._indices)
        {
            if (!newTable._indices.TryGetValue(type, out var newIndex) || newIndex < 0) continue;

            var oldStorage = oldTable._storages[oldIndex];
            var newStorage = newTable._storages[newIndex];

            Array.Copy(oldStorage, oldRow, newStorage, newRow, 1);
        }

        oldTable.Remove(oldRow);

        return newRow;
    }
}