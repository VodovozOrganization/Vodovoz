using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class EmployeesJournalRegistrar : ColumnsConfigRegistrarBase<EmployeesJournalViewModel, EmployeeJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<EmployeeJournalNode> config) =>
			config.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Ф.И.О.")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.FullName)
					.WrapMode(WrapMode.WordChar)
					.WrapWidth(600)
				.AddColumn("Категория")
					.MinWidth(200)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.EmpCatEnum.GetEnumTitle())
				.AddColumn("Статус")
					.AddEnumRenderer(n => n.Status)
				.RowCells()
					.AddSetter<CellRendererText>((c, n) =>
					{
						c.ForegroundGdk = n.Status == EmployeeStatus.IsFired
							? GdkColors.InsensitiveText
							: GdkColors.PrimaryText;
					})
				.Finish();
	}
}
