using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Domain.Payments;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PaymentsJournalRegistrar : ColumnsConfigRegistrarBase<PaymentsJournalViewModel, PaymentJournalNode>
	{
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
						var color = GdkColors.PrimaryBase;

						if(n.Status == PaymentState.undistributed)
						{
							color = GdkColors.Pink;
						}
						if(n.Status == PaymentState.distributed)
						{
							color = GdkColors.SuccessBase;
						}
						if(n.Status == PaymentState.Cancelled)
						{
							color = GdkColors.InsensitiveBase;
						}

						c.CellBackgroundGdk = color;
					})
				.Finish();
	}
}
