using Sandbox;

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
	public Vector3 Nuzzle { get; set; }

	public TimeSince LastShot { get; set; } = 0f;

	protected override void DrawGizmos()
	{
		if ( Gizmo.IsSelected )
		{
			Gizmo.GizmoDraw draw = Gizmo.Draw;

			draw.LineThickness = 10f;
			var totalSegments = 10;
			var rangePerSegment = MaxRange / totalSegments;
			for ( int segment = 0; segment <= totalSegments; segment++ )
			{
				draw.LineCircle( Nuzzle, Vector3.Up, rangePerSegment * segment, -DamageCone / 2, DamageCone, 40 );
			}

			var rotationSpeed = Rotation.FromYaw( RealTime.Now * RotatingSpeed );
			draw.Line( Nuzzle + Vector3.Up * 12f + rotationSpeed.Right * 100f, Nuzzle + Vector3.Up * 12f - rotationSpeed.Right * 100f );

			var firingSpeed = RealTime.Now % FiringRate;
			if ( firingSpeed <= FiringRate / 2f )
				draw.SolidCone( Nuzzle + Vector3.Forward * 16f, Vector3.Backward * 16f, 5 );

			draw.LineCircle( Nuzzle, 5f );
		}
	}


	protected override void OnStart()
	{
		base.OnStart();

	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

	}

}
