using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using Gtk;
using System.Globalization;
using Vodovoz.Domain.Proposal;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Proposal;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class ApplicationDevelopmentProposalsJournalRegistrar : ColumnsConfigRegistrarBase<ApplicationDevelopmentProposalsJournalViewModel, ApplicationDevelopmentProposalsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<ApplicationDevelopmentProposalsJournalNode> config) =>
			config.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Id.ToString())
					.XAlign(0.5f)
				.AddColumn("Дата создания")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.CreationDate.ToString(CultureInfo.CurrentCulture))
					.XAlign(0.5f)
				.AddColumn("Тема")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Subject)
					.XAlign(0.5f)
				.AddColumn("Статус")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Status.GetEnumTitle())
					.XAlign(0.5f)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) =>
						c.ForegroundGdk = n.Status == ApplicationDevelopmentProposalStatus.Rejected ? GdkColors.RedColor2 : GdkColors.BlackColor)
				.Finish();
	}
}
