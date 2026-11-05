using System;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;

namespace Edo.DeviationMonitoring
{
	/// <summary>Выполняет автоматические действия по подтвержденному актуальному отклонению.</summary>
	public interface IEdoDeviationProcessingService
	{
		/// <summary>Проверяет, требуется ли действие по отклонению в текущем проходе, без обращения к БД.</summary>
		/// <param name="deviation">Отклонение.</param>
		/// <param name="checkTime">Время прохода обработки.</param>
		/// <returns>Признак необходимости действия.</returns>
		bool CanProcess(EdoTaskDeviation deviation, DateTime checkTime);

		/// <summary>Ставит повторное действие или уведомление в очередь текущей транзакции.</summary>
		/// <param name="uow">Единица работы, сохраняющая отклонение и сообщения.</param>
		/// <param name="deviation">Актуальное отклонение.</param>
		/// <param name="checkTime">Время прохода обработки.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns>Задача обработки.</returns>
		Task ProcessAsync(IUnitOfWork uow, EdoTaskDeviation deviation, DateTime checkTime, CancellationToken cancellationToken);
	}
}
