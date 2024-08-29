using System;
using System.ComponentModel;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Tdi;
using QS.Views.Dialog;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	[ToolboxItem(true)]
	public partial class RequestForCallView : DialogViewBase<RequestForCallViewModel>
	{
		public RequestForCallView(RequestForCallViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnCreateOrder.Clicked += OnCreateOrderClicked;
			btnCreateOrder.Binding
				.AddBinding(ViewModel, vm => vm.CanCreateOrder, w => w.Sensitive)
				.InitializeFromSource();
			
			btnGetToWork.BindCommand(ViewModel.GetToWorkCommand);
			btnCloseRequestForCall.BindCommand(ViewModel.CloseRequestCommand);
			btnCancel.BindCommand(ViewModel.CancelCommand);

			lblIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();
			lblId.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IdToString, w => w.LabelProp)
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();

			lblStatus.Binding
				.AddBinding(ViewModel, vm => vm.Status, w => w.LabelProp)
				.InitializeFromSource();

			lblContactPhone.Selectable = true;
			lblContactPhone.Binding
				.AddBinding(ViewModel.Entity, e => e.Phone, w => w.LabelProp)
				.InitializeFromSource();
			
			lblAuthor.Binding
				.AddBinding(ViewModel.Entity, e => e.Author, w => w.LabelProp)
				.InitializeFromSource();
			
			lblEmployeeWorkWith.Binding
				.AddBinding(ViewModel, vm => vm.EmployeeWorkWith, w => w.LabelProp)
				.InitializeFromSource();

			lblOrder.Selectable = true;
			lblOrder.Binding
				.AddBinding(ViewModel, vm => vm.Order, w => w.LabelProp)
				.InitializeFromSource();

			nomenclatureEntry.ViewModel = ViewModel.NomenclatureEntryViewModel;
			closedReasonEntry.ViewModel = ViewModel.ClosedReasonEntryViewModel;
		}

		private void OnCreateOrderClicked(object sender, EventArgs e)
		{
			(ViewModel.NavigationManager as ITdiCompatibilityNavigation).OpenTdiTab<OrderDlg>(
				ViewModel,
				OpenPageOptions.AsSlave,
				tab =>
				{
					tab.EntitySaved += TabOnEntitySaved;

					void TabOnEntitySaved(object o, EntitySavedEventArgs entitySavedEventArgs)
					{
						ViewModel.AttachOrder(entitySavedEventArgs.Entity.GetId());
						ViewModel.SaveAndClose();
						tab.EntitySaved -= TabOnEntitySaved;
					}
				});
		}
	}
}
