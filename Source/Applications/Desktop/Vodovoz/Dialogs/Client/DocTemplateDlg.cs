using Autofac;
using Gamma.Utilities;
using QS.DocTemplates;
using QS.Project.Services;
using QS.ViewModels.Control.EEVM;
using QSDocTemplates;
using QSProjectsLib;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.JournalViewModels;
using QS.DocTemplates;
using FileWorker = QSDocTemplates.FileWorker;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.ViewModels.Organizations;

namespace Vodovoz
{
	public partial class DocTemplateDlg : QS.Dialog.Gtk.EntityDialogBase<DocTemplate>
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private QSDocTemplates.FileWorker worker = new QSDocTemplates.FileWorker();

		public DocTemplateDlg()
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<DocTemplate> ();
			ConfigureDlg ();
		}

		public DocTemplateDlg (int id)
		{
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<DocTemplate> (id);
			ConfigureDlg ();
		}

		public DocTemplateDlg (DocTemplate sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			yentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			ylabelSize.Binding.AddFuncBinding(Entity, e => StringWorks.BytesToIECUnitsString((ulong)e.FileSize), w => w.LabelProp).InitializeFromSource();
			ycomboType.ItemsEnum = typeof(TemplateType);
			ycomboType.Binding.AddBinding(Entity, e => e.TemplateType, w => w.SelectedItem).InitializeFromSource();
			ycomboContractType.ItemsEnum = typeof(ContractType);
			ycomboContractType.Binding.AddBinding(Entity, e => e.ContractType, w => w.SelectedItem).InitializeFromSource();
			
			var organizationEntryViewModelBuilder = new LegacyEEVMBuilderFactory<DocTemplate>(
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

			entityentryOrganization.ViewModel = organizationEntryViewModel;

			Entity.PropertyChanged += Entity_PropertyChanged;
		}

		void Entity_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == Entity.GetPropertyName(x => x.TempalteFile))
			{
				labelFileChanged.Markup = $"<span foreground=\"{GdkColors.SuccessText.ToHtmlColor()}\">(файл изменён)</span>";
			}
		}

		public override bool Save ()
		{
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			logger.Info ("Сохраняем шаблон документа...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			labelFileChanged.LabelProp = string.Empty;
			return true;
		}

		protected void OnButtonNewClicked(object sender, EventArgs e)
		{
			Entity.TempalteFile = TemplatesMain.GetEmptyTemplate();
		}

		protected void OnButtonFromFileClicked(object sender, EventArgs e)
		{
			byte[] tempTempalte = TemplatesMain.GetTemplateFromDisk();
			if (tempTempalte != null)
				Entity.TempalteFile = tempTempalte;
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			worker.OpenInOffice(Entity, false, FileEditMode.Template);
		}

		public override void Destroy()
		{
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}

