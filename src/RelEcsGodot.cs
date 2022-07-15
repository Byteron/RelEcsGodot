using Godot;
using Godot.Collections;

namespace RelEcs.Godot
{
    public class Root
    {
        public Node Node;
    }
    
    public interface ISpawnable
    {
        void Spawn(Entity entity);
    }
    
    // wraps an ecs object into a godot variant
    public class Marshallable<T> : Object
    {
        public T Value;
        public Marshallable() => Value = default;
        public Marshallable(T value) => Value = value;
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
            root.SetMeta("Entity", new Marshallable<Entity>(entity));

            var nodes = new Array();
            nodes.Add(root);

            foreach (Node child in root.GetChildren())
            {
                nodes.Add(child);
            }

            foreach (Node node in nodes)
            {
                var addMethod = typeof(WorldExtensions).GetMethod("AddNodeComponent");
                var addChildMethod = addMethod?.MakeGenericMethod(new[] { node.GetType() });
                addChildMethod?.Invoke(null, new object[] { world, entity, node });
            }
            
            if (root is ISpawnable spawnable) spawnable.Spawn(entity);
        }

        public static void AddNodeComponent<T>(World world, Entity entity, T node) where T : Node, new()
        {
            world.AddComponent(entity.Identity, node);
        }
    }

    public static class CommandsExtensions
    {
        public static void SpawnRecursively(this Commands commands, Node node)
        {
            commands.World.SpawnRecursively(node);
        }

        public static Entity Spawn(this Commands commands, Node parent)
        {
            return commands.World.Spawn(parent);
        }

        public static void DespawnAndFree(this Commands commands, Entity entity)
        {
            if (commands.TryGetComponent<Root>(entity, out var root)) root.Node.QueueFree();
            commands.Despawn(entity);
        }

    }

    public static class EntityBuilderExtensions
    {
        public static EntityBuilder Attach(this EntityBuilder entityBuilder, Node node)
        {
            entityBuilder.World.AttachNode(entityBuilder.Id(), node);
            return entityBuilder;
        }
    }
}