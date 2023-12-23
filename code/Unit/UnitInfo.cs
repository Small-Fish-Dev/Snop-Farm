public class UnitInfo : Component
{
	[Property]
	public float Health { get; set; } = 10f;

	[Property]
	public bool ScaleByHealth { get; set; } = false;

	[Range( 0f, 1f, 0.1f )]
	[Property]
	public float MinScale { get; set; } = 0.2f;

	[Property]
	public UnitType UnitType { get; set; } = UnitType.None;

	[Property]
	public ModelRenderer Renderer { get; set; }

	[Property]
	public string HurtAnimationVariable { get; set; } = "hit";

	[Property]
	public List<UnitType> EnemyUnitTypes { get; set; }

	public float LastDamage { get; set; } = 0;
	public float HurtAnimationDuration => Math.Max( LastDamage / 5f, 0.1f );
	public TimeSince LastHurt { get; set; } = float.MaxValue;
	public bool HurtAnimation => LastHurt != float.MaxValue && LastHurt <= HurtAnimationDuration;
	public float MaxHealth;
	public Vector3 MaxScale;


	protected override void OnStart()
	{
		base.OnStart();

		base.Tags.Set( "Unit", true ); // Give the Unit tag
		base.Tags.Set( UnitType.ToString(), true ); // Give tag of whatever unit type this is

		MaxHealth = Health;
		MaxScale = Transform.Scale;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( HurtAnimation )
		{
			var extraScale = 0.1f;
			var animationTime = MathX.Remap( LastHurt, 0f, HurtAnimationDuration, 0f, 1f );
			var sinScale = (float)Math.Sin( animationTime * Math.PI );
			Transform.Scale = _oldScale * (1f + sinScale * extraScale);
		}
	}

	private Vector3 _oldScale = Vector3.One;
	private float _currentScale = 1f;
	public void Damage( float amount )
	{
		Health = Math.Max( Health - amount, 0 );

		if ( ScaleByHealth )
			Transform.Scale = MaxScale * MathX.Remap( Health, 0f, MaxHealth, MinScale, 1f );

		HurtFX();

		if ( Renderer is SkinnedModelRenderer renderer )
			renderer.Set( HurtAnimationVariable, true );

		if ( Health <= 0 )
			Kill();

		LastDamage = amount;
		LastHurt = 0f;
	}

	public virtual void Kill()
	{
		GameObject.Destroy();
	}

	private async void HurtFX()
	{
		_oldScale = HurtAnimation ? _oldScale : Transform.Scale;

		Color oldColor = Color.White;

		if ( Renderer != null )
		{
			oldColor = HurtAnimation ? oldColor : Renderer.Tint;
			Renderer.Tint = Color.Red;
		}

		await GameTask.DelayRealtimeSeconds( HurtAnimationDuration );

		if ( Renderer != null )
			Renderer.Tint = oldColor;

		if ( GameObject != null )
			Transform.Scale = _oldScale;
	}
}
