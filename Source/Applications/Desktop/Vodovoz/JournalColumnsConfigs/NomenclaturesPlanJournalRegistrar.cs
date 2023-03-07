using Gamma.ColumnConfig;
using Gamma.Utilities;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class NomenclaturesPlanJournalRegistrar : ColumnsConfigRegistrarBase<NomenclaturesPlanJournalViewModel, NomenclaturePlanJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<NomenclaturePlanJournalNode> config) =>
			config.AddColumn("Код")
					.AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Номенклатура")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Категория")
					.AddTextRenderer(node => node.Category.GetEnumTitle())
				.AddColumn("Код в ИМ")
					.AddTextRenderer(node => node.OnlineStoreExternalId)
				.AddColumn("План день")
					.AddTextRenderer(node => node.PlanDay.ToString())
				.AddColumn("План месяц")
					.AddTextRenderer(node => node.PlanMonth.ToString())
				.Finish();
	}
}
