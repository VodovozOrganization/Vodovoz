using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using QS.Dialog.GtkUI;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ReportsParameters;
using NHibernate.Transform;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Reports
{
	public partial class SalesReport : SingleUoWWidgetBase, IParametersWidget
	{
		SelectableParametersReportFilter filter;

		public SalesReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			filter = new SelectableParametersReportFilter(UoW);
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today;
			
			var nomenclatureTypeParam = filter.CreateParameterSet(
				"Типы номенклатур",
				"nomenclature_type",
				new ParametersEnumFactory<NomenclatureCategory>()
			);

			var nomenclatureParam = filter.CreateParameterSet(
				"Номенклатуры",
				"nomenclature",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<Nomenclature> resultAlias = null;
					var query = UoW.Session.QueryOver<Nomenclature>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							var filterCriterion = f();
							if(filterCriterion != null) {
								query.Where(filterCriterion);
							}
						}
					}

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.OfficialName).WithAlias(() => resultAlias.EntityTitle)
						);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Nomenclature>>());
					return query.List<SelectableParameter>();
				})
			);

			nomenclatureParam.AddFilterOnSourceSelectionChanged(nomenclatureTypeParam,
				() => {
					var selectedValues = nomenclatureTypeParam.GetSelectedValues();
					if(!selectedValues.Any()) {
						return null;
					}
					return Restrictions.On<Nomenclature>(x => x.Category).IsIn(nomenclatureTypeParam.GetSelectedValues().ToArray());
				}
			);

			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>().Fetch(SelectMode.Fetch, x => x.Childs).List();

			filter.CreateParameterSet(
				"Группы товаров",
				"product_group",
				new RecursiveParametersFactory<ProductGroup>(UoW,
				(filters) => {
					var query = UoW.Session.QueryOver<ProductGroup>();
					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							query.Where(f());
						}
					}
					return query.List();
				},
				x => x.Name,
				x => x.Childs)
			);

			filter.CreateParameterSet(
				"Контрагенты",
				"counterparty",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<Counterparty> resultAlias = null;
					var query = UoW.Session.QueryOver<Counterparty>()
							.Where(x => !x.IsArchive);
					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							query.Where(f());
						}
					}

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.FullName).WithAlias(() => resultAlias.EntityTitle)
						);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Counterparty>>());
					return query.List<SelectableParameter>();
				})
			);

			filter.CreateParameterSet(
				"Организации",
				"organization",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<Organization> resultAlias = null;
					var query = UoW.Session.QueryOver<Organization>();
					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							query.Where(f());
						}
					}

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.FullName).WithAlias(() => resultAlias.EntityTitle)
						);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Organization>>());
					return query.List<SelectableParameter>();
				})
			);

			filter.CreateParameterSet(
				"Основания скидок",
				"discount_reason",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<DiscountReason> resultAlias = null;
					var query = UoW.Session.QueryOver<DiscountReason>();
					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							query.Where(f());
						}
					}

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
						);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<DiscountReason>>());
					return query.List<SelectableParameter>();
				})
			);

			filter.CreateParameterSet(
				"Подразделения",
				"subdivision",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<Subdivision> resultAlias = null;
					var query = UoW.Session.QueryOver<Subdivision>();
					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							query.Where(f());
						}
					}

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
						);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Subdivision>>());
					return query.List<SelectableParameter>();
				})
			);

			filter.CreateParameterSet(
				"Авторы заказов",
				"order_author",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<Employee> resultAlias = null;
					var query = UoW.Session.QueryOver<Employee>();

					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							query.Where(f());
						}
					}

					IProjection authorProjection = Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(' ', ?2, ?1, ?3)"),
						NHibernateUtil.String,
						Projections.Property<Employee>(x => x.Name),
						Projections.Property<Employee>(x => x.LastName),
						Projections.Property<Employee>(x => x.Patronymic)
					);

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(authorProjection).WithAlias(() => resultAlias.EntityTitle)
						);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Employee>>());
					return query.List<SelectableParameter>();
				})
			);

			filter.CreateParameterSet(
				"Части города",
				"geographic_group",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<GeographicGroup> resultAlias = null;
					var query = UoW.Session.QueryOver<GeographicGroup>();

					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							query.Where(f());
						}
					}

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
						);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<GeographicGroup>>());
					return query.List<SelectableParameter>();
				})
			);

			filter.CreateParameterSet(
				"Тип оплаты",
				"payment_type",
				new ParametersEnumFactory<PaymentType>()
			);

			filter.CreateParameterSet(
				"Промонаборы",
				"promotional_set",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<PromotionalSet> resultAlias = null;
					var query = UoW.Session.QueryOver<PromotionalSet>()
							.Where(x => !x.IsArchive);
					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							query.Where(f());
						}
					}

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.Name).WithAlias(() => resultAlias.EntityTitle)
						);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<PromotionalSet>>());
					return query.List<SelectableParameter>();
				})
			);

			var viewModel = new SelectableParameterReportFilterViewModel(filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();
		}


		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по продажам";

		#endregion


		private int[] GetResultIds(IEnumerable<int> ids)
		{
			return ids.Any() ? ids.ToArray() : new int[] { 0 };
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDateOrNull },
				{ "end_date", dateperiodpicker.EndDateOrNull },
				{"creation_date", DateTime.Now}
			};
			foreach(var item in filter.GetParameters()) {
				parameters.Add(item.Key, item.Value);

			}

			return new ReportInfo {
				Identifier = ycheckbuttonDetail.Active ? "Sales.SalesReportDetail" : "Sales.SalesReport",
				Parameters = parameters
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDate != default(DateTime))
				OnUpdate(true);
			else
				MessageDialogHelper.RunWarningDialog("Заполните дату.");
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}
	}
}