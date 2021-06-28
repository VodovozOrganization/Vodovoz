using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Exceptions;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.HistoryLog.Domain;
using QS.Project.DB;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Utilities;
using QSOrmProject;
using QSProjectsLib;
using QSWidgetLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Journal;
using Vodovoz.JournalNodes;
using Vodovoz.Journals;
using Vodovoz.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.HistoryTrace;
using VodovozInfrastructure.Attributes;

namespace Vodovoz.Dialogs
{
    [System.ComponentModel.DisplayName("Просмотр журнала изменений")]
    [WidgetWindow(DefaultWidth = 852, DefaultHeight = 600)]
    public partial class HistoryView : QS.Dialog.Gtk.TdiTabBase, ISingleUoWDialog
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        List<ChangedEntity> changedEntities;
        bool canUpdate = false;
        private int pageSize = 250;
        private int takenRows = 0;
        private bool takenAll = false;
        private bool needToHideProperties = true;
        private IDiffFormatter diffFormatter = new PangoDiffFormater();
        private HistoryTracePropertyJournalViewModel historyTracePropertyJournalViewModel;

        public IUnitOfWork UoW { get; private set; }

        public HistoryView()
        {
            this.Build();

            needToHideProperties = !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_see_history_view_restricted_properties");

            historyTracePropertyJournalViewModel = new HistoryTracePropertyJournalViewModel(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices.InteractiveService);

            UoW = UnitOfWorkFactory.CreateWithoutRoot();

            comboAction.ItemsEnum = typeof(EntityChangeOperation);

            entryUser.SetEntityAutocompleteSelectorFactory(
                new DefaultEntityAutocompleteSelectorFactory<User, UserJournalViewModel, UserJournalFilterViewModel>(ServicesConfig.CommonServices));

            entryUser.ChangedByUser += (sender, e) => UpdateJournal();

            entryObject3.SetNodeAutocompleteSelectorFactory(
                new NodeAutocompleteSelectorFactory<HistoryTraceObjectJournalViewModel>(typeof(HistoryTraceObjectNode),
                () => new HistoryTraceObjectJournalViewModel(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices.InteractiveService)));

            entryObject3.ChangedByUser += OnObjectChangedByUser;

            entryProperty.SetNodeAutocompleteSelectorFactory(
                new NodeAutocompleteSelectorFactory<HistoryTracePropertyJournalViewModel>(typeof(HistoryTracePropertyNode),
                () => historyTracePropertyJournalViewModel));

            entryProperty.ChangedByUser += (sender, e) => UpdateJournal();

            selectperiod.ActiveRadio = SelectPeriod.Period.Today;

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
            GtkScrolledWindowChangesets.Vadjustment.ValueChanged += Vadjustment_ValueChanged;

            datatreeChanges.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<FieldChange>()
                .AddColumn("Поле").AddTextRenderer(x => x.FieldTitle)
                .AddColumn("Операция").AddTextRenderer(x => x.TypeText)
                .AddColumn("Новое значение").AddTextRenderer(x => x.NewFormatedDiffText, useMarkup: true)
                .AddColumn("Старое значение").AddTextRenderer(x => x.OldFormatedDiffText, useMarkup: true)
                .Finish();

            canUpdate = true;
            UpdateJournal();
        }

        void Vadjustment_ValueChanged(object sender, EventArgs e)
        {
            if(takenAll || datatreeChangesets.Vadjustment.Value + datatreeChangesets.Vadjustment.PageSize < datatreeChangesets.Vadjustment.Upper)
                return;

            var lastPos = datatreeChangesets.Vadjustment.Value;
            UpdateJournal(true);
            QSMain.WaitRedraw();
            datatreeChangesets.Vadjustment.Value = lastPos;
        }

        void OnChangeSetSelectionChanged(object sender, EventArgs e)
        {
            var selected = (ChangedEntity)datatreeChangesets.GetSelectedObject();

            if(selected != null)
			{
                List<FieldChange> changes;

				if (needToHideProperties)
                {
                    changes = selected.Changes.Where(FieldChangeNotNeedToBeHided).ToList();
                } 
				else
                {
                    changes = selected.Changes.ToList();
                }

                changes.ForEach(x => x.DiffFormatter = diffFormatter);

                datatreeChanges.ItemsDataSource = changes;
            }
			else
			{
				datatreeChanges.ItemsDataSource = null;
			}
		}

		private bool FieldChangeNotNeedToBeHided(FieldChange fieldChange)
		{
			var restrictedToShowPropertyAttribute = new RestrictedHistoryProperty();

			var persistentClassType = OrmConfig.NhConfig.ClassMappings
				.Where(mc => mc.MappedClass.Name == fieldChange.Entity.EntityClassName)
				.Select(mc => mc.MappedClass).FirstOrDefault();

			return !persistentClassType?.GetProperty(fieldChange.Path)?.GetCustomAttributes(false).Contains(restrictedToShowPropertyAttribute) ?? false;
		}

