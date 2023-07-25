using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Services;
using QS.Report;
using QSReport;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.Reports
{
	public partial class SalesReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly SelectableParametersReportFilter _filter;
		private readonly bool _userIsSalesRepresentative;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IInteractiveService _interactiveService;
		private readonly bool _canSeePhones;

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public string Title => "Отчет по продажам";

		public SalesReport(IEmployeeRepository employeeRepository, IInteractiveService interactiveService)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			_filter = new SelectableParametersReportFilter(UoW);

			_userIsSalesRepresentative =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_is_sales_representative")
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin;

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

			UpdatePaymentTypeParameters(ref parameters);

			return new ReportInfo
			{
				Identifier = ycheckbuttonDetail.Active ? "Sales.SalesReportDetail" : "Sales.SalesReport",
				Parameters = parameters
			};
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

		private void UpdatePaymentTypeParameters(ref Dictionary<string, object> parameters)
		{
			parameters.Add("payment_terminal_subtype_include", new string[] { "0" });
			parameters.Add("payment_terminal_subtype_exclude", new string[] { "0" });

			if(parameters["payment_type_include"] is IEnumerable<object> includedPaymentTypesObject)
			{
				var selectedPaymentTypes = 
					includedPaymentTypesObject
					.Where(x => x is CombinedPaymentNode)
					.Select(x => (CombinedPaymentNode)x)
					.ToList();

				if(selectedPaymentTypes.Count > 0)
				{
					var topLevelSelectedPaymmentTypes = CombinedPaymentNode.GetTopLevelPaymentTypesNames(selectedPaymentTypes);

					parameters["payment_type_include"] =
						(topLevelSelectedPaymmentTypes.Length > 0)
						? topLevelSelectedPaymmentTypes
						: new string[] { "0" };

					var selectedTerminalSubtypes = CombinedPaymentNode.GetTerminalPaymentTypeSubtypesNames(selectedPaymentTypes);

					if(selectedTerminalSubtypes.Length > 0)
					{
						parameters["payment_terminal_subtype_include"] = selectedTerminalSubtypes;
					}
				}
			}

			if(parameters["payment_type_exclude"] is IEnumerable<object> excludedPaymentTypesObject)
			{
				var selectedPaymentTypes =
					excludedPaymentTypesObject
					.Where(x => x is CombinedPaymentNode)
					.Select(x => (CombinedPaymentNode)x)
					.ToList();

				if(selectedPaymentTypes.Count > 0)
				{
					var topLevelSelectedPaymmentTypes = CombinedPaymentNode.GetTopLevelPaymentTypesNames(selectedPaymentTypes);

					parameters["payment_type_exclude"] =
						(topLevelSelectedPaymmentTypes.Length > 0)
						? topLevelSelectedPaymmentTypes
						: new string[] { "0" };

					var selectedTerminalSubtypes = CombinedPaymentNode.GetTerminalPaymentTypeSubtypesNames(selectedPaymentTypes);

					if(selectedTerminalSubtypes.Length > 0)
					{
						parameters["payment_terminal_subtype_exclude"] = selectedTerminalSubtypes;
					}
				}
			}
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

			ProductGroup productGroupChildAlias = null;
			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>()
				.Left.JoinAlias(p => p.Childs,
					() => productGroupChildAlias,
					() => !productGroupChildAlias.IsArchive)
				.Fetch(SelectMode.Fetch, () => productGroupChildAlias)
				.List();

			_filter.CreateParameterSet(
				"Группы товаров",
				"product_group",
				new RecursiveParametersFactory<ProductGroup>(UoW,
					(filters) =>
					{
						var query = UoW.Session.QueryOver<ProductGroup>()
							.Where(p => p.Parent == null)
							.And(p => !p.IsArchive);
						
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
					SelectableEntityParameter<GeoGroup> resultAlias = null;
					var query = UoW.Session.QueryOver<GeoGroup>();

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
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<GeoGroup>>());
					return query.List<SelectableParameter>();
				})
			);

			_filter.CreateParameterSet(
				"Тип оплаты",
				"payment_type",
				new RecursiveParametersFactory<CombinedPaymentNode>(
					UoW,
					(filters) =>
					{
						var paymentByTerminalSources = Enum.GetValues(typeof(PaymentByTerminalSource)).Cast<PaymentByTerminalSource>();
						var paymentByTerminalSourceNodes = paymentByTerminalSources.Select(x =>
							new CombinedPaymentNode
							{
								Id = (int)x,
								Title = x.GetEnumTitle(),
								PaymentType = PaymentType.Terminal,
								PaymentSubType = x.ToString(),
								TopLevelId = (int)PaymentType.Terminal
							})
						.ToList();

						var paymentTypeList = Enum.GetValues(typeof(PaymentType)).Cast<PaymentType>();
						var combinedNodesTypesWithChildFroms = paymentTypeList.Select(x =>
							new CombinedPaymentNode
							{
								Id = (int)x,
								Title = x.GetEnumTitle(),
								PaymentType = x,
								IsTopLevel = true
							})
						.ToList();

						combinedNodesTypesWithChildFroms.Single(x => x.PaymentType == PaymentType.Terminal).Childs = paymentByTerminalSourceNodes;

						return combinedNodesTypesWithChildFroms;
					},
					x => x.Title,
					x => x.Childs,
					true)
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
