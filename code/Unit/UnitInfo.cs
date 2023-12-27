public class UnitInfo : Component
{
	[Property]
	public float Health { get; set; } = 10f;

	[Property]
	public UnitType UnitType { get; set; } = UnitType.None;

	[Property]
	public Curve HurtAnimation { get; set; }

	[Property]
	public Color HurtColor { get; set; } = Color.Red;

	[Property]
	public float HurtScale { get; set; } = 1.5f;

	public float LastDamage { get; set; } = 0;
	public TimeSince LastHurt { get; set; } = float.MaxValue;
	public bool IsHurtAnimating => LastHurt != float.MaxValue && LastHurt <= HurtAnimation.TimeRange.y;

	[Property]
	public bool FadeIn { get; set; } = false;
	private float _fadeAnimation = 0.3f;
	public bool IsFadingIn => FadeIn && SinceSpawned <= _fadeAnimation; // Duration of fadein, no reason to make it editable.

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

	public bool Disabled { get; set; } = false;
	public Color OriginalTint;
	public float MaxHealth;
	public Vector3 MaxScale;
	public TimeSince SinceSpawned;

	protected override void OnStart()
	{
		base.OnStart();

		base.Tags.Set( "Unit", true ); // Give the Unit tag
		base.Tags.Set( UnitType.ToString(), true ); // Give tag of whatever unit type this is

		MaxHealth = Health;
		MaxScale = Transform.Scale;
		SinceSpawned = 0;

		if ( Renderer != null )
		{
			OriginalTint = Renderer.Tint;

			if ( FadeIn )
				Renderer.Tint = Renderer.Tint.WithAlpha( 0 ); // If we fade in, set alpha to 0
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Disabled ) return;

		if ( Renderer != null )
		{
			if ( IsHurtAnimating )
				HurtFX();

			if ( IsFadingIn )
				FadeFX();
		}

		if ( Renderer is SkinnedModelRenderer renderer )
		{
			var relativeHealth = MathX.Remap( Health, 0f, MaxHealth, 10f, 100f );
			var currentHealth = renderer.GetFloat( HealthAmountAnimation );

			if ( currentHealth != 0 )
				renderer.Set( HealthAmountAnimation, MathX.Lerp( currentHealth, relativeHealth, Time.Delta * 10f ) ); // Lerp size based on health instead of instantly setting it.
		}
	}

	public void Damage( float amount )
	{
		if ( Disabled ) return;

		Health = Math.Max( Health - amount, 0 );

		HurtFX();

		if ( Renderer is SkinnedModelRenderer renderer )
		{
			var relativeDamage = MathX.Remap( amount, 0f, MaxHealth, 0f, 100f ) * 3f;
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

	private void HurtFX()
	{
		Renderer.Tint = Color.Lerp( OriginalTint, HurtColor, HurtAnimation.Evaluate( LastHurt ) );
		Transform.Scale = MaxScale * MathX.Lerp( 1f, HurtScale, HurtAnimation.Evaluate( LastHurt ) );
	}

	private void FadeFX()
	{
		Renderer.Tint = Renderer.Tint.WithAlpha( SinceSpawned / _fadeAnimation ); // Fade in when spawned
	}
}
