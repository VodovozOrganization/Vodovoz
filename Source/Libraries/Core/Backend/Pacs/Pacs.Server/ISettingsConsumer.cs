using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	public interface ISettingsConsumer
	{
		void UpdateSettings(DomainSettings newSettings);
	}
}