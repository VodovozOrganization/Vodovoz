using Gamma.Binding.Core;
using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.Edo
{
	public interface IEdoDocumentActionsFactory
	{
		/// <summary>
		/// Строит список доступных действий (переотправка и т.п.) для выбранного документа ЭДО.
		/// </summary>
		/// <param name="document">Строка истории документа (может быть null — тогда вернётся пустой список)</param>
		/// <param name="onActionCompleted">Коллбэк, который нужно вызвать после успешного выполнения действия (обновление списка документов)</param>
		IEnumerable<BusyCommand> CreateActions(
			EdoInOrderDocumentHistoryRowViewModel document,
			Action onActionCompleted);
	}
}
