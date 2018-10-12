using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalFilters;
using Vodovoz.Repositories;
using Vodovoz.Repository;

namespace Vodovoz.Representations
{
	public class UndeliveredOrdersVM
	{
		public UndeliveredOrdersFilter Filter { get; set; }
		public IUnitOfWork UoW { get; set; }

		#region IRepresentationModel implementation

		int undeliveryToShow = 0;
		int currUser = 0;
		public IList<UndeliveredOrdersVMNode> Result { get; set; }

		public virtual void UpdateNodes()
		{
			UndeliveredOrdersVMNode resultAlias = null;
			CommentNode commentsAlias = null;
			UndeliveredOrder undeliveredOrderAlias = null;
			Domain.Orders.Order oldOrderAlias = null;
			Domain.Orders.Order newOrderAlias = null;
			Employee driverAlias = null;
			Employee oldOrderAuthorAlias = null;
			Employee authorAlias = null;
			Employee editorAlias = null;
			Employee registratorAlias = null;
			Employee employeeAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint undeliveredOrderDeliveryPointAlias = null;
			DeliverySchedule undeliveredOrderDeliveryScheduleAlias = null;
			DeliverySchedule newOrderDeliveryScheduleAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Subdivision subdivisionAlias = null;
			UndeliveredOrderComment undeliveredOrderCommentsAlias = null;
			Fine fineAlias = null;
			FineItem fineItemAlias = null;
			Employee finedEmployeeAlias = null;
			Subdivision inProcessAtSubdivisionAlias = null;
			GuiltyInUndelivery guiltyInUndeliveryAlias = null;

			var subqueryDrivers = QueryOver.Of<RouteListItem>(() => routeListItemAlias)
			                              .Where(() => routeListItemAlias.Order.Id == oldOrderAlias.Id)
										  .Left.JoinQueryOver(i => i.RouteList, () => routeListAlias)
										  .Left.JoinAlias(i => i.Driver, () => driverAlias)
										  .Select(
											  Projections.SqlFunction(
					                              new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(CONCAT(?1, ' ', LEFT(?2,1),'.',LEFT(?3,1)) ORDER BY ?4 DESC SEPARATOR '\n\t↑\n')"),//⬆
												  NHibernateUtil.String,
												  Projections.Property(() => driverAlias.LastName),
												  Projections.Property(() => driverAlias.Name),
												  Projections.Property(() => driverAlias.Patronymic),
					                              Projections.Property(() => routeListItemAlias.Id)
												 )
											 );
			
			var subquery19LWatterQty = QueryOver.Of<OrderItem>(() => orderItemAlias)
												.Where(() => orderItemAlias.Order.Id == oldOrderAlias.Id)
												.Left.JoinQueryOver(i => i.Nomenclature, () => nomenclatureAlias)
												.Where(n => n.Category == NomenclatureCategory.water)
												.Select(Projections.Sum(() => orderItemAlias.Count));

			var subqueryGoodsToClient = QueryOver.Of<OrderEquipment>(() => orderEquipmentAlias)
												 .Where(() => orderEquipmentAlias.Order.Id == oldOrderAlias.Id)
												 .Where(() => orderEquipmentAlias.Direction == Direction.Deliver)
												 .Left.JoinQueryOver(i => i.Nomenclature, () => nomenclatureAlias)
												 .Select(
													 Projections.SqlFunction(
														 new SQLFunctionTemplate(NHibernateUtil.String, "TRIM(GROUP_CONCAT(CONCAT(IF(?1 IS NULL, ?2, ?1),':',?3) SEPARATOR ?4))"),
														 NHibernateUtil.String,
														 Projections.Property(() => nomenclatureAlias.ShortName),
														 Projections.Property(() => nomenclatureAlias.Name),
														 Projections.Property(() => orderEquipmentAlias.Count),
														 Projections.Constant("\n")
														)
													);

			var subqueryGoodsFromClient = QueryOver.Of<OrderEquipment>(() => orderEquipmentAlias)
												   .Where(() => orderEquipmentAlias.Order.Id == oldOrderAlias.Id)
												   .Where(() => orderEquipmentAlias.Direction == Direction.PickUp)
												   .Left.JoinQueryOver(i => i.Nomenclature, () => nomenclatureAlias)
												   .Select(
													   Projections.SqlFunction(
														   new SQLFunctionTemplate(NHibernateUtil.String, "TRIM(GROUP_CONCAT(CONCAT(IF(?1 IS NULL, ?2, ?1),':',?3) SEPARATOR ?4))"),
														   NHibernateUtil.String,
														   Projections.Property(() => nomenclatureAlias.ShortName),
														   Projections.Property(() => nomenclatureAlias.Name),
														   Projections.Property(() => orderEquipmentAlias.Count),
														   Projections.Constant("\n")
														)
													);

			var subqueryGuilty = QueryOver.Of<GuiltyInUndelivery>(() => guiltyInUndeliveryAlias)
											.Where(() => undeliveredOrderAlias.Id == guiltyInUndeliveryAlias.UndeliveredOrder.Id)
											.Left.JoinQueryOver(g => g.GuiltyDepartment, () => subdivisionAlias)
											.Select(
				                                Projections.SqlFunction(
					                                new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(CONCAT(CASE ?1 WHEN 'Client' THEN 'Клиент' WHEN 'Driver' THEN 'Водитель' WHEN 'Department' THEN 'Отдел ВВ' WHEN 'ServiceMan' THEN 'Мастер СЦ' WHEN 'None' THEN 'Нет (не недовоз)' ELSE 'Неизвестно' END, IF(?1 = 'Department' AND ?2 = '', ':Неизвестно', IF(?1 = 'Department' AND ?2 != '', CONCAT(':', ?2), ''))) SEPARATOR '\n')"),
					                                NHibernateUtil.String,
					                                Projections.Property(() => guiltyInUndeliveryAlias.GuiltySide),
					                                Projections.Property(() => subdivisionAlias.Name)
					                               )
											   );

			var subqueryFined = QueryOver.Of<Fine>(() => fineAlias)
										 .Where(() => fineAlias.UndeliveredOrder.Id == undeliveredOrderAlias.Id)
										 .Left.JoinAlias(() => fineAlias.Items, () => fineItemAlias)
										 .Left.JoinAlias(() => fineItemAlias.Employee, () => finedEmployeeAlias)
										 .Select(
											 Projections.SqlFunction(
												 new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT(CONCAT_WS(': ', ?1, ?2) SEPARATOR '\n')"),
												 NHibernateUtil.String,
												 Projections.Property(() => finedEmployeeAlias.LastName),
												 Projections.Property(() => fineItemAlias.Money)
												)
											);

			var query = UoW.Session.QueryOver<UndeliveredOrder>(() => undeliveredOrderAlias)
						   .Left.JoinAlias(u => u.OldOrder, () => oldOrderAlias)
						   .Left.JoinAlias(u => u.NewOrder, () => newOrderAlias)
						   .Left.JoinAlias(u => u.OldOrder.Client, () => counterpartyAlias)
						   .Left.JoinAlias(() => newOrderAlias.DeliverySchedule, () => newOrderDeliveryScheduleAlias)
						   .Left.JoinAlias(() => oldOrderAlias.Author, () => oldOrderAuthorAlias)
						   .Left.JoinAlias(() => oldOrderAlias.DeliveryPoint, () => undeliveredOrderDeliveryPointAlias)
						   .Left.JoinAlias(() => oldOrderAlias.DeliverySchedule, () => undeliveredOrderDeliveryScheduleAlias)
						   .Left.JoinAlias(u => u.Author, () => authorAlias)
						   .Left.JoinAlias(u => u.LastEditor, () => editorAlias)
						   .Left.JoinAlias(u => u.EmployeeRegistrator, () => registratorAlias)
						   .Left.JoinAlias(u => u.InProcessAtDepartment, () => inProcessAtSubdivisionAlias)
			               .Left.JoinAlias(() => undeliveredOrderAlias.GuiltyInUndelivery, () => guiltyInUndeliveryAlias)
						   .Left.JoinAlias(() => guiltyInUndeliveryAlias.GuiltyDepartment, () => subdivisionAlias);

			if(Filter?.RestrictDriver != null){
				var oldOrderIds = UndeliveredOrdersRepository.GetListOfUndeliveryIdsForDriver(UoW, Filter.RestrictDriver);
				query.Where(() => oldOrderAlias.Id.IsIn(oldOrderIds.ToArray()));
			}

			if(Filter?.RestrictOldOrder != null)
				query.Where(() => oldOrderAlias.Id == Filter.RestrictOldOrder.Id);

			if(Filter?.RestrictClient != null)
				query.Where(() => counterpartyAlias.Id == Filter.RestrictClient.Id);

			if(Filter?.RestrictAddress != null)
				query.Where(() => undeliveredOrderDeliveryPointAlias.Id == Filter.RestrictAddress.Id);

			if(Filter?.RestrictOldOrderAuthor != null)
				query.Where(() => oldOrderAuthorAlias.Id == Filter.RestrictOldOrderAuthor.Id);

			if(Filter?.RestrictOldOrderStartDate != null)
				query.Where(() => oldOrderAlias.DeliveryDate >= Filter.RestrictOldOrderStartDate);

			if(Filter?.RestrictOldOrderEndDate != null)
				query.Where(() => oldOrderAlias.DeliveryDate <= Filter.RestrictOldOrderEndDate.Value.AddDays(1).AddTicks(-1));

			if(Filter?.RestrictNewOrderStartDate != null)
				query.Where(() => newOrderAlias.DeliveryDate >= Filter.RestrictNewOrderStartDate);

			if(Filter?.RestrictNewOrderEndDate != null)
				query.Where(() => newOrderAlias.DeliveryDate <= Filter.RestrictNewOrderEndDate.Value.AddDays(1).AddTicks(-1));

			if(Filter?.RestrictGuiltySide != null)
				query.Where(() => guiltyInUndeliveryAlias.GuiltySide == Filter.RestrictGuiltySide);

			if(Filter?.RestrictGuiltyDepartment != null)
				query.Where(() => subdivisionAlias.Id == Filter.RestrictGuiltyDepartment.Id);

			if(Filter?.RestrictInProcessAtDepartment != null)
				query.Where(u => u.InProcessAtDepartment.Id == Filter.RestrictInProcessAtDepartment.Id);

			if(Filter?.NewInvoiceCreated != null) {
				if(Filter.NewInvoiceCreated.Value)
					query.Where(u => u.NewOrder != null);
				else
					query.Where(u => u.NewOrder == null);
			}

			if(Filter?.RestrictUndeliveryStatus != null)
				query.Where(u => u.UndeliveryStatus == Filter.RestrictUndeliveryStatus);

			if(Filter?.RestrictUndeliveryAuthor != null)
				query.Where(u => u.Author == Filter.RestrictUndeliveryAuthor);

			if(undeliveryToShow > 0)
				query.Where(() => undeliveredOrderAlias.Id == undeliveryToShow);

			Result = query.SelectList(list => list
			                          .Select(() => newOrderAlias.Id).WithAlias(() => resultAlias.NewOrderId)
			                          .Select(() => newOrderAlias.DeliveryDate).WithAlias(() => resultAlias.NewOrderDeliveryDate)
			                          .Select(() => newOrderDeliveryScheduleAlias.Name).WithAlias(() => resultAlias.NewOrderDeliverySchedule)
			                          .SelectGroup(() => undeliveredOrderAlias.Id).WithAlias(() => resultAlias.Id)
			                          .Select(() => oldOrderAlias.Id).WithAlias(() => resultAlias.OldOrderId)
			                          .Select(() => oldOrderAlias.DeliveryDate).WithAlias(() => resultAlias.OldOrderDeliveryDateTime)
			                          .Select(() => undeliveredOrderAlias.DispatcherCallTime).WithAlias(() => resultAlias.DispatcherCallTime)
			                          .Select(() => undeliveredOrderAlias.DriverCallNr).WithAlias(() => resultAlias.DriverCallNr)
			                          .Select(() => undeliveredOrderAlias.DriverCallTime).WithAlias(() => resultAlias.DriverCallTime)
			                          .Select(() => undeliveredOrderAlias.DriverCallType).WithAlias(() => resultAlias.DriverCallType)
			                          .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Client)
			                          .Select(() => oldOrderAuthorAlias.LastName).WithAlias(() => resultAlias.OldOrderAuthorLastName)
			                          .Select(() => oldOrderAuthorAlias.Name).WithAlias(() => resultAlias.OldOrderAuthorFirstName)
			                          .Select(() => oldOrderAuthorAlias.Patronymic).WithAlias(() => resultAlias.OldOrderAuthorMidleName)
			                          .Select(() => undeliveredOrderDeliveryScheduleAlias.Name).WithAlias(() => resultAlias.OldDeliverySchedule)
			                          .Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
			                          .Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorFirstName)
			                          .Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorMidleName)
			                          .Select(() => registratorAlias.LastName).WithAlias(() => resultAlias.RegistratorLastName)
			                          .Select(() => registratorAlias.Name).WithAlias(() => resultAlias.RegistratorFirstName)
			                          .Select(() => registratorAlias.Patronymic).WithAlias(() => resultAlias.RegistratorMidleName)
			                          .Select(() => editorAlias.LastName).WithAlias(() => resultAlias.EditorLastName)
			                          .Select(() => editorAlias.Name).WithAlias(() => resultAlias.EditorFirstName)
			                          .Select(() => editorAlias.Patronymic).WithAlias(() => resultAlias.EditorMidleName)
			                          .Select(() => undeliveredOrderAlias.Reason).WithAlias(() => resultAlias.Reason)
			                          .Select(() => undeliveredOrderAlias.UndeliveryStatus).WithAlias(() => resultAlias.UndeliveryStatus)
			                          .Select(() => undeliveredOrderAlias.OldOrderStatus).WithAlias(() => resultAlias.StatusOnOldOrderCancel)
			                          .Select(() => oldOrderAlias.OrderStatus).WithAlias(() => resultAlias.OldOrderCurStatus)
			                          .Select(() => inProcessAtSubdivisionAlias.Name).WithAlias(() => resultAlias.InProcessAt)
			                          .SelectSubQuery(subqueryDrivers).WithAlias(() => resultAlias.OldRouteListDriverName)
			                          .SelectSubQuery(subquery19LWatterQty).WithAlias(() => resultAlias.OldOrder19LBottleQty)
			                          .SelectSubQuery(subqueryGoodsToClient).WithAlias(() => resultAlias.OldOrderGoodsToClient)
			                          .SelectSubQuery(subqueryGoodsFromClient).WithAlias(() => resultAlias.OldOrderGoodsFromClient)
			                          .SelectSubQuery(subqueryFined).WithAlias(() => resultAlias.Fined)
			                          .SelectSubQuery(subqueryGuilty).WithAlias(() => resultAlias.Guilty)
			                          .Select(
										  Projections.SqlFunction(
					                          new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(', ', ?1, CONCAT('д.', ?2), CONCAT('лит.', ?3), CONCAT('кв/оф ', ?4))"),
											  NHibernateUtil.String,
					                          Projections.Property(() => undeliveredOrderDeliveryPointAlias.Street),
					                          Projections.Property(() => undeliveredOrderDeliveryPointAlias.Building),
					                          Projections.Property(() => undeliveredOrderDeliveryPointAlias.Letter),
					                          Projections.Property(() => undeliveredOrderDeliveryPointAlias.Room)
											 )
				                         ).WithAlias(() => resultAlias.Address)
			                         ).OrderBy(() => oldOrderAlias.DeliveryDate).Asc
							  .TransformUsing(Transformers.AliasToBean<UndeliveredOrdersVMNode>())
							  .List<UndeliveredOrdersVMNode>();
			
			var allCommentsList = UoW.Session.QueryOver<UndeliveredOrderComment>(() => undeliveredOrderCommentsAlias)
									 .Left.JoinAlias(c => c.Employee, () => employeeAlias)
									 .SelectList(
										 list => list
										 .Select(() => undeliveredOrderCommentsAlias.UndeliveredOrder.Id).WithAlias(() => commentsAlias.Id)
										 .Select(() => undeliveredOrderCommentsAlias.CommentDate).WithAlias(() => commentsAlias.Date)
										 .Select(() => undeliveredOrderCommentsAlias.CommentedField).WithAlias(() => commentsAlias.Field)
										 .Select(
											 Projections.SqlFunction(
												 new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT('<span foreground=\"', ?6, '\">', '<b>', DATE_FORMAT(?1, '%e.%m.%y, %k:%i:%s'), '\n', ?2, ' ', ?4, ':</b>\n', TRIM(?5), '</span>')"),
												 NHibernateUtil.String,
												 Projections.Property(() => undeliveredOrderCommentsAlias.CommentDate),
												 Projections.Property(() => employeeAlias.Name),
												 Projections.Property(() => employeeAlias.Patronymic),
												 Projections.Property(() => employeeAlias.LastName),
												 Projections.Property(() => undeliveredOrderCommentsAlias.Comment),
												 Projections.Conditional(
													 Restrictions.Eq(Projections.Property(() => employeeAlias.Id), currUser),
													 Projections.Constant("blue"),
													 Projections.Constant("red")
													)
												)
											).WithAlias(() => commentsAlias.Comment)
										).TransformUsing(Transformers.AliasToBean<CommentNode>())
									 .List<CommentNode>();

			int counter = 1;
			foreach(var r in Result) {
				var commentsForAllFields = allCommentsList.Where(x => x.Id == r.Id).OrderBy(x => x.Date).ToList();

				List<UndeliveredOrdersVMNode> commentsList = new List<UndeliveredOrdersVMNode>();
				while(commentsForAllFields.Any()) {
					var com = new UndeliveredOrderCommentsNode();
					foreach(CommentedFields field in Enum.GetValues(typeof(CommentedFields))) {
						var comment = commentsForAllFields.FirstOrDefault(x => x.Field == field);
						if(comment == null) continue;
						commentsForAllFields.Remove(comment);

						switch(comment.Field) {
							case CommentedFields.OldOrderDeliveryDate:
								com.OldOrderDeliveryDate = comment.Comment;
								break;
							case CommentedFields.Reason:
								com.Reason = comment.Comment;
								break;
							case CommentedFields.ActionWithInvoice:
								com.ActionWithInvoice = comment.Comment;
								break;
							case CommentedFields.TransferDateTime:
								com.TransferDateTime = comment.Comment;
								break;
							case CommentedFields.Client:
								com.Client = comment.Comment;
								break;
							case CommentedFields.Address:
								com.Address = comment.Comment;
								break;
							case CommentedFields.UndeliveredOrderItems:
								com.UndeliveredOrderItems = comment.Comment;
								break;
							case CommentedFields.OldDeliverySchedule:
								com.OldDeliverySchedule = comment.Comment;
								break;
							case CommentedFields.OldOrderAuthor:
								com.OldOrderAuthor = comment.Comment;
								break;
							case CommentedFields.DriverName:
								com.DriverName = comment.Comment;
								break;
							case CommentedFields.DriversCall:
								com.DriversCall = comment.Comment;
								break;
							case CommentedFields.DispatcherCall:
								com.DispatcherCall = comment.Comment;
								break;
							case CommentedFields.Registrator:
								com.Registrator = comment.Comment;
								break;
							case CommentedFields.UndeliveryAuthor:
								com.UndeliveryAuthor = comment.Comment;
								break;
							case CommentedFields.Guilty:
								com.Guilty = comment.Comment;
								break;
							case CommentedFields.FinedPeople:
								com.FinedPeople = comment.Comment;
								break;
							case CommentedFields.Status:
								com.Status = comment.Comment;
								break;
							case CommentedFields.OldOrderStatus:
								com.OldOrderStatus = comment.Comment;
								break;
							default:
								break;
						}
					}
					com.Parent = r;
					commentsList.Add(com);
				}

				r.NumberInList = counter++;
				r.Children = commentsList;
			}

			#region Формирование цвета строк таблицы недовозов
			Color lightYellow = new Color(255, 255, 192);
			Color lightGreen = new Color(192, 255, 192);
			Color darkerLightYellow = new Color(255, 255, 168);
			Color darkerLightGreen = new Color(168, 255, 168);
			for(int i = 0; i < Result.Count(); i++) {
				Result[i].BGColor = i % 2 == 0 ? lightYellow : lightGreen;
				for(int j = 0; j < Result[i].Children.Count(); j++) {
					var color = Result[i].BGColor.Red == 65535 ? darkerLightYellow : darkerLightGreen;
					Result[i].Children[j].BGColor = j % 2 == 1 ? Result[i].BGColor : color;
				}
			}
			#endregion
		}
		public RecursiveTreeModel<UndeliveredOrdersVMNode> RecursiveTreeModel { get; set; }

		#region IColumnsConfig implementation
		IColumnsConfig columnsConfig = FluentColumnsConfig<UndeliveredOrdersVMNode>
			.Create()
			.AddColumn("№").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.StrId)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Статус").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.Status, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Дата\nзаказа").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.OldOrderDeliveryDate, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Автор\nзаказа").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.OldOrderAuthor, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Клиент и адрес").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.ClientAndAddress, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Интервал\nдоставки").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.OldDeliverySchedule, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Количество\nбутылей").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.UndeliveredOrderItems, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Статус\nначальный ➔\n ➔ текущий").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.OldOrderStatus, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Виновный").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.Guilty, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Причина").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.Reason, useMarkup: true)
				.WrapWidth(300).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Звонок\nв офис").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.DriversCall, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Звонок\nклиенту").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.DispatcherCall, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Водитель").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.DriverName, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Перенос").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.TransferDateTime, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Кто недовоз\nзафиксировал").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.Registrator, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Автор\nнедовоза").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.UndeliveryAuthor, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("Оштрафованные").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.FinedPeople, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.AddColumn("В работе\nу отдела").HeaderAlignment(0.5f)
				.AddTextRenderer(node => node.InProcessAt, useMarkup: true)
				.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.AddSetter((c, n) => c.CellBackgroundGdk = n.BGColor)
			.Finish();
		
		public virtual IColumnsConfig ColumnsConfig => columnsConfig;
		#endregion

		#endregion

		public UndeliveredOrdersVM(UndeliveredOrdersFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public UndeliveredOrdersVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
			Filter = new UndeliveredOrdersFilter(UoW);
		}

		public UndeliveredOrdersVM(IUnitOfWork uow) : base()
		{
			this.UoW = uow;
			currUser = EmployeeRepository.GetEmployeeForCurrentUser(uow).Id;
		}

		public UndeliveredOrdersVM(IUnitOfWork uow, int undeliveryToShow) : this(uow)
		{
			this.undeliveryToShow = undeliveryToShow;
		}
	}

	public class UndeliveredOrdersVMNode
	{
		public int Id { get; set; }
		public int NumberInList { get; set; }
		[UseForSearch]
		[SearchHighlight]

		public bool IsComment { get; set; }

		public virtual string StrId { get => NumberInList.ToString(); set {; } }

		public UndeliveryStatus StatusEnum { get; set; }

		public virtual string DriverName { get => OldRouteListDriverName ?? "Заказ\nне в МЛ"; set {; } }
		[UseForSearch]
		[SearchHighlight]
		public virtual string Address { get; set; }
		[UseForSearch]
		[SearchHighlight]
		public virtual string Client { get; set; }
		public virtual string ClientAndAddress => String.Format("{0}\n{1}", Client, Address);
		[UseForSearch]
		[SearchHighlight]
		public virtual string Reason { get; set; }
		public virtual string OldOrderAuthor { get => StringWorks.PersonNameWithInitials(OldOrderAuthorLastName, OldOrderAuthorFirstName, OldOrderAuthorMidleName); set {; } }
		[UseForSearch]
		[SearchHighlight]
		public virtual string Guilty { get; set; }

		public virtual string UndeliveredOrderItems {
			get {
				if(OldOrder19LBottleQty > 0)
					return OldOrder19LBottleQty.ToString();
				if(OldOrderGoodsToClient != null)
					return "к клиенту:\n" + OldOrderGoodsToClient;
				if(OldOrderGoodsFromClient != null)
					return "от клиента:\n" + OldOrderGoodsFromClient;
				return "Другие\nтовары";
			}
			set {; }
		}

		public virtual string OldDeliverySchedule { get; set; }
		public virtual string DriversCall {
			get {
				if(OldRouteListDriverName == null)
					return "Заказ\nне в МЛ";
				string time = DriverCallType != DriverCallType.NoCall ? DriverCallTime.ToString("HH:mm\n") : "";
				return String.Format("{0}{1}", time, DriverCallType.GetEnumTitle());
			}
			set {; }
		}
		public virtual string OldOrderDeliveryDate { get => OldOrderDeliveryDateTime.ToString("d MMM"); set {; } }

		public virtual string DispatcherCall {
			get => DispatcherCallTime.HasValue ? DispatcherCallTime.Value.ToString("HH:mm") : "Не\nзвонили";
			set {; } 
		}

		[UseForSearch]
		[SearchHighlight]
		public virtual string TransferDateTime { 
			get => NewOrderId > 0 ? NewOrderDeliveryDate?.ToString("d MMM\n") + NewOrderDeliverySchedule + "\n№" + NewOrderId.ToString() : "Новый заказ\nне создан"; 
			set {; } 
		}
		public virtual string ActionWithInvoice { get => NewOrderId > 0 ? NewOrderId.ToString() : "Новый заказ\nне создан"; set {; } }
		public virtual string Registrator { get => StringWorks.PersonNameWithInitials(RegistratorLastName, RegistratorFirstName, RegistratorMidleName); set {; } }
		public virtual string UndeliveryAuthor { get => StringWorks.PersonNameWithInitials(AuthorLastName, AuthorFirstName, AuthorMidleName); set {; } }
		public virtual string Status { get => UndeliveryStatus.GetEnumTitle(); set {; } }
		public virtual string FinedPeople { get => Fined ?? "Не выставлено"; set {; } }
		public virtual string OldOrderStatus { get => String.Format("{0}\n\t↓\n{1}", StatusOnOldOrderCancel.GetEnumTitle(), OldOrderCurStatus.GetEnumTitle()); set {; } }

		public DateTime? DispatcherCallTime { get; set; }
		public DateTime DriverCallTime { get; set; }
		public DriverCallType DriverCallType { get; set; }
		public int DriverCallNr { get; set; }
		public string AuthorLastName { get; set; }
		public string AuthorFirstName { get; set; }
		public string AuthorMidleName { get; set; }
		public string EditorLastName { get; set; }
		public string EditorFirstName { get; set; }
		public string EditorMidleName { get; set; }
		public string RegistratorLastName { get; set; }
		public string RegistratorFirstName { get; set; }
		public string RegistratorMidleName { get; set; }
		public UndeliveryStatus UndeliveryStatus { get; set; }
		public GuiltyTypes GuiltySide { get; set; }
		public string GuiltyDepartment { get; set; }
		public string Fined { get; set; }
		public OrderStatus StatusOnOldOrderCancel { get; set; }
		public string InProcessAt { get; set; }

		//старый заказ
		public int OldOrderId { get; set; }
		public DateTime OldOrderDeliveryDateTime { get; set; }
		public string OldOrderAuthorLastName { get; set; }
		public string OldOrderAuthorFirstName { get; set; }
		public string OldOrderAuthorMidleName { get; set; }
		public int OldOrder19LBottleQty { get; set; }
		public string OldOrderGoodsToClient { get; set; }
		public string OldOrderGoodsFromClient { get; set; }
		public string OldRouteListDriverName { get; set; }
		public OrderStatus OldOrderCurStatus { get; set; }

		//новый заказ
		public int NewOrderId { get; set; }
		public DateTime? NewOrderDeliveryDate { get; set; }
		public string NewOrderDeliverySchedule { get; set; }

		//общее
		public virtual Color BGColor { get; set; }
		public virtual UndeliveredOrdersVMNode Parent { get; set; } = null;
		public virtual List<UndeliveredOrdersVMNode> Children { get; set; }
	}

	public class UndeliveredOrderCommentsNode : UndeliveredOrdersVMNode
	{
		public override string StrId { get => String.Empty; }
		public override string OldOrderDeliveryDate { get; set; } = String.Empty;
		public override string Reason { get; set; } = String.Empty;
		public override string ActionWithInvoice { get; set; } = String.Empty;
		public override string TransferDateTime { get; set; } = String.Empty;
		public override string Client { get; set; } = String.Empty;
		public override string Address { get; set; } = String.Empty;
		public override string UndeliveredOrderItems { get; set; } = String.Empty;
		public override string OldDeliverySchedule { get; set; } = String.Empty;
		public override string OldOrderAuthor { get; set; } = String.Empty;
		public override string DriverName { get; set; } = String.Empty;
		public override string DriversCall { get; set; } = String.Empty;
		public override string DispatcherCall { get; set; } = String.Empty;
		public override string Registrator { get; set; } = String.Empty;
		public override string UndeliveryAuthor { get; set; } = String.Empty;
		public override string Guilty { get; set; } = String.Empty;
		public override string FinedPeople { get; set; } = String.Empty;
		public override string Status { get; set; } = String.Empty;
		public override string OldOrderStatus { get; set; } = String.Empty;

		public override Color BGColor { get; set; }

		public override UndeliveredOrdersVMNode Parent { get; set; }
		public override List<UndeliveredOrdersVMNode> Children { get; set; } = new List<UndeliveredOrdersVMNode>();
	}

	public class CommentNode
	{
		public int Id { get; set; }
		public CommentedFields Field { get; set; }
		public string Comment { get; set; }
		public DateTime Date { get; set; }
	}
}
