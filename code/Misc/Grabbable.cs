using Sandbox;

public class Grabbable : Component
{
	[Property]
	public bool Throwable { get; set; } = false;

	public SnotPlayer Grabber { get; set; }
	public ModelRenderer Renderer { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		Renderer = GameObject.Components.Get<ModelRenderer>();
		Tags.Add( "Grab" );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
	}

	// true = has been grabbed
	public virtual bool OnGrab( SnotPlayer grabber )
	{
		if ( Grabber == null )
		{
			Log.Info( "I'VE BEEN GRABBED!" );
			Grabber = grabber;
			return true;
		}

		return false;
	}

	// true = has been released (placed/thrown)
	public virtual bool OnRelease()
	{
		if ( Grabber != null )
			return true;

		Log.Info( "No grabsie" );
		return false;
	}
}
