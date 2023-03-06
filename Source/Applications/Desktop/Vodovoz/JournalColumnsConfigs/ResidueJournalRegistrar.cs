using Gamma.ColumnConfig;
using System.Globalization;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class ResidueJournalRegistrar : ColumnsConfigRegistrarBase<ResidueJournalViewModel, ResidueJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<ResidueJournalNode> config) =>
			config.AddColumn("Документ").AddTextRenderer(node => $"Ввод остатков №{node.Id}").SearchHighlight()
				.AddColumn("Дата").AddTextRenderer(node => node.DateString)
				.AddColumn("Контрагент").AddTextRenderer(node => node.Counterparty)
				.AddColumn("Точка доставки").AddTextRenderer(node => node.DeliveryPoint)
				.AddColumn("Автор").AddTextRenderer(node => node.Author)
				.AddColumn("Изменил").AddTextRenderer(node => node.LastEditor)
				.AddColumn("Послед. изменения").AddTextRenderer(node =>
					node.LastEditedTime != default ? node.LastEditedTime.ToString(CultureInfo.CurrentCulture) : string.Empty)
				.Finish();
	}
}
