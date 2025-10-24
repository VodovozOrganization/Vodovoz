using Gamma.ColumnConfig;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;

namespace Vodovoz.JournalColumnsConfigs
{
	public class BankAccountsMovementsJournalRegistrar : ColumnsConfigRegistrarBase<BankAccountsMovementsJournalViewModel, BankAccountsMovementsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<BankAccountsMovementsJournalNode> config) =>
			config
				.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Дата начала").AddTextRenderer(x => x.StartDate.ToShortDateString())
				.AddColumn("Дата окончания").AddTextRenderer(x => x.EndDate.ToShortDateString())
				.AddColumn("Р/сч").AddTextRenderer(x => x.Account)
				.AddColumn("Банк").AddTextRenderer(x => x.Bank)
				.AddColumn("").AddTextRenderer(x => x.Name)
				.AddColumn("Сумма из выгрузки").AddNumericRenderer(x => x.Amount).Digits(2)
				.AddColumn("Сумма из ДВ").AddTextRenderer(x =>
					x.AmountFromProgram.HasValue
						? x.AmountFromProgram.ToString()
						: null)
				.AddColumn("Расхождение").AddTextRenderer(x =>
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
				.AddColumn("Описание").AddTextRenderer(x => x.GetDiscrepancyDescription())
				.Finish();
	}
}
