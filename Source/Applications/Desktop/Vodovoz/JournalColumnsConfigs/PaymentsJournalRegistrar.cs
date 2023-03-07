using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using Vodovoz.Domain.Payments;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PaymentsJournalRegistrar : ColumnsConfigRegistrarBase<PaymentsJournalViewModel, PaymentJournalNode>
	{
		private static readonly Color _colorPink = new Color(0xff, 0xc0, 0xc0);
		private static readonly Color _colorWhite = new Color(0xff, 0xff, 0xff);
		private static readonly Color _colorLightGreen = new Color(0xc0, 0xff, 0xc0);
		private static readonly Color _colorLightGray = new Color(0xcc, 0xcc, 0xcc);

		public override IColumnsConfig Configure(FluentColumnsConfig<PaymentJournalNode> config) =>
			config.AddColumn("№")
					.AddTextRenderer(x => x.PaymentNum.ToString())
				.AddColumn("Дата")
					.AddTextRenderer(x => x.Date.ToShortDateString())
				.AddColumn("Cумма")
					.AddTextRenderer(x => x.Total.ToString())
				.AddColumn("Заказы")
					.AddTextRenderer(x => x.Orders)
				.AddColumn("Плательщик")
					.AddTextRenderer(x => x.PayerName)
					.WrapWidth(450)
					.WrapMode(WrapMode.WordChar)
				.AddColumn("Контрагент")
					.AddTextRenderer(x => x.CounterpartyName)
					.WrapWidth(450)
					.WrapMode(WrapMode.WordChar)
				.AddColumn("Получатель")
					.AddTextRenderer(x => x.Organization)
				.AddColumn("Назначение платежа")
					.AddTextRenderer(x => x.PaymentPurpose)
					.WrapWidth(600)
					.WrapMode(WrapMode.WordChar)
				.AddColumn("Категория дохода/расхода")
					.AddTextRenderer(x => x.ProfitCategory)
					.XAlign(0.5f)
				.AddColumn("Создан вручную?")
					.AddToggleRenderer(x => x.IsManualCreated)
					.Editing(false)
				.AddColumn("Нераспределенная сумма")
					.AddNumericRenderer(x => x.UnAllocatedSum)
					.Digits(2)
				.AddColumn("")
				.RowCells().AddSetter<CellRenderer>(
					(c, n) =>
					{
						var color = _colorWhite;

						if(n.Status == PaymentState.undistributed)
						{
							color = _colorPink;
						}
						if(n.Status == PaymentState.distributed)
						{
							color = _colorLightGreen;
						}
						if(n.Status == PaymentState.Cancelled)
						{
							color = _colorLightGray;
						}

						c.CellBackgroundGdk = color;
					})
				.Finish();
	}
}
