using System.Threading;
using System.Threading.Tasks;

namespace Mango.Business.Interfaces
{
	public interface ICallStatisticService
	{
		/// <summary>
		/// Загрузить данные
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task LoadDataAsync(CancellationToken cancellationToken);
	}
}
