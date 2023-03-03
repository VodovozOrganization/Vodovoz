using Gamma.ColumnConfig;
using System.Globalization;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class OrganizationCashTransferDocumentJournalRegistrar : ColumnsConfigRegistrarBase<OrganizationCashTransferDocumentJournalViewModel, OrganizationCashTransferDocumentJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<OrganizationCashTransferDocumentJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Дата создания").AddTextRenderer(node => node.DocumentDate.ToString("d"))
				.AddColumn("Автор").AddTextRenderer(node => node.Author)
				.AddColumn("Орг.откуда").AddTextRenderer(node => node.OrganizationFrom)
				.AddColumn("Орг.куда").AddTextRenderer(node => node.OrganizationTo)
				.AddColumn("Сумма").AddTextRenderer(node => node.TransferedSum.ToString(CultureInfo.CurrentCulture))
				.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
				.Finish();
	}
}
