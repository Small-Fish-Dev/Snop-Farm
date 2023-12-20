public sealed class UnitInfo : Component
{
	[Property]
	public float Health { get; set; } = 10f;

	[Property]
	public UnitType UnitType { get; set; } = UnitType.None;

	[Property]
	public ModelRenderer Renderer { get; set; }

	[Property]
	public List<UnitType> EnemyUnitTypes { get; set; }

	[Property]
	public float HurtInvulnerabilityTimer { get; set; } = 0.15f;
	public TimeSince LastHurt { get; set; } = float.MaxValue;


	protected override void OnStart()
	{
		base.OnStart();

		base.Tags.Set( "Unit", true ); // Give the Unit tag
		base.Tags.Set( UnitType.ToString(), true ); // Give tag of whatever unit type this is
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( LastHurt < HurtInvulnerabilityTimer )
		{
			var extraScale = 0.1f;
			var animationTime = MathX.Remap( LastHurt, 0f, HurtInvulnerabilityTimer, 0f, 1f );
			var sinScale = (float)Math.Sin( animationTime * Math.PI );
			Transform.Scale = _oldScale * (1f + sinScale * extraScale);
		}
	}

	private Vector3 _oldScale = Vector3.One;
	private float _currentScale = 1f;
	public async void Damage( float amount )
	{
		if ( LastHurt <= HurtInvulnerabilityTimer ) return;

		await HurtFX();

		Health = Math.Max( Health - amount, 0 );

		if ( Health <= 0 )
			Kill();
	}

	public void Kill()
	{
		GameObject.Destroy();
	}

	private async Task HurtFX()
	{
		_oldScale = Transform.Scale;
		LastHurt = 0f;

		Color oldColor = Color.White;

		if ( Renderer != null )
		{
			oldColor = Renderer.Tint;
			Renderer.Tint = Color.Red;
		}

		await GameTask.DelayRealtimeSeconds( HurtInvulnerabilityTimer );

		if ( Renderer != null )
			Renderer.Tint = oldColor;
	}
}
