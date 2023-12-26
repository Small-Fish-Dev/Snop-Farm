public class UnitInfo : Component
{
	[Property]
	public float Health { get; set; } = 10f;

	[Property]
	public UnitType UnitType { get; set; } = UnitType.None;

	[Property]
	public bool ScaleByHealth { get; set; } = false;

	[Range( 0f, 1f, 0.1f )]
	[ShowIf( "ScaleByHealth", true )]
	[Property]
	public float MinScale { get; set; } = 0.2f;

	[Property]
	public bool FadeIn { get; set; } = false;

	[Property]
	public ModelRenderer Renderer { get; set; }

	[Property]
	public string HitAnimation { get; set; } = "hit";

	[Property]
	public string DamageAmountAnimation { get; set; } = "damage";

	[Property]
	public string DeadAnimation { get; set; } = "dead";

	[Property]
	public string HealthAmountAnimation { get; set; } = "health";

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

		if ( Renderer != null )
			if ( FadeIn )
				Renderer.Tint = Renderer.Tint.WithAlpha( 0 );
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

		if ( Renderer is SkinnedModelRenderer renderer )
		{
			var relativeHealth = MathX.Remap( Health, 0f, MaxHealth, 10f, 100f );
			var currentHealth = renderer.GetFloat( HealthAmountAnimation );

			if ( currentHealth != 0 )
				renderer.Set( HealthAmountAnimation, MathX.Lerp( currentHealth, relativeHealth, Time.Delta * 10f ) ); // Lerp size based on health instead of instantly setting it.
		}

		if ( Renderer != null )
		{
			if ( FadeIn )
				if ( Renderer.Tint.a < 1f )
					Renderer.Tint = Renderer.Tint.WithAlpha( MathX.Lerp( Renderer.Tint.a, 1f, Time.Delta * 2f ) ); // Fade in when spawned
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
		{
			var relativeDamage = MathX.Remap( amount, 0f, MaxHealth, 0f, 100f );
			renderer.Set( DamageAmountAnimation, relativeDamage );
			renderer.Set( HitAnimation, true );
		}

		if ( Health <= 0 )
			Kill();

		LastDamage = amount;
		LastHurt = 0f;
	}

	public virtual async void Kill()
	{
		if ( Renderer is SkinnedModelRenderer renderer )
			renderer.Set( DeadAnimation, true );

		await GameTask.DelayRealtimeSeconds( 0.5f );

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
