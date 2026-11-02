using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Dialogs.Email;
using VodovozBusiness.Nodes;
using Gamma.Utilities;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;

namespace Vodovoz.Views.Orders.OrdersWithoutShipment
{
	[ToolboxItem(true)]
	public partial class OrderWithoutShipmentForDebtView : TabViewBase<OrderWithoutShipmentForDebtViewModel>
	{
		public OrderWithoutShipmentForDebtView(OrderWithoutShipmentForDebtViewModel viewModel) : base(viewModel)
		{
			Build();
			
			Configure();
		}

		private void Configure()
		{
			btnCancel.Clicked += (sender, e) => ViewModel.CancelCommand.Execute();
			ybtnOpenBill.Clicked += (sender, e) => ViewModel.OpenBillCommand.Execute();

			ylabelOrderNum.Binding
				.AddBinding(ViewModel.Entity, e => e.Id, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();

			yentryDebtName.Binding
				.AddBinding(ViewModel.Entity, e => e.DebtName, w => w.Text)
				.InitializeFromSource();

			yspinbtnDebtSum.Binding
				.AddBinding(ViewModel.Entity, e => e.DebtSum, v => v.ValueAsDecimal)
				.InitializeFromSource();

			ylabelOrderDate.Binding
				.AddFuncBinding(ViewModel, vm => vm.Entity.CreateDate.ToString(), w => w.Text)
				.InitializeFromSource();

			ylabelOrderAuthor.Binding
				.AddFuncBinding(ViewModel, vm => vm.Entity.Author.ShortName, w => w.Text)
				.InitializeFromSource();

			yCheckBtnHideSignature.Binding
				.AddBinding(ViewModel.Entity, e => e.HideSignature, w => w.Active)
				.InitializeFromSource();

			entityViewModelEntryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyAutocompleteSelectorFactory);
			entityViewModelEntryCounterparty.Changed += ViewModel.OnCounterpartyEntityViewModelEntryChanged;

			entityViewModelEntryCounterparty.Binding
				.AddBinding(ViewModel.Entity, e => e.Client, w => w.Subject)
				.InitializeFromSource();

			entityViewModelEntryCounterparty.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsDocumentSent, w => w.Sensitive)
				.InitializeFromSource();
			entityViewModelEntryCounterparty.CanEditReference = true;

			var sendEmailView = new SendDocumentByEmailView(ViewModel.SendDocViewModel);
			hbox7.Add(sendEmailView);
			sendEmailView.Show();

			ViewModel.OpenCounterpartyJournal += entityViewModelEntryCounterparty.OpenSelectDialog;

			organizationEntry.ViewModel = ViewModel.OrganizationViewModel;
			organizationEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanSetOrganization, w => w.Sensitive)
				.InitializeFromSource();
			
			treeViewEdoContainers.ColumnsConfig = FluentColumnsConfig<EdoDockflowData>.Create()
				.AddColumn("Код документооборота")
					.AddTextRenderer(x => x.DocFlowId.HasValue ? x.DocFlowId.ToString() : string.Empty)
				.AddColumn("Отправленные\nдокументы")
					.AddTextRenderer(x => x.DocumentType)
				.AddColumn("Статус\nдокументооборота")
					.AddTextRenderer(x => x.EdoDocFlowStatusString)
				.AddColumn("Статус задачи\nнового ЭДО")
					.AddTextRenderer(x => x.EdoTaskStatus.HasValue ? x.EdoTaskStatus.Value.GetEnumTitle() : string.Empty)
				.AddColumn("Статус документа\nнового ЭДО")
					.AddTextRenderer(x => x.EdoDocumentStatus.HasValue ? x.EdoDocumentStatus.Value.GetEnumTitle() : string.Empty)
				.AddColumn("Доставлено\nклиенту?")
					.AddToggleRenderer(x => x.IsReceived)
					.Editing(false)
				.AddColumn("Описание ошибки")
					.AddTextRenderer(x => x.ErrorDescription)
					.WrapWidth(500)
				.AddColumn("")
				.Finish();

			CustomizeSendDocumentAgainButton();

			treeViewEdoContainers.ItemsDataSource = ViewModel.EdoDocuments;

			btnUpdateEdoDocFlowStatus.Clicked += (sender, args) =>
			{
				ViewModel.UpdateEdoDocuments();
				CustomizeSendDocumentAgainButton();
			};

			ybuttonSendDocumentAgain.Clicked += ViewModel.OnButtonSendDocumentAgainClicked;

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			UpdateContainersVisibility();
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.CanSendBillByEdo)
				|| e.PropertyName == nameof(ViewModel.CanResendEdoBill))
			{
				UpdateContainersVisibility();
				CustomizeSendDocumentAgainButton();
			}
		}

		private void UpdateContainersVisibility()
		{
			vboxEdo.Visible = ViewModel.CanSendBillByEdo || ViewModel.EdoDocuments.Any();
		}

		private void CustomizeSendDocumentAgainButton()
		{
			ybuttonSendDocumentAgain.NoShowAll = true;
			ybuttonSendDocumentAgain.Visible = !ViewModel.EdoDocuments.Any();
			ybuttonSendDocumentAgain.Sensitive = ViewModel.CanSendBillByEdo;
			ybuttonSendDocumentAgain.Label = "Отправить";
		}

		public override void Destroy()
		{
			entityViewModelEntryCounterparty.Changed -= ViewModel.OnCounterpartyEntityViewModelEntryChanged;
			
			base.Destroy();
		}
	}
}
