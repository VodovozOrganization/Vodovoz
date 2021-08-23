using System;
using System.Linq;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Validation;
using Vodovoz.DocTemplates;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModel;
using Vodovoz.Filters.ViewModels;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.JournalFilters;
using Vodovoz.JournalViewModels;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CarProxyDlg : QS.Dialog.Gtk.EntityDialogBase<CarProxyDocument>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private readonly IDocTemplateRepository _docTemplateRepository = new DocTemplateRepository();

		public CarProxyDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<CarProxyDocument>();
			Entity.Date = DateTime.Now;
			TabName = "Новая доверенность на ТС";
			ConfigureDlg();
		}

		public CarProxyDlg(CarProxyDocument sub) : this(sub.Id)
		{}

		public CarProxyDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CarProxyDocument>(id);
			TabName = "Изменение доверенности на ТС";
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			if(Entity.EmployeeDocument == null && Entity.Driver != null)
				GetDocument();

			ylabelNumber.Binding.AddBinding(Entity, x => x.Title, x => x.LabelProp).InitializeFromSource();

			yentryOrganization.SubjectType = typeof(Organization);
			yentryOrganization.Binding.AddBinding(Entity, x => x.Organization, x => x.Subject).InitializeFromSource();
			yentryOrganization.Changed += (sender, e) => {
				UpdateStates();
			};

			var filterDefaultForwarder = new EmployeeRepresentationFilterViewModel();
			filterDefaultForwarder.Status = EmployeeStatus.IsWorking;
			filterDefaultForwarder.SetAndRefilterAtOnce(x => x.RestrictCategory = EmployeeCategory.driver);
			yentryDriver.RepresentationModel = new EmployeesVM(filterDefaultForwarder);
			yentryDriver.Binding.AddBinding(Entity, x => x.Driver, x => x.Subject).InitializeFromSource();
			yentryDriver.Changed += (sender, e) => {
				UpdateStates();
			};

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Car, CarJournalViewModel, CarJournalFilterViewModel>(ServicesConfig.CommonServices));
			entityviewmodelentryCar.Binding.AddBinding(Entity, x => x.Car, x => x.Subject).InitializeFromSource();
			entityviewmodelentryCar.CompletionPopupSetWidth(false);
			entityviewmodelentryCar.Changed += (sender, e) => {
				UpdateStates();
			};

			RefreshParserRootObject();

			templatewidget.CanRevertCommon = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_common_additionalagreement");
			templatewidget.Binding.AddBinding(Entity, e => e.DocumentTemplate, w => w.Template).InitializeFromSource();
			templatewidget.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();
			templatewidget.BeforeOpen += Templatewidget_BeforeOpen;

			UpdateStates();
		}

		void GetDocument()
		{
			var doc = Entity.Driver.GetMainDocuments();
			if(doc.Count>0)
				Entity.EmployeeDocument = doc[0];
		}

		void Templatewidget_BeforeOpen(object sender, EventArgs e)
		{
			if(Entity.EmployeeDocument == null)
				GetDocument();
			if(UoW.HasChanges) {
				if(MessageDialogHelper.RunQuestionDialog("Необходимо сохранить документ перед открытием печатной формы, сохранить?")) {
					UoWGeneric.Save();
					RefreshParserRootObject();
					UpdateStates();
				} else {
					templatewidget.CanOpenDocument = false;
				}
			}
		}

		void RefreshParserRootObject()
		{
			if(Entity.DocumentTemplate != null)
				(Entity.DocumentTemplate.DocParser as CarProxyDocumentParser).RootObject = Entity;
		}

		void UpdateStates()
		{
			bool isNewDoc = !(Entity.Id > 0);
			yentryOrganization.Sensitive = isNewDoc;
			yentryDriver.Sensitive = isNewDoc;
			entityviewmodelentryCar.Sensitive = isNewDoc;
			if(Entity.Organization == null 
				|| Entity.Car == null 
				|| Entity.Driver == null
				|| !isNewDoc) {
				return;
			}
			templatewidget.AvailableTemplates = _docTemplateRepository.GetAvailableTemplates(UoW, TemplateType.CarProxy, Entity.Organization);
			templatewidget.Template = templatewidget.AvailableTemplates.FirstOrDefault();
		}

		public override bool Save()
		{
			var valid = new QSValidator<CarProxyDocument>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save();
			return true;
		}
	}
}
