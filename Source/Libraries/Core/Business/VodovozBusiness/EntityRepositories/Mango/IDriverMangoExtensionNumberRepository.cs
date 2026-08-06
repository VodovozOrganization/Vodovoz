using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Mango;

namespace Vodovoz.EntityRepositories.Mango
{
	/// <summary>
	/// Репозиторий добавочных номеров Манго для водителей
	/// </summary>
	public interface IDriverMangoExtensionNumberRepository
	{
		/// <summary>
		/// Возвращает все занятые добавочные номера (любого статуса).
		/// Деактивированные номера не переиспользуются, поэтому учитываются тоже
		/// </summary>
		Task<IReadOnlyCollection<int>> GetUsedExtensionNumbersAsync(IUnitOfWork uow, CancellationToken cancellationToken);

		/// <summary>
		/// Есть ли у водителя активный добавочный номер
		/// </summary>
		Task<bool> HasActiveExtensionNumberAsync(IUnitOfWork uow, int driverId, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает активные добавочные номера, активированные раньше указанной даты
		/// </summary>
		/// <param name="activatedBefore">
		/// Верхняя граница даты активации (не включительно). Номера, активированные позже, не возвращаются
		/// </param>
		Task<IReadOnlyList<DriverMangoExtensionNumber>> GetActiveExtensionNumbersAsync(
			IUnitOfWork uow,
			DateTime activatedBefore,
			CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает добавочный номер по идентификатору
		/// </summary>
		Task<DriverMangoExtensionNumber> GetByIdAsync(IUnitOfWork uow, int id, CancellationToken cancellationToken);
	}
}
