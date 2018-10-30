using System;
using System.Linq;
using Gamma.GtkWidgets;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Repository;
using Vodovoz.ViewModel;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PremiumDlg : OrmGtkDialogBase<Premium>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public PremiumDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Premium>();
			ConfigureDlg();
		}

		public PremiumDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Premium>(id);
			ConfigureDlg();
		}

		public PremiumDlg(Fine sub) : this (sub.Id)
		{
		}

		void ConfigureDlg()
		{
			ylabelDate.Binding.AddFuncBinding(Entity, e => e.Date.ToString("D"), w => w.LabelProp).InitializeFromSource();
			yspinMoney.Binding.AddBinding(Entity, e => e.TotalMoney, w => w.ValueAsDecimal).InitializeFromSource();
			yentryPremiumReasonString.Binding.AddBinding(Entity, e => e.PremiumReasonString, w => w.Text).InitializeFromSource();

			var filterRouteList = new RouteListsFilter(UoW);
			filterRouteList.SetFilterDates(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1));

			Entity.ObservableItems.ListContentChanged += ObservableItems_ListContentChanged;

			var filterAuthor = new EmployeeFilter(UoW);
			yentryAuthor.RepresentationModel = new EmployeesVM(filterAuthor);
			yentryAuthor.Binding.AddBinding(Entity, e => e.Author, w => w.Subject).InitializeFromSource();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<PremiumItem>()
				.AddColumn("Сотрудник").AddTextRenderer(x => x.Employee.FullName)
				.AddColumn("Премия").AddNumericRenderer(x => x.Money).Editing().Digits(2)
				.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddColumn("Причина штрафа").AddTextRenderer(x => x.Premium.PremiumReasonString)
				.Finish();

			ytreeviewItems.ItemsDataSource = Entity.ObservableItems;
		}

		public override bool Save()
		{
			Employee author;
			if(!GetAuthor(out author)) return false;

			if(Entity.Author == null) {
				Entity.Author = author;
			}
			var valid = new QSValidation.QSValidator<Premium>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			Entity.UpdateWageOperations(UoW);

			logger.Info("Сохраняем премию...");
			UoWGeneric.Save();
			logger.Info("Ok.");
			return true;
		}

		private bool GetAuthor(out Employee cashier)
		{
			cashier = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(cashier == null) {
				MessageDialogWorks.RunErrorDialog(
					"Ваш пользователь не привязан к действующему сотруднику.");
				return false;
			}
			return true;
		}

		void ObservableItems_ListContentChanged(object sender, EventArgs e)
		{
			CalculateTotal();
		}

		protected void OnButtonDivideAtAllClicked(object sender, EventArgs e)
		{
			Entity.DivideAtAll();
		}

		protected void OnButtonGetReasonFromTemplateClicked(object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference(typeof(PremiumTemplate), UoWGeneric);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
			SelectDialog.ObjectSelected += (s, ea) => {
				if(ea.Subject != null) {
					Entity.PremiumReasonString = (ea.Subject as PremiumTemplate).Reason;
					Entity.TotalMoney = (ea.Subject as PremiumTemplate).PremiumMoney;
				}
			};
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var addEmployeeDlg = new ReferenceRepresentation(new EmployeesVM());
			addEmployeeDlg.Mode = OrmReferenceMode.Select;
			addEmployeeDlg.ObjectSelected += AddEmployeeDlg_ObjectSelected;
			TabParent.AddSlaveTab(this, addEmployeeDlg);
		}

		void AddEmployeeDlg_ObjectSelected(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var employee = UoW.GetById<Employee>(e.ObjectId);
			if(Entity.Items.Any(x => x.Employee.Id == employee.Id)) {
				MessageDialogWorks.RunErrorDialog("Сотрудник {0} уже присутствует в списке.", employee.ShortName);
				return;
			}
			Entity.AddItem(employee);
		}

		void CalculateTotal()
		{
			decimal sum = Entity.Items.Sum(x => x.Money);
			labelTotal.LabelProp = String.Format("Итого по сотрудникам: {0}", CurrencyWorks.GetShortCurrencyString(sum));
		}

		protected void OnButtonRemoveClicked(object sender, EventArgs e)
		{
			var row = ytreeviewItems.GetSelectedObject<PremiumItem>();
			if(row.Id > 0) {
				UoW.Delete(row);
				if(row.WageOperation != null)
					UoW.Delete(row.WageOperation);
			}
			Entity.ObservableItems.Remove(row);
		}
	}
}
