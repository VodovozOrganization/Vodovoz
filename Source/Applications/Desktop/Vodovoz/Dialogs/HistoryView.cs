using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Exceptions;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.HistoryLog.Domain;
using QS.Project.DB;
using QS.Project.Services;
using QS.Utilities;
using QSOrmProject;
using QSProjectsLib;
using QSWidgetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using QS.Utilities.Extensions;
using Vodovoz.Commons;
using Vodovoz.Domain.HistoryChanges;
using Vodovoz.Journal;
using Vodovoz.JournalNodes;
using Vodovoz.Settings.Common;
using Vodovoz.ViewModels.Journals.JournalViewModels.HistoryTrace;
using Vodovoz.ViewModels.TempAdapters;
using VodovozInfrastructure.Attributes;
using Microsoft.Extensions.Logging;

namespace Vodovoz.Dialogs
{
	[System.ComponentModel.DisplayName("Просмотр журнала изменений")]
	[WidgetWindow(DefaultWidth = 852, DefaultHeight = 600)]
	public partial class HistoryView : QS.Dialog.Gtk.TdiTabBase, ISingleUoWDialog
	{
		private static ILogger<HistoryView> _logger;
		List<ChangedEntity> _changedEntities;
		List<ArchivedChangedEntity> _oldChangedEntities;
		bool _canUpdate = false;
		bool _canSearchFromArchive = false; 
		private int _pageSize = 250;
		private int _takenRows = 0;
		private int _takenOldRows = 0;
		private bool _takenAll = false;
		private bool _takenAllOld = false;
		private bool _needToHideProperties = true;
		private bool _isSearchFromOldMonitoring;
		private IDiffFormatter _diffFormatter = new ThemmedDiffFormater();
		private HistoryTracePropertyJournalViewModel _historyTracePropertyJournalViewModel;

		public IUnitOfWork UoW { get; private set; }

		public HistoryView(ILogger<HistoryView> logger, IUserJournalFactory userJournalFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			this.Build();

			_needToHideProperties = !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_see_history_view_restricted_properties");

			_historyTracePropertyJournalViewModel = new HistoryTracePropertyJournalViewModel(ServicesConfig.UnitOfWorkFactory, ServicesConfig.CommonServices.InteractiveService);

			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();

			comboAction.ItemsEnum = typeof(EntityChangeOperation);

			entryUser.SetEntityAutocompleteSelectorFactory(userJournalFactory.CreateSelectUserAutocompleteSelectorFactory());
			entryUser.ChangedByUser += (sender, e) => UpdateJournal();

			entryObject3.SetNodeAutocompleteSelectorFactory(
				new NodeAutocompleteSelectorFactory<HistoryTraceObjectJournalViewModel>(typeof(HistoryTraceObjectNode),
				() => new HistoryTraceObjectJournalViewModel(ServicesConfig.UnitOfWorkFactory, ServicesConfig.CommonServices.InteractiveService)));

			entryObject3.ChangedByUser += OnObjectChangedByUser;

			entryProperty.SetNodeAutocompleteSelectorFactory(
				new NodeAutocompleteSelectorFactory<HistoryTracePropertyJournalViewModel>(typeof(HistoryTracePropertyNode),
				() => _historyTracePropertyJournalViewModel));

			entryProperty.ChangedByUser += (sender, e) => UpdateJournal();

			ConfigureDataTrees();

			var archiveSettings = ScopeProvider.Scope.Resolve<IArchiveDataSettings>();

			if(archiveSettings.GetDatabaseNameForOldMonitoringAvailable == UoW.Session.Connection.Database)
			{
				selectperiod.AddCustomPeriodInDays(archiveSettings.GetMonitoringPeriodAvailableInDays, "Архив (медленно)");
				selectperiod.Show3Month = false;
				selectperiod.ShowCustomPeriod = true;
				_canSearchFromArchive = true;
				
				selectperiod.EarlyCustomDateToggled += OnEarlyCustomDateToggled;
			}

			_canUpdate = true;
			vpanedOld.Visible = false;
			vpaned1.PositionSet = false;
			vpanedOld.PositionSet = false;
			selectperiod.ActiveRadio = SelectPeriod.Period.Today;
		}

