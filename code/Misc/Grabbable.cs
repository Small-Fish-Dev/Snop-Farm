using Sandbox;

public class Grabbable : Component
{
	[Property]
	public bool Throwable { get; set; } = false;

	public SnotPlayer Grabber { get; set; }
	public ModelRenderer Renderer { get; set; }
	public bool Grabbed => Grabber != null;

	protected override void OnStart()
	{
		base.OnStart();

		Renderer = GameObject.Components.Get<ModelRenderer>();
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
			Log.Info( "I'VE BEEN GRABBED!" );
			Grabber = grabber;

			if ( Renderer != null )
				Renderer.Tint = Renderer.Tint.WithAlpha( 0.5f );

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

			return true;
		}

		return false;
	}
}
