using System.Threading;
using System.Threading.Tasks;

namespace TrueMark.Codes.Pool
{
	public interface ITrueMarkCodesPool
	{
		void PutCode(int codeId);
		Task PutCodeAsync(int codeId, CancellationToken cancellationToken);
		int TakeCode(string gtin);
		Task<int> TakeCode(string gtin, CancellationToken cancellationToken);
	}
}
