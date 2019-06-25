using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.Tdi;
using QS.Utilities.Text;
using System.ComponentModel;
using Vodovoz.ViewModelBased;

namespace Vodovoz.Core.Journal
{
	/// <summary>
	/// Определяет методы для конфигурирования модели под определенную сущность, и определения конфигурации различных документов для этой сущности
	/// </summary>
	public sealed class MultipleEntityModelConfiguration<TEntity, TNode>
		where TEntity : class, INotifyPropertyChanged, IDomainObject, new()
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
		/// Добавление функций открытия диалогов для документа с определенным идентификатором без возможности создания документа
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddDocumentConfigurationWithoutCreation<TEntityDlg>(Func<TNode, bool> docIdentificationFunc, Func<TNode, TEntityDlg> openDlgFunc)
			where TEntityDlg : EntityDialogBase<TEntity>, ITdiTab
		{
			if(docConfigs.ContainsKey(typeof(TEntityDlg))) {
				throw new InvalidOperationException($"Кофигурация для сущности {nameof(TEntity)} уже содержит кофигурацию документа для диалога {nameof(TEntityDlg)}");
			}

			var dlgInfo = new MultipleEntityModelDocumentConfig<TNode>(typeof(TEntity), openDlgFunc, docIdentificationFunc);
			docConfigs.Add(typeof(TEntityDlg), dlgInfo);
			return this;
		}

		#region ViewModelBasedOld

		[Obsolete("Из-за упразднения IViewModelBaseView, является устаревшей. Необходимо вызывать метод AddViewModelDocumentConfiguration")]
		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, идентификатором документа по умолчанию по типу самого документа, и с именем взятым из описания сущности
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelBasedDocumentConfiguration<TEntityDlg>()
			where TEntityDlg : class, IViewModelBaseView<TEntity>, ITdiTab
		{
			return AddViewModelBasedDocumentConfiguration<TEntityDlg>(GetEntityTitleName());
		}

		[Obsolete("Из-за упразднения IViewModelBaseView, является устаревшей. Необходимо вызывать метод AddViewModelDocumentConfiguration")]
		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, идентификатором документа по умолчанию по типу самого документа, и с определенным именем
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="createActionTitle">Отображаемое имя документа в действиях с документов</param>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelBasedDocumentConfiguration<TEntityDlg>(string createActionTitle)
			where TEntityDlg : class, IViewModelBaseView<TEntity>, ITdiTab
		{
			Func<TNode, bool> docIdentificationFunc = (TNode node) => node.EntityType == typeof(TEntity);

			return AddViewModelBasedDocumentConfiguration<TEntityDlg>(docIdentificationFunc, createActionTitle);
		}

		[Obsolete("Из-за упразднения IViewModelBaseView, является устаревшей. Необходимо вызывать метод AddViewModelDocumentConfiguration")]
		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, с определенным идентификатором
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelBasedDocumentConfiguration<TEntityDlg>(Func<TNode, bool> docIdentificationFunc)
			where TEntityDlg : class, IViewModelBaseView<TEntity>, ITdiTab
		{
			return AddViewModelBasedDocumentConfiguration<TEntityDlg>(docIdentificationFunc, GetEntityTitleName());
		}

		[Obsolete("Из-за упразднения IViewModelBaseView, является устаревшей. Необходимо вызывать метод AddViewModelDocumentConfiguration")]
		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, с определенным идентификатором и именем
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="createActionTitle">Отображаемое имя документа в действиях с документов</param>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelBasedDocumentConfiguration<TEntityDlg>(Func<TNode, bool> docIdentificationFunc, string createActionTitle)
			where TEntityDlg : class, IViewModelBaseView<TEntity>, ITdiTab
		{
			Type dlgType = typeof(TEntityDlg);
			CheckDialogRestrictions(dlgType);

			var dlgCtorForCreateNewEntity = dlgType.GetConstructor(Type.EmptyTypes);
			var dlgCtorForOpenEntity = dlgType.GetConstructor(new[] { typeof(int) });

			Func<TEntityDlg> createDlgFunc = () => (TEntityDlg)dlgCtorForCreateNewEntity.Invoke(Type.EmptyTypes);
			Func<TNode, TEntityDlg> openDlgFunc = (TNode node) => (TEntityDlg)dlgCtorForOpenEntity.Invoke(new object[] { node.DocumentId });

			return AddViewModelBasedDocumentConfiguration<TEntityDlg>(docIdentificationFunc, createActionTitle, createDlgFunc, openDlgFunc);
		}

