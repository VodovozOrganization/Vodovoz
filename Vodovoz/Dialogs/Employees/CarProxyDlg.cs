using System;
using System.Linq;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.DocTemplates;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModel;
namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CarProxyDlg : OrmGtkDialogBase<CarProxyDocument>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

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
			ylabelNumber.Binding.AddBinding(Entity, x => x.Title, x => x.LabelProp).InitializeFromSource();

			yentryOrganization.SubjectType = typeof(Organization);
			yentryOrganization.Binding.AddBinding(Entity, x => x.Organization, x => x.Subject).InitializeFromSource();
			yentryOrganization.Changed += (sender, e) => {
				UpdateStates();
			};

			var filterDefaultForwarder = new EmployeeFilter(UoW);
			filterDefaultForwarder.SetAndRefilterAtOnce(x => x.RestrictCategory = EmployeeCategory.driver);
			yentryDriver.RepresentationModel = new EmployeesVM(filterDefaultForwarder);
			yentryDriver.Binding.AddBinding(Entity, x => x.Driver, x => x.Subject).InitializeFromSource();

			yentryCar.SubjectType = typeof(Car);
			yentryCar.Binding.AddBinding(Entity, x => x.Car, x => x.Subject).InitializeFromSource();

			RefreshParserRootObject();

			templatewidget.CanRevertCommon = QSMain.User.Permissions["can_set_common_additionalagreement"];
			templatewidget.Binding.AddBinding(Entity, e => e.DocumentTemplate, w => w.Template).InitializeFromSource();
			templatewidget.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();

			templatewidget.BeforeOpen += Templatewidget_BeforeOpen;

			UpdateStates();
		}

		void Templatewidget_BeforeOpen(object sender, EventArgs e)
		{
			if(UoW.HasChanges) {
				if(MessageDialogWorks.RunQuestionDialog("Необходимо сохранить документ перед открытием печатной формы, сохранить?")) {
					UoWGeneric.Save();
					RefreshParserRootObject();
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
			yentryCar.Sensitive = isNewDoc;
			if(Entity.Organization == null || !isNewDoc) {
				return;
			}
			templatewidget.AvailableTemplates = Repository.Client.DocTemplateRepository.GetAvailableTemplates(UoW, TemplateType.CarProxy, Entity.Organization);
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
