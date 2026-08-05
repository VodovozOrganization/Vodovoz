using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Zabbix.Sender
{
	public interface IZabbixSender
	{
		Task<bool> SendIsHealthyAsync(string workerName, CancellationToken cancellationToken);
		Task<bool> SendProblemMessageAsync(string workerName, ZabixSenderMessageType zabixSenderMessageType, string message, CancellationToken cancellationToken);
	}
}
