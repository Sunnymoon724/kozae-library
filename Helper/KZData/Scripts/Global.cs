using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal class IsExternalInit { }
}

public enum NetworkErrorResultType
{
	None,
	Popup,
	Toast,
	Title,
}