using Sandbox;

public class Shop : Grabbable
{
	[Property]
	public PrefabFile PrefabToGrab { get; set; }

	[Range( 0f, 100f, 0.1f )]
	[Property]
	public float Price { get; set; } = 10f;


	public override bool OnGrab( SnotPlayer grabber )
	{
		var spawned = SceneUtility.Instantiate( SceneUtility.GetPrefabScene( PrefabToGrab ) );

		if ( spawned == null || !spawned.Components.TryGet<Grabbable>( out var grabbable ) ) return false; // If not valid or cannot be grabbed return false

		spawned.Transform.Position = Transform.Position;
		spawned.Transform.Rotation = Transform.Rotation;

		grabbable.OnGrab( grabber );

		return true;
	}

	public override bool OnRelease()
	{
		return false;
	}
}
