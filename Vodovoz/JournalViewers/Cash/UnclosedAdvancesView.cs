using System;
using QS.DomainModel.UoW;
using QS.Permissions;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	[Obsolete("Старый диалог, требует обновления кода")]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class UnclosedAdvancesView : QS.Dialog.Gtk.TdiTabBase
	{
		private IUnitOfWork _uow;

		public IUnitOfWork UoW
		{
			get
			{
				return _uow;
			}
			set
			{
				if (_uow == value)
				{
					return;
				}

				_uow = value;
				unclosedadvancesfilter1.UoW = value;
				var vm = new ViewModel.UnclosedAdvancesVM(unclosedadvancesfilter1);

				representationUnclosed.RepresentationModel = vm;
				representationUnclosed.RepresentationModel.UpdateNodes();
			}
		}

		public bool? UseSlider => null;

		public UnclosedAdvancesView(Employee accountable, ExpenseCategory expense) : this()
		{
			if(accountable != null)
			{
				unclosedadvancesfilter1.SetAndRefilterAtOnce(x => x.RestrictAccountable = accountable);
			}

			if(expense != null)
			{
				unclosedadvancesfilter1.SetAndRefilterAtOnce(x => x.RestrictExpenseCategory = expense);
			}
		}

		public UnclosedAdvancesView()
		{
			Build();
			TabName = "Незакрытые авансы";
			unclosedadvancesfilter1.Refiltered += Accountableslipfilter1_Refiltered;
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			unclosedadvancesfilter1.UoW = UoW;
			representationUnclosed.Selection.Changed += RepresentationUnclosed_Selection_Changed;
		}

		void RepresentationUnclosed_Selection_Changed(object sender, EventArgs e)
		{
			buttonClose.Sensitive = buttonReturn.Sensitive = representationUnclosed.Selection.CountSelectedRows() > 0;
		}

		void Accountableslipfilter1_Refiltered(object sender, EventArgs e)
		{
			TabName = unclosedadvancesfilter1.RestrictAccountable == null
					? "Незакрытые авансы"
					: $"Незакрытые авансы по { unclosedadvancesfilter1.RestrictAccountable.ShortName }";
		}

		protected void OnButtonReturnClicked(object sender, EventArgs e)
		{
			var expense = UoW.GetById<Expense>(representationUnclosed.GetSelectedId());

			var dlg = new CashIncomeDlg(expense);
			OpenNewTab(dlg);
		}

		protected void OnButtonCloseClicked(object sender, EventArgs e)
		{
			var expense = UoW.GetById<Expense>(representationUnclosed.GetSelectedId());

			var dlg = new AdvanceReportDlg(expense);
			OpenNewTab(dlg);
		}

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}
	}
}

