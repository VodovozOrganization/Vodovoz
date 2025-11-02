using Autofac;
using QS.Dialog.GtkUI;
using QS.Navigation;
using QS.Project.Services;
using QS.ViewModels.Control.EEVM;
using System;
using System.Linq;
using Vodovoz.DocTemplates;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CarProxyDlg : QS.Dialog.Gtk.EntityDialogBase<CarProxyDocument>
	{
		private ILifetimeScope _lifetimeScope;
		private IDocTemplateRepository _docTemplateRepository;

		public CarProxyDlg()
		{
			ResolveDependencies();
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<CarProxyDocument>();
			Entity.Date = DateTime.Now;
			TabName = "Новая доверенность на ТС";
			ConfigureDlg();
		}

		public CarProxyDlg(CarProxyDocument sub) : this(sub.Id)
		{}

		public CarProxyDlg(int id)
		{
			ResolveDependencies();
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<CarProxyDocument>(id);
			TabName = "Изменение доверенности на ТС";
			ConfigureDlg();
		}

		private void ResolveDependencies()
		{
			_lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();

			_docTemplateRepository = _lifetimeScope.Resolve<IDocTemplateRepository>();
		}

		void ConfigureDlg()
		{
			if(Entity.EmployeeDocument == null && Entity.Driver != null)
				GetDocument();

			ylabelNumber.Binding.AddBinding(Entity, x => x.Title, x => x.LabelProp).InitializeFromSource();

			var organizationViewModel = new LegacyEEVMBuilderFactory<CarProxyDocument>(
				this,
				Entity,
				UoW,
				Startup.MainWin.NavigationManager,
				_lifetimeScope)
				.ForProperty(x => x.Organization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();

			organizationViewModel.Changed += (sender, e) => UpdateStates();

			entryOrganization.ViewModel = organizationViewModel;

			var employeeFactory = _lifetimeScope.Resolve<IEmployeeJournalFactory>();
			evmeDriver.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory(true));
			evmeDriver.Binding.AddBinding(Entity, x => x.Driver, x => x.Subject).InitializeFromSource();
			evmeDriver.Changed += (sender, e) => UpdateStates();

			entityentryCar.ViewModel = BuildCarEntryViewModel();
			entityentryCar.ViewModel.Changed += (sender, e) => UpdateStates();

			RefreshParserRootObject();

			templatewidget.CanRevertCommon = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_common_additionalagreement");
			templatewidget.Binding.AddBinding(Entity, e => e.DocumentTemplate, w => w.Template).InitializeFromSource();
			templatewidget.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();
			templatewidget.BeforeOpen += Templatewidget_BeforeOpen;

			UpdateStates();
		}

		private IEntityEntryViewModel BuildCarEntryViewModel()
		{
			var navigationManager = _lifetimeScope.BeginLifetimeScope().Resolve<INavigationManager>();

			var viewModel = new LegacyEEVMBuilderFactory<CarProxyDocument>(this, Entity, UoW, navigationManager, _lifetimeScope)
				.ForProperty(x => x.Car)
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(filter =>
				{
				})
				.UseViewModelDialog<CarViewModel>()
				.Finish();

			viewModel.CanViewEntity = ServicesConfig.CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			return viewModel;
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
			entryOrganization.Sensitive = isNewDoc;
			evmeDriver.Sensitive = isNewDoc;
			entityentryCar.Sensitive = isNewDoc;
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
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			UoWGeneric.Save();
			return true;
		}

		public override void Destroy()
		{
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}
