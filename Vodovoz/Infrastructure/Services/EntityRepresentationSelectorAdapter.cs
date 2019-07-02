using System;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.RepresentationModel.GtkUI;
using QS.Tdi;
using QS.Project.Dialogs.GtkUI;
using System.Collections.Generic;
using QS.Project.Dialogs;

namespace Vodovoz.Infrastructure.Services
{
	public class EntityRepresentationSelectorAdapter : IEntitySelector
	{
		private readonly Type entityType;
		private readonly IRepresentationModel model;
		private readonly bool multipleSelect;

		public ITdiTab JournalTab { get; private set; }

		public EntityRepresentationSelectorAdapter(string tabName, IRepresentationModel model, bool multipleSelect = false) : base()
		{
			this.model = model ?? throw new ArgumentNullException(nameof(model));
			this.multipleSelect = multipleSelect;
			TabName = tabName;
			if(model.EntityType == null) {
				throw new ArgumentException("Модель должна иметь информацию о загружаемой сущности");
			}
			this.entityType = model.EntityType;
			Configure();
		}

		public EntityRepresentationSelectorAdapter(string tabName, Type entityType,  IRepresentationModel model, bool multipleSelect = false)
		{
			this.entityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
			this.model = model ?? throw new ArgumentNullException(nameof(model));
			this.multipleSelect = multipleSelect;
			TabName = tabName;
			Configure();
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
			JournalTab.CloseTab += (sender, e) => {
				CloseTab?.Invoke(this, e);
				journal.Destroy();
				Dispose();
			};
			journal.Destroyed += (sender, e) => {
				CloseTab?.Invoke(this, new TdiTabCloseEventArgs(false));
				journal.Destroy();
				Dispose();
			};
		}

		public event EventHandler<JournalSelectedNodesEventArgs> OnEntitySelectedResult;

		#region ITdiTab implementation

		public HandleSwitchIn HandleSwitchIn => JournalTab.HandleSwitchIn;

		public HandleSwitchOut HandleSwitchOut => JournalTab.HandleSwitchOut;

		public string TabName { get; private set; }

		public ITdiTabParent TabParent { get => JournalTab.TabParent; set => JournalTab.TabParent = value; }

		public bool FailInitialize => JournalTab.FailInitialize;

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public bool CompareHashName(string hashName)
		{
			return JournalTab.CompareHashName(hashName);
		}

		#endregion ITdiTab implementation

		public void Dispose()
		{
			model.UoW.Dispose();
		}

	}

	public class EntityNode : JournalEntityNodeBase
	{
		public EntityNode(int id, Type type) : base(type)
		{
			Id = id;
		}
	}
}
