using System.Threading;
using System.Threading.Tasks;

namespace Mango.Business.Interfaces
{
	public interface ICallStatisticService
	{
		Task LoadDataAsync(CancellationToken cancellationToken);
	}
}
