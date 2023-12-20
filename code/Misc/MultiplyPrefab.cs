using Sandbox;

public sealed class MultiplyPrefab : Component
{
	[Property]
	public PrefabFile PrefabToSpawn { get; set; }

	[Property]
	public float MinDistance { get; set; } = 40f;

	[Property]
	public float MaxDistance { get; set; } = 70f;

	[Property]
	public float TimeBetweenAttempts { get; set; } = 5f;

	public TimeSince LastSpawnAttempt { get; set; } = 0f;

	protected override void DrawGizmos()
	{
		Gizmo.GizmoDraw draw = Gizmo.Draw;

		draw.LineThickness = 5f;
		draw.LineCircle( 0f, Vector3.Up, MinDistance , 0f, 360f, 40 );
		draw.LineCircle( 0f, Vector3.Up, MaxDistance, 0f, 360f, 40 );
	}


	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( PrefabToSpawn == null ) return;

		if ( LastSpawnAttempt >= TimeBetweenAttempts )
			AttemptSpawn();
	}

	public void AttemptSpawn()
	{
		LastSpawnAttempt = 0f;

		var anglesToTry = 8;

		for ( int angle = 0; angle <= 360; angle += 360 / anglesToTry )
		{
			var directionToTry = Rotation.FromYaw( angle );
			var randomDistance = Game.Random.Float( MinDistance, MaxDistance );
			var startPos = Transform.Position + Vector3.Up * 32f;
			var midPos = startPos + directionToTry.Forward * randomDistance;
			var horizontalTrace = Scene.Trace.FromTo( startPos, midPos )
				.WithoutTags( "Unit" )
				.Run();

			if ( horizontalTrace.Hit ) continue;

			var endPos = midPos + Vector3.Down * 64f;
			var verticalTrace = Scene.Trace.FromTo( midPos, endPos )
				.WithoutTags( "Unit" )
				.Run();

			if ( !verticalTrace.Hit ) continue;

			Log.Info( verticalTrace.GameObject.Tags.TryGetAll().Count() );

			SpawnPrefab( verticalTrace.HitPosition );
			return;
		}
	}

	public void SpawnPrefab( Vector3 position )
	{
		var spawned = SceneUtility.Instantiate( SceneUtility.GetPrefabScene( PrefabToSpawn ) );
		spawned.Transform.Position = position;
	}
}
