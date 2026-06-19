namespace KZLib.Data
{
	public interface IConfig { }

	public interface IFacet
	{
		public void Initialize();
		public void Release();

		public void Apply(IFacet newFct);
	}

	public interface IProto
	{
		int Num { get; }
	}

	public interface ICluster { }

	public enum EffectType
	{
		VisualEffect,
		SoundEffect,
	}
	
	public enum NetworkErrorResultType
	{
		None,
		Popup,
		Toast,
		Title,
	}
}