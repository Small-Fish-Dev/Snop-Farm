using Sandbox;
using System.Diagnostics;

public class PointAndShoot : Component
{

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
	public Vector3 Muzzle { get; set; }

	[Property]
	public PrefabFile MuzzleFX { get; set; }

	/// <summary>
	/// If the FX should be disposed after the duration or only once nobody is nearby
	/// </summary>
	[Property]
	public bool MuzzleFXLooping { get; set; } = false;

	public GameObject MuzzleParticle { get; set; }
	public TimeSince LastShot { get; set; } = 0f;
	public UnitInfo ClosestEnemy { get; set; }
	public UnitInfo UnitInfo { get; set; }
	public bool IsShooting { get; set; } = false;

	protected override void DrawGizmos()
	{
		if ( Gizmo.IsSelected )
		{
			Gizmo.GizmoDraw draw = Gizmo.Draw;

			using ( Gizmo.Scope( "donut", 0, Rotation.FromPitch( 90f ) ) )
			{
				draw.Color = Color.Red.WithAlpha( 0.4f );
				draw.SolidCircle( 0f, MaxRange, -DamageCone / 2f, DamageCone, (int)(DamageCone) ); // Display range and damage cone
				draw.Color = Color.White;
				draw.SolidRing( Vector3.Backward, MaxRange, MaxRange + 5f, sections: 360 ); // Outer rotation ring
			}
			var rotationSpeed = Rotation.FromYaw( RealTime.Now * RotatingSpeed );
			draw.LineThickness = 10f;
			draw.Line( Vector3.Up - rotationSpeed.Forward * MaxRange, Vector3.Up + rotationSpeed.Forward * MaxRange ); // Display rotation speed

			if ( Time.Now % FiringRate >= FiringRate / 2f )
				draw.SolidCone( Muzzle + Vector3.Forward * 15f, Vector3.Backward * 15f, 2f );
		}
	}


	protected override void OnStart()
	{
		base.OnStart();

		UnitInfo = GameObject.Components.Get<UnitInfo>();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( UnitInfo == null || UnitInfo.Disabled ) return;

		// TODO Check every tot seconds or else too laggy

		if ( LastShot >= FiringRate )
		{
			ClosestEnemy = AreaOfEffect ? AOEDamage() : SingleDamage();

			LastShot = 0f;
		}

		if ( ClosestEnemy != null )
		{
			var goalRotation = Rotation.LookAt( ClosestEnemy.Transform.Position.WithZ( 0 ) - Transform.Position.WithZ( 0 ), Vector3.Up );
			Transform.Rotation = RotateTowards( Transform.Rotation, goalRotation, RotatingSpeed * Time.Delta );

			IsShooting = true;
		}
		else
			IsShooting = false;

		if ( MuzzleFXLooping )
		{
			if ( IsShooting )
			{
				Log.Info( IsShooting );
				if ( MuzzleParticle == null )
				{
					MuzzleParticle = SceneUtility.Instantiate( SceneUtility.GetPrefabScene( MuzzleFX ) );

					if ( MuzzleParticle != null )
					{
						MuzzleParticle.Transform.Position = Transform.World.PointToWorld( Muzzle );
						MuzzleParticle.Transform.Rotation = Transform.Rotation;
						MuzzleParticle.SetParent( GameObject );
					}
				}
			}
			else
			{
				MuzzleParticle?.Destroy();
			}
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

		var closestEnemy = allEnemies.OrderBy( x => x.GameObject.Transform.Position.Distance( Transform.Position ) ) // TODO: Check line of sight!
			.FirstOrDefault();

		foreach ( var enemy in allEnemies )
		{
			var direction = Rotation.LookAt( enemy.Transform.Position.WithZ( 0 ) - Transform.Position.WithZ( 0 ), Vector3.Up );
			var relativeAngle = Transform.Rotation.Forward.WithZ( 0 ).Normal.Angle( direction.Forward.WithZ( 0 ).Normal );

			if ( relativeAngle <= DamageCone / 2 )
				enemy.Damage( Damage );
		}

		if ( MuzzleFX != null )
		{
			//var muzzleParticle = new SceneParticles( Scene.SceneWorld, MuzzleFX );
			//muzzleParticle.Position = Transform.World.PointToWorld( Muzzle );
		}

		return closestEnemy;
	}

	protected UnitInfo SingleDamage()
	{
		var allEnemies = Scene.GetAllComponents<UnitInfo>()
			.Where( x => UnitInfo.EnemyUnitTypes.Contains( x.UnitType ) )
			.Where( x => x.Transform.Position.Distance( Transform.Position ) <= MaxRange );

		var closestEnemy = allEnemies.OrderBy( x => x.GameObject.Transform.Position.Distance( Transform.Position ) ) // TODO: Check line of sight!
			.FirstOrDefault();

		if ( closestEnemy != null )
		{
			var direction = Rotation.LookAt( closestEnemy.Transform.Position.WithZ( 0 ) - Transform.Position.WithZ( 0 ), Vector3.Up );
			var relativeAngle = Transform.Rotation.Forward.WithZ( 0 ).Normal.Angle( direction.Forward.WithZ( 0 ).Normal );

			if ( relativeAngle <= DamageCone / 2 )
			{
				closestEnemy.Damage( Damage );

				if ( MuzzleFX != null )
				{
					//var spawned = SceneUtility.Instantiate( SceneUtility.GetPrefabScene( MuzzleFX ) );
					//spawned.Transform.Position = Transform.World.PointToWorld( Muzzle );
					//spawned.Transform.Rotation = Rotation.FromYaw( Game.Random.Float( 360f ) );
					//var muzzleParticle = new SceneParticles( Scene.SceneWorld, MuzzleFX );
					//muzzleParticle.Position = Transform.World.PointToWorld( Muzzle );

					//Log.Info( Transform.World.PointToWorld( Muzzle ));
					//Log.Info( Transform.Position );
					//Log.Info( muzzleParticle.Position );
				}
			}
		}

		return closestEnemy;
	}
}