		[Obsolete("Из-за упразднения IViewModelBaseView, является устаревшей. Необходимо вызывать метод AddViewModelDocumentConfiguration")]
		/// <summary>
		/// Добавление конфигурации документа с не стандартным опредлением диалогов, с определенным идентификатором и именем взятым из описания сущности
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="createDlgFunc">Функция вызова диалога создания нового документа</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelBasedDocumentConfiguration<TEntityDlg>(Func<TNode, bool> docIdentificationFunc, Func<TEntityDlg> createDlgFunc, Func<TNode, TEntityDlg> openDlgFunc)
			where TEntityDlg : class, IViewModelBaseView<TEntity>, ITdiTab
		{
			return AddViewModelBasedDocumentConfiguration<TEntityDlg>(docIdentificationFunc, GetEntityTitleName(), createDlgFunc, openDlgFunc);
		}

		[Obsolete("Из-за упразднения IViewModelBaseView, является устаревшей. Необходимо вызывать метод AddViewModelDocumentConfiguration")]
		/// <summary>
		/// Добавление функций открытия диалогов для документа с определенным идентификатором и именем
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="createActionTitle">Отображаемое имя документа в действиях с документов</param>
		/// <param name="createDlgFunc">Функция вызова диалога создания нового документа</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelBasedDocumentConfiguration<TEntityDlg>(Func<TNode, bool> docIdentificationFunc, string createActionTitle, Func<TEntityDlg> createDlgFunc, Func<TNode, TEntityDlg> openDlgFunc)
			where TEntityDlg : class, IViewModelBaseView<TEntity>, ITdiTab
		{
			if(docConfigs.ContainsKey(typeof(TEntityDlg))) {
				throw new InvalidOperationException($"Кофигурация для сущности {nameof(TEntity)} уже содержит кофигурацию документа для диалога {nameof(TEntityDlg)}");
			}

			var dlgInfo = new MultipleEntityModelDocumentConfig<TNode>(typeof(TEntity), createActionTitle, createDlgFunc, openDlgFunc, docIdentificationFunc);
			docConfigs.Add(typeof(TEntityDlg), dlgInfo);
			return this;
		}

		[Obsolete("Из-за упразднения IViewModelBaseView, является устаревшей. Необходимо вызывать метод AddViewModelDocumentConfiguration")]
		/// <summary>
		/// Добавление функций открытия диалогов для документа с определенным идентификатором без возможности создания документа
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TEntityDlg">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelBasedDocumentConfigurationWithoutCreation<TEntityDlg>(Func<TNode, bool> docIdentificationFunc, Func<TNode, TEntityDlg> openDlgFunc)
			where TEntityDlg : class, IViewModelBaseView<TEntity>, ITdiTab
		{
			if(docConfigs.ContainsKey(typeof(TEntityDlg))) {
				throw new InvalidOperationException($"Кофигурация для сущности {nameof(TEntity)} уже содержит кофигурацию документа для диалога {nameof(TEntityDlg)}");
			}

			var dlgInfo = new MultipleEntityModelDocumentConfig<TNode>(typeof(TEntity), openDlgFunc, docIdentificationFunc);
			docConfigs.Add(typeof(TEntityDlg), dlgInfo);
			return this;
		}

		#endregion

		#region ViewModelBased

		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, идентификатором документа по умолчанию по типу самого документа, и с именем взятым из описания сущности
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <typeparam name="TViewModel">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelDocumentConfiguration<TViewModel>()
			where TViewModel : QS.ViewModels.TabViewModelBase
		{
			return AddViewModelDocumentConfiguration<TViewModel>(GetEntityTitleName());
		}

		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, идентификатором документа по умолчанию по типу самого документа, и с определенным именем
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="createActionTitle">Отображаемое имя документа в действиях с документов</param>
		/// <typeparam name="TViewModel">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelDocumentConfiguration<TViewModel>(string createActionTitle)
			where TViewModel : QS.ViewModels.TabViewModelBase
		{
			Func<TNode, bool> docIdentificationFunc = (TNode node) => node.EntityType == typeof(TEntity);

			return AddViewModelDocumentConfiguration<TViewModel>(docIdentificationFunc, createActionTitle);
		}

		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, с определенным идентификатором
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <typeparam name="TViewModel">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelDocumentConfiguration<TViewModel>(Func<TNode, bool> docIdentificationFunc)
			where TViewModel : QS.ViewModels.TabViewModelBase
		{
			return AddViewModelDocumentConfiguration<TViewModel>(docIdentificationFunc, GetEntityTitleName());
		}

