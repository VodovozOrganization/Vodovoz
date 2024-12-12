using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Counterparties;
using static Vodovoz.ViewModels.Counterparties.CallTaskJournalViewModel;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal class CallTaskJournalRegistrar : ColumnsConfigRegistrarBase<CallTaskJournalViewModel, CallTaskJournalNode>
	{
		private static readonly Pixbuf _emptyImg = new Pixbuf(typeof(Startup).Assembly, "Vodovoz.icons.common.empty16.png");
		private static readonly Pixbuf _fire = new Pixbuf(typeof(Startup).Assembly, "Vodovoz.icons.common.fire16.png");

		public override IColumnsConfig Configure(FluentColumnsConfig<CallTaskJournalNode> config) => 
			config.AddColumn("№").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Срочность").AddPixbufRenderer(node => node.ImportanceDegree == ImportanceDegreeType.Important && !node.IsTaskComplete ? _fire : _emptyImg)
				.AddColumn("Статус").AddEnumRenderer(node => node.TaskStatus)
				.AddColumn("Клиент").AddTextRenderer(node => node.ClientName ?? string.Empty).WrapWidth(500).WrapMode(WrapMode.WordChar)
				.AddColumn("Адрес").AddTextRenderer(node => node.AddressName ?? "Самовывоз").WrapWidth(500).WrapMode(WrapMode.WordChar)
				.AddColumn("Долг по адресу").AddTextRenderer(node => node.DebtByAddress.ToString()).XAlign(0.5f)
				.AddColumn("Долг по клиенту").AddTextRenderer(node => node.DebtByClient.ToString()).XAlign(0.5f)
				.AddColumn("Телефоны").AddTextRenderer(node => node.DeliveryPointPhones == "+7" ? string.Empty : node.DeliveryPointPhones)
					.WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Ответственный").AddTextRenderer(node => node.AssignedEmployeeName ?? string.Empty)
				.AddColumn("Выполнить до").AddTextRenderer(node => node.Deadline.ToString("dd / MM / yyyy  HH:mm"))
				.RowCells().AddSetter<CellRendererText>((c, n) =>
				{
					var color = GdkColors.PrimaryText;

					if(n.IsTaskComplete)
					{
						color = GdkColors.SuccessText;
					}

					if(DateTime.Now > n.Deadline)
					{
						color = GdkColors.DangerText;
					}

					c.ForegroundGdk = color;
				})
				.Finish();
	}
}
