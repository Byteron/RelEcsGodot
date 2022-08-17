using System.Collections.Generic;
using Godot;

namespace RelEcs;

public class Root
{
    public Node Node;
}

public interface ISpawnable
{
    void Spawn(EntityBuilder entityBuilder);
}

public static class WorldExtensions
{
    public static void SpawnRecursively(this World world, Node node)
    {
        world.Spawn(node);

        foreach (Node child in node.GetChildren())
        {
            if (child.GetChildCount() == 0) continue;

            world.SpawnRecursively(child);
        }
    }

    public static Entity Spawn(this World world, Node root)
    {
        var entity = world.Spawn();
        world.AttachNode(entity, root);
        return entity;
    }

    public static void AttachNode(this World world, Entity entity, Node root)
    {
        world.AddComponent(entity.Identity, new Root { Node = root });
        root.SetMeta("Entity", entity);

        var nodes = new List<Node> { root };

        foreach (Node child in root.GetChildren())
        {
            nodes.Add(child);
        }

        foreach (var node in nodes)
        {
            var addMethod = typeof(WorldExtensions).GetMethod("AddNodeComponent");
            var addChildMethod = addMethod?.MakeGenericMethod(node.GetType());
            addChildMethod?.Invoke(null, new object[] { world, entity, node });
        }

        if (root is ISpawnable spawnable) spawnable.Spawn(new EntityBuilder(world, entity.Identity));
    }

    public static void AddNodeComponent<T>(World world, Entity entity, T node) where T : Node, new()
    {
        world.AddComponent(entity.Identity, node);
    }
}

public abstract class GdSystem : EcsSystem
{
    public void SpawnRecursively(Node node)
    {
        World.SpawnRecursively(node);
    }

    public EntityBuilder Spawn(Node parent)
    {
        return new EntityBuilder(World, World.Spawn(parent).Identity);
    }

    public void DespawnAndFree(Entity entity)
    {
        if (TryGetComponent<Root>(entity, out var root)) root.Node.QueueFree();
        Despawn(entity);
    }

    public override abstract void Run();
}

public static class EntityBuilderExtensions
{
    public static EntityBuilder Attach(this EntityBuilder entityBuilder, Node node)
    {
        entityBuilder.World.AttachNode(entityBuilder.Id(), node);
        return entityBuilder;
    }
}