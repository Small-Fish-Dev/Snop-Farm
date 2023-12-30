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
	public float MaxGroundAngle { get; set; } = 60f;

	[Property]
	public float MinTimeBetweenAttempts { get; set; } = 4f;

	[Property]
	public float MaxTimeBetweenAttempts { get; set; } = 8f;

	[Property]
	public Vector3 BoundsCheck { get; set; }

	public UnitInfo UnitInfo { get; set; }
	public TimeSince LastSpawnAttempt { get; set; } = 0f;
	private float _nextAttempt;

	protected override void DrawGizmos()
	{
		if ( Gizmo.IsSelected )
		{
			Gizmo.GizmoDraw draw = Gizmo.Draw;

			using ( Gizmo.Scope( "donut", 0, Transform.Rotation * Rotation.FromPitch( 90f ) ) )
			{
				draw.Color = Color.White.WithAlpha( 0.2f );
				draw.SolidRing( 0, MinDistance, MaxDistance, sections: 30 ); // Display spawning distance
			}

			var animationRotation = Rotation.FromYaw( Time.Now * 50f );
			var animationSin = MathF.Sin( Time.Now * 5f );
			var animationDistance = MathX.Remap( animationSin, -1f, 1f, MinDistance, MaxDistance );

			using ( Gizmo.Scope( "rotatearound", animationRotation.Forward * animationDistance + Vector3.Up * BoundsCheck.z / 2f ) )
			{
				draw.LineBBox( BBox.FromPositionAndSize( 0, BoundsCheck ) );
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

			var horizontalFrom = Transform.Position + Vector3.Up * BoundsCheck.z;
			var horizontalTo = horizontalFrom + directionToTry.Forward * randomDistance;
			var horizontalTrace = Scene.Trace.FromTo( horizontalFrom, horizontalTo )
				.Run();

			if ( horizontalTrace.Hit ) continue;

			var verticalTo = horizontalTo - Vector3.Up * BoundsCheck * 2f;
			var verticalTrace = Scene.Trace.FromTo( horizontalTo, verticalTo )
				.Run();

			if ( !verticalTrace.Hit || verticalTrace.StartedSolid || Vector3.GetAngle( verticalTrace.Normal, Vector3.Up ) > MaxGroundAngle || verticalTrace.GameObject != null && verticalTrace.GameObject.Tags.Has( "Unit" ) ) continue;

			var boundsTrace = Scene.Trace.Box( BoundsCheck, verticalTo, verticalTo )
				.WithTag( "Unit" )
				.Run();

			if ( boundsTrace.Hit ) continue;

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
