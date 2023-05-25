using Gamma.ColumnConfig;
using Gamma.Utilities;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class NomenclaturesJournalRegistrar : ColumnsConfigRegistrarBase<NomenclaturesJournalViewModel, NomenclatureJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<NomenclatureJournalNode> config) =>
			config.AddColumn("Код")
					.AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Номенклатура")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Категория")
					.AddTextRenderer(node => node.Category.GetEnumTitle())
				.AddColumn("Кол-во")
					.AddTextRenderer(node => node.InStockText)
				.AddColumn("Зарезервировано")
					.AddTextRenderer(node => node.ReservedText)
				.AddColumn("Доступно")
					.AddTextRenderer(node => node.AvailableText)
					.AddSetter((cell, node) => cell.ForegroundGdk = node.Available > 0 ? GdkColors.BlackColor : GdkColors.RedColor2)
				.AddColumn("Код в ИМ")
					.AddTextRenderer(node => node.OnlineStoreExternalId)
				.Finish();
	}
}
