using Sandbox.Citizen;

public class SnotPlayer : Component
{

	[Property]
	public ThirdPersonController Controller { get; set; }

	[Property]
	public CitizenAnimationHelper CitizenAnimation { get; set; }

	[Property]
	public UnitInfo UnitInfo { get; set; }

	[Range( 0f, 10f, 0.1f, true, true )]
	[Property]
	public float PunchDamage { get; set; } = 1f;

	[Range( 10f, 500f, 1f, true, true )]
	[Property]
	public float PunchRange { get; set; } = 120f;

	[Range( 1f, 100f, 1f, true, true )]
	[Property]
	public float PunchWidth { get; set; } = 6f;

	[Range( 0f, 2f, 0.1f, true, true )]
	[Property]
	public float PunchCooldown { get; set; } = 0.5f;

	[Range( 10f, 500f, 1f, true, true )]
	[Property]
	public float GrabRange { get; set; } = 140f;

	[Range( 1f, 100f, 1f, true, true )]
	[Property]
	public float GrabWidth { get; set; } = 16f;

	[Range( 0f, 2f, 0.1f, true, true )]
	[Property]
	public float GrabCooldown { get; set; } = 0.1f;

	public Grabbable Grabbed { get; set; }
	public TimeSince LastPunch { get; set; } = 0f;
	public TimeSince LastGrab { get; set; } = 0f;

	protected override void OnUpdate() // This stuff could be in OnFixedUpdate but Input.Pressed doesn't work (Issue #4318)
	{
		if ( Controller == null ) return;
		//if ( Animator == null ) return;

		//Animator.WithVelocity( Controller.Velocity );

		if ( Input.Pressed( "Punch" ) )
		{
			if ( Grabbed != null )
				if ( LastPunch >= PunchCooldown )
					Punch();
		}

		if ( Input.Pressed( "Grab" ) )
		{
			if ( LastGrab >= GrabCooldown )
			{
				if ( Grabbed != null )
					Release();
				else
					Grab();
			}
		}

		if ( CitizenAnimation != null )
		{
			if ( Grabbed != null )
				CitizenAnimation.HoldType = CitizenAnimationHelper.HoldTypes.HoldItem;
			else
			{
				if ( LastPunch >= PunchCooldown * 4f )
					CitizenAnimation.HoldType = CitizenAnimationHelper.HoldTypes.None;
			}
		}

		// Update minimap.
		var minimap = Minimap.Instance;
		var camera = Controller.Camera;
		if ( minimap == null || camera == null )
			return;

		minimap.Position = GameObject.Transform.Position;
		minimap.Rotation = camera.Transform.Rotation.Yaw();
	}

	public void Punch()
	{
		if ( Controller == null ) return;
		if ( UnitInfo == null ) return;

		var enemyTagsArray = UnitInfo.EnemyUnitTypes
			.Select( x => x.ToString() )
			.ToArray();

		var punchTrace = Scene.Trace
			.FromTo( Transform.Position + Controller.EyePosition, Transform.Position + Controller.EyePosition + Controller.EyeRotation.Forward * PunchRange )
			.Size( PunchWidth )
			.WithAnyTags( enemyTagsArray )
			.WithoutTags( "player" )
			.Run();

		if ( punchTrace.Hit )
			if ( punchTrace.GameObject.Components.TryGet<UnitInfo>( out UnitInfo unitInfo ) )
				unitInfo.Damage( PunchDamage );

		if ( CitizenAnimation != null )
		{
			CitizenAnimation.HoldType = CitizenAnimationHelper.HoldTypes.Punch;
			CitizenAnimation.Target.Set( "b_attack", true );
		}

		LastPunch = 0;

		//Log.Info( punchTrace.GameObject.GetAllObjects( true ).FirstOrDefault().Name );
	}

	public void Grab()
	{
		var grabTrace = Scene.Trace
			.FromTo( Transform.Position + Controller.EyePosition, Transform.Position + Controller.EyePosition + Controller.EyeRotation.Forward * PunchRange )
			.Size( PunchWidth )
			.WithTag( "Grab" )
			.Run();

		if ( grabTrace.Hit )
			if ( grabTrace.GameObject.Components.TryGet<Grabbable>( out Grabbable grab ) )
				if ( grab.OnGrab( this ) )
					Grabbed = grab;

		LastGrab = 0;
	}

	public void Release()
	{
		if ( Grabbed.OnRelease() )
			Grabbed = null;

		LastGrab = 0;
	}
}
