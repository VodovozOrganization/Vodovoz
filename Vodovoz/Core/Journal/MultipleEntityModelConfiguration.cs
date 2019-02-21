using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.Tdi;
using QS.Utilities.Text;

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
		private Dictionary<Type, MultipleEntityModelDocumentConfig<TNode>> docConfigs;
		private Action<Func<IList<TNode>>, IEnumerable<MultipleEntityModelDocumentConfig<TNode>>> finishConfigAction;
		private MultipleEntityModelDocumentConfig<TNode> defaultConfig;

		public MultipleEntityModelConfiguration(Action<Func<IList<TNode>>, IEnumerable<MultipleEntityModelDocumentConfig<TNode>>> finishConfigAction)
		{
			docConfigs = new Dictionary<Type, MultipleEntityModelDocumentConfig<TNode>>();
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

		private string GetEntityTitleName()
		{
			var names = DomainHelper.GetSubjectNames(typeof(TEntity));
			if(names == null || string.IsNullOrWhiteSpace(names.Nominative)) {
				throw new ApplicationException($"Для типа {nameof(TEntity)} не проставлен аттрибут AppellativeAttribute, или в аттрибуте не проставлено имя Nominative, из-за чего невозможно разрешить правильное имя документа для отображения в журнале с конфигурацией документов по умолчанию.");
			}
			return names.Nominative.StringToTitleCase();
		}

		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, идентификатором документа по умолчанию по типу самого документа, и с именем взятым из описания сущности
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddDocumentConfiguration<TEntityDlg>()
			where TEntityDlg : EntityDialogBase<TEntity>, ITdiTab
		{
			return AddDocumentConfiguration<TEntityDlg>(GetEntityTitleName());
		}

		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, идентификатором документа по умолчанию по типу самого документа, и с определенным именем
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="createActionTitle">Отображаемое имя документа в действиях с документов</param>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddDocumentConfiguration<TEntityDlg>(string createActionTitle)
			where TEntityDlg : EntityDialogBase<TEntity>, ITdiTab
		{
			Func<TNode, bool> docIdentificationFunc = (TNode node) => node.EntityType == typeof(TEntity);

			return AddDocumentConfiguration<TEntityDlg>(docIdentificationFunc, createActionTitle);
		}

		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, с определенным идентификатором
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddDocumentConfiguration<TEntityDlg>(Func<TNode, bool> docIdentificationFunc)
			where TEntityDlg : EntityDialogBase<TEntity>, ITdiTab
		{
			return AddDocumentConfiguration<TEntityDlg>(docIdentificationFunc, GetEntityTitleName());
		}

		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, с определенным идентификатором и именем
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="createActionTitle">Отображаемое имя документа в действиях с документов</param>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddDocumentConfiguration<TEntityDlg>(Func<TNode, bool> docIdentificationFunc, string createActionTitle)
			where TEntityDlg : EntityDialogBase<TEntity>, ITdiTab
		{
			Type dlgType = typeof(TEntityDlg);
			CheckDialogRestrictions(dlgType);

			var dlgCtorForCreateNewEntity = dlgType.GetConstructor(Type.EmptyTypes);
			var dlgCtorForOpenEntity = dlgType.GetConstructor(new[] { typeof(int) });

			Func<TEntityDlg> createDlgFunc = () => (TEntityDlg)dlgCtorForCreateNewEntity.Invoke(Type.EmptyTypes);
			Func<TNode, TEntityDlg> openDlgFunc = (TNode node) => (TEntityDlg)dlgCtorForOpenEntity.Invoke(new object[] { node.DocumentId });

			return AddDocumentConfiguration<TEntityDlg>(docIdentificationFunc, createActionTitle, createDlgFunc, openDlgFunc);
		}

		/// <summary>
		/// Добавление конфигурации документа с не стандартным опредлением диалогов, с определенным идентификатором и именем взятым из описания сущности
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="createDlgFunc">Функция вызова диалога создания нового документа</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddDocumentConfiguration<TEntityDlg>(Func<TNode, bool> docIdentificationFunc, Func<TEntityDlg> createDlgFunc, Func<TNode, TEntityDlg> openDlgFunc)
			where TEntityDlg : EntityDialogBase<TEntity>, ITdiTab
		{
			return AddDocumentConfiguration<TEntityDlg>(docIdentificationFunc, GetEntityTitleName(), createDlgFunc, openDlgFunc);
		}

		/// <summary>
		/// Добавление функций открытия диалогов для документа с определенным идентификатором и именем
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="createActionTitle">Отображаемое имя документа в действиях с документов</param>
		/// <param name="createDlgFunc">Функция вызова диалога создания нового документа</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddDocumentConfiguration<TEntityDlg>(Func<TNode, bool> docIdentificationFunc, string createActionTitle, Func<TEntityDlg> createDlgFunc, Func<TNode, TEntityDlg> openDlgFunc)
			where TEntityDlg : EntityDialogBase<TEntity>, ITdiTab
		{
			if(docConfigs.ContainsKey(typeof(TEntityDlg))) {
				throw new InvalidOperationException($"Кофигурация для сущности {nameof(TEntity)} уже содержит кофигурацию документа для диалога {nameof(TEntityDlg)}");
			}

			var dlgInfo = new MultipleEntityModelDocumentConfig<TNode>(typeof(TEntity), createActionTitle, createDlgFunc, openDlgFunc, docIdentificationFunc);
			docConfigs.Add(typeof(TEntityDlg), dlgInfo);
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
			finishConfigAction.Invoke(dataFunc, docConfigs.Values.AsEnumerable());
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
