﻿using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
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
using QS.Dialog;
using QS.Project.DB;
using Vodovoz.Domain.Organizations;
using QS.Project.Services;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Reports
{
	public partial class SalesReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly SelectableParametersReportFilter _filter;
		private readonly bool _userIsSalesRepresentative;
		private readonly ReportFactory _reportFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly bool _canSeePhones;

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public string Title => "Отчет по продажам";

		public SalesReport(ReportFactory reportFactory, IEmployeeRepository employeeRepository, IInteractiveService interactiveService)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			_filter = new SelectableParametersReportFilter(UoW);

			_userIsSalesRepresentative =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_is_sales_representative")
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser(UoW).IsAdmin;

			_canSeePhones = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("phones_in_detailed_sales_report");

			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today;
			buttonInfo.Clicked += (sender, args) => ShowInfoWindow();

			ycheckbuttonDetail.Toggled += (sender, args) =>
			{
				ycheckbuttonPhones.Sensitive = _canSeePhones && ycheckbuttonDetail.Active;
			};

			SetupFilter();
		}

		private void ShowInfoWindow()
		{
			var info =
				"<b>1.</b> Подсчет продаж ведется на основе заказов. В отчете учитываются заказы со статусами:" +
				$"\n\t'{OrderStatus.Accepted.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.InTravelList.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.OnLoading.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.OnTheWay.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.Shipped.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.UnloadingOnStock.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.Closed.GetEnumTitle()}'" +
				$"\n\t'{OrderStatus.WaitForPayment.GetEnumTitle()}' и заказ - самовывоз с оплатой после отгрузки." +
				"\nВ отчет <b>не попадают</b> заказы, являющиеся закрывашками по контракту." +
				"\nФильтр по дате отсекает заказы, если дата доставки не входит в выбранный период." +

				"\n\n<b>2.</b> Подсчет тары ведется следующим образом:" +
				"\n\tПлановое значение - сумма бутылей на возврат попавших в отчет заказов;" +
				"\n\tФактическое значение - сумма фактически возвращенных бутылей по адресам маршрутного листа." +
				"\n\t\tФактическое значение возвращенных бутылей по адресу зависит от того, доставлен<b>(*)</b> заказ или нет:" +
				"\n\t\t\t <b>-</b> Если да - берется кол-во бутылей, которое по факту забрал водитель. " +
				"Это кол-во может быть вручную указано при закрытии МЛ;" +

				"\n\t\t\t <b>-</b> Если не доставлен - берется кол-во бутылей на возврат из заказа;" +
				"\n\t\t\t <b>-</b> Если заказ является самовывозом - берется значение возвращенной тары, указанное в отпуске самовывоза;" +
				$"\n\t\t <b>*</b> Заказ считается доставленным, если его статус в МЛ: '{RouteListItemStatus.Completed.GetEnumTitle()}' или " +
				$"'{RouteListItemStatus.EnRoute.GetEnumTitle()}' и статус МЛ '{RouteListStatus.Closed.GetEnumTitle()}' " +
				$"или '{RouteListStatus.OnClosing.GetEnumTitle()}'." +
				"\n\nДетальный отчет аналогичен обычному, лишь предоставляет расширенную информацию.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Справка по работе с отчетом");
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDateOrNull },
				{ "end_date", dateperiodpicker.EndDateOrNull },
				{ "creation_date", DateTime.Now },
				{ "show_phones", ycheckbuttonPhones.Active },
			};

			if(_userIsSalesRepresentative)
			{
				var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);

				parameters.Add("order_author_include", new[] { currentEmployee.Id.ToString() });
				parameters.Add("order_author_exclude", new[] { "0" });
			}

			foreach(var item in _filter.GetParameters())
			{
				parameters.Add(item.Key, item.Value);
			}

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = ycheckbuttonDetail.Active ? "Sales.SalesReportDetail" : "Sales.SalesReport";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			if(dateperiodpicker.StartDate != default(DateTime))
			{
				OnUpdate(true);
			}
			else
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Заполните дату.");
			}
		}

		private void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		private void SetupFilter()
		{
			var nomenclatureTypeParam = _filter.CreateParameterSet(
				"Типы номенклатур",
				"nomenclature_type",
				new ParametersEnumFactory<NomenclatureCategory>()
			);

			var nomenclatureParam = _filter.CreateParameterSet(
				"Номенклатуры",
				"nomenclature",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Nomenclature> resultAlias = null;
					var query = UoW.Session.QueryOver<Nomenclature>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							var filterCriterion = f();
							if(filterCriterion != null)
							{
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
				() =>
				{
					var selectedValues = nomenclatureTypeParam.GetSelectedValues().ToArray();
					return !selectedValues.Any()
						? null
						: nomenclatureTypeParam.FilterType == SelectableFilterType.Include
							? Restrictions.On<Nomenclature>(x => x.Category).IsIn(selectedValues)
							: Restrictions.On<Nomenclature>(x => x.Category).Not.IsIn(selectedValues);
				}
			);

			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>().Fetch(SelectMode.Fetch, x => x.Childs).List();

			_filter.CreateParameterSet(
				"Группы товаров",
				"product_group",
				new RecursiveParametersFactory<ProductGroup>(UoW,
					(filters) =>
					{
						var query = UoW.Session.QueryOver<ProductGroup>();
						if(filters != null && filters.Any())
						{
							foreach(var f in filters)
							{
								query.Where(f());
							}
						}

						return query.List();
					},
					x => x.Name,
					x => x.Childs)
			);

			_filter.CreateParameterSet(
				"Контрагенты",
				"counterparty",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Counterparty> resultAlias = null;
					var query = UoW.Session.QueryOver<Counterparty>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
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

			_filter.CreateParameterSet(
				"Организации",
				"organization",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Organization> resultAlias = null;
					var query = UoW.Session.QueryOver<Organization>();
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
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

			_filter.CreateParameterSet(
				"Основания скидок",
				"discount_reason",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<DiscountReason> resultAlias = null;
					var query = UoW.Session.QueryOver<DiscountReason>();
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
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

			_filter.CreateParameterSet(
				"Подразделения",
				"subdivision",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Subdivision> resultAlias = null;
					var query = UoW.Session.QueryOver<Subdivision>();
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
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

			if(!_userIsSalesRepresentative)
			{
				_filter.CreateParameterSet(
					"Авторы заказов",
					"order_author",
					new ParametersFactory(UoW, (filters) =>
					{
						SelectableEntityParameter<Employee> resultAlias = null;
						var query = UoW.Session.QueryOver<Employee>();

						if(filters != null && filters.Any())
						{
							foreach(var f in filters)
							{
								query.Where(f());
							}
						}

						var authorProjection = CustomProjections.Concat_WS(
							" ",
							Projections.Property<Employee>(x => x.LastName),
							Projections.Property<Employee>(x => x.Name),
							Projections.Property<Employee>(x => x.Patronymic)
						);

						query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(authorProjection).WithAlias(() => resultAlias.EntityTitle)
						);
						query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Employee>>());
						var paremetersSet = query.List<SelectableParameter>();

						return paremetersSet;
					})
				);
			}

			_filter.CreateParameterSet(
				"Части города",
				"geographic_group",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<GeographicGroup> resultAlias = null;
					var query = UoW.Session.QueryOver<GeographicGroup>();

					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
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

			_filter.CreateParameterSet(
				"Тип оплаты",
				"payment_type",
				new ParametersEnumFactory<PaymentType>()
			);

			_filter.CreateParameterSet(
				"Промонаборы",
				"promotional_set",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<PromotionalSet> resultAlias = null;
					var query = UoW.Session.QueryOver<PromotionalSet>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
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

			var viewModel = new SelectableParameterReportFilterViewModel(_filter);
			var filterWidget = new SelectableParameterReportFilterView(viewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();
		}
	}
}
