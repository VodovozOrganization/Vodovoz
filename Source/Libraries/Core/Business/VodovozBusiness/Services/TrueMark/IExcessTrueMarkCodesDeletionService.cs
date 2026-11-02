using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.Services.TrueMark
{
	/// <summary>
	/// Удаляет выбранные лишние коды из задачи ЭДО с возвратом в пул.
	/// </summary>
	public interface IExcessTrueMarkCodesDeletionService
	{
		/// <summary>Проверяет возможность удаления до распределения кодов.</summary>
		/// <param name="task">Задача выбранного документа.</param>
		/// <returns>Доступно ли удаление лишних кодов.</returns>
		bool CanDelete(OrderEdoTask task);

		/// <summary>Возвращает выбранные коды в пул и удаляет элементы задачи без фиксации транзакции.</summary>
		/// <param name="uow">UOW.</param>
		/// <param name="taskId">Номер задачи выбранного документа.</param>
		/// <param name="productCodeIds">Идентификаторы выбранных кодов товаров.</param>
		/// <returns>Результат проверки и удаления.</returns>
		Result Delete(IUnitOfWork uow, int taskId, IEnumerable<int> productCodeIds);
	}
}
