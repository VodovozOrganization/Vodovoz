using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Gamma.Binding.Core;
using Gtk;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.RepresentationModel.GtkUI;
using QS.Tdi;
using Vodovoz.Journal;

namespace Vodovoz.JournalViewers
{
    [ToolboxItem(true)]
    public partial class NodeViewModelEntry : WidgetOnDialogBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private bool entryChangedByUser;

        public BindingControler<NodeViewModelEntry> Binding { get; private set; }
        public bool CanEditReference { get; set; } = true;
        private ListStore completionListStore;

        public event EventHandler Changed;
        public event EventHandler ChangedByUser;

        public NodeViewModelEntry()
        {
            this.Build();
            Binding = new BindingControler<NodeViewModelEntry>(this, new Expression<Func<NodeViewModelEntry, object>>[] {
                (w => w.Subject)
            });
        }

        private bool sensitive = true;
        [Browsable(false)]
        public new bool Sensitive {
            get { return sensitive; }
            set {
                if(sensitive == value)
                    return;
                sensitive = value;
                UpdateSensitive();
            }
        }

        bool isEditable = true;
        [Browsable(false)]
        public bool IsEditable {
            get { return isEditable; }
            set {
                isEditable = value;
                UpdateSensitive();
            }
        }

        private INodeAutocompleteSelectorFactory _nodeSelectorAutocompleteFactory;
        private INodeSelectorFactory _nodeSelectorFactory;

        public void SetNodeAutocompleteSelectorFactory(INodeAutocompleteSelectorFactory nodeAutocompleteSelectorFactory)
        {
            _nodeSelectorAutocompleteFactory = nodeAutocompleteSelectorFactory ?? throw new ArgumentNullException(nameof(nodeAutocompleteSelectorFactory));
            _nodeSelectorFactory = _nodeSelectorAutocompleteFactory;
            SubjectType = nodeAutocompleteSelectorFactory.EntityType;
            entryObject.IsEditable = true;
            entryChangedByUser = true;
            ConfigureEntryComplition();
        }

        public void SetEntitySelectorFactory(INodeSelectorFactory nodeSelectorFactory)
        {
            _nodeSelectorFactory = nodeSelectorFactory ?? throw new ArgumentNullException(nameof(nodeSelectorFactory));
            SubjectType = nodeSelectorFactory.EntityType;
            entryObject.IsEditable = false;
            entryChangedByUser = false;
            ConfigureEntryComplition();
        }

        public void CompletionPopupSetWidth(bool isResized)
        {
            entryObject.Completion.PopupSetWidth = isResized;
        }

