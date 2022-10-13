using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Report.ViewModels;
using QS.Services;
using QS.ViewModels.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.ReportsParameters.Profitability
{
	public class ProfitabilitySalesReportViewModel : ReportParametersViewModelBase
	{
		private Dictionary<string, object> _parameters = new Dictionary<string, object>();
		private readonly SelectableParametersReportFilter _filter;
		private SelectableParameterReportFilterViewModel _filterViewModel;
		private LeftRightListViewModel<GroupingNode> _groupViewModel;
		private readonly bool _userIsSalesRepresentative;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _uow;
		private DelegateCommand _loadReportCommand;
		private DelegateCommand _showInfoCommand;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _showPhones;
		private readonly bool _canSeePhones;
		private bool _isDetailed;

		public ProfitabilitySalesReportViewModel(RdlViewerViewModel rdlViewerViewModel, IEmployeeRepository employeeRepository, ICommonServices commonServices) : base(rdlViewerViewModel)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_interactiveService = commonServices.InteractiveService;

			Title = "Отчет по продажам с рентабельностью";
			Identifier = "Sales.SalesReport";

			_uow = UnitOfWorkFactory.CreateWithoutRoot();
			_filter = new SelectableParametersReportFilter(_uow);

			_userIsSalesRepresentative =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("user_is_sales_representative")
				&& !_commonServices.UserService.GetCurrentUser(_uow).IsAdmin;

			_canSeePhones = _commonServices.CurrentPermissionService.ValidatePresetPermission("phones_in_detailed_sales_report");

			StartDate = DateTime.Now;
			EndDate = DateTime.Now;

			SetupFilter();

			var groupingNodes = GetGroupingNodes();
			LeftRightListViewModel<GroupingNode> leftRightListViewModel = new LeftRightListViewModel<GroupingNode>();
			leftRightListViewModel.LeftLabel = "Доступные группировки";
			leftRightListViewModel.RightLabel = "Выбранные группировки";
			leftRightListViewModel.SetLeftItems(groupingNodes, x => x.Name);
			GroupingSelectViewModel = leftRightListViewModel;

			IEnumerable<GroupingNode> selectedItems = GroupingSelectViewModel.GetRightItems();
		}

		

		protected override Dictionary<string, object> Parameters => _parameters;

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public virtual bool ShowPhones
		{
			get => _showPhones;
			set => SetField(ref _showPhones, value);
		}

		public virtual bool CanEditShowPhones => _canSeePhones && IsDetailed;

		public virtual bool IsDetailed
		{
			get => _isDetailed;
			set
			{
				if(SetField(ref _isDetailed, value))
				{
					OnPropertyChanged(nameof(CanEditShowPhones));
				}
			}
		}

		public virtual SelectableParameterReportFilterViewModel FilterViewModel
		{
			get => _filterViewModel;
			set => SetField(ref _filterViewModel, value);
		}

		public virtual LeftRightListViewModel<GroupingNode> GroupingSelectViewModel
		{
			get => _groupViewModel;
			set => SetField(ref _groupViewModel, value);
		}

		private IEnumerable<GroupingNode> GetGroupingNodes()
		{
			return new[] { 
				new GroupingNode{ Name = "Заказ", GroupFieldName = "" },
				new GroupingNode{ Name = "Контрагент", GroupFieldName = "" },
				new GroupingNode{ Name = "Подразделение", GroupFieldName = "" },
				new GroupingNode{ Name = "Дата доставки", GroupFieldName = "" },
				new GroupingNode{ Name = "Маршрутный лист", GroupFieldName = "" },
				new GroupingNode{ Name = "Номенклатура", GroupFieldName = "" },
				new GroupingNode{ Name = "Группа уровень 1", GroupFieldName = "" },
				new GroupingNode{ Name = "Группа уровень 2", GroupFieldName = "" },
				new GroupingNode{ Name = "Группа уровень 3", GroupFieldName = "" }
			};
		}

		public class GroupingNode
		{
			public string Name { get; set; }
			public string GroupFieldName { get; set; }
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
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<Nomenclature> resultAlias = null;
					var query = _uow.Session.QueryOver<Nomenclature>()
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
			_uow.Session.QueryOver<ProductGroup>().Fetch(SelectMode.Fetch, x => x.Childs).List();

			_filter.CreateParameterSet(
				"Группы товаров",
				"product_group",
				new RecursiveParametersFactory<ProductGroup>(_uow,
					(filters) =>
					{
						var query = _uow.Session.QueryOver<ProductGroup>()
							.Where(p => p.Parent == null);

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
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<Counterparty> resultAlias = null;
					var query = _uow.Session.QueryOver<Counterparty>()
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
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<Organization> resultAlias = null;
					var query = _uow.Session.QueryOver<Organization>();
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
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<DiscountReason> resultAlias = null;
					var query = _uow.Session.QueryOver<DiscountReason>();
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
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<Subdivision> resultAlias = null;
					var query = _uow.Session.QueryOver<Subdivision>();
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
					new ParametersFactory(_uow, (filters) =>
					{
						SelectableEntityParameter<Employee> resultAlias = null;
						var query = _uow.Session.QueryOver<Employee>();

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
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<GeoGroup> resultAlias = null;
					var query = _uow.Session.QueryOver<GeoGroup>();

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
				new ParametersEnumFactory<PaymentType>()
			);

			_filter.CreateParameterSet(
				"Промонаборы",
				"promotional_set",
				new ParametersFactory(_uow, (filters) =>
				{
					SelectableEntityParameter<PromotionalSet> resultAlias = null;
					var query = _uow.Session.QueryOver<PromotionalSet>()
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

			FilterViewModel = new SelectableParameterReportFilterViewModel(_filter);
		}

		public DelegateCommand LoadReportCommand
		{
			get
			{
				if(_loadReportCommand == null)
				{
					_loadReportCommand = new DelegateCommand(GenerateReport);
				}
				return _loadReportCommand;
			}
		}

		private void GenerateReport()
		{
			if(StartDate == null || StartDate == default(DateTime))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Заполните дату.");
			}

			_parameters = new Dictionary<string, object>
			{
				{ "start_date", StartDate },
				{ "end_date", EndDate },
				{ "creation_date", DateTime.Now },
				{ "show_phones", ShowPhones },
			};

			if(_userIsSalesRepresentative)
			{
				var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(_uow);

				_parameters.Add("order_author_include", new[] { currentEmployee.Id.ToString() });
				_parameters.Add("order_author_exclude", new[] { "0" });
			}

			foreach(var item in _filter.GetParameters())
			{
				_parameters.Add(item.Key, item.Value);
			}

			Identifier = IsDetailed ? "Sales.SalesReportDetail" : "Sales.SalesReport";

			LoadReport();
		}

		public DelegateCommand ShowInfoCommand
		{
			get
			{
				if(_showInfoCommand == null)
				{
					_showInfoCommand = new DelegateCommand(ShowInfo);
				}
				return _showInfoCommand;
			}
		}

		private void ShowInfo()
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
	}
}