        void UpdateJournal(bool nextPage = false)
        {
            DateTime startTime = DateTime.Now;
            if(!nextPage) {
                takenRows = 0;
                takenAll = false;
            }

            if(!canUpdate)
                return;

            logger.Info("Получаем журнал изменений{0}...", takenRows > 0 ? $"({takenRows}+)" : "");
            ChangeSet changeSetAlias = null;

            var query = UoW.Session.QueryOver<ChangedEntity>()
                .JoinAlias(ce => ce.ChangeSet, () => changeSetAlias)
                .Fetch(SelectMode.Fetch, x => x.ChangeSet)
                .Fetch(SelectMode.Fetch, x => x.ChangeSet.User);

            if(!selectperiod.IsAllTime)
                query.Where(ce => ce.ChangeTime >= selectperiod.DateBegin && ce.ChangeTime < selectperiod.DateEnd);

            if(entryUser.Subject != null)
                query.Where(() => changeSetAlias.User.Id == entryUser.SubjectId);

            if(entryObject3.Subject is HistoryTraceObjectNode selectedClassType)
                query.Where(ce => ce.EntityClassName == selectedClassType.ObjectName);

            if(comboAction.SelectedItem is EntityChangeOperation)
                query.Where(ce => ce.Operation == (EntityChangeOperation)comboAction.SelectedItem);

            if(!string.IsNullOrWhiteSpace(entrySearchEntity.Text)) {
                var pattern = $"%{entrySearchEntity.Text}%";
                query.Where(ce => ce.EntityTitle.IsLike(pattern));
            }

            if(!string.IsNullOrWhiteSpace(entSearchId.Text)) {
                if(int.TryParse(entSearchId.Text, out int id))
                    query.Where(ce => ce.EntityId == id);
            }

            if(!string.IsNullOrWhiteSpace(entrySearchValue.Text) || entryProperty.Subject is HistoryTracePropertyNode) {
                FieldChange fieldChangeAlias = null;
                query.JoinAlias(ce => ce.Changes, () => fieldChangeAlias);

                if(entryProperty.Subject is HistoryTracePropertyNode selectedProperty)
                    query.Where(() => fieldChangeAlias.Path == selectedProperty.PropertyPath);

                if(!string.IsNullOrWhiteSpace(entrySearchValue.Text)) {
                    var pattern = $"%{entrySearchValue.Text}%";
                    query.Where(
                        () => fieldChangeAlias.OldValue.IsLike(pattern) || fieldChangeAlias.NewValue.IsLike(pattern)
                    );
                }
            }

            try{
                var taked = query.OrderBy(x => x.ChangeTime).Desc
                             .Skip(takenRows)
                             .Take(pageSize)
                             .List();

                if(takenRows > 0){
                    changedEntities.AddRange(taked);
                    datatreeChangesets.YTreeModel.EmitModelChanged();
                }else{
                    changedEntities = taked.ToList();
                    datatreeChangesets.ItemsDataSource = changedEntities;
                }

                if(taked.Count < pageSize)
                    takenAll = true;

            } catch (GenericADOException ex) {
                if (ex?.InnerException?.InnerException?.InnerException?.InnerException?.InnerException is System.Net.Sockets.SocketException exception 
                    && exception.SocketErrorCode == System.Net.Sockets.SocketError.TimedOut){
                    MessageDialogHelper.RunWarningDialog("Превышен интервал ожидания ответа от сервера:\n" +
                        "Попробуйте выбрать меньший интервал времени\n" +
                        "или уточнить условия поиска");
                }
            }

            takenRows = changedEntities.Count;

            logger.Debug("Время запроса {0}", DateTime.Now - startTime);
            logger.Info(NumberToTextRus.FormatCase(changedEntities.Count, "Загружено изменение {0}{1} объекта.", "Загружено изменение {0}{1} объектов.", "Загружено изменение {0}{1} объектов.", takenAll ? "" : "+"));
        }

        private void OnObjectChangedByUser(object sender, EventArgs e)
        {
            historyTracePropertyJournalViewModel.ObjectType = entryObject3.Subject is HistoryTraceObjectNode node ? node.ObjectType : null;
            UpdateJournal();
        }

        protected void OnButtonSearchClicked(object sender, EventArgs e)
        {
            UpdateJournal();
        }

        protected void OnSelectperiodDatesChanged(object sender, EventArgs e)
        {
            UpdateJournal();
        }

        protected void OnEntrySearchValueActivated(object sender, EventArgs e)
        {
            buttonSearch.Click();
        }

        protected void OnComboActionChanged(object sender, EventArgs e)
        {
            UpdateJournal();
        }

        public override void Destroy()
        {
            UoW.Dispose();
            base.Destroy();
        }

        protected void OnEntrySearchEntityActivated(object sender, EventArgs e)
        {
            UpdateJournal();
        }

        protected void OnBtnFilterClicked(object sender, EventArgs e)
        {
            tblSettings.Visible = !tblSettings.Visible;
            btnFilter.Label = tblSettings.Visible ? "Скрыть фильтр" : "Показать фильтр";
        }
    }
}
