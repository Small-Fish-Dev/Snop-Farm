using Sandbox;

public class PointAndShoot : Component
{
	[Property]
	public UnitInfo UnitInfo { get; set; }

	[Range( 10f, 1000f, 1f, true, true )]
	[Property]
	public float MaxRange { get; set; } = 250f;

	[Range( 1f, 360f, 1f, true, true )]
	[Property]
	public float DamageCone { get; set; } = 15f;

	[Range( 0f, 360f, 1f, true, true )]
	[Property]
	public float RotatingSpeed { get; set; } = 40f;

	[Range( 0f, 5f, 0.01f, true, true )]
	[Property]
	public float FiringRate { get; set; } = 0.5f;

	[Range( 0f, 100f, 0.01f, true, true )]
	[Property]
	public float Damage { get; set; } = 1f;

	[Property]
	public bool AreaOfEffect { get; set; } = false;

	[Property]
	public Vector3 Nuzzle { get; set; }

	public TimeSince LastShot { get; set; } = 0f;

	protected override void DrawGizmos()
	{
		if ( Gizmo.IsSelected )
		{
			using ( Gizmo.Scope( "donut", 0, Rotation.FromPitch( 90f ) ) )
			{
				Gizmo.GizmoDraw draw = Gizmo.Draw;

				draw.SolidCircle( 0f, MaxRange, -DamageCone / 2f, DamageCone, (int)(DamageCone) ); // Display range and damage cone

				draw.SolidRing( Vector3.Backward * Nuzzle.z, Nuzzle.x - 1f, Nuzzle.x + 1f, sections: 24 ); // Outer rotation ring
			}

			Gizmo.GizmoDraw draw2 = Gizmo.Draw;

			var rotationSpeed = Rotation.FromYaw( RealTime.Now * RotatingSpeed );
			draw2.LineThickness = 10f;
			draw2.Line( Vector3.Up * Nuzzle.z - rotationSpeed.Forward * Nuzzle.x, Vector3.Up * Nuzzle.z + rotationSpeed.Forward * Nuzzle.x ); // Display rotation speed

			var firingSpeed = RealTime.Now % FiringRate;
			if ( firingSpeed <= FiringRate / 2f )
				draw2.SolidCone( Nuzzle + Vector3.Forward * 16f, Vector3.Backward * 16f, 5 ); // Display firing rate
		}
	}


	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( UnitInfo == null ) return;

		// TODO Check every tot seconds or else too laggy

		if ( LastShot >= FiringRate )
		{
			var closestEnemy = AreaOfEffect ? AOEDamage() : SingleDamage();

			if ( closestEnemy != null )
			{
				var goalRotation = Rotation.LookAt( closestEnemy.Transform.Position.WithZ( 0 ) - Transform.Position.WithZ( 0 ), Vector3.Up );
				Transform.Rotation = RotateTowards( Transform.Rotation, goalRotation, RotatingSpeed * Time.Delta );
			}

			LastShot = 0f;
		}		
	}

	// Thank you ShadowBrain!
	Rotation RotateTowards( Rotation from, Rotation to, float maxDegreesDelta )
	{
		float angle = Rotation.Difference( from, to ).Angle();
		float t = MathF.Min( 1f, maxDegreesDelta / angle );
		return Rotation.Slerp( from, to, t );
	}

	protected UnitInfo AOEDamage()
	{
		var allEnemies = Scene.GetAllComponents<UnitInfo>()
			.Where( x => UnitInfo.EnemyUnitTypes.Contains( x.UnitType ) )
			.Where( x => x.Transform.Position.Distance( Transform.Position ) <= MaxRange );

		var closestEnemy = allEnemies.OrderBy( x => x.GameObject.Transform.Position.Distance( Transform.Position ) )
			.FirstOrDefault();

		foreach ( var enemy in allEnemies )
		{
			var direction = Rotation.LookAt( enemy.Transform.Position.WithZ( 0 ) - Transform.Position.WithZ( 0 ), Vector3.Up );
			var relativeAngle = Transform.Rotation.Forward.WithZ( 0 ).Normal.Angle( direction.Forward.WithZ( 0 ).Normal );

			if ( relativeAngle <= DamageCone / 2 )
				enemy.Damage( Damage );
		}

		return closestEnemy;
	}

	protected UnitInfo SingleDamage()
	{
		var allEnemies = Scene.GetAllComponents<UnitInfo>()
			.Where( x => UnitInfo.EnemyUnitTypes.Contains( x.UnitType ) )
			.Where( x => x.Transform.Position.Distance( Transform.Position ) <= MaxRange );

		var closestEnemy = allEnemies.OrderBy( x => x.GameObject.Transform.Position.Distance( Transform.Position ) )
			.FirstOrDefault();

		if ( closestEnemy != null )
		{
			var direction = Rotation.LookAt( closestEnemy.Transform.Position.WithZ( 0 ) - Transform.Position.WithZ( 0 ), Vector3.Up );
			var relativeAngle = Transform.Rotation.Forward.WithZ( 0 ).Normal.Angle( direction.Forward.WithZ( 0 ).Normal );

			if ( relativeAngle <= DamageCone / 2 )
				closestEnemy.Damage( Damage );
		}

		return closestEnemy;
	}
}
