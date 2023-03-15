using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PremiumJournalRegistrar : ColumnsConfigRegistrarBase<PremiumJournalViewModel, PremiumJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<PremiumJournalNode> config) =>
			config.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
				.AddColumn("Тип").AddTextRenderer(node => node.ViewType)
					.WrapMode(WrapMode.WordChar)
					.WrapWidth(100)
				.AddColumn("Сотрудники").AddTextRenderer(node => node.EmployeesName)
				.AddColumn("Сумма премии").AddNumericRenderer(node => node.PremiumSum)
				.AddColumn("Причина премии").AddTextRenderer(node => node.PremiumReason)
				.Finish();
	}
}
