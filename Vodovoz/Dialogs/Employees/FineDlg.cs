using System;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using System.Linq;
using Vodovoz.Domain.Logistic;
using NHibernate.Criterion;
using Vodovoz.Domain;

namespace Vodovoz
{
	public partial class FineDlg : OrmGtkDialogBase<Fine>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public FineDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Fine> ();
			ConfigureDlg ();
		}

		/// <summary>
		/// Создаем новый диалог с штрафа с уже заполненными сотрудниками.
		/// </summary>
		public FineDlg(decimal money, params Employee[] employees) : this()
		{
			employees.ToList().ForEach(Entity.AddItem);
			Entity.TotalMoney = money;
			Entity.DivideAtAll();
		}

		public FineDlg(string reasonString) : this()
		{
			Entity.FineReasonString = reasonString;
		}

		public FineDlg(decimal money, RouteList routeList, string reasonString, DateTime date, params Employee[] employees) : this()
		{
			employees.ToList().ForEach(Entity.AddItem);
			Entity.TotalMoney = money;
			Entity.DivideAtAll();
			Entity.FineReasonString = reasonString;
			Entity.Date = date;
			Entity.RouteList = routeList;
		}

		public FineDlg(decimal money, RouteList routeList) : this(money, routeList.Driver)
		{
			Entity.RouteList = routeList;
			Entity.Date = routeList.Date;
		}

		public FineDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Fine> (id);
			ConfigureDlg ();
		}

		public FineDlg (Fine sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			ylabelDate.Binding.AddFuncBinding(Entity, e => e.Date.ToString("D"), w => w.LabelProp).InitializeFromSource();
			yspinMoney.Binding.AddBinding(Entity, e => e.TotalMoney, w => w.ValueAsDecimal).InitializeFromSource();
			yentryFineReasonString.Binding.AddBinding(Entity, e => e.FineReasonString, w => w.Text).InitializeFromSource();
			fineitemsview.FineUoW = UoWGeneric;

			var filter = new RouteListsFilter(UoW);
			filter.SetFilterDates(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1));
			yentryreferenceRouteList.RepresentationModel = new ViewModel.RouteListsVM(filter);
			yentryreferenceRouteList.Binding.AddBinding(Entity, e => e.RouteList, w => w.Subject).InitializeFromSource();
		}

		public override bool Save ()
		{
			var valid = new QSValidation.QSValidator<Fine> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			foreach (var item in Entity.Items)
			{
				Entity.UpdateWageOperations(UoW, item.Money);
			}

			logger.Info ("Сохраняем штраф...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

		protected void OnButtonDivideAtAllClicked(object sender, EventArgs e)
		{
			Entity.DivideAtAll();
		}

		protected void OnButtonGetReasonFromTemplateClicked (object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference (typeof(FineTemplate), UoWGeneric);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
			SelectDialog.ObjectSelected += (s, ea) => {
				if (ea.Subject != null) {
					UoWGeneric.Root.FineReasonString = (ea.Subject as FineTemplate).Reason;
					UoWGeneric.Root.TotalMoney = (ea.Subject as FineTemplate).FineMoney;
				}
			};
			TabParent.AddSlaveTab (this, SelectDialog);
		}
	}
}

