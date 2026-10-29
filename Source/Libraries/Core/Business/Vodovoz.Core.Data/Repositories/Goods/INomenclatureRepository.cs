using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Data.Repositories.Goods
{
	public interface INomenclatureRepository
	{
		/// <summary>
		/// Получить номенклатуру по GTIN
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="gtin">GTIN</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Номенклатура</returns>
		Task<NomenclatureEntity> GetNomenclatureByGtinAsync(
			IUnitOfWork uow,
			string gtin,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Получить список GTIN
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список GTIN</returns>
		Task<IEnumerable<GtinEntity>> GetGtinsAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Получить список групп GTIN
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список групп GTIN</returns>
		Task<IEnumerable<GroupGtinEntity>> GetGroupGtinsAsync(CancellationToken cancellationToken);
	}
}
