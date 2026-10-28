using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo.Upd;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;

namespace Vodovoz.ViewModels.Services.Edo
{
	/// <summary>Получение сохранённых УПД для журнала и просмотра.</summary>
	public interface IEdoUpdDocumentService
	{
		/// <summary>Получает страницу журнала УПД.</summary>
		/// <param name="uow">Контекст загрузчика журнала.</param>
		/// <param name="filter">Условия выборки.</param>
		/// <param name="offset">Число пропускаемых строк.</param>
		/// <param name="limit">Максимальное число строк.</param>
		/// <returns>Строки журнала в порядке убывания даты и кода документа.</returns>
		IList<EdoUpdJournalNode> GetItems(IUnitOfWork uow, EdoUpdJournalFilter filter, int? offset, int? limit);

		/// <summary>Получает количество строк журнала по фильтру.</summary>
		/// <param name="uow">Контекст загрузчика журнала.</param>
		/// <param name="filter">Условия выборки.</param>
		/// <returns>Количество строк.</returns>
		int GetItemsCount(IUnitOfWork uow, EdoUpdJournalFilter filter);

		/// <summary>Загружает сохранённый УПД.</summary>
		/// <param name="uow">Контекст просмотра документа.</param>
		/// <param name="documentId">Код УПД.</param>
		/// <returns>Документ или null, если он не найден.</returns>
		EdoUpdDocument GetDocument(IUnitOfWork uow, int documentId);
	}
}
