public sealed class SnotPlayer : Component
{

	[Property] public ThirdPersonController Controller { get; set; }
	//[Property] public CitizenAnimation Animator { get; set; }

	protected override void OnFixedUpdate()
	{
		if ( Controller == null ) return;
		//if ( Animator == null ) return;

		//Animator.WithVelocity( Controller.Velocity );
	}
}
