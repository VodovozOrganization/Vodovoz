using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using QS.DomainModel.UoW;
using QSProjectsLib;
using QS.Tdi;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ServiceDialogs.Database
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrdersWithoutBottlesOperationDlg : QS.Dialog.Gtk.TdiTabBase
	{
		IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		GenericObservableList<Order> observableOrders;

		List<Order> orders;

		public OrdersWithoutBottlesOperationDlg()
		{
			if(!QSMain.User.Permissions["database_maintenance"]) {
				MessageDialogWorks.RunWarningDialog("Доступ запрещён!", "У вас недостаточно прав для доступа к этой вкладке. Обратитесь к своему руководителю.", Gtk.ButtonsType.Ok);
				FailInitialize = true;
				return;
			}

			this.Build();

			TabName = "Заказы без передвижения бутылей";

			ytreeviewOrders.ColumnsConfig = FluentColumnsConfig<Order>.Create()
				.AddColumn("№ заказа").AddNumericRenderer(x => x.Id)
				.AddColumn("Клиент").AddNumericRenderer(x => x.Client.Name)
				.AddColumn("Дата").SetDataProperty(x => x.DeliveryDate.HasValue 
				                                   ? x.DeliveryDate.Value.ToShortDateString() 
				                                   : "")
				.AddColumn("Кол-во бутылей").AddNumericRenderer(x => x.OrderItems.Sum(item => item.Count))
				.Finish();
		}

		protected void OnButtonFindOrdersClicked(object sender, EventArgs e)
		{
			var docList = uow.Session.QueryOver<SelfDeliveryDocument>()
			   .Where(x => x.Order != null)
			   .List();

			orders = new List<Order>(
				docList.Select(x => x.Order)
				.Where(x => x.BottlesMovementOperation == null
					  && x.SelfDelivery
					  && x.OrderStatus == OrderStatus.Closed
				      && x.OrderItems.Any(oi => oi.Nomenclature?.Category == NomenclatureCategory.water && oi.Nomenclature?.TareVolume == TareVolume.Vol19L))
			).Distinct().ToList();

			ytreeviewOrders.SetItemsSource(orders);
			labelOrdersCount.Text = String.Format("Найдено заказов: {0}", orders.Count);
		}

		protected void OnButtonCreateBottleOperationsClicked(object sender, EventArgs e)
		{
			orders.ForEach(x => x.UpdateBottlesMovementOperationWithoutDelivery(uow));
			if(uow.HasChanges && MessageDialogWorks.RunQuestionDialog("Создано \"{0}\" недостающих операций передвижения бутылей, сохранить изменения?", 
		                                        orders.Where(x => x.BottlesMovementOperation != null).Count())){
				uow.Commit();
			}
			OnCloseTab(false);
		}
	}
}
