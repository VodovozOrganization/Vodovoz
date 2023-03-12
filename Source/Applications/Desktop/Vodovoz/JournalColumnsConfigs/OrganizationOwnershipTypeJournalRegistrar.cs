using Gamma.ColumnConfig;
using Gtk;
using Gdk;
using Vodovoz.ViewModels.Journals.JournalNodes.Organizations;
using Vodovoz.ViewModels.Journals.JournalViewModels.Organizations;

namespace Vodovoz.JournalColumnsConfigs
{
	internal class OrganizationOwnershipTypeJournalRegistrar : ColumnsConfigRegistrarBase<OrganizationOwnershipTypeJournalViewModel, OrganizationOwnershipTypeJournalNode>
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorDarkGray = new Color(0x80, 0x80, 0x80);

		public override IColumnsConfig Configure(FluentColumnsConfig<OrganizationOwnershipTypeJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Аббревиатура").AddTextRenderer(n => n.Abbreviation)
				.AddColumn("Полное название").AddTextRenderer(n => n.FullName)
				.AddColumn("В архиве").AddTextRenderer(n => n.IsArchive ? "Да" : "Нет")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? _colorDarkGray : _colorBlack)
				.Finish();
	}
}
