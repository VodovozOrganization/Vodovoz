using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo.Upd;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>Выборка сохранённых УПД клиентских заказов и трансферов.</summary>
	public interface IEdoUpdDocumentRepository
	{
		/// <summary>Получает страницу сохранённых УПД.</summary>
		/// <param name="uow">Контекст загрузки.</param>
		/// <param name="filter">Условия выборки.</param>
		/// <param name="offset">Число пропускаемых строк.</param>
		/// <param name="limit">Максимальное число строк.</param>
		/// <returns>Реквизиты документов и связанных задач в порядке убывания даты и кодов.</returns>
		IList<EdoUpdDocumentNode> GetItems(IUnitOfWork uow, EdoUpdDocumentFilter filter, int? offset, int? limit);

		/// <summary>Получает количество строк по условиям выборки.</summary>
		/// <param name="uow">Контекст загрузки.</param>
		/// <param name="filter">Условия выборки.</param>
		/// <returns>Количество строк.</returns>
		int GetItemsCount(IUnitOfWork uow, EdoUpdDocumentFilter filter);

		/// <summary>Загружает сохранённый УПД.</summary>
		/// <param name="uow">Контекст просмотра документа.</param>
		/// <param name="documentId">Код УПД.</param>
		/// <returns>Документ или null, если он не найден.</returns>
		EdoUpdDocument GetDocument(IUnitOfWork uow, int documentId);
	}
}
