using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public interface ITrueMarkRepository
	{
		Task<IEnumerable<TrueMarkWaterIdentificationCode>> LoadWaterCodes(List<int> codeIds, CancellationToken cancellationToken);

		ISet<string> GetAllowedCodeOwnersInn();

		ISet<string> GetAllowedCodeOwnersGtins();

		IEnumerable<TrueMarkWaterIdentificationCode> GetTrueMarkCodeDuplicates(IUnitOfWork uow, string gtin, string serialNumber, string checkCode);

		/// <summary>
		/// Получить все значения Gtin в базе с названиями номенклатур
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Словарь. Ключ - gtin, значение - список названий номенклатур с данным gtin</returns>
		Task<IDictionary<string, List<string>>> GetGtinsNomenclatureData(IUnitOfWork uow, CancellationToken cancellationToken);

		/// <summary>
		/// Получить все значения Gtin товаров, проданных за предыдущий день, и их количество
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Словарь. Ключ - gtin, значение - количество единиц товаров с данным gtin</returns>
		Task<IDictionary<string, int>> GetSoldYesterdayGtinsCount(IUnitOfWork uow, CancellationToken cancellationToken);

		/// <summary>
		/// Получить значение Gtin и количество кодов, отсутствующих в пуле, необходимых для формирования передаточных документов
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Словарь. Ключ - gtin, значение - количество кодов с данным gtin, необходимых для документов</returns>
		Task<IDictionary<string, int>> GetMissingCodesCount(IUnitOfWork uow, CancellationToken cancellationToken);
	}
}
