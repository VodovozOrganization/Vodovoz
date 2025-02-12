using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.Representations;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class DebtorsJournalRegistrar : ColumnsConfigRegistrarBase<DebtorsJournalViewModel, DebtorJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DebtorJournalNode> config) =>
			config.AddColumn("Номер").AddTextRenderer(x => x.AddressId > 0 ? x.AddressId.ToString() : x.ClientId.ToString())
				.AddColumn("Клиент").AddTextRenderer(node => node.ClientName)
				.AddColumn("Адрес").AddTextRenderer(node => node.Address)
				.AddColumn("Кол-во точек доставки").AddTextRenderer(node => node.CountOfDeliveryPoint.ToString())
				.AddColumn("ОПФ").AddTextRenderer(node => node.OPF.GetEnumTitle())
				.AddColumn("Последний заказ по адресу").AddTextRenderer(node => node.LastOrderDate != null ? node.LastOrderDate.Value.ToString("dd / MM / yyyy") : string.Empty)
				.AddColumn("Кол-во отгруженных в последнюю реализацию бутылей").AddNumericRenderer(node => node.LastOrderBottles)
				.AddColumn("Долг по таре (по адресу)").AddNumericRenderer(node => node.DebtByAddress)
				.AddColumn("Долг по таре (по клиенту)").AddNumericRenderer(node => node.DebtByClient)
				.AddColumn("Фикс. цена").AddNumericRenderer(node => node.FixPrice)
				.AddColumn("Ввод остат.").AddTextRenderer(node => node.IsResidueExist)
				.AddColumn("Резерв").AddNumericRenderer(node => node.Reserve)
				.RowCells().AddSetter((CellRendererText c, DebtorJournalNode n) =>
				{
					c.ForegroundGdk = n.TaskId.HasValue
						? GdkColors.InsensitiveText
						: GdkColors.PrimaryText;
				})
				.Finish();
	}
}
