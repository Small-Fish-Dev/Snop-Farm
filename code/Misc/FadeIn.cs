using Sandbox;

public class FadeIn : Component
{
	[Property]
	public Curve FadeInCurve { get; set; }
	public ModelRenderer Renderer { get; set; }

	public TimeSince SinceSpawned { get; set; }
	public bool IsFadingIn => SinceSpawned <= FadeInCurve.TimeRange.y; // Duration of fadein, no reason to make it editable.

	protected override void OnStart()
	{
		base.OnStart();

		Renderer = GameObject.Components.Get<ModelRenderer>();
		SinceSpawned = 0f;

		Renderer.Tint = Renderer.Tint.WithAlpha( FadeInCurve.Evaluate( 0f ) );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsFadingIn )
			Renderer.Tint = Renderer.Tint.WithAlpha( FadeInCurve.Evaluate( SinceSpawned ) ); // Fade in when spawned
	}
}
