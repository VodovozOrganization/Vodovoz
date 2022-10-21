using System;
namespace Vodovoz.Services
{
	public interface IVpbxSettings
	{
		string VpbxApiKey { get; }
		string VpbxApiSalt { get; }
	}
}