        private void ConfigureEntryComplition()
        {
            entryObject.Completion = new EntryCompletion();
            entryObject.Completion.MatchSelected += Completion_MatchSelected;
            entryObject.Completion.MatchFunc = Completion_MatchFunc;
            var cell = new CellRendererText();
            entryObject.Completion.PackStart(cell, true);
            entryObject.Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);
        }

        void JournalViewModel_OnNodeSelectedResult(object sender, JournalSelectedNodesNodesEventArgs e)
        {
            Subject = e.SelectedNodes[0];

            ChangedByUser?.Invoke(sender, e);
        }

        private object subject;

        public object Subject {
            get { return subject; }
            set {
                if(subject == value)
                    return;
                if(subject is INotifyPropertyChanged notifyPropertyChangedSubject) {
                    notifyPropertyChangedSubject.PropertyChanged -= OnSubjectPropertyChanged;
                    notifyPropertyChangedSubject.PropertyChanged += OnSubjectPropertyChanged;
                }
                subject = value;
                UpdateWidget();
                OnChanged();
            }
        }

        void OnSubjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateWidget();
        }

        private Type subjectType;
        protected Type SubjectType {
            get { return subjectType; }
            set {
                subjectType = value;
            }
        }

        private void UpdateWidget()
        {
            buttonViewEntity.Sensitive = CanEditReference && subject != null;
            if(subject == null) {
                InternalSetEntryText(string.Empty);
                return;
            }

            InternalSetEntryText(DomainHelper.GetTitle(Subject)); // Тащит DomainModelEntity, надо избавиться
        }

        private void InternalSetEntryText(string text)
        {
            entryChangedByUser = false;
            entryObject.Text = text;
            entryChangedByUser = true;
        }

        protected void OnButtonSelectEntityClicked(object sender, EventArgs e)
        {
            OpenSelectDialog();
        }

        protected void OnButtonClearClicked(object sender, EventArgs e)
        {
            ClearSubject();
        }

        private void ClearSubject()
        {
            Subject = null;
            OnChangedByUser();
            UpdateWidget();
        }

        INodeSelector _nodeSelector;

        public void OpenSelectDialog(string newTabTitle = null)
        {
            if(_nodeSelector != null) {
                MyTab.TabParent.SwitchOnTab(_nodeSelector);
                return;
            }

            _nodeSelector = _nodeSelectorFactory.CreateSelector();
            _nodeSelector.OnEntitySelectedResult += JournalViewModel_OnNodeSelectedResult;
            _nodeSelector.TabClosed += NodeSelector_TabClosed;
            MyTab.TabParent.AddSlaveTab(MyTab, _nodeSelector);
        }

        void NodeSelector_TabClosed(object sender, EventArgs e)
        {
            _nodeSelector = null;
        }

        protected void OnButtonViewNodeClicked(object sender, EventArgs e)
        {
            ITdiTab mytab = DialogHelper.FindParentTab(this);
            if(mytab == null) {
                logger.Warn("Родительская вкладка не найдена.");
                return;
            }
        }

        protected virtual void OnChanged()
        {
            Binding.FireChange(new Expression<Func<NodeViewModelEntry, object>>[] {
                (w => w.Subject)
            });

            if(Changed != null)
                Changed(this, EventArgs.Empty);

        }

        protected virtual void OnChangedByUser()
        {
            if(ChangedByUser != null)
                ChangedByUser(this, EventArgs.Empty);
        }

        void UpdateSensitive()
        {
            buttonSelectEntity.Sensitive = entryObject.Sensitive = sensitive && IsEditable;
            buttonViewEntity.Sensitive = sensitive && CanEditReference && subject != null;
            buttonClear.Sensitive = sensitive && (subject != null || string.IsNullOrWhiteSpace(entryObject.Text));
        }

        bool Completion_MatchFunc(EntryCompletion completion, string key, TreeIter iter)
        {
            return true;
        }

        void OnCellLayoutDataFunc(CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
        {
            var title = (string)tree_model.GetValue(iter, 0);
            string pattern = String.Format("{0}", Regex.Escape(entryObject.Text));
            (cell as CellRendererText).Markup =
                Regex.Replace(title, pattern, (match) => String.Format("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
        }

        [GLib.ConnectBefore]
        void Completion_MatchSelected(object o, MatchSelectedArgs args)
        {
            var node = args.Model.GetValue(args.Iter, 1);
            Subject = node;
            OnChangedByUser();
            args.RetVal = true;
        }

        private INodeAutocompleteSelector _nodeAutoCompleteSelector;

        private void FillAutocomplete()
        {
            logger.Info("Запрос данных для автодополнения...");
            completionListStore = new ListStore(typeof(string), typeof(object));
            if(_nodeSelectorAutocompleteFactory == null) {
                return;
            }
            _nodeAutoCompleteSelector = _nodeSelectorAutocompleteFactory.CreateAutocompleteSelector();
            _nodeAutoCompleteSelector.ListUpdated += OnListUpdated;
            _nodeAutoCompleteSelector.SearchValues(entryObject.Text);
        }

        private void OnListUpdated(object sender, EventArgs e)
        {
            if(cts.IsCancellationRequested)
                return;

            Gtk.Application.Invoke((senderObject, eventArgs) => {
                if(_nodeAutoCompleteSelector?.Items == null)
                    return;

                foreach(var item in _nodeAutoCompleteSelector.Items) {
                    if(item is IJournalNode) {
                        completionListStore.AppendValues(
                            (item as IJournalNode).Title,
                            item
                        );
                    } else if(item is INodeWithEntryFastSelect) {
                        completionListStore.AppendValues(
                            (item as INodeWithEntryFastSelect).EntityTitle,
                            item
                        );
                    }
                }
                entryObject.Completion.Model = completionListStore;
                entryObject.Completion.PopupCompletion = true;
                logger.Debug("Получено {0} строк автодополения...", completionListStore.IterNChildren());

                _nodeAutoCompleteSelector.ListUpdated -= OnListUpdated;
                _nodeAutoCompleteSelector.Dispose();
            });
        }

        protected void OnEntryObjectFocusOutEvent(object o, FocusOutEventArgs args)
        {
            if(string.IsNullOrWhiteSpace(entryObject.Text)) {
                Subject = null;
                OnChangedByUser();
            }
        }

        DateTime lastChangedTime = DateTime.Now;
        bool fillingInProgress = false;
        private CancellationTokenSource cts = new CancellationTokenSource();

        protected void OnEntryObjectChanged(object sender, EventArgs e)
        {
            lastChangedTime = DateTime.Now;
            if(!fillingInProgress && entryChangedByUser) {
                Task.Run(() => {
                    fillingInProgress = true;
                    try {
                        while((DateTime.Now - lastChangedTime).TotalMilliseconds < 200) {
                            if(cts.IsCancellationRequested) {
                                return;
                            }
                        }
                        Gtk.Application.Invoke((s, arg) => {
                            FillAutocomplete();
                        });
                    } catch(Exception ex) {
                        logger.Error(ex, $"Ошибка во время формирования автодополнения для {nameof(NodeViewModelEntry)}");
                    } finally {
                        fillingInProgress = false;
                    }
                });
            }
        }

        protected override void OnDestroyed()
        {
            logger.Debug("EntityViewModelEntry Destroyed() called.");
            //Отписываемся от событий.
            if(subject is INotifyPropertyChanged) {
                (subject as INotifyPropertyChanged).PropertyChanged -= OnSubjectPropertyChanged;
            }
            cts.Cancel();
            base.OnDestroyed();
        }

        protected void OnButtonViewEntityClicked(object sender, EventArgs e)
        {
        }
    }
}
