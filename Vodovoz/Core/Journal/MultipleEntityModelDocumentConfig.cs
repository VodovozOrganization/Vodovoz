using System;
using QS.Tdi;

namespace Vodovoz.Core.Journal
{
	/// <summary>
	/// Определяет идентификацию документа в журнале и действия по изменению и созданию
	/// </summary>
	public sealed class MultipleEntityModelDocumentConfig<TNode>
			where TNode : MultipleEntityVMNodeBase
	{
		private ActionForCreateEntityConfig newEntityActionConfig;
		private Func<TNode, ITdiTab> openDialogFunc;
		private Func<TNode, bool> nodeIdentifierFunc;

		public MultipleEntityModelDocumentConfig(Type entityType, string createActionTitle, Func<ITdiTab> createDialogFunc, Func<TNode, ITdiTab> openDialogFunc, Func<TNode, bool> nodeIdentifierFunc)
		{
			this.openDialogFunc = openDialogFunc;
			this.nodeIdentifierFunc = nodeIdentifierFunc;
			newEntityActionConfig = new ActionForCreateEntityConfig(entityType, createActionTitle, createDialogFunc);
		}

		public bool IsIdentified(TNode node)
		{
			return nodeIdentifierFunc.Invoke(node);
		}

		public ActionForCreateEntityConfig CreateEntityActionConfig => newEntityActionConfig;

		public ITdiTab GetOpenEntityDlg(TNode node)
		{
			return openDialogFunc.Invoke(node);
		}
	}
}
