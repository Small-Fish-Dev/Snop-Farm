using Sandbox;

public class Grabbable : Component
{
	[Property]
	public bool Throwable { get; set; } = false;

	public SnotPlayer Grabber { get; set; }
	public ModelRenderer Renderer { get; set; }
	public Collider Collider { get; set; }
	public Rigidbody Rigidbody { get; set; }
	public UnitInfo UnitInfo { get; set; }
	public bool Grabbed => Grabber != null;

	protected override void OnStart()
	{
		base.OnStart();

		Renderer = GameObject.Components.Get<ModelRenderer>();
		Collider = GameObject.Components.Get<Collider>();
		Rigidbody = GameObject.Components.Get<Rigidbody>();
		UnitInfo = GameObject.Components.Get<UnitInfo>();
		Tags.Add( "Grab" );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( Grabbed )
		{
			Transform.Position = Grabber.Transform.Position + Grabber.Transform.Rotation.Forward * 45f;
			Transform.Rotation = Grabber.Transform.Rotation;
		}
	}

	// true = has been grabbed
	public virtual bool OnGrab( SnotPlayer grabber )
	{
		if ( Grabber == null )
		{
			Grabber = grabber;

			if ( Renderer != null )
				Renderer.Tint = Renderer.Tint.WithAlpha( 0.5f );

			if ( Collider != null )
				Collider.Enabled = false;

			if ( Rigidbody != null )
				Rigidbody.Enabled = false;

			if ( UnitInfo != null )
				UnitInfo.Disabled = true;

			grabber.Grabbed = this;

			return true;
		}

		return false;
	}

	// true = has been released (placed/thrown)
	public virtual bool OnRelease()
	{
		if ( Grabber != null )
		{
			Grabber = null;

			if ( Renderer != null )
				Renderer.Tint = Renderer.Tint.WithAlpha( 1f );

			if ( Collider != null )
				Collider.Enabled = true;

			if ( Rigidbody != null )
				Rigidbody.Enabled = true;

			if ( UnitInfo != null )
				UnitInfo.Disabled = false;

			return true;
		}

		return false;
	}
}
