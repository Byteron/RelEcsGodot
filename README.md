# RelEcsGodot
 ## Tools for an easier usage of RelEcs with the Godot Engine.

## Spawning Nodes into the ECS

```csharp
// creating an instance of SomeScene.tscn and adding it to the scene tree.
var someScene = GD.Load<PackedScene>("res://SomeScene.tscn");
var instance = someScene.Instance<SomeScene>();
AddChild(instance);

// RelEcsGodot adds a method overload for Spawn, that allows to take a Node type object
// RelEcsGodot will automatically convert that Node into an Entity with Components in the ECS.
Entity entity = commands.Spawn(instance);

// Let's say our Scene looks like this:
//
// SomeScene (SomeScene : Node2D)
// -- Sprite (Sprite : Node2D)
// -- Health (Health : Node)
// 
// Then we can query for those Nodes like so:
var query = commands.Query().Has<Node<SomeScene>, Node<Sprite>, Node<health>>();
// nodes will automatically be wrapped into a struct Node<T> { public T Value; } component, 
// where T is the class of the node.
// note that the root node, in this case SomeScene, is also added as a component to the ecs entity, 
// next to it's children.

// and we can of course also use our handy ForEach function for iterating.
commands.ForEach((Entity entity, ref Node<SomeScene> someScene, ref Node<Sprite> sprite) =>
{
    // do something with your nodes here
});

// entities that are spawned from using a node also have a special relation that you can query for
var allNodeEntities = commands.Query().IsA<NodeEntity>();
```
