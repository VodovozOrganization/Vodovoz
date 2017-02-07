using System;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using System.Linq;
using Vodovoz.Domain.Logistic;
using NHibernate.Criterion;

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

		public FineDlg(decimal money, RouteList routeList) : this(money, routeList.Driver)
		{
			Entity.RouteList = routeList;
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

			yentryreferenceRouteList.ItemsQuery = QueryOver.Of<RouteList>();
			yentryreferenceRouteList.Binding.AddBinding(Entity, e => e.RouteList, w => w.Subject).InitializeFromSource();
			yentryreferenceRouteList.SetObjectDisplayFunc<RouteList>(r => r?.Id.ToString() ?? "Маршрутный лист не назначен");
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
	}
}

