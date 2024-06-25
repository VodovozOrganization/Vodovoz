using System;
using System.Linq;
using Autofac;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Factories;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewWidgets.Mango;
using IDeliveryPointInfoProvider = Vodovoz.ViewModels.Infrastructure.InfoProviders.IDeliveryPointInfoProvider;

namespace Vodovoz.SidePanel.InfoViews
{
	public partial class DeliveryPointPanelView : Gtk.Bin, IPanelView
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private readonly IDeliveryPointRepository _deliveryPointRepository;
		private readonly IBottlesRepository _bottlesRepository;
		private readonly IDepositRepository _depositRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IDeliveryPointViewModelFactory _deliveryPointViewModelFactory;
		private readonly IPermissionResult _deliveryPointPermissionResult;
		private readonly IPermissionResult _orderPermissionResult;
		private readonly ICommonServices _commonServices;

		DeliveryPoint DeliveryPoint { get; set; }
		private bool _textviewcommentBufferChanged = false;
		private bool _textviewcommentLogistBufferChanged = false;

		public DeliveryPointPanelView(
			ICommonServices commonServices,
			IDeliveryPointRepository deliveryPointRepository,
			IBottlesRepository bottlesRepository,
			IDepositRepository depositRepository,
			IOrderRepository orderRepository)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));
			_bottlesRepository = bottlesRepository ?? throw new ArgumentNullException(nameof(bottlesRepository));
			_depositRepository = depositRepository ?? throw new ArgumentNullException(nameof(depositRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));

			Build();
			_deliveryPointPermissionResult = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DeliveryPoint));
			_orderPermissionResult = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Order));
			_deliveryPointViewModelFactory = new DeliveryPointViewModelFactory(_lifetimeScope);
			Configure();
		}

		void Configure()
		{
			label5.Visible = labelDeposits.Visible = false;
			labelAddress.LineWrapMode = Pango.WrapMode.WordChar;
			labelLastOrders.LineWrapMode = Pango.WrapMode.WordChar;

			ytreeLastOrders.RowActivated += OnOrdersRowActivated;
			ytreeLastOrders.Selection.Changed += OnOrdersSelectionChanged;

			ytreeLastOrders.ColumnsConfig = ColumnsConfigFactory.Create<Order>()
				.AddColumn("Дата")
				.AddTextRenderer(node => node.DeliveryDate.HasValue ? node.DeliveryDate.Value.ToString("dd.MM.yy") : String.Empty)
				.AddColumn("Тип оплаты")
				.AddTextRenderer(node => GetDisplayShortName(node.PaymentType))
				.AddColumn("Бутылей")
				.AddNumericRenderer(node => node.Total19LBottlesToDeliver).Editing(false)
				.Finish();

			textviewComment.Buffer.Changed += OnTextviewCommentBufferChanged;
			textviewComment.FocusOutEvent += OnTextviewCommentFocusOut;

			logisticsRequirementsView.ViewModel = new LogisticsRequirementsViewModel(GetLogisticsRequirements(), _commonServices);
		}

		private void Refresh(object changedObj)
		{
			if(InfoProvider == null)
			{
				return;
			}

			DeliveryPoint = changedObj as DeliveryPoint;
			RefreshData();
		}

		#region LogisticsRequirements
		private LogisticsRequirements GetLogisticsRequirements()
		{
			return DeliveryPoint?.LogisticsRequirements ?? new LogisticsRequirements();
		}

		private void SetLogisticsRequirementsCheckboxes()
		{
			var requirements = GetLogisticsRequirements();

			logisticsRequirementsView.ViewModel.Entity.CopyRequirementPropertiesValues(requirements);
		}

		private void SaveLogisticsRequirements()
		{
			using(var uow =
					ServicesConfig.UnitOfWorkFactory.CreateForRoot<DeliveryPoint>(DeliveryPoint.Id, "Кнопка «Cохранить требования к логистике на панели точки доставки"))
			{
				if(uow.Root.LogisticsRequirements == null)
				{
					uow.Root.LogisticsRequirements = new LogisticsRequirements();
				}

				uow.Root.LogisticsRequirements.ForwarderRequired = logisticsRequirementsView.ViewModel.Entity.ForwarderRequired;
				uow.Root.LogisticsRequirements.DocumentsRequired = logisticsRequirementsView.ViewModel.Entity.DocumentsRequired;
				uow.Root.LogisticsRequirements.RussianDriverRequired = logisticsRequirementsView.ViewModel.Entity.RussianDriverRequired;
				uow.Root.LogisticsRequirements.PassRequired = logisticsRequirementsView.ViewModel.Entity.PassRequired;
				uow.Root.LogisticsRequirements.LargusRequired = logisticsRequirementsView.ViewModel.Entity.LargusRequired;

				uow.Save();
			}
		}
		#endregion

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public void Refresh()
		{
			DeliveryPoint = (InfoProvider as IDeliveryPointInfoProvider)?.DeliveryPoint;
			RefreshData();
		}

		private void RefreshData()
		{
			if(DeliveryPoint == null)
			{
				buttonSaveComment.Sensitive = false;
				return;
			}

			labelAddress.Text = DeliveryPoint.CompiledAddress;

			foreach(var child in PhonesTable.Children)
			{
				PhonesTable.Remove(child);
				child.Destroy();
			}
			var phones = DeliveryPoint.Phones.Where(p => !p.IsArchive).ToList();
			uint rowsCount = Convert.ToUInt32(phones.Count) + 1;
			PhonesTable.Resize(rowsCount, 2);
			for(uint row = 0; row < rowsCount - 1; row++)
			{
				Label label = new Label();
				label.Selectable = true;
				label.Markup = $"{phones[Convert.ToInt32(row)].LongText}";

				HandsetView handsetView = new HandsetView(phones[Convert.ToInt32(row)].DigitsNumber);

				PhonesTable.Attach(label, 0, 1, row, row + 1);
				PhonesTable.Attach(handsetView, 1, 2, row, row + 1);
			}

			Label labelAddPhone = new Label() { LabelProp = "Щёлкните чтоб\n добавить телефон-->" };
			PhonesTable.Attach(labelAddPhone, 0, 1, rowsCount - 1, rowsCount);

			Image addIcon = new Image();
			addIcon.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-add", IconSize.Menu);
			Button btn = new Button();
			btn.Image = addIcon;
			btn.Clicked += OnBtnAddPhoneClicked;
			PhonesTable.Attach(btn, 1, 2, rowsCount - 1, rowsCount);
			PhonesTable.ShowAll();

			var bottlesAtDeliveryPoint = _bottlesRepository.GetBottlesDebtAtDeliveryPoint(InfoProvider.UoW, DeliveryPoint);
			var bottlesAvgDeliveryPoint = _deliveryPointRepository.GetAvgBottlesOrdered(InfoProvider.UoW, DeliveryPoint, 5);
			lblBottlesQty.LabelProp = $"{bottlesAtDeliveryPoint} шт. (сред. зак.: {bottlesAvgDeliveryPoint:G3})";
			var bottlesAtCounterparty = _bottlesRepository.GetBottlesDebtAtCounterparty(InfoProvider.UoW, DeliveryPoint.Counterparty);
			debtByClientLabel.LabelProp = $"{bottlesAtCounterparty} шт.";
			var depositsAtDeliveryPoint = _depositRepository.GetDepositsAtDeliveryPoint(InfoProvider.UoW, DeliveryPoint, null);
			labelDeposits.LabelProp = CurrencyWorks.GetShortCurrencyString(depositsAtDeliveryPoint);

			textviewComment.Buffer.Text = DeliveryPoint.Comment;

			var currentOrders = _orderRepository.GetLatestOrdersForDeliveryPoint(InfoProvider.UoW, DeliveryPoint, 5);
			ytreeLastOrders.SetItemsSource<Order>(currentOrders);
			vboxLastOrders.Visible = currentOrders.Any();

			table2.ShowAll();

			buttonSaveComment.Sensitive = 
				btn.Sensitive = 
				textviewComment.Editable = _deliveryPointPermissionResult.CanUpdate;

			var isLogistcsRequirementsEditable = _deliveryPointPermissionResult.CanUpdate && DeliveryPoint.Id != 0;
			logisticsRequirementsView.Sensitive = isLogistcsRequirementsEditable;
			buttonSaveLogisticsRequirements.Sensitive = isLogistcsRequirementsEditable;

			if(InfoProvider is OrderDlg)
			{
				yvboxLogisticsRequirements.Visible = true;
				SetLogisticsRequirementsCheckboxes();
			}
		}

		public bool VisibleOnPanel
		{
			get
			{
				return DeliveryPoint != null;
			}
		}

		public void OnCurrentObjectChanged(object changedObject)
		{
			if(changedObject is DeliveryPoint deliveryPoint)
			{
				DeliveryPoint = deliveryPoint;
				Refresh();
			}

			if(InfoProvider is OrderDlg && changedObject is Counterparty)
			{
				Refresh();
			}
		}
		#endregion

		string GetDisplayShortName(Enum enumerator)
		{
			var attr = enumerator.GetAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
			return attr == null ? "" : attr.ShortName;
		}

		void OnOrdersRowActivated(object sender, RowActivatedArgs args)
		{
			if(InfoProvider is OrderDlg orderDlg &&
			   (_orderPermissionResult.CanUpdate || _orderPermissionResult.CanCreate && orderDlg.Order.Id == 0))
			{
				var order = ytreeLastOrders.GetSelectedObject() as Order;
				orderDlg.FillOrderItems(order);
				orderDlg.Entity.ObservablePromotionalSets.Clear();
			}
		}

		void OnOrdersSelectionChanged(object sender, EventArgs args)
		{
			var order = ytreeLastOrders.GetSelectedObject() as Order;
			GenerateTooltip(order);
		}

		private void GenerateTooltip(Order order)
		{
			ytreeLastOrders.HasTooltip = false;

			if(order == null)
			{
				return;
			}
			string tooltip = "Заказ №" + order.Id + ":";

			foreach(OrderItem orderItem in order.OrderItems)
			{
				tooltip += "\n"
				   + orderItem.Nomenclature.Name + ": "
				   + orderItem.Count.ToString(orderItem.Nomenclature.Unit != null ? $"N{orderItem.Nomenclature.Unit.Digits}" : "G29");
			}

			ytreeLastOrders.TooltipText = tooltip;
			ytreeLastOrders.HasTooltip = true;
		}

		private void SaveComment()
		{
			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateForRoot<DeliveryPoint>(DeliveryPoint.Id, "Кнопка «Cохранить комментарий» на панели точки доставки"))
			{
				uow.Root.Comment = textviewComment.Buffer.Text;
				uow.Save();
			}
			_textviewcommentBufferChanged = false;
		}

		protected void OnButtonSaveCommentClicked(object sender, EventArgs e)
		{
			SaveComment();
		}

		protected void OnButtonSaveLogisticsRequirementsClicked(object sender, EventArgs e)
		{
			SaveLogisticsRequirements();
		}

		private void OnTextviewCommentBufferChanged(object sender, EventArgs e)
		{
			_textviewcommentBufferChanged = true;
		}

		private void OnTextviewCommentFocusOut(object o, FocusOutEventArgs args)
		{
			if(_textviewcommentBufferChanged && buttonSaveComment.State != StateType.Prelight)
			{
				Gtk.Application.Invoke((s, ea) =>
				{
					bool isRequiredToSaveComment = MessageDialogHelper.RunQuestionDialog("Сохранить изменения в комментарии?");
					if(isRequiredToSaveComment)
					{
						SaveComment();
					}
					else
					{
						textviewComment.Buffer.Text = DeliveryPoint.Comment ?? String.Empty;
						_textviewcommentBufferChanged = false;
					}
				});
			}
		}

		protected void OnBtnAddPhoneClicked(object sender, EventArgs e)
		{
			var dpViewModel = _deliveryPointViewModelFactory.GetForOpenDeliveryPointViewModel(DeliveryPoint.Id);
			dpViewModel.EntitySaved += (o, args) => Refresh(args.Entity);
			TDIMain.MainNotebook.OpenTab(() => dpViewModel);
		}

		#region overrided Dispose() method

		private bool _disposed = false;

		public override void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(_disposed)
			{
				return;
			}

			if(disposing)
			{

				textviewComment.Buffer.Changed -= OnTextviewCommentBufferChanged;
				textviewComment.FocusOutEvent -= OnTextviewCommentFocusOut;

				base.Dispose();
			}

			_disposed = true;
		}
		#endregion

		public override void Destroy()
		{
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}
