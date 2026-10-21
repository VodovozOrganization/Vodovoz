using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;

namespace EdoService.Library.Services
{
	public interface IBillEdoRequestSender
	{
		/// <summary>
		/// Сохраняет подготовленную заявку счета и событие её создания в одной транзакции
		/// </summary>
		/// <param name="request">Заявка со снимком счета без созданной задачи</param>
		/// <param name="source">Источник запуска отправки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Идентификатор сохраненной заявки</returns>
		Task<int> SendAsync(
			InformalEdoRequest request, EdoRequestSource source, CancellationToken cancellationToken = default);

		/// <summary>
		/// Сохраняет заявку, событие её создания и изменения инициатора в одной транзакции
		/// </summary>
		/// <param name="uow">Единица работы инициатора отправки</param>
		/// <param name="request">Заявка со снимком счета без созданной задачи</param>
		/// <param name="source">Источник запуска отправки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Идентификатор сохраненной заявки</returns>
		Task<int> SendAsync(
			IUnitOfWork uow, InformalEdoRequest request, EdoRequestSource source, CancellationToken cancellationToken = default);
	}
}
