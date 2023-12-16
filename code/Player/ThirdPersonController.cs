
[Title( "Third Person Controller" )]
[Category( "Physics" )]
[Icon( "directions_walk" )]
[EditorHandle( "materials/gizmo/charactercontroller.png" )]
public class ThirdPersonController : Component
{
	[Property]
	public GameObject Camera { get; set; }
	[Property]
	public CitizenAnimation CitizenAnimation { get; set; }

	[Range( 0f, 400f, 1f, true, true )]
	[Property]
	public float WalkSpeed { get; set; } = 120f; // How fast we walk

	[Range( 0f, 800f, 1f, true, true )]
	[Property]
	public float RunSpeed { get; set; } = 250f; // How fast we run

	[Range( 0f, 600f, 1f, true, true )]
	[Property]
	public float JumpStrength { get; set; } = 300f; // How high we jump

	[Property]
	public bool CapsuleCollisions { get; set; } = true; // Capsule or BBox

	[Range( 0f, 64f, 1f, true, true )]
	[Property]
	public float CollisionRadius { get; set; } = 16f; // The thickness of our collision

	[Range( 0f, 200f, 1f, true, true )]
	[Property]
	public float CollisionHeight { get; set; } = 72f; // The height of our collision

	[Range( 0f, 64f, 1f, true, true )]
	[Property]
	public float StepHeight { get; set; } = 18f; // How high a step can be that you can step over

	[Range( 0f, 90f, 0.01f, true, true )]
	[Property]
	public float GroundAngle { get; set; } = 45f; // How steep terrain can be for you to walk over


	[Range( 0f, 20f, 0.01f, true, true )]
	[Property]
	public float Acceleration { get; set; } = 10f;

	[Range( 0f, 20f, 0.01f, true, true )]
	[Property]
	public float Deceleration { get; set; } = 10f;

	[Property]
	public TagSet IgnoreLayers { get; set; } = new TagSet();

	public BBox CollisionBBox => new BBox( new Vector3( 0f - CollisionRadius, 0f - CollisionRadius, 0f ), new Vector3( CollisionRadius, CollisionRadius, CollisionHeight ) );
	public Capsule CollisionCapsule => new Capsule( Vector3.Up * CollisionRadius, Vector3.Up * (CollisionHeight - CollisionRadius), CollisionRadius );

	public Vector3 InitialCameraPosition { get; private set; }
	public Angles EyeAngles { get; private set; }
	public Vector3 WishVelocity { get; private set; }
	public Vector3 Velocity { get; set; }
	public bool IsOnGround { get; set; }
	private int _stuckTries;


	protected override void DrawGizmos()
	{
		Gizmo.GizmoDraw draw = Gizmo.Draw;

		if ( CapsuleCollisions )
			draw.LineCapsule( CollisionCapsule );
		else
		{
			var box = CollisionBBox;
			draw.LineBBox( in box );
		}
	}

	//
	// Summary:
	//     Add acceleration to the current velocity. No need to scale by time delta - it
	//     will be done inside.
	public void Accelerate( Vector3 vector )
	{
		if ( vector.IsNearZeroLength )
			return;

		Vector3 normal = vector.Normal;
		float length = vector.Length;
		float num = Velocity.Dot( normal );
		float num2 = length - num;
		if ( !(num2 <= 0f) )
		{
			float num3 = Acceleration * Time.Delta * length;
			if ( num3 > num2 )
				num3 = num2;

			Velocity += normal * num3;
		}
	}

	//
	// Summary:
	//     Apply an amount of friction to the current velocity. No need to scale by time
	//     delta - it will be done inside.
	public void ApplyFriction( float frictionAmount, float stopSpeed = 140f )
	{
		float length = Velocity.Length;
		if ( !(length < 0.01f) )
		{
			float num = ((length < stopSpeed) ? stopSpeed : length);
			float num2 = num * Time.Delta * frictionAmount;
			float num3 = length - num2;
			if ( num3 < 0f )
				num3 = 0f;

			if ( num3 != length )
			{
				num3 /= length;
				Velocity *= num3;
			}
		}
	}

	private PhysicsTraceBuilder BuildTrace( Vector3 from, Vector3 to )
	{
		return BuildTrace( base.Scene.PhysicsWorld.Trace.Ray( in from, in to ) );
	}

	private PhysicsTraceBuilder BuildTrace( PhysicsTraceBuilder source )
	{
		if ( CapsuleCollisions )
			return source.Capsule( CollisionCapsule ).WithoutTags( IgnoreLayers );
		else
		{
			BBox hull = CollisionBBox;
			return source.Size( in hull ).WithoutTags( IgnoreLayers );
		}
	}

	private void Move( bool step )
	{
		if ( step && IsOnGround )
			Velocity = Velocity.WithZ( 0f );

		if ( Velocity.IsNearlyZero( 0.001f ) )
		{
			Velocity = Vector3.Zero;
			return;
		}

		Vector3 position = base.GameObject.Transform.Position;
		CharacterControllerHelper characterControllerHelper = new CharacterControllerHelper( BuildTrace( position, position ), position, Velocity );
		characterControllerHelper.Bounce = 0.3f;
		characterControllerHelper.MaxStandableAngle = GroundAngle;

		if ( step && IsOnGround )
			characterControllerHelper.TryMoveWithStep( Time.Delta, StepHeight );
		else
			characterControllerHelper.TryMove( Time.Delta ); // TODO: Maybe this fucks platforming up?

		base.Transform.Position = characterControllerHelper.Position;
		Velocity = characterControllerHelper.Velocity;
	}

