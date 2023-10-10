namespace Mango.Core.Sign
{
	public interface ISignGenerator
	{
		string GetSign(string vpbxApiKey, string vpbxApiSalt, string json);
	}
}
