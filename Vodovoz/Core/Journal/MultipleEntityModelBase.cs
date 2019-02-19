using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.Project.Repositories;
using QS.Tdi;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QS.RepresentationModel.GtkUI;
using QS.RepresentationModel;

namespace Vodovoz.Core.Journal
{
	public abstract class MultipleEntityModelBase<TNode> : RepresentationModelBase<TNode>, IMultipleEntityRepresentationModel, IMultipleEntityPermissionModel
		where TNode : MultipleEntityVMNodeBase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private List<Func<IList<TNode>>> dataFunctions;
		private List<Type> registeredTypes;
		private List<MultipleEntityModelDocumentConfig<TNode>> configList;

		private Dictionary<Type, EntityPermission> permissions;


		#region IMultipleEntityPermissionModel implementation

		public bool CanCreateNewEntity(Type entityType)
		{
			if(!permissions.ContainsKey(entityType)) {
				return false;
			}
			return permissions[entityType].Create;
		}

		public bool CanOpenEntity(object node)
		{
			if(node == null) {
				return false;
			}
			TNode document = CastToNode(node);
			if(!permissions.ContainsKey(document.EntityType)) {
				return false;
			}
			return permissions[document.EntityType].Update;
		}

		public bool CanDeleteEntity(object node)
		{
			if(node == null) {
				return false;
			}
			TNode document = CastToNode(node);
			if(!permissions.ContainsKey(document.EntityType)) {
				return false;
			}
			return permissions[document.EntityType].Delete;
		}

		public virtual string GetSummaryInfo()
		{
			return $"Количество: {ItemsList.Count}";
		}

		#endregion

		#region IMultipleEntityRepresentationModel implementation

		//В данной модели возможно отображение множества сущностей разных типов, поэтому свойство всегда возвращает null
		public Type EntityType => null;

		public IEnumerable<ActionForCreateEntityConfig> NewEntityActionsConfigs => configList.Select(x => x.CreateEntityActionConfig);

		private TNode CastToNode(object node)
		{
			TNode document = node as TNode;
			if(document == null) {
				throw new ArgumentException($"Объект не является типом \"{typeof(TNode)}\"");
			}
			return document;
		}

		public Type GetEntityType(object node)
		{
			TNode document = CastToNode(node);
			return document.EntityType;
		}

		public int GetDocumentId(object node)
		{
			TNode document = CastToNode(node);
			return document.DocumentId;
		}

		public ITdiTab GetOpenEntityDlg(object node)
		{
			TNode document = CastToNode(node);

			MultipleEntityModelDocumentConfig<TNode> config;

			try {
				config = configList.SingleOrDefault(x => x.IsIdentified(document));
			} catch(InvalidOperationException) {
				throw new InvalidOperationException("В списке конфигураций одному условию идентификации документа соответствует более одной конфигурации документа.");
			} catch(Exception ex) {
				throw ex;
			}

			if(config == null) {
				throw new InvalidOperationException("В списке конфигураций не найдено ни одной конфигурации для текущего документа");
			}

			return config.GetOpenEntityDlg(document);
		}

		public IJournalFilter JournalFilter { get; set; }

		#endregion

		public IColumnsConfig TreeViewConfig { get; set; }

		#region RepresentationModelBase implementation

		public override IColumnsConfig ColumnsConfig => TreeViewConfig;

		public override void UpdateNodes()
		{
			List<TNode> source = new List<TNode>();
			foreach(var item in dataFunctions) {
				source.AddRange(item.Invoke());
			}
			if(FinalListFunction != null) {
				source = FinalListFunction.Invoke(source);
			}

			SetItemsSource(source);
		}

		#endregion

		public Func<List<TNode>, List<TNode>> FinalListFunction;

		protected EntityPermission GetPermissionForEntity<TEntity>()
		{
			if(PermissionsSettings.EntityPermissionValidator == null) {
				return EntityPermission.AllAllowed;
			}
			var user = UserRepository.GetCurrentUser(UoW);
			return PermissionsSettings.EntityPermissionValidator.Validate<TEntity>(user.Id);
		}

		public MultipleEntityModelBase()
		{
			dataFunctions = new List<Func<IList<TNode>>>();
			registeredTypes = new List<Type>();
			configList = new List<MultipleEntityModelDocumentConfig<TNode>>();
			permissions = new Dictionary<Type, EntityPermission>();
		}

		protected MultipleEntityModelConfiguration<TEntityType, TNode> RegisterEntity<TEntityType>()
			where TEntityType : class, IDomainObject, new()
		{
			//проверка прав на регистрируемую сущность
			var permission  = GetPermissionForEntity<TEntityType>();
			if(permission.Read) {
				var type = typeof(TEntityType);
				if(!permissions.ContainsKey(type)) {
					permissions.Add(type, permission);
				}
				if(!registeredTypes.Contains(type)) {
					registeredTypes.Add(type);
					SubscribeType(type);
				}
			}

			//FIXME Сомнительный вариант, возможно можно сделать лучше
			Action<Func<IList<TNode>>, List<MultipleEntityModelDocumentConfig<TNode>>> finishConfigAction = (Func<IList<TNode>> dataFunc, List<MultipleEntityModelDocumentConfig<TNode>> docConfigs) => {
				if(permission.Read) {
					configList.AddRange(docConfigs);
					dataFunctions.Add(dataFunc);
				}
			};

			return new MultipleEntityModelConfiguration<TEntityType, TNode>(finishConfigAction);
		}

		/// <summary>
		/// Запрос у модели о необходимости обновления списка если объект изменился.
		/// </summary>
		/// <returns><c>true</c>, если небходимо обновлять список.</returns>
		/// <param name="updatedSubject">Обновившийся объект</param>
		protected bool NeedUpdateFunc(object updatedSubject)
		{
			return registeredTypes.Contains(updatedSubject.GetType());
		}

		/// <summary>
		/// Создает новый базовый клас и подписывается на обновления указанных типов, при этом конструкторе необходима реализация NeedUpdateFunc (object updatedSubject);
		/// </summary>
		private void SubscribeType(Type type)
		{
			var map = OrmMain.GetObjectDescription(type);
			if(map != null) {
				map.ObjectUpdated += OnExternalUpdateCommon;
			} else {
				logger.Warn("Невозможно подписаться на обновления класа {0}. Не найден класс маппинга.", type);
			}		
		}

		private void OnExternalUpdateCommon(object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedEventArgs e)
		{
			if(!UoW.IsAlive) {
				logger.Warn("Получена нотификация о внешнем обновлении данные в {0}, в тот момент когда сессия уже закрыта. Возможно RepresentationModel, осталась в памяти при закрытой сессии.",
					this);
				return;
			}

			if(e.UpdatedSubjects.Any(NeedUpdateFunc)) { 
				UpdateNodes(); 
			}
		}

		public void Destroy()
		{
			logger.Debug("{0} called Destroy()", this.GetType());

			foreach(var type in registeredTypes) {
				var map = OrmMain.GetObjectDescription(type);
				if(map != null)
					map.ObjectUpdated -= OnExternalUpdateCommon;
			}
		}
	}

	public interface IMultipleEntityPermissionModel
	{
		bool CanCreateNewEntity(Type entityType);
		bool CanOpenEntity(object node);
		bool CanDeleteEntity(object node);
	}
}
