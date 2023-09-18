using Gamma.ColumnConfig;
using Gtk;
using System;
using Vodovoz.Infrastructure;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class BusinessTasksJournalRegistrar : ColumnsConfigRegistrarBase<BusinessTasksJournalViewModel, BusinessTaskJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<BusinessTaskJournalNode> config) =>
			config.AddColumn("№").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Статус").AddEnumRenderer(node => node.TaskStatus)
				.AddColumn("Клиент").AddTextRenderer(node => node.ClientName ?? string.Empty)
				.AddColumn("Адрес").AddTextRenderer(node => node.AddressName ?? "Самовывоз")
				.AddColumn("Долг по адресу").AddTextRenderer(node => node.DebtByAddress.ToString()).XAlign(0.5f)
				.AddColumn("Долг по клиенту").AddTextRenderer(node => node.DebtByClient.ToString()).XAlign(0.5f)
				.AddColumn("Телефоны").AddTextRenderer(node => node.DeliveryPointPhones == "+7" ? string.Empty : node.DeliveryPointPhones)
					.WrapMode(WrapMode.WordChar)
				.AddColumn("Ответственный").AddTextRenderer(node => node.AssignedEmployeeName ?? string.Empty)
				.AddColumn("Выполнить до").AddTextRenderer(node => node.Deadline.ToString("dd / MM / yyyy  HH:mm"))
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsTaskComplete
					? GdkColors.SuccessText
					: DateTime.Now > n.Deadline
						? GdkColors.DangerText
						: GdkColors.PrimaryText)
				.Finish();
	}
}
