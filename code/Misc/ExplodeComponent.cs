using Sandbox;

public class ExplodeComponent : Component
{
	[Range( 0f, 100f, 0.1f )]
	[Property]
	public float Damage { get; set; } = 5f;

	[Range( 0f, 500, 1f )]
	[Property]
	public float Range { get; set; } = 100f;

	[Range( 0f, 10f, 0.1f )]
	[Property]
	public float Timer { get; set; } = 4f;

	public ModelRenderer Renderer { get; set; }
	public Collider Collider { get; set; }
	public Rigidbody Rigidbody { get; set; }
	public UnitInfo UnitInfo { get; set; }

	public float TimeSinceActive { get; set; } = 0;

	protected override void DrawGizmos()
	{
		if ( Gizmo.IsSelected )
		{
			Gizmo.GizmoDraw draw = Gizmo.Draw;

			draw.Color = Color.Red.WithAlpha( 0.4f );
			draw.LineSphere( 0f, Range, 30 );
		}
	}

	protected override void OnStart()
	{
		base.OnStart();

		Renderer = GameObject.Components.Get<ModelRenderer>();
		Collider = GameObject.Components.Get<Collider>();
		Rigidbody = GameObject.Components.Get<Rigidbody>();
		UnitInfo = GameObject.Components.Get<UnitInfo>();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( UnitInfo == null || UnitInfo.Disabled ) return;

		TimeSinceActive += Time.Delta;

		if ( TimeSinceActive >= Timer )
			GameObject.Destroy();
	}
}