		/// <summary>
		/// Добавление конфигурации документа с диалогами по умолчанию, с определенным идентификатором и именем
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="createActionTitle">Отображаемое имя документа в действиях с документов</param>
		/// <typeparam name="TViewModel">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelDocumentConfiguration<TViewModel>(Func<TNode, bool> docIdentificationFunc, string createActionTitle)
			where TViewModel : QS.ViewModels.TabViewModelBase
		{
			Type dlgType = typeof(TViewModel);
			CheckDialogRestrictions(dlgType);

			var dlgCtorForCreateNewEntity = dlgType.GetConstructor(Type.EmptyTypes);
			var dlgCtorForOpenEntity = dlgType.GetConstructor(new[] { typeof(int) });

			Func<TViewModel> createDlgFunc = () => (TViewModel)dlgCtorForCreateNewEntity.Invoke(Type.EmptyTypes);
			Func<TNode, TViewModel> openDlgFunc = (TNode node) => (TViewModel)dlgCtorForOpenEntity.Invoke(new object[] { node.DocumentId });

			return AddViewModelDocumentConfiguration<TViewModel>(docIdentificationFunc, createActionTitle, createDlgFunc, openDlgFunc);
		}

		/// <summary>
		/// Добавление конфигурации документа с не стандартным опредлением диалогов, с определенным идентификатором и именем взятым из описания сущности
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="createDlgFunc">Функция вызова диалога создания нового документа</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TViewModel">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelDocumentConfiguration<TViewModel>(Func<TNode, bool> docIdentificationFunc, Func<TViewModel> createDlgFunc, Func<TNode, TViewModel> openDlgFunc)
			where TViewModel : QS.ViewModels.TabViewModelBase
		{
			return AddViewModelDocumentConfiguration<TViewModel>(docIdentificationFunc, GetEntityTitleName(), createDlgFunc, openDlgFunc);
		}

		/// <summary>
		/// Добавление функций открытия диалогов для документа с определенным идентификатором и именем
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="createActionTitle">Отображаемое имя документа в действиях с документов</param>
		/// <param name="createDlgFunc">Функция вызова диалога создания нового документа</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TViewModel">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelDocumentConfiguration<TViewModel>(Func<TNode, bool> docIdentificationFunc, string createActionTitle, Func<TViewModel> createDlgFunc, Func<TNode, TViewModel> openDlgFunc)
			where TViewModel : QS.ViewModels.TabViewModelBase
		{
			if(docConfigs.ContainsKey(typeof(TViewModel))) {
				throw new InvalidOperationException($"Кофигурация для сущности {nameof(TEntity)} уже содержит кофигурацию документа для диалога {nameof(TViewModel)}");
			}

			var dlgInfo = new MultipleEntityModelDocumentConfig<TNode>(typeof(TEntity), createActionTitle, createDlgFunc, openDlgFunc, docIdentificationFunc);
			docConfigs.Add(typeof(TViewModel), dlgInfo);
			return this;
		}

		/// <summary>
		/// Добавление функций открытия диалогов для документа с определенным идентификатором без возможности создания документа
		/// </summary>
		/// <returns>Конфигурация документа</returns>
		/// <param name="docIdentificationFunc">Уникальный идентификатор типа документа, должен возвращать true только для тех строк для которых должен открываться выбранный тип диалога и больше никакой другой</param>
		/// <param name="openDlgFunc">Функция вызова диалога открытия нового документа</param>
		/// <typeparam name="TViewModel">Тип диалога для конфигурируемого документа</typeparam>
		public MultipleEntityModelConfiguration<TEntity, TNode> AddViewModelDocumentConfigurationWithoutCreation<TViewModel>(Func<TNode, bool> docIdentificationFunc, Func<TNode, TViewModel> openDlgFunc)
			where TViewModel : QS.ViewModels.TabViewModelBase
		{
			if(docConfigs.ContainsKey(typeof(TViewModel))) {
				throw new InvalidOperationException($"Кофигурация для сущности {nameof(TEntity)} уже содержит кофигурацию документа для диалога {nameof(TViewModel)}");
			}

			var dlgInfo = new MultipleEntityModelDocumentConfig<TNode>(typeof(TEntity), openDlgFunc, docIdentificationFunc);
			docConfigs.Add(typeof(TViewModel), dlgInfo);
			return this;
		}

		#endregion

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
