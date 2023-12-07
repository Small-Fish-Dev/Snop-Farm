using Sandbox;

public sealed class SnotPlayer : BaseComponent
{

	[Property] public CharacterController Controller { get; set; }
	[Property] public CitizenAnimation Animator { get; set; }

	protected override void OnFixedUpdate()
	{
		if ( Controller == null ) return;
		if ( Animator == null ) return;

		Animator.WithVelocity( Controller.Velocity );
	}
}
