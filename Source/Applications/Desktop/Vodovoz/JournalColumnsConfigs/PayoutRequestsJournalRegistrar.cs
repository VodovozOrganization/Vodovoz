using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using QSProjectsLib;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PayoutRequestsJournalRegistrar : ColumnsConfigRegistrarBase<PayoutRequestsJournalViewModel, PayoutRequestJournalNode>
	{
		private static readonly Pixbuf _emptyImg = new Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), "Vodovoz.icons.common.empty16.png");
		private static readonly Pixbuf _fire = new Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), "Vodovoz.icons.common.fire16.png");

		public override IColumnsConfig Configure(FluentColumnsConfig<PayoutRequestJournalNode> config) =>
			config.AddColumn("")
				.AddPixbufRenderer((node) =>
					node.IsImidiatelyBill
					? _fire
					: _emptyImg)
				.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
					.XAlign(0.5f)
				.AddColumn("Дата создания")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Date.ToString())
					.XAlign(0.5f)
				.AddColumn("Дата платежа (план)")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n =>
						n.PaymentDatePlanned == null
						? "" 
						: n.PaymentDatePlanned.Value.ToString())
					.XAlign(0.5f)
				.AddColumn("Тип документа")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.PayoutRequestDocumentType.GetEnumTitle())
					.WrapWidth(155)
					.WrapMode(WrapMode.WordChar)
					.XAlign(0.5f)
				.AddColumn("Статус")
					.HeaderAlignment(0.5f)
					.AddEnumRenderer(n => n.PayoutRequestState)
					.XAlign(0.5f)
				.AddColumn("Автор")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.AuthorWithInitials)
					.XAlign(0.5f)
				.AddColumn("Подотчетное лицо /\r\n\tПоставщик")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => !string.IsNullOrWhiteSpace(n.AccountablePersonWithInitials) ? n.AccountablePersonWithInitials : n.CounterpartyName)
					.WrapWidth(200)
					.WrapMode(WrapMode.WordChar)
					.XAlign(0.5f)
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(n => CurrencyWorks.GetShortCurrencyString(n.Sum))
					.XAlign(0.5f)
				.AddColumn("Остаток на выдачу")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(n => CurrencyWorks.GetShortCurrencyString(n.SumResidue))
					.XAlign(0.5f)
				.AddColumn("Основание")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Basis)
					.WrapWidth(700)
					.WrapMode(WrapMode.WordChar)
				.AddColumn("Статья")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.ExpenseCategory)
					.WrapWidth(250)
					.WrapMode(WrapMode.WordChar)
				.AddColumn("Наличие чека")
				.AddToggleRenderer(n => n.HaveReceipt)
					.Editing(false)
					.XAlign(0.5f)
				.AddColumn("Дата выдачи")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.MoneyTransferDate.ToShortDateString())
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();
	}
}
