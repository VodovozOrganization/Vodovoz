using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Payments;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;
using Vodovoz.Tools;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PaymentsJournalRegistrar : ColumnsConfigRegistrarBase<PaymentsJournalViewModel, PaymentJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<PaymentJournalNode> config) =>
			config.AddColumn(PaymentsJournalColumns.Id)
					.AddNumericRenderer(x => x.Id)
				.AddColumn(PaymentsJournalColumns.Number)
					.AddTextRenderer(x => x.PaymentNum.ToString())
				.AddColumn(PaymentsJournalColumns.Date)
					.AddTextRenderer(x => x.Date.ToShortDateString())
				.AddColumn(PaymentsJournalColumns.Total)
					.AddTextRenderer(x => x.Total.ToString())
				.AddColumn(PaymentsJournalColumns.Orders)
					.AddTextRenderer(x => x.Orders)
				.AddColumn(PaymentsJournalColumns.Payer)
					.AddTextRenderer(x => x.PayerName)
					.WrapWidth(300)
					.WrapMode(WrapMode.WordChar)
				.AddColumn(PaymentsJournalColumns.Counterparty)
					.AddTextRenderer(x => x.CounterpartyName)
					.WrapWidth(300)
					.WrapMode(WrapMode.WordChar)
				.AddColumn(PaymentsJournalColumns.Organization)
					.AddTextRenderer(x => x.Organization)
				.AddColumn(PaymentsJournalColumns.OrganizationBank)
					.AddTextRenderer(x => x.OrganizationBank)
					.WrapWidth(100)
					.WrapMode(WrapMode.WordChar)
				.AddColumn(PaymentsJournalColumns.OrganizationAccount)
					.AddTextRenderer(x => x.OrganizationAccountNumber)
				.AddColumn(PaymentsJournalColumns.Purpose)
					.AddTextRenderer(x => x.PaymentPurpose)
					.WrapWidth(600)
					.WrapMode(WrapMode.WordChar)
				.AddColumn(PaymentsJournalColumns.ProfitCategory)
					.AddTextRenderer(x => x.ProfitCategory)
					.XAlign(0.5f)
				.AddColumn(PaymentsJournalColumns.IsManuallyCreated)
					.AddToggleRenderer(x => x.IsManualCreated)
					.Editing(false)
				.AddColumn(PaymentsJournalColumns.UnAllocatedSum)
					.AddNumericRenderer(x => x.UnAllocatedSum)
					.Digits(2)
				.AddColumn(PaymentsJournalColumns.DocumentType)
					.AddTextRenderer(x => x.EntityType.GetClassUserFriendlyName().Nominative.CapitalizeSentence())
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
