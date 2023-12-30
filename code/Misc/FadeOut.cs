using Sandbox;

public class FadeOut : Component
{
	[Property]
	public Curve FadeOutCurve { get; set; }
	public ModelRenderer Renderer { get; set; }

	public TimeSince SinceStarted { get; set; }
	public bool IsFadingOut { get; set; } = false;

	protected override void OnStart()
	{
		base.OnStart();

		Renderer = GameObject.Components.Get<ModelRenderer>();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Renderer == null ) return;

		if ( IsFadingOut )
			Renderer.Tint = Renderer.Tint.WithAlpha( FadeOutCurve.Evaluate( SinceStarted ) ); // Fade in when spawned
	}

	public async Task StartFadeOut()
	{
		IsFadingOut = true;
		SinceStarted = 0f;

		await Task.DelaySeconds( FadeOutCurve.TimeRange.y );
	}
}
