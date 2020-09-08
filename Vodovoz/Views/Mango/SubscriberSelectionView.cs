using System;
using QS.Views.Dialog;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.ViewModels.Mango;
using Vodovoz.Domain.Orders;
using Gamma.GtkWidgets;
using Gtk;
using Vodovoz.Domain.Client;
using FluentNHibernate.Data;
using System.Collections.Generic;
using ClientMangoService.DTO.Users;
using System.Linq;

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
			}
			else if(ViewModel.dialogType == SubscriberSelectionViewModel.DialogType.Telephone) {
				Header_Label.Text = "Телефон";
				ForwardingToConsultationButton.Visible = false;
				ForwardingToConsultationButton.Sensitive = false;
				ForwardingButton.Label = "Позвонить";

				ForwardingButton.Clicked += Clicked_MakeCall;
			}

			ySearchTable.ColumnsConfig = ColumnsConfigFactory.Create<User>()
				.AddColumn("Сотрудник")
				.AddTextRenderer(user => user.general.name)
				.AddColumn("Отдел")
				.AddTextRenderer(user => user.general.department)
				.AddColumn("Номер")
				.AddTextRenderer(user => user.telephony.extension)
				.AddColumn("Статус")
				.AddTextRenderer(user => user.telephony.numbers.Any(x => x.status == "on") ? "<span foreground=\"green\">●</span>" : "<span foreground=\"red\">●</span>", useMarkup:true)
				.Finish();
			ySearchTable.SetItemsSource<User>(ViewModel.Users);

			ySearchTable.RowActivated += SelectCursorRow_OrderYTreeView;
		}

		private void SelectCursorRow_OrderYTreeView(object sender, EventArgs e)
		{
			var row = ySearchTable.GetSelectedObject<User>();
			if(row.telephony.numbers.Any(x => x.status == "on"))
				extension = row.telephony.extension;
		}

		protected void Clicked_MakeCall(object sender , EventArgs e)
		{
			if(!String.IsNullOrWhiteSpace(extension))
				ViewModel.MakeCall(extension);

		}

		protected void Clicked_ForwardingButton(object sender, EventArgs e)
		{
			if(!String.IsNullOrWhiteSpace(extension))
				ViewModel.ForwardCall(extension, SubscriberSelectionViewModel.ForwardingMethod.blind);
		}

		protected void Clicked_ForwardingToConsultationButton(object sender, EventArgs e)
		{
			if(!String.IsNullOrWhiteSpace(extension))
				ViewModel.ForwardCall(extension, SubscriberSelectionViewModel.ForwardingMethod.hold);
		}

		protected void Table_SizeRequested(object o, EventArgs args)
		{
			Console.WriteLine($"{this.WidthRequest} : {this.HeightRequest}");

		}
	}

}
