using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
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
			get => "Не отсканировано необходиомое количество кодов";
		}

		public override string Description
		{
			get => "Проверяет, что все коды отсканированы";
		}

		public override string Recommendation
		{
			get => "Добавить недостающие коды";
		}

		public string GetTemplateMessage(EdoTask edoTask)
		{
			return $"Для задачи  №{edoTask.Id} не отсканировано необходиомое количество кодов";
		}

		public override bool IsApplicable(EdoTask edoTask)
		{
			var documentEdoTask = edoTask as DocumentEdoTask;
			if(documentEdoTask == null)
			{
				return false;
			}

			var order = documentEdoTask.OrderEdoRequest.Order;

			return documentEdoTask.OrderEdoRequest.Source != CustomerEdoRequestSource.Selfdelivery && order.IsOrderForResale && !order.IsNeedIndividualSetOnLoad;
		}

		public override async Task<EdoValidationResult> ValidateAsync(EdoTask edoTask, IServiceProvider serviceProvider,
			CancellationToken cancellationToken)
		{
			var uowFactory = serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
			var documentEdoTask = edoTask as DocumentEdoTask;
			var orderId = documentEdoTask.OrderEdoRequest.Order.Id;

			using(var uow = uowFactory.CreateWithoutRoot(nameof(AllCodeScannedEdoValidator)))
			{
				return IsAllRouteListItemTrueMarkProductCodesAddedToOrder(uow, orderId)
					? EdoValidationResult.Valid(this)
					: EdoValidationResult.Invalid(this);
			}
		}

		private bool IsAllRouteListItemTrueMarkProductCodesAddedToOrder(IUnitOfWork uow, int orderId)
		{
			var accountableInTrueMarkItemsGtins =
				(from oi in uow.Session.Query<OrderItemEntity>()
					where oi.Order.Id == orderId && oi.Nomenclature.IsAccountableInTrueMark
					from gtin in oi.Nomenclature.Gtins
					group oi by gtin.GtinNumber
					into grouped
					select new
					{
						Gtin = grouped.Key,
						TotalCount = grouped.Sum(item => item.Count)
					})
				.ToDictionary(result => result.Gtin, result => result.TotalCount);

			var addedTrueMarkCodes = (from routeListItem in uow.Session.Query<RouteListItemEntity>()
					join productCode in uow.Session.Query<RouteListItemTrueMarkProductCode>()
						on routeListItem.Id equals productCode.RouteListItem.Id
					where routeListItem.Order.Id == orderId
					      && productCode.SourceCodeStatus == SourceProductCodeStatus.Accepted
					group productCode by productCode.ResultCode.GTIN
					into grouped
					select new
					{
						GTIN = grouped.Key,
						TotalCount = grouped.Count()
					})
				.ToDictionary(group => group.GTIN, group => group.TotalCount);


			foreach(var accountableItem in accountableInTrueMarkItemsGtins)
			{
				var added = addedTrueMarkCodes.FirstOrDefault(x => x.Key == accountableItem.Key);

				if(added.Value < accountableItem.Value)
				{
					return false;
				}
			}

			return true;
		}
	}
}
