using System.Linq;
using Godot;

namespace RelEcs
{
    public class Root
    {
        public Node Node;
    }

    public interface ISpawnable
    {
        void Spawn(EntityBuilder entityBuilder);
    }

    public partial class Marshallable<T> : RefCounted where T: class
    {
        public T Value;
        public Marshallable(T value) => Value = value;
    }

    public static class GodotExtensions
    {
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
            
            var nodes = root.GetChildren().Cast<Node>().Prepend(root).ToList();

            foreach (var node in nodes)
            {
                var addMethod = typeof(GodotExtensions).GetMethod("AddNodeComponent");
                var addChildMethod = addMethod?.MakeGenericMethod(node.GetType());
                addChildMethod?.Invoke(null, new object[] { world, entity, node });
            }

            if (root is ISpawnable spawnable) spawnable.Spawn(new EntityBuilder(world, entity.Identity));
        }

        public static void AddNodeComponent<T>(World world, Entity entity, T node) where T : Node, new()
        {
            world.AddComponent(entity.Identity, node);
        }

        public static EntityBuilder Spawn(this ISystem system, Node node)
        {
            return new EntityBuilder(system.World, system.World.Spawn(node).Identity);
        }

        public static void DespawnAndFree(this ISystem system, Entity entity)
        {
            if (system.TryGetComponent<Root>(entity, out var root)) root.Node.QueueFree();
            system.Despawn(entity);
        }

        public static EntityBuilder Attach(this EntityBuilder entityBuilder, Node node)
        {
            entityBuilder.World.AttachNode(entityBuilder.Id(), node);
            return entityBuilder;
        }
    }
}