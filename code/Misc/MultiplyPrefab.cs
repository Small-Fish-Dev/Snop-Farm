using Sandbox;

public class MultiplyPrefab : Component
{
	[Property]
	public PrefabFile PrefabToSpawn { get; set; }

	[Property]
	public float MinDistance { get; set; } = 40f;

	[Property]
	public float MaxDistance { get; set; } = 70f;

	[Property]
	public float MinTimeBetweenAttempts { get; set; } = 4f;

	[Property]
	public float MaxTimeBetweenAttempts { get; set; } = 8f;

	public UnitInfo UnitInfo { get; set; }
	public TimeSince LastSpawnAttempt { get; set; } = 0f;
	private float _nextAttempt;

	protected override void DrawGizmos()
	{
		if ( Gizmo.IsSelected )
		{
			using ( Gizmo.Scope( "donut", 0, Transform.Rotation * Rotation.FromPitch( 90f ) ) )
			{
				Gizmo.GizmoDraw draw = Gizmo.Draw;

				draw.SolidRing( 0, MinDistance, MaxDistance, sections: 30 ); // Display spawning distance
			}
		}
	}


	protected override void OnStart()
	{
		base.OnStart();

		_nextAttempt = Game.Random.Float( MinTimeBetweenAttempts, MaxTimeBetweenAttempts );

		UnitInfo = GameObject.Components.Get<UnitInfo>();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( UnitInfo == null || UnitInfo.Disabled ) return;
		if ( PrefabToSpawn == null ) return;

		if ( LastSpawnAttempt >= _nextAttempt )
			AttemptSpawn();
	}

	public void AttemptSpawn()
	{
		LastSpawnAttempt = 0f;
		_nextAttempt = Game.Random.Float( MinTimeBetweenAttempts, MaxTimeBetweenAttempts );

		var anglesToTry = 16;
		var angleSize = 360f / anglesToTry;
		var randomAngle = Game.Random.Int( anglesToTry ) * angleSize;

		for ( float angle = 0f; angle <= 360f; angle += angleSize )
		{
			var directionToTry = Rotation.FromYaw( angle + randomAngle);
			var randomDistance = Game.Random.Float( MinDistance, MaxDistance );
			var startPos = Transform.Position + Vector3.Up * 32f;
			var midPos = startPos + directionToTry.Forward * randomDistance;
			var horizontalTrace = Scene.Trace.FromTo( startPos, midPos )
				.Run();

			if ( horizontalTrace.Hit ) continue;

			var endPos = midPos + Vector3.Down * 64f;
			var verticalTrace = Scene.Trace.FromTo( midPos, endPos )
				.Run();

			if ( !verticalTrace.Hit || verticalTrace.GameObject != null && verticalTrace.GameObject.Tags.Has( "Unit" ) ) continue;

			SpawnPrefab( verticalTrace.HitPosition );
			return;
		}
	}

	public void SpawnPrefab( Vector3 position )
	{
		var spawned = SceneUtility.Instantiate( SceneUtility.GetPrefabScene( PrefabToSpawn ) );
		spawned.Transform.Position = position;
		spawned.Transform.Rotation = Rotation.FromYaw( Game.Random.Float( 360f ) );
	}
}
