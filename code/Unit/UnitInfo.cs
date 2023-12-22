public class UnitInfo : Component
{
	[Property]
	public float Health { get; set; } = 10f;

	[Property]
	public UnitType UnitType { get; set; } = UnitType.None;

	[Property]
	public ModelRenderer Renderer { get; set; }

	[Property]
	public List<UnitType> EnemyUnitTypes { get; set; }

	public float LastDamage { get; set; } = 0;
	public float HurtAnimationDuration => Math.Max( LastDamage / 10f, 0.05f );
	public TimeSince LastHurt { get; set; } = float.MaxValue;
	public bool HurtAnimation => LastHurt != float.MaxValue && LastHurt <= HurtAnimationDuration;


	protected override void OnStart()
	{
		base.OnStart();

		base.Tags.Set( "Unit", true ); // Give the Unit tag
		base.Tags.Set( UnitType.ToString(), true ); // Give tag of whatever unit type this is
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

		HurtFX();

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
