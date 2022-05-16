# RelEcsGodot
 ## Tools for an easier usage of RelEcs with the Godot Engine.

## Spawning Nodes into the ECS

```csharp
// a pure ecs component
struct Health { public int Value; }

// Let's say our Scene looks like this:
//
// SomeScene (SomeScene : Node2D)
// -- Sprite (Sprite : Node2D)
// 
// creating an instance of SomeScene.tscn and adding it to the scene tree.
var someScene = GD.Load<PackedScene>("res://SomeScene.tscn");
var instance = someScene.Instance<SomeScene>();
AddChild(instance);

// RelEcsGodot adds a method overload for Spawn, that allows to take a Node type object
// RelEcsGodot will automatically convert that Node into an Entity with Components in the ECS.
Entity entity = commands.Spawn(instance);

// this is now a normal ecs entity and we can do the things we're used to
// like adding other struct components to it.
entity.Add(new Health { Value = 10 })

// despawns the entity AND frees the node it was created from.
entity.DespawnAndFree();
// NOTE: just calling entity.Despawn() will not free the node, only the entity.

// We can query for those Nodes like so:
var query = commands.Query().Has<Node<SomeScene>, Node<Sprite>, Health>();
// nodes will automatically be wrapped into a struct Node<T> { public T Value; } component, 
// where T is the class of the node.
// note that the root node, in this case SomeScene, is also added as a component to the ecs entity, 
// next to it's children.

// and we can of course also use our handy ForEach function for iterating.
commands.ForEach((Entity entity, ref Node<Sprite> sprite, ref Health health) =>
{
    // do something with your nodes here
});

// entities that are spawned from a node also have a special component that you can query for.
var allNodeEntities = commands.Query().Has<Root>();
```
