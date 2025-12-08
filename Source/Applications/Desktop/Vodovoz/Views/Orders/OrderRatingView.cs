using Gamma.ColumnConfig;
using QS.DomainModel.Entity;
using QS.Views.Dialog;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class OrderRatingView : DialogViewBase<OrderRatingViewModel>
	{
		public OrderRatingView(OrderRatingViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.BindCommand(ViewModel.SaveAndCloseCommand);
			btnCancel.BindCommand(ViewModel.CloseCommand);
			
			btnOpenOrder.BindCommand(ViewModel.OpenOrderCommand);
			btnOpenOrder.Binding
				.AddBinding(ViewModel, vm => vm.OrderIsNotNull, w => w.Visible)
				.InitializeFromSource();
			
			btnOpenOnlineOrder.BindCommand(ViewModel.OpenOnlineOrderCommand);
			btnOpenOnlineOrder.Binding
				.AddBinding(ViewModel, vm => vm.OnlineOrderIsNotNull, w => w.Visible)
				.InitializeFromSource();
			btnProcess.BindCommand(ViewModel.ProcessCommand);
			
			btnCreateComplaint.BindCommand(ViewModel.CreateComplaintCommand);
			btnCreateComplaint.Binding
				.AddBinding(ViewModel, vm => vm.CreateOrOpenComplaint, w => w.Label)
				.InitializeFromSource();
			
			lblIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();
			
			lblId.Selectable = true;
			lblId.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IdToString, w => w.LabelProp)
				.AddBinding(vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();

			lblOrderlIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.OrderIsNotNull, w => w.Visible)
				.InitializeFromSource();
			
			lblOrderId.Selectable = true;
			lblOrderId.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderIdString, w => w.LabelProp)
				.AddBinding(vm => vm.OrderIsNotNull, w => w.Visible)
				.InitializeFromSource();
			
			lblOnlineOrderlIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.OnlineOrderIsNotNull, w => w.Visible)
				.InitializeFromSource();
			
			lblOnlineOrderId.Selectable = true;
			lblOnlineOrderId.Binding
				.AddBinding(ViewModel, o => o.OnlineOrderIdString, w => w.LabelProp)
				.AddBinding(ViewModel, o => o.OnlineOrderIsNotNull, w => w.Visible)
				.InitializeFromSource();
			
			lblRating.Binding
				.AddBinding(ViewModel.Entity, e => e.Rating, w => w.LabelProp, new IntToStringConverter())
				.InitializeFromSource();

			ConfigureTreeReasons();

			txtViewComment.Editable = false;
			txtViewComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();
			
			lblProcessedByTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowProcessedBy, w => w.Visible)
				.InitializeFromSource();
			
			lblProcessedBy.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ProcessedBy, w => w.LabelProp)
				.AddBinding(vm => vm.CanShowProcessedBy, w => w.Visible)
				.InitializeFromSource();
		}

		private void ConfigureTreeReasons()
		{
			treeViewReasons.ColumnsConfig = FluentColumnsConfig<INamedDomainObject>.Create()
				.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Причина").AddTextRenderer(node => node.Name)
				.AddColumn("")
				.Finish();

			treeViewReasons.ItemsDataSource = ViewModel.OrderRatingReasons;
		}
	}
}
