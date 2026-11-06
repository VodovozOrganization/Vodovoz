using System.Threading;
using System.Threading.Tasks;

namespace Mindbox.CounterpartiesAdmin
{
	/// <summary>
	/// Отчёт по статусам и повторная отправка контрагентов
	/// </summary>
	public interface IMindboxAdminService
	{
		Task<int> ReportAsync(MindboxAdminOptions options, CancellationToken cancellationToken);
		Task<int> ResendAsync(MindboxAdminOptions options, CancellationToken cancellationToken);
	}
}
