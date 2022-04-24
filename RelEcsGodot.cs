using Godot;

namespace RelEcs.Godot
{
    public class CurrentGameState
    {
        public GameState State;
    }

    public class GodotInputEvent
    {
        public InputEvent Event;
    }

    public class DeltaTime
    {
        public float Value;
    }

    // wraps a godot node into an ecs component
    public struct Node<T> where T : Node
    {
        public T Value;
        public Node(T value) => Value = value;
    }

    // wraps an ecs object into a godot variant
    public class Marshallable<T> : Reference
    {
        public T Value;
        public Marshallable(T value) => Value = value;
    }
}