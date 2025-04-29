using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Zabbix.Sender
{
	public interface IZabbixSender
	{
		Task<bool> SendIsHealthyAsync(CancellationToken cancellationToken);
		Task<bool> SendProblemMessageAsync(ZabixSenderMessageType zabixSenderMessageType, string message, CancellationToken cancellationToken);
	}
}
