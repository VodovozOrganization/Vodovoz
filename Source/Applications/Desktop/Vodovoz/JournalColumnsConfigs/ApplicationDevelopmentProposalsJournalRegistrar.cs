using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using Gtk;
using System.Globalization;
using Vodovoz.Domain.Proposal;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Proposal;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class ApplicationDevelopmentProposalsJournalRegistrar : ColumnsConfigRegistrarBase<ApplicationDevelopmentProposalsJournalViewModel, ApplicationDevelopmentProposalsJournalNode>
	{
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorRed = new Color(0xfe, 0x5c, 0x5c);

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
						c.ForegroundGdk = n.Status == ApplicationDevelopmentProposalStatus.Rejected ? _colorRed : _colorBlack)
				.Finish();
	}
}
