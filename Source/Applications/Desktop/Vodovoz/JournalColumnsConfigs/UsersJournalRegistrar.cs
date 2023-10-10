using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Users;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class UsersJournalRegistrar : ColumnsConfigRegistrarBase<UsersJournalViewModel, UserJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<UserJournalNode> config) =>
			config.AddColumn("Код")
					.AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Имя")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Логин")
					.AddTextRenderer(node => node.Login)
				.AddColumn("Id сотрудника")
					.AddTextRenderer(node => node.EmployeeId.HasValue ? node.EmployeeId.ToString() : string.Empty)
				.AddColumn("ФИО сотрудника")
					.AddTextRenderer(node => node.EmployeeFIO)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) =>
					{
						if(n.Deactivated)
						{
							c.ForegroundGdk = n.IsAdmin ? GdkColors.BabyBlue : GdkColors.InsensitiveText;
						}
						else
						{
							c.ForegroundGdk = n.IsAdmin ? GdkColors.InfoText : GdkColors.PrimaryText;
						}
					})
				.Finish();
	}
}
