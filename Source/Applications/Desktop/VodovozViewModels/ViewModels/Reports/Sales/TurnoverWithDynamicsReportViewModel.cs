using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using VodovozCounterparty = Vodovoz.Domain.Client.Counterparty;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel : DialogTabViewModelBase
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly string _templatePath = @".\Reports\Sales\TurnoverWithDynamicsReport.xlsx";
		private readonly SelectableParametersReportFilter _filter;
		private readonly bool _userIsSalesRepresentative;
		private SelectableParameterReportFilterViewModel _filterViewModel;
		private DelegateCommand _loadReportCommand;
		private DelegateCommand _showInfoCommand;
		private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
		private DateTime? _startDate;
		private DateTime? _endDate;
		private bool _showDynamics;
		private string _slice;
		private string _measurementUnit;
		private string _dynamicsIn;
		private bool _showLastSale;
		private TurnoverWithDynamicsReport _report;

		public TurnoverWithDynamicsReportViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, ICommonServices commonServices, INavigationManager navigation, IEmployeeRepository employeeRepository)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_interactiveService = commonServices.InteractiveService;

			Title = "Отчет по оборачиваемости с динамикой";

			_unitOfWork = UnitOfWorkFactory.CreateWithoutRoot();
			_filter = new SelectableParametersReportFilter(_unitOfWork);

			_userIsSalesRepresentative =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("user_is_sales_representative")
				&& !_commonServices.UserService.GetCurrentUser(_unitOfWork).IsAdmin;

			StartDate = DateTime.Now.Date.AddDays(-6);
			EndDate = DateTime.Now.Date;

			ConfigureFilter();
		}

		public virtual SelectableParameterReportFilterViewModel FilterViewModel
		{
			get => _filterViewModel;
			set => SetField(ref _filterViewModel, value);
		}

		protected Dictionary<string, object> Parameters => _parameters;

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

		public bool ShowDynamics
		{
			get => _showDynamics;
			set => SetField(ref _showDynamics, value);
		}

		public string MeasurementUnit
		{
			get => _measurementUnit;
			set => SetField(ref _measurementUnit, value);
		}

		public string SlicingType
		{
			get => _slice;
			set => SetField(ref _slice, value);
		}

		public string DynamicsIn
		{
			get => _dynamicsIn;
			set => SetField(ref _dynamicsIn, value);
		}

		public bool ShowLastSale
		{
			get => _showLastSale;
			set => SetField(ref _showLastSale, value);
		}

		public DelegateCommand LoadReportCommand
		{
			get
			{
				if(_loadReportCommand is null)
				{
					_loadReportCommand = new DelegateCommand(GenerateReport);
				}
				return _loadReportCommand;
			}
		}

		public DelegateCommand ShowInfoCommand
		{
			get
			{
				if(_showInfoCommand is null)
				{
					_showInfoCommand = new DelegateCommand(ShowInfo);
				}
				return _showInfoCommand;
			}
		}

		private void ConfigureFilter()
		{
			var nomenclatureTypeParam = _filter.CreateParameterSet(
				"Типы номенклатур",
				"nomenclature_type",
				new ParametersEnumFactory<NomenclatureCategory>());

			var nomenclatureParam = _filter.CreateParameterSet(
				"Номенклатуры",
				"nomenclature",
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<Nomenclature> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<Nomenclature>()
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
				}));

			nomenclatureParam.AddFilterOnSourceSelectionChanged(nomenclatureTypeParam,
				() =>
				{
					var selectedValues = nomenclatureTypeParam.GetSelectedValues().ToArray();
					return !selectedValues.Any()
						? null
						: nomenclatureTypeParam.FilterType == SelectableFilterType.Include
							? Restrictions.On<Nomenclature>(x => x.Category).IsIn(selectedValues)
							: Restrictions.On<Nomenclature>(x => x.Category).Not.IsIn(selectedValues);
				});

			//Предзагрузка. Для избежания ленивой загрузки
			_unitOfWork.Session.QueryOver<ProductGroup>().Fetch(SelectMode.Fetch, x => x.Childs).List();

			_filter.CreateParameterSet(
				"Группы товаров",
				"product_group",
				new RecursiveParametersFactory<ProductGroup>(
					_unitOfWork,
					(filters) =>
					{
						var query = _unitOfWork.Session.QueryOver<ProductGroup>()
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
					x => x.Childs));

			_filter.CreateParameterSet(
				"Контрагенты",
				"counterparty",
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<VodovozCounterparty> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<VodovozCounterparty>()
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
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<VodovozCounterparty>>());
					return query.List<SelectableParameter>();
				}));

			_filter.CreateParameterSet(
				"Организации",
				"organization",
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<Organization> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<Organization>();
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
				}));

			_filter.CreateParameterSet(
				"Основания скидок",
				"discount_reason",
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<DiscountReason> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<DiscountReason>();
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
				}));

			_filter.CreateParameterSet(
				"Подразделения",
				"subdivision",
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<Subdivision> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<Subdivision>();
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
				}));

			if(!_userIsSalesRepresentative)
			{
				_filter.CreateParameterSet(
					"Авторы заказов",
					"order_author",
					new ParametersFactory(_unitOfWork, (filters) =>
					{
						SelectableEntityParameter<Employee> resultAlias = null;
						var query = _unitOfWork.Session.QueryOver<Employee>();

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
					}));
			}

			_filter.CreateParameterSet(
				"Части города",
				"geographic_group",
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<GeoGroup> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<GeoGroup>();

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
				}));

			_filter.CreateParameterSet(
				"Тип оплаты",
				"payment_type",
				new ParametersEnumFactory<PaymentType>()
			);

			_filter.CreateParameterSet(
				"Промонаборы",
				"promotional_set",
				new ParametersFactory(_unitOfWork, (filters) =>
				{
					SelectableEntityParameter<PromotionalSet> resultAlias = null;
					var query = _unitOfWork.Session.QueryOver<PromotionalSet>()
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
				}));

			FilterViewModel = new SelectableParameterReportFilterViewModel(_filter);
		}

		private void ShowInfo()
		{
			throw new NotImplementedException();
		}

		private void GenerateReport()
		{
			if(!ValidateParameters())
			{
				return;
			}

			_report = TurnoverWithDynamicsReport.Make(StartDate.Value, EndDate.Value, SlicingType);
		}

		private bool ValidateParameters()
		{
			if(StartDate == null || StartDate == default(DateTime)
			|| EndDate == null || EndDate == default(DateTime))
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Заполните дату.");
				return false;
			}

			var deltaTime = EndDate - StartDate;

			if(SlicingType == SliceValues.Day && deltaTime?.TotalDays >= 62)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Для разреза день нельзя выбрать интервал более 62х дней");
				return false;
			}

			if(SlicingType == SliceValues.Week
			&& StartDate?.DayOfWeek == DayOfWeek.Monday ? deltaTime?.TotalDays / 7 >= 54 : deltaTime?.TotalDays / 7 > 54)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Для разреза неделя нельзя выбрать интервал более 54х недель");
				return false;
			}

			var monthBetweenDates = 0;
			for(DateTime monthDate = StartDate.Value; monthDate < EndDate; monthDate = monthDate.AddMonths(1))
			{
				monthBetweenDates++;
			}

			if(SlicingType == SliceValues.Month
			&& StartDate?.Day == 1 ? monthBetweenDates >= 60 : monthBetweenDates > 60)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Для разреза месяц нельзя выбрать интервал более 60х месяцев");
				return false;
			}

			return true;
		}
	}
}
