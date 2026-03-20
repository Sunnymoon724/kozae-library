
namespace KZLib.Data
{
	public interface IColorProto : IProto
	{
		string[] ColorArray { get; }
	}

	public interface IMotionProto : IProto
	{
		string StateName { get; }
		MotionEvent[] EventArray { get; }
	}

	public interface INetworkErrorProto : IProto
	{
		string Description { get; }
		NetworkErrorResultType ResultMainType { get; }
		NetworkErrorResultType ResultSubType { get; }
	}
}