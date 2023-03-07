using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Users;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class UsersJournalRegistrar : ColumnsConfigRegistrarBase<UsersJournalViewModel, UserJournalNode>
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorDarkGray = new Color(0x80, 0x80, 0x80);
		private static readonly Color _colorBlue = new Color(0x00, 0x18, 0xf9);
		private static readonly Color _colorBabyBlue = new Color(0x89, 0xcf, 0xef);

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
							c.ForegroundGdk = n.IsAdmin ? _colorBabyBlue : _colorDarkGray;
						}
						else
						{
							c.ForegroundGdk = n.IsAdmin ? _colorBlue : _colorBlack;
						}
					})
				.Finish();
	}
}