		private void OnEarlyCustomDateToggled(bool value)
		{
			_isSearchFromOldMonitoring = value;

			if(value)
			{
				vpaned1.Visible = false;
				vpanedOld.Visible = true;
			}
			else
			{
				vpaned1.Visible = true;
				vpanedOld.Visible = false;
			}
		}

		private void ConfigureDataTrees()
		{
			datatreeChangesets.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ChangedEntity>()
				.AddColumn("Время").AddTextRenderer(x => x.ChangeTimeText)
				.AddColumn("Пользователь").AddTextRenderer(x => x.ChangeSet.UserName)
				.AddColumn("Действие").AddTextRenderer(x => x.OperationText)
				.AddColumn("Тип объекта").AddTextRenderer(x => x.ObjectTitle)
				.AddColumn("Код объекта").AddTextRenderer(x => x.EntityId.ToString())
				.AddColumn("Имя объекта").AddTextRenderer(x => x.EntityTitle)
				.AddColumn("Откуда изменялось").AddTextRenderer(x => x.ChangeSet.ActionName)
				.Finish();
			datatreeChangesets.Selection.Changed += OnChangeSetSelectionChanged;
			GtkScrolledWindowChangesets.Vadjustment.ValueChanged += OnVadjustmentValueChanged;

			datatreeChanges.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<FieldChange>()
				.AddColumn("Поле").AddTextRenderer(x => x.FieldTitle)
				.AddColumn("Операция").AddTextRenderer(x => x.TypeText)
				.AddColumn("Новое значение").AddTextRenderer(x => x.NewFormatedDiffText, useMarkup: true)
				.AddColumn("Старое значение").AddTextRenderer(x => x.OldFormatedDiffText, useMarkup: true)
				.Finish();

			dataTreeOldChangeSets.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ArchivedChangedEntity>()
				.AddColumn("Время").AddTextRenderer(x => x.ChangeTimeText)
				.AddColumn("Пользователь").AddTextRenderer(x => x.ChangeSet.UserName)
				.AddColumn("Действие").AddTextRenderer(x => x.OperationText)
				.AddColumn("Тип объекта").AddTextRenderer(x => x.ObjectTitle)
				.AddColumn("Код объекта").AddTextRenderer(x => x.EntityId.ToString())
				.AddColumn("Имя объекта").AddTextRenderer(x => x.EntityTitle)
				.AddColumn("Откуда изменялось").AddTextRenderer(x => x.ChangeSet.ActionName)
				.Finish();
			dataTreeOldChangeSets.Selection.Changed += OnOldChangeSetSelectionChanged;
			GtkScrolledWindowOldChangesets.Vadjustment.ValueChanged += OnOldVadjustmentValueChanged;

			dataTreeOldChanges.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ArchivedFieldChange>()
				.AddColumn("Поле").AddTextRenderer(x => x.FieldTitle)
				.AddColumn("Операция").AddTextRenderer(x => x.TypeText)
				.AddColumn("Новое значение").AddTextRenderer(x => x.NewFormatedDiffText, useMarkup: true)
				.AddColumn("Старое значение").AddTextRenderer(x => x.OldFormatedDiffText, useMarkup: true)
				.Finish();
		}

		void OnVadjustmentValueChanged(object sender, EventArgs e)
		{
			if(_takenAll || datatreeChangesets.Vadjustment.Value + datatreeChangesets.Vadjustment.PageSize < datatreeChangesets.Vadjustment.Upper)
			{
				return;
			}

			var lastPos = datatreeChangesets.Vadjustment.Value;
			UpdateJournal(true);
			QSMain.WaitRedraw();
			datatreeChangesets.Vadjustment.Value = lastPos;
		}

		private void OnOldVadjustmentValueChanged(object sender, EventArgs e)
		{
			if(_takenAllOld
				|| dataTreeOldChangeSets.Vadjustment.Value + dataTreeOldChangeSets.Vadjustment.PageSize < dataTreeOldChangeSets.Vadjustment.Upper)
			{
				return;
			}

			var lastPos = dataTreeOldChangeSets.Vadjustment.Value;
			UpdateJournalOld(true);
			QSMain.WaitRedraw();
			dataTreeOldChangeSets.Vadjustment.Value = lastPos;
		}

