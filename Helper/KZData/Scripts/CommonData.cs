
namespace KZLib.Data
{
	public interface IColorProto : IProto
	{
		string[] ColorArray { get; }
	}

	public interface IMotionProto : IProto
	{
		string StateName { get; }
		MotionEntry[] MotionEntryArray { get; }
	}

	public interface IBuffProto : IProto
	{
		public string BuffName { get; }
		public float Duration { get; }
		public int MaxStackCount { get; }
		public BuffEntry[] BuffEntryArray { get; }
	}

	public interface INetworkErrorProto : IProto
	{
		string Description { get; }
		NetworkErrorResultType ResultMainType { get; }
		NetworkErrorResultType ResultSubType { get; }
	}
}