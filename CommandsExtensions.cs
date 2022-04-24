using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace RelEcs.Godot
{
    public struct NodeEntity { }
    
    public static class CommandsExtensions
    {
        public static Entity Spawn(this Commands commands, Node parent)
        {
            var entity = commands.Spawn().Add<IsA, NodeEntity>();

            Array nodes = new Array();
            nodes.Add(parent);

            foreach (Node child in parent.GetChildren())
            {
                nodes.Add(child);
            }

            foreach (Node node in nodes)
            {
                var addMethod = typeof(CommandsExtensions).GetMethod("AddNodeHandle");
                var addChildMethod = addMethod?.MakeGenericMethod(new[] { node.GetType() });
                addChildMethod?.Invoke(null, new object[] { entity, node });
            }

            return entity;
        }

        public static void AddNodeHandle<T>(Entity entity, T node) where T : Node
        {
            entity.Add<Node<T>>(new Node<T>(node));
        }
    }
}