using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSProjectsLib;
using System;
using System.Linq;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AccountableSlipsView : QS.Dialog.Gtk.TdiTabBase
	{
		private readonly IAccountableDebtsRepository _accountableDebtsRepository = ScopeProvider.Scope.Resolve<IAccountableDebtsRepository>();
		private IUnitOfWork _uow;

		public IUnitOfWork UoW
		{
			get => _uow;
			set
			{
				if(_uow == value)
				{
					return;
				}

				_uow = value;
				accountableslipfilter1.UoW = value;
				var vm = new ViewModel.AccountableSlipsVM(accountableslipfilter1);
				vm.ItemsListUpdated += Vm_ItemsListUpdated;
				representationSlips.RepresentationModel = vm;
				representationSlips.RepresentationModel.UpdateNodes();
			}
		}

		private void Vm_ItemsListUpdated(object sender, EventArgs e)
		{
			Calculate();
		}

		public AccountableSlipsView(
			Employee accountable,
			FinancialExpenseCategory financialExpenseCategory) : this()
		{
			if(accountable != null)
			{
				accountableslipfilter1.SetAndRefilterAtOnce(x => x.RestrictAccountable = accountable);
			}

			if(financialExpenseCategory != null)
			{
				accountableslipfilter1.SetAndRefilterAtOnce(x => x.FinancialExpenseCategory = financialExpenseCategory);
			}
		}

		public AccountableSlipsView()
		{
			Build();
			TabName = "Движения по подотчетным деньгам";
			accountableslipfilter1.Refiltered += Accountableslipfilter1_Refiltered;
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			accountableslipfilter1.JournalTab = this;
		}

		private void Accountableslipfilter1_Refiltered(object sender, EventArgs e)
		{
			if(accountableslipfilter1.RestrictAccountable == null)
			{
				TabName = "Движения по подотчетным деньгам";
			}
			else
			{
				TabName = $"Движения по {accountableslipfilter1.RestrictAccountable.ShortName}";
			}
		}

		private void Calculate()
		{
			if(accountableslipfilter1.RestrictAccountable != null)
			{
				labelCurrentDebt.LabelProp = $"Текущий долг: {CurrencyWorks.GetShortCurrencyString(_accountableDebtsRepository.EmployeeDebt(UoW, accountableslipfilter1.RestrictAccountable))}";
			}
			else
			{
				labelCurrentDebt.LabelProp = string.Empty;
			}

			decimal Recived = 0, Closed = 0;
			foreach(var node in representationSlips.RepresentationModel.ItemsList.Cast<ViewModel.AccountableSlipsVMNode>())
			{
				Recived += node.Append;
				Closed += node.Removed;
			}
			labelRecived.LabelProp = $"Получено: {CurrencyWorks.GetShortCurrencyString(Recived)}";
			labelClosed.LabelProp = $"Закрыто: {CurrencyWorks.GetShortCurrencyString(Closed)}";
		}
	}
}

