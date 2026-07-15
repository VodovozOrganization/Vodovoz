using System.Threading.Tasks;
using System.Threading;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using System.Collections.Generic;
using QS.DomainModel.UoW;

namespace Vodovoz.Core.Data.Repositories
{
	public interface ITrueMarkCodeRepository
	{
		/// <summary>
		/// Загрузить экземплярные коды маркировки
		/// </summary>
		/// <param name="unitOfWork">IUnitOfWork</param>
		/// <param name="codeIds">Массив идентификаторов кодов маркировки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Коллекция экземплярных кодов маркировки</returns>
		Task<IEnumerable<TrueMarkWaterIdentificationCode>> LoadWaterCodes(
			IUnitOfWork unitOfWork,
			int[] codeIds,
			CancellationToken cancellationToken);

		Task PreloadCodes(IEnumerable<TrueMarkWaterIdentificationCode> codes, CancellationToken cancellationToken);
		Task<TrueMarkTransportCode> FindParentTransportCode(
			TrueMarkWaterIdentificationCode code,
			CancellationToken cancellationToken
		);
		Task<TrueMarkTransportCode> FindParentTransportCode(
			TrueMarkWaterGroupCode code,
			CancellationToken cancellationToken
		);
		Task<TrueMarkWaterGroupCode> GetGroupCode(int id, CancellationToken cancellationToken);

		/// <summary>
		/// Групповой код используется в ЭДО документе?
		/// </summary>
		/// <param name="groupCodeId">Id группового кода</param>
		/// <returns>Признак использования группового кода в ЭДО документе</returns>
		bool IsGroupCodeUsedInEdoDocument(int groupCodeId);
	}
}
