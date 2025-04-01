namespace KZLib.KZData
{
	public interface IConfig { }

	public interface ICluster { }

	public interface IClusterParam
	{
		string Key { get; }
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
}