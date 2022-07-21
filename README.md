# RelEcsGodot
## Tools for an easier usage of RelEcs with the Godot Engine.

```csharp
// a pure ecs component
class Health { public int Value; }

// For the extra godot functionality, inherit from GDSystem
public class ExampleSystem : GDSystem
{
    // Godot Scene
    PackedScene SomeScene = GD.Load<PackedScene>("res://SomeScene.tscn");

    public override void Run()
    {
        // Let's say our Scene looks like this:
        //
        // SomeScene (SomeScene : Node2D)
        // -- Sprite (Sprite : Node2D)
        // 
        // creating an instance of SomeScene.tscn and adding it to the scene tree.
        var node = SomeScene.Instance<SomeScene>();
        AddChild(node);

        // RelEcsGodot adds a method overload for Spawn, that allows to take a Node type object
        // RelEcsGodot will automatically convert that Node into an Entity with Components in the ECS.
        Entity entity = Spawn(node).Id();

        // this is now a normal ecs entity and we can do the things we're used to
        // like adding other struct components to it.
        On(entity).Add(new Health { Value = 10 })

        // despawns the entity AND frees the node it was created from.
        DespawnAndFree(entity);
        // NOTE: just calling Despawn(entity) will not free the node, only the entity.

        // We can query for those Nodes like so:
        foreach (var (entity, sprite, health) in Query<SomeScene, Sprite, Health>())
        {
            // do something with your nodes here
        }

        // entities that are spawned from a node also have a special component that you can query for.
        var query = Query<Root>();
    }
}
```
