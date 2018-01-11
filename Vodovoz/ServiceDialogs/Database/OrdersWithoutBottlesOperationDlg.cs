﻿using System;
using System.Data.Bindings.Collections.Generic;
using Gamma.ColumnConfig;
using QSOrmProject;
using System.Linq;
using QSTDI;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Documents;
using NHibernate.Transform;
using System.Collections.Generic;
using QSProjectsLib;

namespace Vodovoz.ServiceDialogs.Database
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrdersWithoutBottlesOperationDlg : TdiTabBase
	{
		IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		GenericObservableList<Order> observableOrders;

		List<Order> orders;

		public OrdersWithoutBottlesOperationDlg()
		{
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
				      && x.OrderItems.Any(oi => oi.Nomenclature?.Category == Domain.Goods.NomenclatureCategory.water))
			).Distinct().ToList();

			ytreeviewOrders.SetItemsSource(orders);
			labelOrdersCount.Text = String.Format("Найдено заказов: {0}", orders.Count);
		}

		protected void OnButtonCreateBottleOperationsClicked(object sender, EventArgs e)
		{
			orders.ForEach(x => x.CreateBottlesMovementOperation(uow));
			if(uow.HasChanges && MessageDialogWorks.RunQuestionDialog("Создано \"{0}\" недостающих операций передвижения бутылей, сохранить изменения?", 
		                                        orders.Where(x => x.BottlesMovementOperation != null).Count())){
				uow.Commit();
			}
			OnCloseTab(false);
		}
	}
}
