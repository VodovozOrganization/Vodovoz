﻿using System;
using System.Linq;
using Fias.Client;
using Fias.Client.Cache;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Tdi;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewWidgets.Mango;
using IDeliveryPointInfoProvider = Vodovoz.ViewModels.Infrastructure.InfoProviders.IDeliveryPointInfoProvider;

namespace Vodovoz.SidePanel.InfoViews
{
	public partial class DeliveryPointPanelView : Gtk.Bin, IPanelView
	{
		private readonly IDeliveryPointRepository _deliveryPointRepository = new DeliveryPointRepository();
		private readonly IBottlesRepository _bottlesRepository = new BottlesRepository();
		private readonly IDepositRepository _depositRepository = new DepositRepository();
		private readonly IOrderRepository _orderRepository = new OrderRepository();
		private readonly IDeliveryPointViewModelFactory _deliveryPointViewModelFactory;
		private readonly IPermissionResult _deliveryPointPermissionResult;
		private readonly IPermissionResult _orderPermissionResult;
		DeliveryPoint DeliveryPoint { get; set; }

		public DeliveryPointPanelView(ICommonServices commonServices)
		{
			if(commonServices == null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}
			Build();
			_deliveryPointPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DeliveryPoint));
			_orderPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Order));
			IParametersProvider parametersProvider = new ParametersProvider();
			IFiasApiParametersProvider fiasApiParametersProvider = new FiasApiParametersProvider(parametersProvider);
			var geoCoderCache = new GeocoderCache(UnitOfWorkFactory.GetDefaultFactory);
			IFiasApiClient fiasApiClient = new FiasApiClient(fiasApiParametersProvider.FiasApiBaseUrl, fiasApiParametersProvider.FiasApiToken, geoCoderCache);
			_deliveryPointViewModelFactory = new DeliveryPointViewModelFactory(fiasApiClient);
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
			textviewCommentLogist.Buffer.Text = DeliveryPoint.CommentLogist;

			var currentOrders = _orderRepository.GetLatestOrdersForDeliveryPoint(InfoProvider.UoW, DeliveryPoint, 5);
			ytreeLastOrders.SetItemsSource<Order>(currentOrders);
			vboxLastOrders.Visible = currentOrders.Any();

			table2.ShowAll();

			buttonSaveComment.Sensitive = 
				btn.Sensitive = 
				textviewComment.Editable = _deliveryPointPermissionResult.CanUpdate;
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

		protected void OnButtonSaveCommentClicked(object sender, EventArgs e)
		{
			using(var uow = UnitOfWorkFactory.CreateForRoot<DeliveryPoint>(DeliveryPoint.Id, "Кнопка «Cохранить комментарий» на панели точки доставки"))
			{
				uow.Root.Comment = textviewComment.Buffer.Text;
				uow.Save();
			}
		}

		protected void OnButtonSaveCommentLogistClicked(object sender, EventArgs e)
		{
			using(var uow = UnitOfWorkFactory.CreateForRoot<DeliveryPoint>(DeliveryPoint.Id, "Кнопка «Cохранить комментарий для логиста» на панели точки доставки"))
			{
				uow.Root.CommentLogist = textviewCommentLogist.Buffer.Text;
				uow.Save();
			}
		}

		protected void OnBtnAddPhoneClicked(object sender, EventArgs e)
		{
			var dpViewModel = _deliveryPointViewModelFactory.GetForOpenDeliveryPointViewModel(DeliveryPoint.Id);
			dpViewModel.EntitySaved += (o, args) => Refresh(args.Entity);
			TDIMain.MainNotebook.OpenTab(() => dpViewModel);
		}
	}
}
