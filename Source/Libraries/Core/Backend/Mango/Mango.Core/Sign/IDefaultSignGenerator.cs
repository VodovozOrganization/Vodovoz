namespace Mango.Core.Sign
{
	public interface IDefaultSignGenerator
	{
		string GetSign(string json);
	}
}
