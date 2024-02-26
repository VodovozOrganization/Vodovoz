using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using System;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz
{
	[Obsolete("Старый диалог, требует обновления кода")]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class UnclosedAdvancesView : QS.Dialog.Gtk.TdiTabBase
	{
		private IUnitOfWork _uow;
		private readonly ILogger<UnclosedAdvancesView> _logger;

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

		INavigationManager NavigationManager { get; }

		public bool? UseSlider => null;

		public UnclosedAdvancesView(
			Employee accountable,
			FinancialExpenseCategory financialExpenseCategory,
			INavigationManager navigationManager)
			: this(navigationManager)
		{
			if(accountable != null)
			{
				unclosedadvancesfilter1.SetAndRefilterAtOnce(x => x.RestrictAccountable = accountable);
			}

			if(financialExpenseCategory != null)
			{
				unclosedadvancesfilter1.SetAndRefilterAtOnce(x => x.FinancialExpenseCategory = financialExpenseCategory);
			}
		}

		public UnclosedAdvancesView(
			INavigationManager navigationManager)
		{
			Build();
			TabName = "Незакрытые авансы";
			unclosedadvancesfilter1.Refiltered += Accountableslipfilter1_Refiltered;
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			unclosedadvancesfilter1.UoW = UoW;
			representationUnclosed.Selection.Changed += RepresentationUnclosed_Selection_Changed;
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			unclosedadvancesfilter1.JournalTab = this;
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
			var page = NavigationManager.OpenViewModel<IncomeViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForCreate());

			page.ViewModel.ConfigureForReturn(representationUnclosed.GetSelectedId());
		}

		protected void OnButtonCloseClicked(object sender, EventArgs e)
		{
			var page = NavigationManager.OpenViewModel<AdvanceReportViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForCreate());

			page.ViewModel.ConfigureForReturn(representationUnclosed.GetSelectedId());
		}

		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}
	}
}

