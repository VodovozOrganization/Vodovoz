using System;
using System.Linq;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Validation;
using Vodovoz.DocTemplates;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.Core.Domain.Employees;

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

			var orgFactory = new OrganizationJournalFactory();
			evmeOrganisation.SetEntityAutocompleteSelectorFactory(orgFactory.CreateOrganizationAutocompleteSelectorFactory());
			evmeOrganisation.Binding.AddBinding(Entity, x => x.Organization, x => x.Subject).InitializeFromSource();
			evmeOrganisation.Changed += (sender, e) => UpdateStates();

			var driverFilter = new EmployeeFilterViewModel();
			driverFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.driver);
			var employeeFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager, driverFilter);
			evmeDriver.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeDriver.Binding.AddBinding(Entity, x => x.Driver, x => x.Subject).InitializeFromSource();
			evmeDriver.Changed += (sender, e) => UpdateStates();

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(new CarJournalFactory(Startup.MainWin.NavigationManager).CreateCarAutocompleteSelectorFactory());
			entityviewmodelentryCar.Binding.AddBinding(Entity, x => x.Car, x => x.Subject).InitializeFromSource();
			entityviewmodelentryCar.CompletionPopupSetWidth(false);
			entityviewmodelentryCar.Changed += (sender, e) => UpdateStates();

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
			var organizationVersion = Entity.Organization.OrganizationVersionOnDate(Entity.Date);
			
			if(organizationVersion == null)
			{
				MessageDialogHelper.RunErrorDialog($"На дату доверенности {Entity.Date.ToString("G")} отсутствует версия организации. Создайте версию организации.") ;
				templatewidget.CanOpenDocument = false;
				return;
			}

			if(organizationVersion.Leader == null)
			{
				MessageDialogHelper.RunErrorDialog($"Не выбран руководитель в версии №{organizationVersion.Id} организации \"{Entity.Organization.Name}\"");
				templatewidget.CanOpenDocument = false;
				return;
			}

			if(organizationVersion.Accountant == null)
			{
				MessageDialogHelper.RunErrorDialog($"Не выбран бухгалтер в версии №{organizationVersion.Id} организации \"{Entity.Organization.Name}\"");
				templatewidget.CanOpenDocument = false;
				return;
			}

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
			evmeOrganisation.Sensitive = isNewDoc;
			evmeDriver.Sensitive = isNewDoc;
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
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(Entity))
			{
				return false;
			}

			UoWGeneric.Save();
			return true;
		}
	}
}
