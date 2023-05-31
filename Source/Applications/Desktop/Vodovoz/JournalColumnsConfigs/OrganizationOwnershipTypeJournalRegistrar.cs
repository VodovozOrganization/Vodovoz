using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes.Organizations;
using Vodovoz.ViewModels.Journals.JournalViewModels.Organizations;
using VodovozInfrastructure.Extensions;

namespace Vodovoz.JournalColumnsConfigs
{
	internal class OrganizationOwnershipTypeJournalRegistrar : ColumnsConfigRegistrarBase<OrganizationOwnershipTypeJournalViewModel, OrganizationOwnershipTypeJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<OrganizationOwnershipTypeJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Аббревиатура").AddTextRenderer(n => n.Abbreviation)
				.AddColumn("Полное название").AddTextRenderer(n => n.FullName)
				.AddColumn("В архиве").AddTextRenderer(n => n.IsArchive.ConvertToYesOrNo())
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? GdkColors.DarkGrayColor : GdkColors.BlackColor)
				.Finish();
	}
}
