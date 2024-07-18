using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Zabbix.Sender
{
	public interface IZabbixSender
	{
		Task<bool> SendIsHealthyAsync(CancellationToken cancellationToken, bool isHealthy = true);
		void SetWorkerName(string workerName);
	}
}
