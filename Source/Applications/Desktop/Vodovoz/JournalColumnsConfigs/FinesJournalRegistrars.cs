using Gamma.ColumnConfig;
using System.Globalization;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Employees;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FinesJournalRegistrars : ColumnsConfigRegistrarBase<FinesJournalViewModel, FineJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FineJournalNode> config) =>
			config.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
				.AddColumn("Сотудники").AddTextRenderer(node => node.EmployeesName)
				.AddColumn("Сумма штрафа").AddTextRenderer(node => node.FineSumm.ToString(CultureInfo.CurrentCulture))
				.AddColumn("Причина штрафа").AddTextRenderer(node => node.FineReason)
				.Finish();
	}
}
