using Autofac;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.ViewModels.Control.EEVM;
using System;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.DocTemplates;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Organizations;

namespace Vodovoz.Dialogs.Employees
{
	[ToolboxItem(true)]
	public partial class EmployeeContractDlg : SingleUowTabBase
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private IDocTemplateRepository _docTemplateRepository;

		public event EventHandler Save;

		public EmployeeContract Entity;

		public EmployeeContractDlg(EmployeeDocument document,Employee employee,IUnitOfWork uow)
		{
			ResolveDependancies();
			this.Build();
			TabName = "Новый договор";
			UoW = uow;
			Entity = new EmployeeContract();
			Entity.FirstDay = DateTime.Now;
			Entity.LastDay = DateTime.Now.AddYears(1);
			Entity.ContractDate = DateTime.Now;
			Entity.Document = document;
			Entity.Employee = employee;
			ConfigureDlg();
			this.ShowAll();
		}

		public EmployeeContractDlg(int id, IUnitOfWork uow)
		{
			ResolveDependancies();
			this.Build();
			UoW = uow;
			Entity = (EmployeeContract)UoW.GetById(typeof(EmployeeContract), id);
			TabName = Entity.Name+ "договор";
			ConfigureDlg();
			this.ShowAll();
		}

		private void ResolveDependancies()
		{
			_docTemplateRepository = _lifetimeScope.Resolve<IDocTemplateRepository>();
		}

		void ConfigureDlg()
		{
			var organizationEntryViewModelBuilder = new LegacyEEVMBuilderFactory<EmployeeContract>(
				this,
				Entity,
				UoW,
				Startup.MainWin.NavigationManager,
				_lifetimeScope);

			var organizationEntryViewModel = organizationEntryViewModelBuilder.ForProperty(x => x.Organization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();

			organizationEntryViewModel.CanViewEntity = false;
			organizationEntryViewModel.Changed += (s, e) => UpdateStates();

			entityentryOrganization.ViewModel = organizationEntryViewModel;

			yentry1.Binding.AddBinding(Entity,e=>e.Name,w=>w.Text).InitializeFromSource();
			yContractDatepicker.Binding.AddBinding(Entity,e=>e.ContractDate,w=>w.DateOrNull).InitializeFromSource();
			ydateperiodpicker1.Binding.AddBinding(Entity, e => e.FirstDay, w => w.StartDateOrNull).InitializeFromSource();
			ydateperiodpicker1.Binding.AddBinding(Entity, e => e.LastDay, w => w.EndDateOrNull).InitializeFromSource();
			savetemplatewidget1.Binding.AddBinding(Entity,e=>e.EmployeeContractTemplate, w => w.Template).InitializeFromSource();
			savetemplatewidget1.Binding.AddBinding(Entity, e => e.TemplateFile, w => w.File).InitializeFromSource();

			RefreshParserRootObject();
			UpdateStates();
			SetActive();
		}

		void UpdateStates()
		{
			savetemplatewidget1.AvailableTemplates = _docTemplateRepository.GetAvailableTemplates(UoW, TemplateType.EmployeeContract, Entity.Organization);
			savetemplatewidget1.Template = savetemplatewidget1.AvailableTemplates.FirstOrDefault();
			
			if(savetemplatewidget1.Template != null)
			{
				labelTem.Text = savetemplatewidget1.Template.Name != null ? savetemplatewidget1.Template.Name : "Без названия";
			}
			else
			{
				labelTem.Text = "Шаблон отсутствует";
			}
		}

		void RefreshParserRootObject()
		{
			if(Entity.EmployeeContractTemplate != null)
			{
				(Entity.EmployeeContractTemplate.DocParser as EmployeeContractParser).RootObject = Entity;
			}
		}

		void Templatewidget_BeforeOpen(object sender, EventArgs e)
		{
			RefreshParserRootObject();
		}

		private void SaveCont()
		{
			UoW.Save(Entity);
			OnCloseTab(false);
		}

		private void SetActive()
		{
			bool isTemplateActive = savetemplatewidget1.File != null;
			savetemplatewidget1.Sensitive = isTemplateActive;
			yContractDatepicker.Sensitive = !isTemplateActive;
			ydateperiodpicker1.Sensitive = !isTemplateActive;
			entityentryOrganization.Sensitive = !isTemplateActive;
			button15.Sensitive = !isTemplateActive;
		}

		#region ButtonEventhandler
		protected void OnSaveButtonClicked(object sender, EventArgs e)
		{
			SaveCont();
			Save?.Invoke(sender, e);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}

		protected void OnButtonCreateClicked(object sender, EventArgs e)
		{
			RefreshParserRootObject();
			savetemplatewidget1.LoadFile();
			SetActive();
		}
		#endregion

		public override void Destroy()
		{
			_docTemplateRepository = null;
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}