		private void OnChangeSetSelectionChanged(object sender, EventArgs e)
		{
			var selected = (ChangedEntity)datatreeChangesets.GetSelectedObject();

			if(selected != null)
			{
				var changes = _needToHideProperties
					? selected.Changes.Where(FieldChangeNotNeedToBeHided).ToList()
					: selected.Changes.ToList();
				changes.ForEach(x => x.DiffFormatter = _diffFormatter);
				datatreeChanges.ItemsDataSource = changes;
			}
			else
			{
				datatreeChanges.ItemsDataSource = null;
			}
		}

		private void OnOldChangeSetSelectionChanged(object sender, EventArgs e)
		{
			var selected = (ArchivedChangedEntity)dataTreeOldChangeSets.GetSelectedObject();

			if(selected != null)
			{
				var changes = _needToHideProperties
					? selected.Changes.Where(OldFieldChangeNotNeedToBeHided).ToList()
					: selected.Changes.ToList();
				changes.ForEach(x => x.DiffFormatter = _diffFormatter);
				dataTreeOldChanges.ItemsDataSource = changes;
			}
			else
			{
				dataTreeOldChanges.ItemsDataSource = null;
			}
		}

		private bool FieldChangeNotNeedToBeHided(FieldChange fieldChange)
		{
			var restrictedToShowPropertyAttribute = new RestrictedHistoryProperty();

			var persistentClassType = OrmConfig.NhConfig.ClassMappings
				.Where(mc => mc.MappedClass.Name == fieldChange.Entity.EntityClassName)
				.Select(mc => mc.MappedClass).FirstOrDefault();

			return !persistentClassType?.GetPropertyInfo(fieldChange.Path)?.GetCustomAttributes(false)
				.Contains(restrictedToShowPropertyAttribute) ?? false;
		}

		private bool OldFieldChangeNotNeedToBeHided(ArchivedFieldChange oldFieldChange)
		{
			var restrictedToShowPropertyAttribute = new RestrictedHistoryProperty();

			var persistentClassType = OrmConfig.NhConfig.ClassMappings
				.Where(mc => mc.MappedClass.Name == oldFieldChange.Entity.EntityClassName)
				.Select(mc => mc.MappedClass).FirstOrDefault();

			return !persistentClassType?.GetPropertyInfo(oldFieldChange.Path)?.GetCustomAttributes(false)
				.Contains(restrictedToShowPropertyAttribute) ?? false;
		}

