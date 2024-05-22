using System.Threading.Tasks;

namespace Vodovoz.Zabbix.Sender
{
	public interface IZabbixSender
	{
		Task SendIsHealthyAsync(bool isHealthy);
	}
}
