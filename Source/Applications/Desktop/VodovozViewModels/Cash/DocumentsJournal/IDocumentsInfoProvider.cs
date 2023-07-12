using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.ViewModels.Cash.DocumentsJournal
{
	public interface IDocumentsInfoProvider : IInfoProvider
	{
		DocumentsFilterViewModel DocumentsFilterViewModel  { get; }
	}
}