		private void UpdateJournal(bool nextPage = false)
		{
			DateTime startTime = DateTime.Now;
			if(!nextPage)
			{
				_takenRows = 0;
				_takenAll = false;
			}

			if(!_canUpdate)
			{
				return;
			}

			_logger.LogInformation("Получаем журнал изменений{0}...", _takenRows > 0 ? $"({_takenRows}+)" : "");
			ChangeSet changeSetAlias = null;

			var query = UoW.Session.QueryOver<ChangedEntity>()
				.JoinAlias(ce => ce.ChangeSet, () => changeSetAlias)
				.Fetch(SelectMode.Fetch, x => x.ChangeSet)
				.Fetch(SelectMode.Fetch, x => x.ChangeSet.User);

			if(!selectperiod.IsAllTime)
			{
				query.Where(ce => ce.ChangeTime >= selectperiod.DateBegin && ce.ChangeTime < selectperiod.DateEnd);
			}

			if(entryUser.Subject != null)
			{
				query.Where(() => changeSetAlias.User.Id == entryUser.SubjectId);
			}

			if(entryObject3.Subject is HistoryTraceObjectNode selectedClassType)
			{
				query.Where(ce => ce.EntityClassName == selectedClassType.ObjectName);
			}

			if(comboAction.SelectedItem is EntityChangeOperation)
			{
				query.Where(ce => ce.Operation == (EntityChangeOperation)comboAction.SelectedItem);
			}

			if(!string.IsNullOrWhiteSpace(entrySearchEntity.Text))
			{
				var pattern = $"%{entrySearchEntity.Text}%";
				query.Where(ce => ce.EntityTitle.IsLike(pattern));
			}

			if(!string.IsNullOrWhiteSpace(entSearchId.Text))
			{
				if(int.TryParse(entSearchId.Text, out int id))
				{
					query.Where(ce => ce.EntityId == id);
				}
			}

			if(!string.IsNullOrWhiteSpace(entrySearchValue.Text) || entryProperty.Subject is HistoryTracePropertyNode)
			{
				FieldChange fieldChangeAlias = null;
				query.JoinAlias(ce => ce.Changes, () => fieldChangeAlias);

				if(entryProperty.Subject is HistoryTracePropertyNode selectedProperty)
				{
					query.Where(() => fieldChangeAlias.Path == selectedProperty.PropertyPath);
				}

				if(!string.IsNullOrWhiteSpace(entrySearchValue.Text))
				{
					var pattern = $"%{entrySearchValue.Text}%";
					query.Where(() => fieldChangeAlias.OldValue.IsLike(pattern) || fieldChangeAlias.NewValue.IsLike(pattern));
				}
			}

			try
			{
				var taked = query.OrderBy(x => x.ChangeTime).Desc
							 .Skip(_takenRows)
							 .Take(_pageSize)
							 .List();

				if(_takenRows > 0)
				{
					_changedEntities.AddRange(taked);
					datatreeChangesets.YTreeModel.EmitModelChanged();
				}
				else
				{
					_changedEntities = taked.ToList();
					datatreeChangesets.ItemsDataSource = _changedEntities;
				}

				if(taked.Count < _pageSize)
				{
					_takenAll = true;
				}
			}
			catch (GenericADOException ex)
			{
				if(ex?.InnerException?.InnerException?.InnerException?.InnerException?.InnerException is System.Net.Sockets.SocketException exception 
					&& exception.SocketErrorCode == System.Net.Sockets.SocketError.TimedOut){
					MessageDialogHelper.RunWarningDialog("Превышен интервал ожидания ответа от сервера:\n" +
						"Попробуйте выбрать меньший интервал времени\n" +
						"или уточнить условия поиска");
				}
			}

			_takenRows = _changedEntities.Count;

			_logger.LogDebug("Время запроса {RequestTime}", DateTime.Now - startTime);
			_logger.LogInformation(NumberToTextRus.FormatCase(_changedEntities.Count, "Загружено изменение {0}{1} объекта.", "Загружено изменение {0}{1} объектов.", "Загружено изменение {0}{1} объектов.", _takenAll ? "" : "+"));
		}

