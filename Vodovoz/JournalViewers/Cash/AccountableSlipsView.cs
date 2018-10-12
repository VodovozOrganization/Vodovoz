using System;
using System.Linq;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AccountableSlipsView : TdiTabBase
	{
		private IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				if (uow == value)
					return;
				uow = value;
				accountableslipfilter1.UoW = value;
				var vm = new ViewModel.AccountableSlipsVM (accountableslipfilter1);
				vm.ItemsListUpdated += Vm_ItemsListUpdated;
				representationSlips.RepresentationModel = vm;
				representationSlips.RepresentationModel.UpdateNodes ();
			}
		}

		void Vm_ItemsListUpdated (object sender, EventArgs e)
		{
			Calculate ();
		}

		public AccountableSlipsView(Employee accountable, ExpenseCategory expense): this()
		{
			if (accountable != null)
				accountableslipfilter1.SetAndRefilterAtOnce(x => x.RestrictAccountable = accountable);
			if (expense != null)
				accountableslipfilter1.SetAndRefilterAtOnce(x => x.RestrictExpenseCategory = expense);
		}

		public AccountableSlipsView ()
		{
			this.Build ();
			this.TabName = "Движения по подотчетным деньгам";
			accountableslipfilter1.Refiltered += Accountableslipfilter1_Refiltered;
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
		}

		void Accountableslipfilter1_Refiltered (object sender, EventArgs e)
		{
			if(accountableslipfilter1.RestrictAccountable == null)
				TabName = "Движения по подотчетным деньгам";
			else
				TabName = String.Format ("Движения по {0}", accountableslipfilter1.RestrictAccountable.ShortName);
		}

		void Calculate()
		{
			if (accountableslipfilter1.RestrictAccountable != null)
				labelCurrentDebt.LabelProp = String.Format ("Текущий долг: {0}",
					CurrencyWorks.GetShortCurrencyString (
						Repository.Cash.AccountableDebtsRepository.EmloyeeDebt (UoW, accountableslipfilter1.RestrictAccountable))
				);
			else
				labelCurrentDebt.LabelProp = String.Empty;
			
			decimal Recived = 0, Closed = 0;
			foreach(var node in representationSlips.RepresentationModel.ItemsList.Cast<ViewModel.AccountableSlipsVMNode> ())
			{
				Recived += node.Append;
				Closed += node.Removed;
			}
			labelRecived.LabelProp = String.Format ("Получено: {0}",
				CurrencyWorks.GetShortCurrencyString (Recived));
			labelClosed.LabelProp = String.Format ("Закрыто: {0}",
				CurrencyWorks.GetShortCurrencyString (Closed));
		}
	}
}

