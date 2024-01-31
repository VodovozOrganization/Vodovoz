using System;
using QS.Tdi;

namespace Vodovoz.Core.Journal
{
	/// <summary>
	/// Задает параметры для определения действия по созданию новой сущности в журнале
	/// </summary>
	public sealed class ActionForCreateEntityConfig
	{
		private string title;
		private Func<ITdiTab> createEntityFunc;
		Type entityType;

		public ActionForCreateEntityConfig(Type entityType, string title, Func<ITdiTab> createEntityFunc)
		{
			this.entityType = entityType;
			this.title = title;
			this.createEntityFunc = createEntityFunc;
		}

		public Type EntityType => entityType;

		public string Title => title;

		public ITdiTab GetNewEntityDlg()
		{
			return createEntityFunc.Invoke();
		}
	}
}