		private void UpdateJournalOld(bool nextPage = false)
		{
			DateTime startTime = DateTime.Now;
			if(!nextPage)
			{
				_takenOldRows = 0;
				_takenAllOld = false;
			}

			if(!_canUpdate || !_canSearchFromArchive)
			{
				return;
			}

			_logger.LogInformation("Получаем журнал изменений{TakenOldRows}...", _takenOldRows > 0 ? $"({_takenOldRows}+)" : "");
			ArchivedChangeSet changeSetAlias = null;

			var query = UoW.Session.QueryOver<ArchivedChangedEntity>()
				.JoinAlias(ce => ce.ChangeSet, () => changeSetAlias)
				.Fetch(SelectMode.Fetch, x => x.ChangeSet)
				.Fetch(SelectMode.Fetch, x => x.ChangeSet.User);

			if(!selectperiod.IsAllTime)
			{
				query.Where(ce => ce.ChangeTime >= selectperiod.DateBegin && ce.ChangeTime < selectperiod.DateEnd);
			}

			if(entryUser.Subject != null)
			{
				query.Where(() => changeSetAlias.User.Id == entryUser.SubjectId);
			}

			if(entryObject3.Subject is HistoryTraceObjectNode selectedClassType)
			{
				query.Where(ce => ce.EntityClassName == selectedClassType.ObjectName);
			}

			if(comboAction.SelectedItem is EntityChangeOperation)
			{
				query.Where(ce => ce.Operation == (EntityChangeOperation)comboAction.SelectedItem);
			}

			if(!string.IsNullOrWhiteSpace(entrySearchEntity.Text))
			{
				var pattern = $"%{entrySearchEntity.Text}%";
				query.Where(ce => ce.EntityTitle.IsLike(pattern));
			}

			if(!string.IsNullOrWhiteSpace(entSearchId.Text))
			{
				if(int.TryParse(entSearchId.Text, out int id))
				{
					query.Where(ce => ce.EntityId == id);
				}
			}

			if(!string.IsNullOrWhiteSpace(entrySearchValue.Text) || entryProperty.Subject is HistoryTracePropertyNode)
			{
				ArchivedFieldChange fieldChangeAlias = null;
				query.JoinAlias(ce => ce.Changes, () => fieldChangeAlias);

				if(entryProperty.Subject is HistoryTracePropertyNode selectedProperty)
				{
					query.Where(() => fieldChangeAlias.Path == selectedProperty.PropertyPath);
				}

				if(!string.IsNullOrWhiteSpace(entrySearchValue.Text))
				{
					var pattern = $"%{entrySearchValue.Text}%";
					query.Where(() => fieldChangeAlias.OldValue.IsLike(pattern) || fieldChangeAlias.NewValue.IsLike(pattern));
				}
			}

			try
			{
				var taked = query.OrderBy(x => x.ChangeTime).Desc
							 .Skip(_takenOldRows)
							 .Take(_pageSize)
							 .List();

				if(_takenOldRows > 0)
				{
					_oldChangedEntities.AddRange(taked);
					dataTreeOldChangeSets.YTreeModel.EmitModelChanged();
				}
				else
				{
					_oldChangedEntities = taked.ToList();
					dataTreeOldChangeSets.ItemsDataSource = _oldChangedEntities;
				}

				if(taked.Count < _pageSize)
				{
					_takenAllOld = true;
				}
			}
			catch(GenericADOException ex)
			{
				if(ex?.InnerException?.InnerException?.InnerException?.InnerException?.InnerException is System.Net.Sockets.SocketException exception
					&& exception.SocketErrorCode == System.Net.Sockets.SocketError.TimedOut)
				{
					MessageDialogHelper.RunWarningDialog("Превышен интервал ожидания ответа от сервера:\n" +
						"Попробуйте выбрать меньший интервал времени\n" +
						"или уточнить условия поиска");
				}
			}

			_takenOldRows = _oldChangedEntities.Count;

			_logger.LogDebug("Время запроса {RequestTime}", DateTime.Now - startTime);
			_logger.LogInformation(NumberToTextRus.FormatCase(_oldChangedEntities.Count, "Загружено изменение {0}{1} объекта.", "Загружено изменение {0}{1} объектов.", "Загружено изменение {0}{1} объектов.", _takenAllOld ? "" : "+"));
		}

		private void OnObjectChangedByUser(object sender, EventArgs e)
		{
			_historyTracePropertyJournalViewModel.ObjectType = entryObject3.Subject is HistoryTraceObjectNode node ? node.ObjectType : null;
			UpdateNodes();
		}

		protected void OnButtonSearchClicked(object sender, EventArgs e)
		{
			UpdateNodes();
		}

		protected void OnSelectperiodDatesChanged(object sender, EventArgs e)
		{
			UpdateNodes();
		}

		protected void OnEntrySearchValueActivated(object sender, EventArgs e)
		{
			buttonSearch.Click();
		}

		protected void OnComboActionChanged(object sender, EventArgs e)
		{
			UpdateNodes();
		}

		public override void Destroy()
		{
			UoW.Dispose();
			base.Destroy();
		}

		protected void OnEntrySearchEntityActivated(object sender, EventArgs e)
		{
			UpdateNodes();
		}

		protected void OnBtnFilterClicked(object sender, EventArgs e)
		{
			tblSettings.Visible = !tblSettings.Visible;
			btnFilter.Label = tblSettings.Visible ? "Скрыть фильтр" : "Показать фильтр";
		}

		private void UpdateNodes()
		{
			if(_isSearchFromOldMonitoring)
			{
				UpdateJournalOld();
			}
			else
			{
				UpdateJournal();
			}
		}
	}
}
