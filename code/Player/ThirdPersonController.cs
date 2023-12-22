using Sandbox.Citizen;

public class ThirdPersonController : Component
{
	[Property]
	public GameObject Camera { get; set; }

	[Property]
	public bool CameraCollisions { get; set; } = true;

	[Property]
	public CitizenAnimationHelper CitizenAnimation { get; set; }

	[Property]
	public MoveHelper MoveHelper { get; set; }

	[Property]
	public Vector3 EyePosition { get; set; } = new Vector3( 0f, 0f, 64f );

	[Range( 0f, 400f, 1f, true, true )]
	[Property]
	public float WalkSpeed { get; set;  } = 120f; // How fast we walk

	[Range( 0f, 800f, 1f, true, true )]
	[Property]
	public float RunSpeed { get; set; } = 250f; // How fast we run

	[Range( 0f, 600f, 1f, true, true )]
	[Property]
	public float JumpStrength { get; set; } = 400f; // How high we jump

	public Vector3 WishVelocity;
	public Vector3 InitialCameraPosition { get; private set; }
	public Angles EyeAngles { get; private set; }
	public Rotation EyeRotation => EyeAngles.ToRotation();
	public Vector3 EyeWorldPosition => Transform.World.PointToWorld( EyePosition );

	protected override void DrawGizmos()
	{
		Gizmo.GizmoDraw draw = Gizmo.Draw;
		var forward = base.Transform.Rotation.Forward;
		var right = base.Transform.Rotation.Right;

		draw.SolidCone( EyePosition + forward * 5f - right * 2f, forward * 10f, 1f );
		draw.SolidCone( EyePosition + forward * 5f + right * 2f, forward * 10f, 1f );
	}

	protected override void OnStart() // Called as soon as the component gets enabled
	{
		base.OnStart();

		if ( Camera != null )
		{
			EyeAngles = Camera.Transform.Rotation.Angles(); // Starting eye angles set to whatever the camera is
			InitialCameraPosition = Camera.Transform.LocalPosition;
		}
	}

	protected override void OnUpdate() // Called every frame
	{
		if ( Camera != null )
		{
			EyeAngles += Input.AnalogLook * 5f; // Rotate our view angles
			EyeAngles = EyeAngles.WithPitch( Math.Clamp( EyeAngles.pitch, -80f, 80f ) ); // Clamp view angles

			var eyeRotation = EyeAngles.ToRotation();

			Camera.Transform.Position = EyeWorldPosition + (InitialCameraPosition - EyePosition) * eyeRotation; // Rotate camera around eyeposition relative to initial position

			var traceDirection = (Camera.Transform.World.Position - EyeWorldPosition ).Normal;
			var maxDistance = Vector3.DistanceBetween( EyePosition, InitialCameraPosition );
			var cameraTrace = Scene.Trace.FromTo( EyeWorldPosition, EyeWorldPosition + traceDirection * maxDistance )
				.Size( 5f )
				.WithoutTags( "Unit", "Player" )
				.Run();

			Camera.Transform.Position = cameraTrace.EndPosition;
			Camera.Transform.Rotation = eyeRotation; // Set the camera's rotation based off of our eye angles
		}
	}

	protected override void OnFixedUpdate() // Called every tick
	{
		ComputeWishVelocity();

		if ( MoveHelper != null )
		{
			MoveHelper.WishVelocity = WishVelocity;
			ComputeJump();
		}

		var bodyHeadRotationDifference = Vector3.GetAngle( Transform.Rotation.Forward.WithZ( 0 ), EyeAngles.Forward.WithZ( 0 ) );

		if ( ( MoveHelper != null && !MoveHelper.Velocity.IsNearlyZero( 1f ) ) || bodyHeadRotationDifference > 50f )
			Transform.Rotation = Rotation.Slerp( Transform.Rotation, Rotation.FromYaw( EyeAngles.yaw ), Time.Delta * 6f );

		if ( CitizenAnimation != null )
		{
			if ( MoveHelper != null )
			{
				CitizenAnimation.WithVelocity( MoveHelper.Velocity );
				CitizenAnimation.IsGrounded = MoveHelper.IsOnGround;
			}

			CitizenAnimation.WithLook( EyeAngles.ToRotation().Forward );
		}
	}

	public void ComputeWishVelocity()
	{
		var playerYaw = Rotation.FromYaw( EyeAngles.yaw ); // Horizontal movement only needs yaw based off of where we're looking
		var direction = Input.AnalogMove * playerYaw; // Rotate your inputs based on your eye angles
		var wishSpeed = Input.Down( "Run" ) ? RunSpeed : WalkSpeed; // If we're running we use the running speed, else the walking speed

		WishVelocity = direction * wishSpeed; // The direction is normal, so we multiply its magnitude with the wish speed

		if ( CitizenAnimation != null )
			CitizenAnimation.WithWishVelocity( WishVelocity );
	}

	public void ComputeJump()
	{
		if ( MoveHelper.IsOnGround ) // If you're on the ground
		{
			if ( Input.Down( "Jump" ) ) // And you're holding the JUMP button
			{
				MoveHelper.Punch( Vector3.Up * JumpStrength ); // Make the player jump (Punch unsticks you from the ground and applies velocity)
				if ( CitizenAnimation != null )
					CitizenAnimation.TriggerJump();
			}
		}
	}
}
