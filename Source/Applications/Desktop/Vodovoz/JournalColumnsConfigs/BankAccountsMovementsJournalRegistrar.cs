using Core.Infrastructure;
using Gamma.ColumnConfig;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;

namespace Vodovoz.JournalColumnsConfigs
{
	/// <summary>
	/// Регистратор таблицы журнала движений по р/сч <see cref="BankAccountsMovementsJournalViewModel"/>
	/// При изменении структуры колонок необходимо также изменить их и в отчете <see cref="BankAccountsMovementsJournalReport"/>
	/// </summary>
	public class BankAccountsMovementsJournalRegistrar : ColumnsConfigRegistrarBase<BankAccountsMovementsJournalViewModel, BankAccountsMovementsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<BankAccountsMovementsJournalNode> config) =>
			config
				.AddColumn(BankAccountsMovementsJournalColumns.Id).AddTextRenderer(node => node.Id.HasValue ? node.Id.ToString() : null)
				.AddColumn(BankAccountsMovementsJournalColumns.StartDate).AddTextRenderer(x => x.StartDate.ToShortDateString())
				.AddColumn(BankAccountsMovementsJournalColumns.EndDate).AddTextRenderer(x => x.EndDate.ToShortDateString())
				.AddColumn(BankAccountsMovementsJournalColumns.Account).AddTextRenderer(x => x.Account)
				.AddColumn(BankAccountsMovementsJournalColumns.Bank).AddTextRenderer(x => x.Bank)
				.AddColumn(BankAccountsMovementsJournalColumns.Organization).AddTextRenderer(x => x.Organization)
				.AddColumn(BankAccountsMovementsJournalColumns.Empty)
					.AddEnumRenderer(x => x.AccountMovementDataType)
					.Editing(false)
				.AddColumn(BankAccountsMovementsJournalColumns.AmountFromDocument)
					.AddTextRenderer(x => x.Amount.HasValue ? x.Amount.ToString() : StringConstants.NotSet)
					.AddSetter((c, n) =>
					{
						c.BackgroundGdk = n.Amount == null ? GdkColors.DangerBase : GdkColors.PrimaryBase;
					})
				.AddColumn(BankAccountsMovementsJournalColumns.AmountFromProgram).AddTextRenderer(x =>
					x.AmountFromProgram.HasValue
						? x.AmountFromProgram.ToString()
						: null)
				.AddColumn(BankAccountsMovementsJournalColumns.Discrepancy).AddTextRenderer(x =>
					x.Difference.HasValue && x.Difference.Value != 0
						? x.Difference.ToString()
						: null)
					.AddSetter((c, n) =>
					{
						if(n.Difference.HasValue && n.Difference.Value != 0)
						{
							c.BackgroundGdk = GdkColors.DangerBase;
						}
						else
						{
							c.BackgroundGdk = GdkColors.PrimaryBase;
						}
					})
				.AddColumn(BankAccountsMovementsJournalColumns.DiscrepancyDescription).AddTextRenderer(x => x.GetDiscrepancyDescription())
				.Finish();
	}
}
