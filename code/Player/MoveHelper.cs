public class MoveHelper : Component
{
	[Property]
	public bool UseCollider { get; set; } = false;

	[Property]
	[HideIf( "UseCollider", true )]
	public Collider Collider { get; set; }

	[Property]
	[HideIf( "UseCollider", true )]
	public bool UseCapsule { get; set; } = true;

	[Range( 0f, 64f, 1f, true, true )]
	[Property]
	[HideIf( "UseCollider", true )]
	public float CollisionRadius { get; set; } = 16f; // The thickness of our collision

	[Range( 0f, 200f, 1f, true, true )]
	[Property]
	[HideIf( "UseCollider", true )]
	public float CollisionHeight { get; set; } = 72f; // The height of our collision

	[Range( 0f, 64f, 1f, true, true )]
	[Property]
	public float StepHeight { get; set; } = 18f; // How high a step can be that you can step over

	[Range( 0f, 90f, 0.01f, true, true )]
	[Property]
	public float GroundAngle { get; set; } = 45f; // How steep terrain can be for you to walk over

	[Property]
	public bool StickToGround { get; set; } = true;


	[Range( 0f, 20f, 0.01f, true, true )]
	[Property]
	public float Acceleration { get; set; } = 10f;

	[Range( 0f, 20f, 0.01f, true, true )]
	[Property]
	public float Deceleration { get; set; } = 10f;

	[Range( 0f, 10f, 0.01f, true, true )]
	[Property]
	public float GroundFriction { get; set; } = 4f;

	[Range( 0f, 10f, 0.01f, true, true )]
	[Property]
	public float AirFriction { get; set; } = 0.1f;

	[Property]
	public bool UseSceneGravity { get; set; } = true;

	[Property]
	[HideIf( "UseSceneGravity", true )]
	public Vector3 Gravity { get; set; } = new Vector3( 0f, 0f, 850f );

	[Property]
	public bool EnableUnstuck { get; set; } = true;

	[Range( 1, 100, 1, true, true )]
	[Property]
	public int MaxUnstuckTries { get; set; } = 20;

	[Property]
	public TagSet IgnoreLayers { get; set; } = new TagSet();

	public BBox CollisionBBox;
	public Capsule CollisionCapsule;

	public Vector3 InitialCameraPosition { get; private set; }
	public Angles EyeAngles { get; private set; }
	public Vector3 WishVelocity { get; set; }
	public Vector3 Velocity { get; set; }
	public bool IsOnGround { get; set; }
	public bool IsCapsuleCollider => UseCollider ? Collider is CapsuleCollider : UseCapsule;
	private int _stuckTries;

	protected override void DrawGizmos()
	{
		Gizmo.GizmoDraw draw = Gizmo.Draw;

		if ( UseCapsule )
			draw.LineCapsule( DefineCapsule() );
		else
			draw.LineBBox( DefineBBox() );
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
		if ( IsCapsuleCollider )
		{
			return source.Capsule( CollisionCapsule )
				.WithoutTags( IgnoreLayers );
		}
		else
		{
			BBox hull = CollisionBBox;
			return source.Size( in hull )
				.WithoutTags( IgnoreLayers );
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
		if ( !EnableUnstuck || !TryUnstuck() )
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
		if ( !EnableUnstuck || !TryUnstuck() )
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

		int num = MaxUnstuckTries;
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

	public BBox DefineBBox()
	{
		if ( !UseCollider || Collider == null || Collider is not BoxCollider box )
			return new BBox( new Vector3( 0f - CollisionRadius, 0f - CollisionRadius, 0f ), new Vector3( CollisionRadius, CollisionRadius, CollisionHeight ) );
		else
			return new BBox( box.Center - box.Scale / 2f, box.Center + box.Scale / 2f );
	}

	public Capsule DefineCapsule()
	{
		if ( !UseCollider || Collider == null || Collider is not CapsuleCollider capsule )
			return new Capsule( Vector3.Up * CollisionRadius, Vector3.Up * (CollisionHeight - CollisionRadius), CollisionRadius );
		else
			return new Capsule( capsule.Start, capsule.End, capsule.Radius );
	}
	protected override void OnStart() // Called as soon as the component gets enabled
	{
		base.OnStart();

		CollisionBBox = DefineBBox();
		CollisionCapsule = DefineCapsule();
	}

	protected override void OnFixedUpdate() // Called every tick
	{
		base.OnFixedUpdate();

		if ( IsOnGround ) // If we're touching the ground VVV
		{
			if ( StickToGround )
				Velocity = Velocity.WithZ( 0 ); // Nullify any vertical velocity to stick to the ground

			Accelerate( WishVelocity );
			ApplyFriction( GroundFriction );
		}
		else // If we're in air VVV
		{
			var gravity = UseSceneGravity ? Scene.PhysicsWorld.Gravity : Gravity;
			Velocity += gravity * Time.Delta; // Apply the scene's gravity to the controller
			Accelerate( WishVelocity );
			ApplyFriction( AirFriction );
		}

		Move(); // Move our character
	}
}
