using Autofac;
using Gamma.ColumnConfig;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Controllers;

namespace Vodovoz.ServiceDialogs.Database
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrdersWithoutBottlesOperationDlg : QS.Dialog.Gtk.TdiTabBase
	{
		private ILifetimeScope _scope = Startup.AppDIContainer.BeginLifetimeScope();
		private IUnitOfWork _uow;
		private INomenclatureSettings _nomenclatureSettings;
		private ICashRepository _cashRepository;
		private IRouteListItemRepository _routeListItemRepository;
		private IOrderSaleHandler _saleHandler;

		private List<Order> _orders;

		public OrdersWithoutBottlesOperationDlg()
		{
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("database_maintenance")) {
				MessageDialogHelper.RunWarningDialog("Доступ запрещён!", "У вас недостаточно прав для доступа к этой вкладке. Обратитесь к своему руководителю.", Gtk.ButtonsType.Ok);
				FailInitialize = true;
				return;
			}

			Build();
			ResolveDependencies();

			TabName = "Заказы без передвижения бутылей";

			ytreeviewOrders.ColumnsConfig = FluentColumnsConfig<Order>.Create()
				.AddColumn("№ заказа").AddNumericRenderer(x => x.Id)
				.AddColumn("Клиент").AddNumericRenderer(x => x.Client.Name)
				.AddColumn("Дата").AddTextRenderer(x => x.DeliveryDate.HasValue 
				                                   ? x.DeliveryDate.Value.ToShortDateString() 
				                                   : "")
				.AddColumn("Кол-во бутылей").AddNumericRenderer(x => x.OrderItems.Sum(item => item.Count))
				.Finish();
		}

		private void ResolveDependencies()
		{
			_nomenclatureSettings = _scope.Resolve<INomenclatureSettings>();
			_cashRepository = _scope.Resolve<ICashRepository>();
			_routeListItemRepository = _scope.Resolve<IRouteListItemRepository>();
			_saleHandler = _scope.Resolve<IOrderSaleHandler>();
			_uow = _scope.Resolve<IUnitOfWorkFactory>().CreateWithoutRoot();
		}

		protected void OnButtonFindOrdersClicked(object sender, EventArgs e)
		{
			var docList = _uow.Session.QueryOver<SelfDeliveryDocument>()
			   .Where(x => x.Order != null)
			   .List();

			_orders = new List<Order>(
				docList.Select(x => x.Order)
				.Where(x => x.BottlesMovementOperation == null
					  && x.SelfDelivery
					  && x.OrderStatus == OrderStatus.Closed
				      && x.OrderItems.Any(oi => oi.Nomenclature?.Category == NomenclatureCategory.water && oi.Nomenclature?.TareVolume == TareVolume.Vol19L))
			).Distinct().ToList();

			ytreeviewOrders.SetItemsSource(_orders);
			labelOrdersCount.Text = String.Format("Найдено заказов: {0}", _orders.Count);
		}

		protected void OnButtonCreateBottleOperationsClicked(object sender, EventArgs e)
		{
			_orders.ForEach(x => x.UpdateBottlesMovementOperationWithoutDelivery(
				_uow,
				_saleHandler,
				_nomenclatureSettings,
				_routeListItemRepository,
				_cashRepository)
			);
			if(_uow.HasChanges && MessageDialogHelper.RunQuestionDialog(
				"Создано \"{0}\" недостающих операций передвижения бутылей, сохранить изменения?",
				_orders.Count(x => x.BottlesMovementOperation != null))){
				_uow.Commit();
			}
			OnCloseTab(false);
		}

		protected override void OnDestroyed()
		{
			if(_scope != null)
			{
				_scope.Dispose();
				_scope = null;
			}
			
			_uow?.Dispose();
			base.OnDestroyed();
		}
	}
}
