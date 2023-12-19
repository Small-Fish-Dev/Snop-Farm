public sealed class UnitInfo : Component
{
	[Property]
	public float Health { get; set; } = 10f;

	[Property]
	public UnitType UnitType { get; set; } = UnitType.None;

	[Property]
	public List<UnitType> EnemyUnitTypes { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		base.Tags.Set( "Unit", true ); // Give the Unit tag
		base.Tags.Set( UnitType.ToString(), true ); // Give tag of whatever unit type this is
	}

	public void Damage( float amount )
	{
		Health = Math.Max( Health - amount, 0 );

		if ( Health <= 0 )
			Kill();
	}

	public void Kill()
	{
		GameObject.Destroy();
	}
}
