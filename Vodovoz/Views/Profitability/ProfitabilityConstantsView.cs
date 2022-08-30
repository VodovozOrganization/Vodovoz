using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using NHibernate;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewWidgets.Profitability;
using Vodovoz.ViewModels.Widgets;
using Gtk;
using Vodovoz.ViewModels.Widgets.Profitability;
using System;

namespace Vodovoz.Views.Profitability
{
	public partial class ProfitabilityConstantsView : TdiTabBase
	{
		IUnitOfWork _uow;
		public ProfitabilityConstantsView()
		{
			Build();
			_uow = UnitOfWorkFactory.CreateWithoutRoot();
			Configure();
		}

		private void Configure()
		{
			ConfigureChkButtons();
			var monthPicker = new MonthPickerView(new MonthPickerViewModel(DateTime.Today));
			monthPicker.Show();
			hboxMonth.Add(monthPicker);

			_uow.Session.QueryOver<ProductGroup>().Fetch(SelectMode.Fetch, x => x.Childs).List();

			var hboxProfitabilityFilters = new HBox();
			var adminExpensesProductGroupsFilter =
				new SelectableParametersFilterView(new SelectableParametersFilterViewModel(
					new RecursiveParametersFactory<ProductGroup>(_uow,
					(filters) =>
					{
						var query = _uow.Session.QueryOver<ProductGroup>();
						return query.List();
					},
					x => x.Name,
					x => x.Childs)));

			adminExpensesProductGroupsFilter.Show();
			hboxProfitabilityFilters.Add(adminExpensesProductGroupsFilter);
			panelFilters.Panel = hboxProfitabilityFilters;
			panelFilters.IsHided = true;
			panelFilters.WidthRequest = 350;
		}

		private void ConfigureChkButtons()
		{

		}
	}
}
