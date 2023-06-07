using System;
using System.Collections;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.RepresentationModel.GtkUI;
using QS.Tdi;
using QS.Utilities.Text;

namespace Vodovoz.Infrastructure.Services
{
	public class EntityRepresentationSelectorAdapter : IEntityAutocompleteSelector
	{
		private readonly Type entityType;
		private readonly IRepresentationModel model;
		private readonly bool multipleSelect;

		public ITdiTab JournalTab { get; private set; }

		public EntityRepresentationSelectorAdapter(IRepresentationModel model, string tabName = null, bool multipleSelect = false) : base()
		{
			this.model = model ?? throw new ArgumentNullException(nameof(model));
			this.multipleSelect = multipleSelect;
			if(model.EntityType == null) {
				throw new ArgumentException("Модель должна иметь информацию о загружаемой сущности");
			}
			TabName = tabName;
			this.entityType = model.EntityType;
			SetTabName(tabName);
			Configure();
		}

		public EntityRepresentationSelectorAdapter(Type entityType,  IRepresentationModel model, string tabName = null, bool multipleSelect = false)
		{
			this.entityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
			this.model = model ?? throw new ArgumentNullException(nameof(model));
			this.multipleSelect = multipleSelect;
			SetTabName(tabName);
			Configure();
		}

		private void SetTabName(string tabName)
		{
			if(!string.IsNullOrWhiteSpace(tabName)) {
				TabName = tabName;
				return;
			}
			var subjectNames = entityType.GetSubjectNames();
			if(subjectNames == null) {
				throw new ApplicationException("Невозможно разрешить имя вкладки, не указано имя журнала и не указано имя сущности");
			}
			TabName = subjectNames.NominativePlural.StringToTitleCase();
		}

		private void Configure()
		{
			var journal = new PermissionControlledRepresentationJournal(model);
			journal.Mode = multipleSelect ? JournalSelectMode.Multiple : JournalSelectMode.Single;
			journal.ObjectSelected += (sender, e) => {
				List<EntityNode> selectedResult = new List<EntityNode>();
				foreach(int selectedId in e.GetSelectedIds()) {
					selectedResult.Add(new EntityNode(selectedId, entityType));
				}
				OnEntitySelectedResult?.Invoke(sender, new JournalSelectedNodesEventArgs(selectedResult.ToArray()));
			};
			JournalTab = journal;
			JournalTab.TabNameChanged += (sender, e) => TabNameChanged?.Invoke(sender, e);
			JournalTab.TabClosed += (sender, e) => {
				TabParent?.ForceCloseTab(this);
				journal.Destroy();
				Dispose();
			};
			journal.Destroyed += (sender, e) => {
				TabParent?.ForceCloseTab(this);
				journal.Destroy();
				Dispose();
			};
		}

		public event EventHandler<JournalSelectedNodesEventArgs> OnEntitySelectedResult;
		public bool CanOpen(Type subjectType) => false;

		public ITdiTab GetTabToOpen(Type subjectType, int subjectId) => throw new NotSupportedException();

		#region ITdiTab implementation

		public HandleSwitchIn HandleSwitchIn => JournalTab.HandleSwitchIn;

		public HandleSwitchOut HandleSwitchOut => JournalTab.HandleSwitchOut;

		public string TabName { get; private set; }

		public ITdiTabParent TabParent { get => JournalTab.TabParent; set => JournalTab.TabParent = value; }

		public bool FailInitialize => JournalTab.FailInitialize;


		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler TabClosed;
		public event EventHandler ListUpdated;

		public bool CompareHashName(string hashName)
		{
			return JournalTab.CompareHashName(hashName);
		}

		#endregion ITdiTab implementation

		public void Dispose()
		{
			model.UoW.Dispose();
		}

		#region IEntityAutocompleteSelector implementation

		public IList Items => model.ItemsList;

		public void SearchValues(params string[] values)
		{
			model.SearchStrings = values;
		}

		public void OnTabClosed()
		{
			TabClosed?.Invoke(this, EventArgs.Empty);
		}

		#endregion IEntityAutocompleteSelector implementation
	}

	public class EntityNode : JournalEntityNodeBase
	{
		public EntityNode(int id, Type type) : base(type)
		{
			Id = id;
		}

		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
	}
}
