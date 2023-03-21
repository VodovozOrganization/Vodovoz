using Gamma.ColumnConfig;
using QSProjectsLib;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class SalaryByEmployeeJournalRegistrar : ColumnsConfigRegistrarBase<SalaryByEmployeeJournalViewModel, EmployeeWithLastWorkingDayJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<EmployeeWithLastWorkingDayJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Ф.И.О.").AddTextRenderer(node => node.FullName)
				.AddColumn("Категория").AddEnumRenderer(node => node.EmpCatEnum)
				.AddColumn("Статус").AddEnumRenderer(node => node.Status)
				.AddColumn("Подразделение").AddTextRenderer(node => node.SubdivisionTitle)
				.AddColumn("Баланс").AddNumericRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Balance)).Digits(2)
				.AddColumn("Комментарий по сотруднику").AddTextRenderer(node => node.EmployeeComment)
				.AddColumn("Последний рабочий день").AddTextRenderer(node => node.LastWorkingDayString)
				.AddColumn("")
				.Finish();
	}
}
