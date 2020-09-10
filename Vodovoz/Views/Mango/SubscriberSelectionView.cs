using System;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Mango;
using Gamma.GtkWidgets;
using System.Collections.Generic;
using System.Linq;
using MangoService;

namespace Vodovoz.Views.Mango
{
	public partial class SubscriberSelectionView : DialogViewBase<SubscriberSelectionViewModel>
	{
		private string extension { get; set; }
		public SubscriberSelectionView(SubscriberSelectionViewModel model) : base(model)
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			if(ViewModel.dialogType == SubscriberSelectionViewModel.DialogType.AdditionalCall) {
				Header_Label.Text = "Доплнительный звонок";
				ForwardingToConsultationButton.Visible = true;
				ForwardingToConsultationButton.Sensitive = true;

				ForwardingToConsultationButton.Clicked += Clicked_ForwardingToConsultationButton;
				ForwardingButton.Clicked += Clicked_ForwardingButton;
			} else if(ViewModel.dialogType == SubscriberSelectionViewModel.DialogType.Telephone) {
				Header_Label.Text = "Телефон";
				ForwardingToConsultationButton.Visible = false;
				ForwardingToConsultationButton.Sensitive = false;
				ForwardingButton.Label = "Позвонить";

				ForwardingButton.Clicked += Clicked_MakeCall;
			}

			ySearchTable.ColumnsConfig = ColumnsConfigFactory.Create<SearchTableEntity>()
				.AddColumn("Имя")
				.AddTextRenderer(entity => entity.Name)
				.AddColumn("Отдел")
				.AddTextRenderer(entity => entity.Department)
				.AddColumn("Номер")
				.AddTextRenderer(entity => entity.Extension)
				.AddColumn("Статус")
				.AddTextRenderer(entity => entity.Status ? "<span foreground=\"green\">●</span>" : "<span foreground=\"red\">●</span>", useMarkup: true)
				.Finish();
			ySearchTable.SetItemsSource<SearchTableEntity>(ViewModel.SearchTableEntities);

			ySearchTable.RowActivated += SelectCursorRow_OrderYTreeView;
		}

		private void SelectCursorRow_OrderYTreeView(object sender, EventArgs e)
		{
			var row = ySearchTable.GetSelectedObject<SearchTableEntity>();
			if(row.Status == true)
				extension = row.Extension;
		}

		protected void Clicked_MakeCall(object sender, EventArgs e)
		{
			if(!String.IsNullOrWhiteSpace(extension))
				ViewModel.MakeCall(extension);

		}

		protected void Clicked_ForwardingButton(object sender, EventArgs e)
		{
			if(!String.IsNullOrWhiteSpace(extension))
				ViewModel.ForwardCall(extension, ForwardingMethod.blind);
		}

		protected void Clicked_ForwardingToConsultationButton(object sender, EventArgs e)
		{
			if(!String.IsNullOrWhiteSpace(extension))
				ViewModel.ForwardCall(extension, ForwardingMethod.hold);
		}

		protected void Clicked_RollUpButton(object sender, EventArgs e)
		{
			
		}

		private bool isFistTime = true;
		private int filterStringLength = 0;

		protected void Changed_FilterEntry(object sender, EventArgs args)
		{

			string input = ((yEntry)sender).Text;
			if(input.Length > filterStringLength && !isFistTime) {
				var array = ViewModel.SearchTableEntities
				.Where(e =>
					   (!String.IsNullOrWhiteSpace(e.Name) && e.Name.IndexOf(input) != -1)
					|| (!String.IsNullOrWhiteSpace(e.Department) && e.Department.IndexOf(input) != -1)
					|| (!String.IsNullOrWhiteSpace(e.Department) && e.Extension.IndexOf(input) != -1))
					.OrderBy(en => en.Name).ToList();
				filterStringLength = input.Length;
				ViewModel.LocalEntities = array;

			} else if(input.Length < filterStringLength || isFistTime) {
				var array = ViewModel.SearchTableEntities
				.Where(e => 
					   (!String.IsNullOrWhiteSpace(e.Name) && e.Name.IndexOf(input) != -1 )
					|| (!String.IsNullOrWhiteSpace(e.Department) && e.Department.IndexOf(input) != -1) 
					|| (!String.IsNullOrWhiteSpace(e.Department) && e.Extension.IndexOf(input) != -1))
					.OrderBy(en => en.Name).ToList();
				filterStringLength = input.Length;
				isFistTime = false;
				ViewModel.LocalEntities = array;
			} 

			if(String.IsNullOrWhiteSpace(input)) {
				isFistTime = true;
			}
			Update_SearchTree();
 		}

		private void Update_SearchTree() 
		{
			if(String.IsNullOrWhiteSpace(FilterEntry.Text)) {
				ySearchTable.SetItemsSource(ViewModel.SearchTableEntities);
				ViewModel.LocalEntities = new List<SearchTableEntity>();
			}
			else
				ySearchTable.SetItemsSource(ViewModel.LocalEntities);
			ySearchTable.ShowAll();

		}
	}
}
