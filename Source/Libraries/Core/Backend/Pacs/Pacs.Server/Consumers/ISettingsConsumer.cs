using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Consumers
{
	public interface ISettingsConsumer
	{
		void UpdateSettings(DomainSettings newSettings);
	}
}
