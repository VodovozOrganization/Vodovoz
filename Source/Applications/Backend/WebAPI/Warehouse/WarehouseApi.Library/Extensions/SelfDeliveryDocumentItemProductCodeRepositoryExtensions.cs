using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Specifications.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;

namespace WarehouseApi.Library.Extensions
{
	/// <summary>
	/// Загрузка кодов продуктов строк отпуска самовывоза
	/// </summary>
	public static class SelfDeliveryDocumentItemProductCodeRepositoryExtensions
	{
		/// <summary>
		/// Загружает одним запросом коды продуктов строк документа и группирует их по идентификатору строки.
		/// Несохранённые строки (Id == 0) кодов не имеют
		/// </summary>
		/// <param name="repository">Репозиторий кодов продуктов строк самовывоза</param>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="documentItems">Строки документа</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		public static async Task<ILookup<int, SelfDeliveryDocumentItemTrueMarkProductCode>> GetProductCodesByItemIdAsync(
			this IGenericRepository<SelfDeliveryDocumentItemTrueMarkProductCode> repository,
			IUnitOfWork uow,
			IEnumerable<SelfDeliveryDocumentItem> documentItems,
			CancellationToken cancellationToken)
		{
			var savedItemsIds = documentItems
				.Where(x => x.Id > 0)
				.Select(x => x.Id)
				.Distinct()
				.ToList();

			if(savedItemsIds.Count == 0)
			{
				return Enumerable.Empty<SelfDeliveryDocumentItemTrueMarkProductCode>().ToLookup(x => x.SelfDeliveryDocumentItem.Id);
			}

			var productCodes = (await repository
				.GetAsync(
					uow,
					SelfDeliveryDocumentItemTrueMarkProductCodeSpecification.CreateForSelfDeliveryDocumentItemIds(savedItemsIds),
					cancellationToken: cancellationToken))
				.Value;

			return productCodes.ToLookup(x => x.SelfDeliveryDocumentItem.Id);
		}
	}
}
