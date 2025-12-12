namespace KZLib.KZData
{
	public interface IConfig { }

	public interface IAffix
	{
		public void Initialize();
		public void Release();
		
		public void Set(IAffix newAfx);
		public void Update(IAffix newAfx);
	}

	public interface IProto
	{
		int Num { get; }
	}

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