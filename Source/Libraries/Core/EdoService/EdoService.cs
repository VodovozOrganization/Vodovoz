using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Errors;
using Vodovoz.Extensions;
using EdoContainer = Vodovoz.Domain.Orders.Documents.EdoContainer;
using Order = Vodovoz.Domain.Orders.Order;
using Type = Vodovoz.Domain.Orders.Documents.Type;

namespace EdoService.Library
{
	public class EdoService : IEdoService
	{
		private static EdoDocFlowStatus[] _successfulEdoStatuses => new[] { EdoDocFlowStatus.Succeed, EdoDocFlowStatus.InProgress };

		public virtual void SetNeedToResendEdoDocumentForOrder<T>(T entity, Type type) where T : IDomainObject
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Ставим документ в очередь на переотправку в ЭДО"))
			{
				var edoDocumentsActions = UpdateEdoDocumentAction(uow, entity, type);

				if(type == Type.Upd)
				{
					var orderLastTrueMarkDocument = uow.GetAll<TrueMarkApiDocument>()
						.Where(x => x.Order.Id == entity.GetId())
						.OrderByDescending(x => x.CreationDate)
						.FirstOrDefault();

					if(orderLastTrueMarkDocument != null
						&& orderLastTrueMarkDocument.Type != TrueMarkApiDocument.TrueMarkApiDocumentType.WithdrawalCancellation)
					{
						edoDocumentsActions.IsNeedToCancelTrueMarkDocument = true;
					}
				}

				uow.Save(edoDocumentsActions);
				uow.Commit();
			}
		}

		private OrderEdoTrueMarkDocumentsActions UpdateEdoDocumentAction(IUnitOfWork uow, IDomainObject entity, Type type)
		{
			var restriction = GetRestrictionByType(entity, type);

			var edoDocumentsAction = uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
					.Where(restriction)
					.FirstOrDefault();

			if(edoDocumentsAction == null)
			{
				edoDocumentsAction = new OrderEdoTrueMarkDocumentsActions();
			}

			FillEdoDocumentsActionByType(edoDocumentsAction, entity, type);

			if(type == Type.Upd)
			{
				edoDocumentsAction.IsNeedToResendEdoUpd = true;
			}
			else
			{
				edoDocumentsAction.IsNeedToResendEdoBill = true;
			}

			return edoDocumentsAction;
		}

		private void FillEdoDocumentsActionByType(OrderEdoTrueMarkDocumentsActions edoDocumentsAction, IDomainObject entity, Type type)
		{
			switch(type)
			{
				case Type.Bill:
				case Type.Upd:
					edoDocumentsAction.Order = (Order)entity;
					break;
				case Type.BillWSForDebt:
					edoDocumentsAction.OrderWithoutShipmentForDebt = (OrderWithoutShipmentForDebt)entity;
					break;
				case Type.BillWSForPayment:
					edoDocumentsAction.OrderWithoutShipmentForPayment = (OrderWithoutShipmentForPayment)entity;
					break;
				case Type.BillWSForAdvancePayment:
					edoDocumentsAction.OrderWithoutShipmentForAdvancePayment = (OrderWithoutShipmentForAdvancePayment)entity;
					break;
				default:
					throw new NotImplementedException($"Не поддерживаемый тип {type.GetEnumDisplayName()}");
			}
		}

		private Expression<Func<OrderEdoTrueMarkDocumentsActions, bool>> GetRestrictionByType(IDomainObject entity, Type type)
		{
			switch(type)
			{
				case Type.Bill:
				case Type.Upd:
					return x => x.Order.Id == entity.GetId();
				case Type.BillWSForDebt:
					return x => x.OrderWithoutShipmentForDebt.Id == entity.GetId();
				case Type.BillWSForPayment:
					return x => x.OrderWithoutShipmentForPayment.Id == entity.GetId();
				case Type.BillWSForAdvancePayment:
					return x => x.OrderWithoutShipmentForAdvancePayment.Id == entity.GetId();
				default:
					throw new NotImplementedException($"Не поддерживаемый тип {type.GetEnumDisplayName()}");
			}
		}

		public Result ValidateEdoContainers(IList<EdoContainer> edoContainers)
		{
			var errors = new List<Error>();

			foreach(var edoContainer in edoContainers)
			{
				if(_successfulEdoStatuses.Contains(edoContainer.EdoDocFlowStatus))
				{
					errors.Add(Vodovoz.Errors.Edo.Edo.CreateAlreadySuccefullSended(edoContainer));
				}
			}

			if(errors.Any())
			{
				return Result.Failure(errors);
			}

			return Result.Success();
		}

		public Result ValidateOrderForUpd(Order order)
		{
			var errors = new List<Error>();

			if(order.OrderPaymentStatus == OrderPaymentStatus.Paid)
			{
				errors.Add(Vodovoz.Errors.Edo.Edo.CreateAlreadyPaidUpd(order.Id));
			}

			if(errors.Any())
			{
				return Result.Failure(errors);
			}

			return Result.Success();
		}
	}
}
