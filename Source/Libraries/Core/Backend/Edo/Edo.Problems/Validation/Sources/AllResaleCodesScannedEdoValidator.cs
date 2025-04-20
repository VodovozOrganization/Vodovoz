using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Common;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Problems.Validation.Sources
{
	public class AllCodeScannedEdoValidator : EdoTaskProblemValidatorSource, IEdoTaskValidator
	{
		public override string Name
		{
			get => "Order.AllCodesScanned";
		}

		public override EdoProblemImportance Importance
		{
			get => EdoProblemImportance.Problem;
		}

		public override string Message
		{
			get => "Недостаточно валидных кодов";
		}

		public override string Description
		{
			get => "Проверяет, что валидных кодов достаточно";
		}

		public override string Recommendation
		{
			get => "Добавить недостающие валидные коды";
		}

		public string GetTemplateMessage(EdoTask edoTask)
		{
			return $"Для задачи  №{edoTask.Id} недостаточное количество валидных кодов";
		}

		public override bool IsApplicable(EdoTask edoTask)
		{
			var documentEdoTask = edoTask as DocumentEdoTask;
			var receiptEdoTask = edoTask as ReceiptEdoTask;

			if(documentEdoTask == null && receiptEdoTask == null)
			{
				return false;
			}

			var orderEdoRequest = documentEdoTask?.OrderEdoRequest ?? receiptEdoTask.OrderEdoRequest;

			return orderEdoRequest.Order.IsOrderForResale && !orderEdoRequest.Order.IsNeedIndividualSetOnLoad;
		}

		public override async Task<EdoValidationResult> ValidateAsync(EdoTask edoTask, IServiceProvider serviceProvider,
			CancellationToken cancellationToken)
		{
			var uowFactory = serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
			var trueMarkTaskCodesValidator = serviceProvider.GetRequiredService<ITrueMarkCodesValidator>();
			var edoTaskTrueMarkCodeCheckerFactory = serviceProvider.GetRequiredService<EdoTaskItemTrueMarkStatusProviderFactory>();
			var trueMarkCodesChecker = edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);

			var documentEdoTask = edoTask as DocumentEdoTask;
			var receiptEdoTask = edoTask as ReceiptEdoTask;
			var orderEdoRequest = documentEdoTask?.OrderEdoRequest ?? receiptEdoTask.OrderEdoRequest;

			using(var uow = uowFactory.CreateWithoutRoot(nameof(AllCodeScannedEdoValidator)))
			{
				return await IsAllTrueMarkProductCodesAddedToOrder(uow, trueMarkTaskCodesValidator, trueMarkCodesChecker,
					orderEdoRequest, cancellationToken)
					? EdoValidationResult.Valid(this)
					: EdoValidationResult.Invalid(this);
			}
		}

		private async Task<bool> IsAllTrueMarkProductCodesAddedToOrder(
			IUnitOfWork unitOfWork,
			ITrueMarkCodesValidator trueMarkTaskCodesValidator,
			EdoTaskItemTrueMarkStatusProvider trueMarkCodesChecker,
			OrderEdoRequest orderEdoRequest,
			CancellationToken cancellationToken)
		{
			var accountableInTrueMarkItemsGtins =
				(from oi in unitOfWork.Session.Query<OrderItemEntity>()
					where oi.Order.Id == orderEdoRequest.Order.Id && oi.Nomenclature.IsAccountableInTrueMark
					from gtin in oi.Nomenclature.Gtins
					group oi by gtin.GtinNumber
					into grouped
					select new
					{
						Gtin = grouped.Key,
						TotalCount = grouped.Sum(item => item.Count)
					})
				.ToDictionary(result => result.Gtin, result => result.TotalCount);

			var addedTrueMarkCodesFromRouteList =
				(from routeListItem in unitOfWork.Session.Query<RouteListItemEntity>()
					join productCode in unitOfWork.Session.Query<RouteListItemTrueMarkProductCode>()
						on routeListItem.Id equals productCode.RouteListItem.Id
					where routeListItem.Order.Id == orderEdoRequest.Order.Id
					      && productCode.SourceCodeStatus == SourceProductCodeStatus.Accepted
					group productCode by productCode.ResultCode.GTIN
					into grouped
					select new
					{
						GTIN = grouped.Key,
						TotalCount = grouped.Count()
					})
				.ToDictionary(group => group.GTIN, group => group.TotalCount);

			var addedTrueMarkCodesFromSelfDelivery =
				(from item in unitOfWork.Session.Query<SelfDeliveryDocumentItemEntity>()
					join document in unitOfWork.Session.Query<SelfDeliveryDocumentEntity>() on item.SelfDeliveryDocument.Id equals document.Id
					where document.Order.Id == orderEdoRequest.Order.Id
					join productCode in unitOfWork.Session.Query<SelfDeliveryDocumentItemTrueMarkProductCode>()
						on item.Id equals productCode.SelfDeliveryDocumentItem.Id
					group productCode by productCode.ResultCode.GTIN
					into grouped
					select new
					{
						GTIN = grouped.Key,
						TotalCount = grouped.Count(),
					})
				.ToDictionary(group => group.GTIN, group => group.TotalCount);

			var addedTrueMarkCodes = addedTrueMarkCodesFromRouteList.Concat(addedTrueMarkCodesFromSelfDelivery);

			foreach(var accountableItem in accountableInTrueMarkItemsGtins)
			{
				var added = addedTrueMarkCodes.FirstOrDefault(x => x.Key == accountableItem.Key);

				if(added.Value < accountableItem.Value)
				{
					return false;
				}
			}

			var trueMarkValidationResult = await trueMarkTaskCodesValidator.ValidateAsync(orderEdoRequest.Task, trueMarkCodesChecker, cancellationToken);

			return trueMarkValidationResult.ReadyToSell;
		}
	}
}
