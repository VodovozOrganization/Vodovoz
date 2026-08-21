using System;
using System.Linq;
using Gamma.GtkWidgets;
using Vodovoz.SidePanel.InfoProviders;
using VodovozBusiness.Nodes;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmailsPanelView : Gtk.Bin, IPanelView
	{
		public EmailsPanelView()
		{
			Build();
			
			ytreeviewEmails.ColumnsConfig = ColumnsConfigFactory.Create<SentStoredEmailNode>()
				.AddColumn("Дата")
					.AddTextRenderer(x => x.SentDate.ToString("dd.MM.yy HH:mm"))
				.AddColumn("Почта")
					.AddTextRenderer(x => x.RecipientAddress)
				.AddColumn("Статус")
					.AddEnumRenderer(x => x.State)
				.Finish();
		}

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public void Refresh()
		{
			IEmailsInfoProvider emailProvider = (InfoProvider as IEmailsInfoProvider);

			var emailsList = emailProvider.GetEmails();
			var haveEmails = emailsList != null && emailsList.Any();

			if(haveEmails) {
				ytreeviewEmails.SetItemsSource(emailsList);
			}

			labelUnsendedBill.Visible = !haveEmails;
			ytreeviewEmails.Visible = haveEmails;
		}

		public bool VisibleOnPanel {
			get{
				IEmailsInfoProvider emailProvider = (InfoProvider as IEmailsInfoProvider);
				return emailProvider != null && emailProvider.CanHaveEmails;
			}
		}

		public void OnCurrentObjectChanged(object changedObject)
		{
			Refresh();
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			Refresh();
		}

		#endregion
	}
}
