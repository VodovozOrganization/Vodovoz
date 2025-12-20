using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Common;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Problems.Validation.Sources
{
	public partial class AllResaleCodesScannedEdoValidator : OrderEdoValidatorBase, IEdoTaskValidator
	{
		private readonly ICounterpartyEdoAccountEntityController _edoAccountEntityController;

		public AllResaleCodesScannedEdoValidator(ICounterpartyEdoAccountEntityController edoAccountEntityController)
		{
			_edoAccountEntityController =
				edoAccountEntityController ?? throw new ArgumentNullException(nameof(edoAccountEntityController));
		}
		
		public override string Name
		{
			get => "Order.AllResaleCodesScanned";
		}

		public override EdoProblemImportance Importance
		{
			get => EdoProblemImportance.Problem;
		}

		public override string Message
		{
			get => "На стадии подготовки данных в задаче на отправку обнаружено недостаточно количество валидных кодов для перепродажи";
		}

		public override string Description
		{
			get => "Проверяет, что для перепродажи достаточно валидных кодов";
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
			if(!(edoTask is OrderEdoTask orderEdoTask))
			{
				return false;
			}
			
			var orderEdoRequest = orderEdoTask.FormalEdoRequest;

			return (orderEdoRequest.Order.IsOrderForResale && !orderEdoRequest.Order.IsNeedIndividualSetOnLoad(_edoAccountEntityController))
				|| orderEdoRequest.Order.IsOrderForTender;
		}

		public override async Task<EdoValidationResult> ValidateAsync(EdoTask edoTask, IServiceProvider serviceProvider,
			CancellationToken cancellationToken)
		{
			var uowFactory = serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
			var trueMarkTaskCodesValidator = serviceProvider.GetRequiredService<ITrueMarkCodesValidator>();
			var edoTaskTrueMarkCodeCheckerFactory = serviceProvider.GetRequiredService<EdoTaskItemTrueMarkStatusProviderFactory>();
			var trueMarkCodesChecker = edoTaskTrueMarkCodeCheckerFactory.Create(edoTask);
			
			if(!(edoTask is OrderEdoTask orderEdoTask))
			{
				return EdoValidationResult.Invalid(this);
			}

			using(var uow = uowFactory.CreateWithoutRoot(nameof(AllResaleCodesScannedEdoValidator)))
			{
				return await IsAllTrueMarkProductCodesAddedToOrder(uow, trueMarkTaskCodesValidator, trueMarkCodesChecker,
					orderEdoTask.FormalEdoRequest, cancellationToken)
					? EdoValidationResult.Valid(this)
					: EdoValidationResult.Invalid(this);
			}
		}

		private async Task<bool> IsAllTrueMarkProductCodesAddedToOrder(
			IUnitOfWork unitOfWork,
			ITrueMarkCodesValidator trueMarkTaskCodesValidator,
			EdoTaskItemTrueMarkStatusProvider trueMarkCodesChecker,
			FormalEdoRequest orderEdoRequest,
			CancellationToken cancellationToken)
		{
			#region Запросы в БД
			
			// Что нужно отсканировать в заказе
			var accountableInTrueMarkItemsOrderGtins =
				(from oi in unitOfWork.Session.Query<OrderItemEntity>()
					where oi.Order.Id == orderEdoRequest.Order.Id && oi.Nomenclature.IsAccountableInTrueMark
					from gtin in oi.Nomenclature.Gtins
					group oi by gtin.GtinNumber
					into grouped
					select new ScannedNomenclatureGtinDto
					{
						NomenclatureId = grouped.Select(x => x.Nomenclature.Id).FirstOrDefault(),
						Gtin = grouped.Key,
						Amount = grouped.Sum(item => item.Count)
					})
				.ToFuture();

			// Что отсканировано в МЛ
			var scannedRouteListCodes =
				(from routeListItem in unitOfWork.Session.Query<RouteListItemEntity>()
					join productCode in unitOfWork.Session.Query<RouteListItemTrueMarkProductCode>()
						on routeListItem.Id equals productCode.RouteListItem.Id
					where routeListItem.Order.Id == orderEdoRequest.Order.Id
					      && productCode.SourceCodeStatus == SourceProductCodeStatus.Accepted
					group productCode by productCode.ResultCode.Gtin
					into grouped
					select new ScannedNomenclatureGtinDto
					{
						Gtin = grouped.Key,
						Amount = grouped.Count()
					})
				.ToFuture();

			// Что отсканировано в самовывозах
			var scannedSelfDeliveryCodes =
				(from item in unitOfWork.Session.Query<SelfDeliveryDocumentItemEntity>()
					join document in unitOfWork.Session.Query<SelfDeliveryDocumentEntity>() on item.Document.Id equals document.Id
					where document.Order.Id == orderEdoRequest.Order.Id
					join productCode in unitOfWork.Session.Query<SelfDeliveryDocumentItemTrueMarkProductCode>()
						on item.Id equals productCode.SelfDeliveryDocumentItem.Id
					group productCode by productCode.ResultCode.Gtin
					into grouped
					select new ScannedNomenclatureGtinDto
					{
						Gtin = grouped.Key,
						Amount = grouped.Count(),
					})
				.ToFuture();
			
			#endregion Запросы в БД

			// Всё, что отсканировано
			var allScannedCodes = scannedRouteListCodes.Concat(scannedSelfDeliveryCodes).ToArray();
			
			// У одной номенклатуры несколько Gtin - собираем все Gtin-ы каждой номенклатуры в один список
			var needToScanNomenclaturesWithGtinLists =
				accountableInTrueMarkItemsOrderGtins
					.GroupBy(x => x.NomenclatureId)
					.Select(g =>
						new
						{
							NomenclatureId = g.Key,
							GtinList = g.Select(x => x.Gtin),
							Amount = g.First().Amount
						});
			
			// Все ли коды отсканированы?
			foreach(var needToScan in needToScanNomenclaturesWithGtinLists)
			{
				var scanned = allScannedCodes.FirstOrDefault(x => needToScan.GtinList.Contains(x.Gtin));

				if(scanned == null || scanned.Amount < needToScan.Amount)
				{
					return false;
				}
			}
			
			// Все ли отсканированные коды прикреплены к задаче?
			if(orderEdoRequest.Task.Items.Count < allScannedCodes.Sum(x => x.Amount))
			{
				return false;
			}

			// Валидны ли коды в ЧЗ?
			var trueMarkValidationResult =
				await trueMarkTaskCodesValidator.ValidateAsync(orderEdoRequest.Task, trueMarkCodesChecker, cancellationToken);
			
			return trueMarkValidationResult.IsAllValid;
		}
	}
}