	private void CategorizePosition()
	{
		Vector3 position = base.Transform.Position;
		Vector3 to = position + Vector3.Down * 2f;
		Vector3 from = position;
		bool isOnGround = IsOnGround;
		if ( !IsOnGround && Velocity.z > 50f )
		{
			IsOnGround = false;
			return;
		}

		to.z -= (isOnGround ? StepHeight : 0.1f);
		PhysicsTraceResult physicsTraceResult = BuildTrace( from, to ).Run();
		if ( !physicsTraceResult.Hit || Vector3.GetAngle( Vector3.Up, physicsTraceResult.Normal ) > GroundAngle )
		{
			IsOnGround = false;
			return;
		}

		IsOnGround = true;
		if ( isOnGround && !physicsTraceResult.StartedSolid && physicsTraceResult.Fraction > 0f && physicsTraceResult.Fraction < 1f )
			base.Transform.Position = physicsTraceResult.EndPosition + physicsTraceResult.Normal * 0.01f;
	}

	//
	// Summary:
	//     Disconnect from ground and punch our velocity. This is useful if you want the
	//     player to jump or something.
	public void Punch( in Vector3 amount )
	{
		IsOnGround = false;
		Velocity += amount;
	}

	//
	// Summary:
	//     Move a character, with this velocity
	public void Move()
	{
		if ( !TryUnstuck() )
		{
			if ( IsOnGround )
				Move( step: true );
			else
				Move( step: false );

			CategorizePosition();
		}
	}

	//
	// Summary:
	//     Move from our current position to this target position, but using tracing an
	//     sliding. This is good for different control modes like ladders and stuff.
	public void MoveTo( Vector3 targetPosition, bool useStep )
	{
		if ( !TryUnstuck() )
		{
			Vector3 position = base.Transform.Position;
			Vector3 velocity = targetPosition - position;
			CharacterControllerHelper characterControllerHelper = new CharacterControllerHelper( BuildTrace( position, position ), position, velocity );
			characterControllerHelper.MaxStandableAngle = GroundAngle;

			if ( useStep )
				characterControllerHelper.TryMoveWithStep( 1f, StepHeight );
			else
				characterControllerHelper.TryMove( 1f );

			base.Transform.Position = characterControllerHelper.Position;
		}
	}

	private bool TryUnstuck()
	{
		if ( !BuildTrace( base.Transform.Position, base.Transform.Position ).Run().StartedSolid )
		{
			_stuckTries = 0;
			return false;
		}

		int num = 20;
		for ( int i = 0; i < num; i++ )
		{
			Vector3 vector = base.Transform.Position + Vector3.Random.Normal * ((float)_stuckTries / 2f);
			if ( i == 0 )
				vector = base.Transform.Position + Vector3.Up * 2f;

			if ( !BuildTrace( vector, vector ).Run().StartedSolid )
			{
				base.Transform.Position = vector;
				return false;
			}
		}

		_stuckTries++;
		return true;
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

			Camera.Transform.Position = Transform.Position + Vector3.Up * InitialCameraPosition.z + InitialCameraPosition.WithZ( 0 ) * eyeRotation;
			Camera.Transform.Rotation = eyeRotation; // Set the camera's rotation based off of our eye angles
		}
	}

	protected override void OnFixedUpdate() // Called every tick
	{
		ComputeWishVelocity();
		ComputeHelper();
		ComputeJump();

		Transform.Rotation = Rotation.Slerp( Transform.Rotation, Rotation.FromYaw( EyeAngles.yaw ), Time.Delta * 10f );

		if ( CitizenAnimation != null )
		{
			CitizenAnimation.WithVelocity( Velocity );
			CitizenAnimation.IsGrounded = IsOnGround;
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

	public void ComputeHelper()
	{
		if ( IsOnGround ) // If we're touching the ground VVV
		{
			Velocity = Velocity.WithZ( 0 ); // Nullify any vertical velocity to stick to the ground
			Accelerate( WishVelocity ); // Accelerate by our wish velocity
			ApplyFriction( 4.0f ); // Make movements on ground responsive
		}
		else // If we're in air VVV
		{
			Velocity += Scene.PhysicsWorld.Gravity * Time.Delta; // Apply the scene's gravity to the controller
			Accelerate( WishVelocity.ClampLength( WalkSpeed / 2f ) ); // Give some control in air but not too much
			ApplyFriction( 0.1f ); // Make movements in air slippery
		}

		Move(); // Move our character
	}

	public void ComputeJump()
	{
		if ( IsOnGround ) // If you're on the ground
		{
			if ( Input.Down( "Jump" ) ) // And you're holding the JUMP button
			{
				Punch( Vector3.Up * JumpStrength ); // Make the player jump (Punch unsticks you from the ground and applies velocity)
				if ( CitizenAnimation != null )
					CitizenAnimation.TriggerJump();
			}
		}
	}
}
