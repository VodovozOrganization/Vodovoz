using Gamma.ColumnConfig;
using QS.Utilities;
using QS.Utilities.Text;
using Vodovoz.JournalColumnsConfigs;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Cash.DocumentsJournal;
using static Vodovoz.ViewModels.Cash.DocumentsJournal.DocumentsJournalViewModel;

namespace Vodovoz.Cash.DocumentsJournal
{
	internal sealed class DocumentsJournalRegistrar : ColumnsConfigRegistrarBase<DocumentsJournalViewModel, DocumentNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DocumentNode> config) =>
			config.AddColumn("№ РКО/ПКО").AddTextRenderer(node => node.Id.ToString())
				  .AddColumn("Тип документа").AddTextRenderer(node => node.EntityType.GetClassUserFriendlyName().Nominative)
				  .AddColumn("Дата").AddTextRenderer(node => node.Date.ToShortDateString())
				  .AddColumn("Сотрудник").AddTextRenderer(node => PersonHelper.PersonNameWithInitials(node.EmployeeSurname, node.EmployeeName, node.EmployeePatronymic))
				  .AddColumn("Статья").AddTextRenderer(node => node.Category)
				  .AddColumn("Сумма").AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.Money))
				  .AddColumn("Кассир").AddTextRenderer(node => PersonHelper.PersonNameWithInitials(node.CasherSurname, node.CasherName, node.CasherPatronymic))
				  .AddColumn("Основание").AddTextRenderer(node => node.Description)
				  .Finish();
	}
}
