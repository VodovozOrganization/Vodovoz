using System;
using System.Linq;
using Gamma.GtkWidgets;
using MangoService;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Mango;

namespace Vodovoz.Views.Mango
{
	public partial class SubscriberSelectionView : DialogViewBase<SubscriberSelectionViewModel>
	{
		public SubscriberSelectionView(SubscriberSelectionViewModel model) : base(model)
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			if(ViewModel.dialogType == SubscriberSelectionViewModel.DialogType.AdditionalCall) {
				ForwardingToConsultationButton.Visible = true;
				ForwardingToConsultationButton.Clicked += Clicked_ForwardingToConsultationButton;
				ForwardingButton.Clicked += Clicked_ForwardingButton;
			} else if(ViewModel.dialogType == SubscriberSelectionViewModel.DialogType.Telephone) {
				ForwardingToConsultationButton.Visible = false;
				ForwardingButton.Label = "Позвонить";
				ForwardingButton.Clicked += Clicked_MakeCall;
			}

			ySearchTable.ColumnsConfig = ColumnsConfigFactory.Create<SearchTableEntity>()
				.AddColumn("Имя")
				.AddTextRenderer(entity => entity.Name).SearchHighlight()
				.AddColumn("Отдел")
				.AddTextRenderer(entity => entity.Department).SearchHighlight()
				.AddColumn("Номер")
				.AddTextRenderer(entity => entity.Extension).SearchHighlight()
				.AddColumn("Статус")
				.AddTextRenderer(entity => entity.Status ? "<span foreground=\"green\">☎</span>" : "<span foreground=\"red\">☎</span>", useMarkup: true)
				.Finish();
			ySearchTable.SetItemsSource<SearchTableEntity>(ViewModel.SearchTableEntities);
			ySearchTable.Selection.Changed += Selection_Changed;
			ySearchTable.RowActivated += SelectCursorRow_OrderYTreeView;
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			var row = ySearchTable.GetSelectedObject<SearchTableEntity>();
			ForwardingButton.Sensitive = ForwardingToConsultationButton.Sensitive = row?.Status == true;
		}

		private void SelectCursorRow_OrderYTreeView(object sender, EventArgs e)
		{
			ForwardingButton.Click();
		}

		protected void Clicked_MakeCall(object sender, EventArgs e)
		{
			var row = ySearchTable.GetSelectedObject<SearchTableEntity>();
			ViewModel.MakeCall(row);

		}

		protected void Clicked_ForwardingButton(object sender, EventArgs e)
		{
			var row = ySearchTable.GetSelectedObject<SearchTableEntity>();
			ViewModel.ForwardCall(row, ForwardingMethod.blind);
		}

		protected void Clicked_ForwardingToConsultationButton(object sender, EventArgs e)
		{
			var row = ySearchTable.GetSelectedObject<SearchTableEntity>();
			ViewModel.ForwardCall(row, ForwardingMethod.hold);
		}

		protected void Changed_FilterEntry(object sender, EventArgs args)
		{
			ySearchTable.SearchHighlightText = FilterEntry.Text;
			if(String.IsNullOrWhiteSpace(FilterEntry.Text)) {
				ySearchTable.SetItemsSource(ViewModel.SearchTableEntities);
			} else {
				string input = FilterEntry.Text.ToLower();
				ySearchTable.SetItemsSource(ViewModel.SearchTableEntities
					.Where(x => (x.Extension?.Contains(input) ?? false)
					 	|| (x.Name?.ToLower().Contains(input) ?? false)
						|| (x.Department?.ToLower().Contains(input) ?? false)
				).ToList());
			}
 		}

		protected void OnFilterEntryActivated(object sender, EventArgs e)
		{
			ySearchTable.Selection.SelectPath(new Gtk.TreePath("0"));
			if(ySearchTable.Selection.CountSelectedRows() > 0)
				ForwardingButton.Click();

		}
	}
}
