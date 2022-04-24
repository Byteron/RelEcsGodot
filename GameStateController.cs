using System;
using System.Collections.Generic;
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

    public class GameStateController : Node
    {
        Dictionary<Type, GameState> states = new Dictionary<Type, GameState>();

        Stack<GameState> stack = new Stack<GameState>();

        RelEcs.World world = new RelEcs.World();

        public GameStateController()
        {
            world.AddResource(this);

            world.AddResource(new CurrentGameState());
            world.AddResource(new GodotInputEvent());
            world.AddResource(new DeltaTime());
        }

        public override void _UnhandledInput(InputEvent e)
        {
            if (stack.Count == 0)
            {
                return;
            }

            var currentState = stack.Peek();
            world.GetResource<GodotInputEvent>().Event = e;
            currentState.InputSystems.Run(world);
        }

        public override void _Process(float delta)
        {
            if (stack.Count == 0)
            {
                return;
            }

            var currentState = stack.Peek();
            world.GetResource<DeltaTime>().Value = delta;
            currentState.UpdateSystems.Run(world);

            world.Tick();
        }

        public override void _ExitTree()
        {
            foreach (var state in stack)
            {
                state.ExitSystems.Run(world);
            }
        }

        public void PushState(GameState newState)
        {
            CallDeferred(nameof(PushStateDeferred), newState);
        }

        public void PopState()
        {
            CallDeferred(nameof(PopStateDeferred));
        }

        public void ChangeState(GameState newState)
        {
            CallDeferred(nameof(ChangeStateDeferred), newState);
        }

        void PopStateDeferred()
        {
            if (stack.Count == 0)
            {
                return;
            }

            var currentState = stack.Pop();
            currentState.ExitSystems.Run(world);
            RemoveChild(currentState);
            currentState.QueueFree();

            if (stack.Count > 0)
            {
                currentState = stack.Peek();
                world.GetResource<CurrentGameState>().State = currentState;
                currentState.ContinueSystems.Run(world);
            }
        }

        void PushStateDeferred(GameState newState)
        {
            if (stack.Count > 0)
            {
                var currentState = stack.Peek();

                if (currentState.GetType() == newState.GetType())
                {
                    GD.PrintErr($"{currentState.GetType().ToString()} already at the top of the stack!");
                    return;
                }

                currentState.PauseSystems.Run(world);
            }

            newState.Name = newState.GetType().ToString();
            stack.Push(newState);
            AddChild(newState);
            world.GetResource<CurrentGameState>().State = newState;
            newState.Init(this);
            newState.InitSystems.Run(world);
        }

        void ChangeStateDeferred(GameState newState)
        {
            if (stack.Count > 0)
            {
                var currentState = stack.Pop();
                currentState.ExitSystems.Run(world);
                RemoveChild(currentState);
                currentState.QueueFree();
            }

            newState.Name = newState.GetType().ToString();
            stack.Push(newState);
            AddChild(newState);
            world.GetResource<CurrentGameState>().State = newState;
            newState.Init(this);
            newState.InitSystems.Run(world);
        }
    }
}