using Sandbox;

public class FadeIn : Component
{
	[Property]
	public Curve FadeInCurve { get; set; }
	public ModelRenderer Renderer { get; set; }

	public TimeSince SinceSpawned { get; set; }
	public bool IsFadingIn => SinceSpawned <= FadeInCurve.TimeRange.y;

	protected override void OnStart()
	{
		base.OnStart();

		Renderer = GameObject.Components.Get<ModelRenderer>();
		SinceSpawned = 0f;

		if ( Renderer == null ) return;

		Renderer.Tint = Renderer.Tint.WithAlpha( FadeInCurve.ValueRange.x );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Renderer == null ) return;

		if ( IsFadingIn )
			Renderer.Tint = Renderer.Tint.WithAlpha( FadeInCurve.Evaluate( SinceSpawned ) ); // Fade in when spawned
	}
}
