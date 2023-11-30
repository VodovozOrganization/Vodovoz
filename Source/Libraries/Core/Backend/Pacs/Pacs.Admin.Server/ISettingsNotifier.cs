using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Admin.Server
{
	public interface ISettingsNotifier
	{
		Task SettingsChanged(DomainSettings settings);
	}
}
