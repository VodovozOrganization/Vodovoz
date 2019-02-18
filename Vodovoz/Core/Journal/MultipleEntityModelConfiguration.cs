using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.Tdi;

namespace Vodovoz.Core.Journal
{
	/// <summary>
	/// Определяет методы для конфигурирования модели под определенную сущность, и определения конфигурации различных документов для этой сущности
	/// </summary>
	public sealed class MultipleEntityModelConfiguration<TEntity, TNode>
		where TEntity : class, IDomainObject, new()
		where TNode : MultipleEntityVMNodeBase
	{
		private Func<IList<TNode>> dataFunc;
		private List<MultipleEntityModelDocumentConfig<TNode>> docConfigs;
		private Action<Func<IList<TNode>>, List<MultipleEntityModelDocumentConfig<TNode>>> finishConfigAction;

		public MultipleEntityModelConfiguration(Action<Func<IList<TNode>>, List<MultipleEntityModelDocumentConfig<TNode>>> finishConfigAction)
		{
			docConfigs = new List<MultipleEntityModelDocumentConfig<TNode>>();
			this.finishConfigAction = finishConfigAction;
		}

		/// <summary>
		/// Добавление функции загрузки списка документов
		/// </summary>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddDataFunction(Func<IList<TNode>> dataFunc)
		{
			this.dataFunc = dataFunc;
			return this;
		}

		/// <summary>
		/// Добавление функций открытия диалогов для документа с определенным идентификатором
		/// </summary>
		/// <returns>The configuration.</returns>
		/// <param name="docIdentificator">Уникальный идентификатор типа документа</param>
		/// <param name="createDlgFunc">Функция вызова диалога создания нового документа</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddDocumentConfiguration<TEntityDlg>(Func<TNode, bool> docIdentificationFunc, string createActionTitle, Func<TEntityDlg> createDlgFunc, Func<TNode, TEntityDlg> openDlgFunc)
			where TEntityDlg : EntityDialogBase<TEntity>, ITdiTab
		{
			CheckDialogRestrictions(typeof(TEntityDlg));

			var dlgInfo = new MultipleEntityModelDocumentConfig<TNode>(typeof(TEntity), createActionTitle, createDlgFunc, openDlgFunc, docIdentificationFunc);
			docConfigs.Add(dlgInfo);
			return this;
		}

		/// <summary>
		/// Завершение конфигурации документа, проверка конфликтов и запись конфиругации в модель
		/// </summary>
		public void FinishConfiguration()
		{
			if(dataFunc == null) {
				throw new ArgumentNullException($"Для класса \"{nameof(TEntity)}\" не определена функция получения данных. Для ее определения необходимо вызвать метод \"{nameof(AddDataFunction)}\"");
			}
			if(!docConfigs.Any()) {
				throw new ArgumentNullException($"Для класса \"{nameof(TEntity)}\" должна быть определена как минимум одна конфигурация диалогов. Для ее определения необходимо вызвать метод \"{nameof(AddDocumentConfiguration)}\"");
			}
			finishConfigAction.Invoke(dataFunc, docConfigs);
		}

		private void CheckDialogRestrictions(Type dialogType)
		{
			var dlgCtorForCreateNewEntity = dialogType.GetConstructor(Type.EmptyTypes);
			if(dlgCtorForCreateNewEntity == null) {
				throw new InvalidOperationException("Диалог должен содержать конструктор без параметров для создания новой сущности");
			}

			var dlgCtorForOpenEntity = dialogType.GetConstructor(new[] { typeof(int) });
			if(dlgCtorForOpenEntity == null) {
				throw new InvalidOperationException("Диалог должен содержать конструктор принимающий id сущности для ее открытия");
			}
		}
	}
}
