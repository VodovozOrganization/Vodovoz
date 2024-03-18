using Autofac;
using Gamma.ColumnConfig;
using NLog;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Filters.ViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz
{
	public partial class ProxyDlg : QS.Dialog.Gtk.EntityDialogBase<Proxy>
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public ProxyDlg(Counterparty counterparty)
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<Proxy>();
			UoWGeneric.Root.Counterparty = counterparty;
			ConfigureDlg();
		}

		public ProxyDlg(Proxy sub) : this(sub.Id) { }

		public ProxyDlg(int id)
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<Proxy>(id);
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			entryNumber.IsEditable = true;
			entryNumber.Binding.AddBinding(Entity, e => e.Number, w => w.Text).InitializeFromSource();

			personsView.Session = UoW.Session;
			if(UoWGeneric.Root.Persons == null)
				UoWGeneric.Root.Persons = new List<Person>();
			personsView.Persons = UoWGeneric.Root.Persons;

			datepickerIssue.Binding.AddBinding(Entity, e => e.IssueDate, w => w.Date).InitializeFromSource();
			datepickerIssue.DateChanged += OnIssueDateChanged;
			datepickerStart.Binding.AddBinding(Entity, e => e.StartDate, w => w.Date).InitializeFromSource();
			datepickerExpiration.Binding.AddBinding(Entity, e => e.ExpirationDate, w => w.Date).InitializeFromSource();

			buttonDeleteDeliveryPoint.Sensitive = false;

			ytreeDeliveryPoints.ColumnsConfig = FluentColumnsConfig<DeliveryPoint>.Create()
				.AddColumn("Точки доставки").AddTextRenderer(x => x.CompiledAddress).Finish();
			ytreeDeliveryPoints.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeDeliveryPoints.ItemsDataSource = Entity.ObservableDeliveryPoints;
			ytreeDeliveryPoints.Selection.Changed += YtreeDeliveryPoints_Selection_Changed;
		}

		void YtreeDeliveryPoints_Selection_Changed(object sender, EventArgs e)
		{
			buttonDeleteDeliveryPoint.Sensitive = ytreeDeliveryPoints.GetSelectedObjects().Length > 0;
		}

		private void OnIssueDateChanged(object sender, EventArgs e)
		{
			if(datepickerIssue.Date != default(DateTime) &&
				UoWGeneric.Root.StartDate == default(DateTime) || datepickerStart.Date < datepickerIssue.Date)
				datepickerStart.Date = datepickerIssue.Date;
		}

		public override bool Save()
		{
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			logger.Info("Сохраняем доверенность...");
			personsView.SaveChanges();
			UoWGeneric.Save();
			logger.Info("Ok");
			return true;
		}

		private void OnDeliveryPointJournalObjectSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			var selectedIds = e.SelectedNodes;
			if(!selectedIds.Any())
			{
				return;
			}
			var points = UoW.GetById<DeliveryPoint>(e.SelectedNodes.Select(n => n.Id)).ToList();
			points.ForEach(Entity.AddDeliveryPoint);
		}

		protected void OnButtonAddDeliveryPointsClicked(object sender, EventArgs e)
		{
			var filter = new DeliveryPointJournalFilterViewModel
			{
				Counterparty = Entity.Counterparty
			};
			var dpFactory = _lifetimeScope.Resolve<IDeliveryPointJournalFactory>(new TypedParameter(typeof(DeliveryPointJournalFilterViewModel), filter));
			var dpJournal = dpFactory.CreateDeliveryPointByClientJournal();
			dpJournal.SelectionMode = JournalSelectionMode.Multiple;
			dpJournal.OnEntitySelectedResult += OnDeliveryPointJournalObjectSelected;
			TabParent.AddSlaveTab(this, dpJournal);
		}

		protected void OnButtonDeleteDekiveryPointClicked(object sender, EventArgs e)
		{
			var selected = ytreeDeliveryPoints.GetSelectedObjects<DeliveryPoint>();
			foreach(var toDelete in selected)
			{
				Entity.ObservableDeliveryPoints.Remove(toDelete);
			}
		}

		public override void Destroy()
		{
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}
