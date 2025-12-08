using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DateTimeHelpers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using QS.ViewModels.Widgets;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.NHibernateProjections.Goods;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters;
using Vodovoz.Reports.Editing.Modifiers;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Order = Vodovoz.Domain.Orders.Order;
using ExcelExporter = Vodovoz.ViewModels.ViewModels.Reports.WageCalculation.CallCenterMotivation.CallCenterMotivationReport.ExcelExporter;

namespace Vodovoz.ViewModels.ViewModels.Reports.WageCalculation.CallCenterMotivation
{
	public class CallCenterMotivationReportViewModel : DialogTabViewModelBase
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private DateTimeSliceType _dateSlicingType;
		private bool _showDynamics;
		private CallCenterMotivationReport _report;
		private bool _canSave;
		private bool _isGenerating;
		private IEnumerable<string> _lastGenerationErrors = Enumerable.Empty<string>();
		private string _saveProgressText;

		public CallCenterMotivationReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IIncludeExcludeSalesFilterFactory includeExcludeSalesFilterFactory,
			ILeftRightListViewModelFactory leftRightListViewModelFactory,
			IFileDialogService fileDialogService,
			IGuiDispatcher guiDispatcher)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			if(includeExcludeSalesFilterFactory == null)
			{
				throw new ArgumentNullException(nameof(includeExcludeSalesFilterFactory));
			}

			if(leftRightListViewModelFactory == null)
			{
				throw new ArgumentNullException(nameof(leftRightListViewModelFactory));
			}

			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			Title = "Отчет по мотивации КЦ";

			_unitOfWork = UnitOfWorkFactory.CreateWithoutRoot();

			StartDate = DateTime.Now.Date.AddDays(-6);
			EndDate = DateTime.Now.Date;

			ShowInfoCommand = new DelegateCommand(ShowInfo);

			SaveReportCommand = new DelegateCommand(SaveReport, () => CanSave);
			SaveReportCommand.CanExecuteChangedWith(this, vm => vm.CanSave);

			CreateReportCommand = new AsyncCommand(guiDispatcher, CreateReportAsync);

			AbortCreateReportCommand = new DelegateCommand(() => CreateReportCommand.Abort());

			FilterViewModel = includeExcludeSalesFilterFactory.CreateCallCenterMotivationReportIncludeExcludeFilter(_unitOfWork);
			GroupingSelectViewModel = leftRightListViewModelFactory.CreateCallCenterMotivationReportGroupingsConstructor();
		}

		private IEnumerable<GroupingType> SelectedGroupings => GroupingSelectViewModel.GetRightItems().Select(x => x.GroupType);

		public virtual DateTime? StartDate
		{
			get => _startDate;
			private set => SetField(ref _startDate, value);
		}

		public virtual DateTime? EndDate
		{
			get => _endDate;
			private set => SetField(ref _endDate, value);
		}

		public bool ShowDynamics
		{
			get => _showDynamics;
			private set => SetField(ref _showDynamics, value);
		}

		public DateTimeSliceType DateSlicingType
		{
			get => _dateSlicingType;
			private set => SetField(ref _dateSlicingType, value);
		}

		public CallCenterMotivationReport Report
		{
			get => _report;
			private set
			{
				SetField(ref _report, value);
				CanSave = _report != null;
			}
		}

		public bool CanSave
		{
			get => _canSave;
			private set => SetField(ref _canSave, value);
		}
		
		public bool IsGenerating
		{
			get => _isGenerating;
			private set => SetField(ref _isGenerating, value);
		}

		public DelegateCommand ShowInfoCommand { get; }
		public DelegateCommand SaveReportCommand { get; }
		public DelegateCommand AbortCreateReportCommand { get; }
		public AsyncCommand CreateReportCommand { get; }

		public IncludeExludeFiltersViewModel FilterViewModel { get; }
		public LeftRightListViewModel<GroupingNode> GroupingSelectViewModel { get; }

		public string SaveProgressText
		{
			get => _saveProgressText;
			private set => SetField(ref _saveProgressText, value);
		}

		public Action ShowReportAction { get; set; }
		
		private async Task CreateReportAsync(CancellationToken cancellationToken)
		{
			_guiDispatcher.RunInGuiTread(() => IsGenerating = true);

			try
			{
				Report = await GenerateReportAsync(cancellationToken);
				
				_guiDispatcher.RunInGuiTread(() => ShowReportAction?.Invoke());
			}
			catch(OperationCanceledException)
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					if(_lastGenerationErrors.Any())
					{
						ShowWarning(string.Join("\n", _lastGenerationErrors));
						_lastGenerationErrors = Enumerable.Empty<string>();
					}
					else
					{
						ShowWarning("Формирование отчета было прервано");
					}
				});
			}
			finally
			{
				UoW.Session.Clear();
				_guiDispatcher.RunInGuiTread(() => IsGenerating = false);
			}
		}

		private void SaveReport()
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить отчет...",
				DefaultFileExtention = ".xlsx",
				FileName = $"{TabName} {Report.CreatedAt:yyyy-MM-dd-HH-mm}.xlsx"
			};

			var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(!saveDialogResult.Successful)
			{
				return;
			}

			var path = saveDialogResult.Path;

			Task.Run(() =>
			{
				try
				{
					_guiDispatcher.RunInGuiTread(() =>
					{
						CanSave = false;
						SaveProgressText = "Отчет сохраняется...";
					});

					var exporter = new ExcelExporter(Report);
					exporter.Export(path);

					_guiDispatcher.RunInGuiTread(() => _interactiveService.ShowMessage(ImportanceLevel.Info, "Экспорт завершён"));
				}
				finally
				{
					_guiDispatcher.RunInGuiTread(() =>
					{
						CanSave = true;
						SaveProgressText = "Сохранить";
					});
				}
			});
		}

		private void ShowInfo()
		{
			var info = "1. Подсчет отчёта ведется на основе заказов. В отчёте учитываются заказы со статусами:\r\n" +
			           "    'Принят'\r\n" +
			           "    'В маршрутном листе'\r\n" +
			           "    'На погрузке'\r\n" +
			           "    'В пути'\r\n" +
			           "    'Доставлен'\r\n" +
			           "    'Выгрузка на складе'\r\n" +
			           "    'Закрыт'\r\n" +
			           "В отчет не попадают заказы, являющиеся закрывашками по контракту.\r\n" +
			           "Фильтр по дате отсекает заказы, если дата доставки не входит в выбранный период.\r\n" +
			           "2. Настройки отчёта:\r\n" +
			           "«В разрезе» - Выбор разбивки по периодам. В отчет попадают периоды согласно выбранного разреза, но не выходя за границы выставленного периода.\r\n" +
			           "«В динамике» - показывает изменения по отношению к предыдущему столбцу.\r\n";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
		}

		private void ShowWarning(string message)
		{
			_interactiveService.ShowMessage(ImportanceLevel.Warning, message);
		}

		private async Task<CallCenterMotivationReport> GenerateReportAsync(CancellationToken cancellationToken)
		{
			var errors = ValidateParameters();
			
			if(errors.Any())
			{
				_lastGenerationErrors = errors;
				IsGenerating = false;
				throw new OperationCanceledException("Операция отменена.", cancellationToken);
			}

			FilterViewModel.GetReportParametersSet(out var sb, withCounts: false);

			var selectedGroupings = SelectedGroupings;

			if(!selectedGroupings.Any())
			{
				selectedGroupings = new List<GroupingType>() { GroupingType.Nomenclature };
			}

			return await Task.Run(() =>
				CallCenterMotivationReport.Create(
					StartDate.Value,
					EndDate.Value,
					sb.ToString(),
					selectedGroupings,
					DateSlicingType,
					ShowDynamics,
					GetData,
					cancellationToken), cancellationToken);
		}

		private IList<CallCenterMotivationReportOrderItemNode> GetData(CallCenterMotivationReport report)
		{
			#region Сбор параметров

			var nomenclaturesCategoriesFilter = FilterViewModel.GetFilter<IncludeExcludeEnumFilter<NomenclatureCategory>>();
			var includedNomenclatureCategories = nomenclaturesCategoriesFilter.GetIncluded().ToArray();
			var excludedNomenclatureCategories = nomenclaturesCategoriesFilter.GetExcluded().ToArray();

			var nomenclaturesFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<Nomenclature>>();
			var includedNomenclatures = nomenclaturesFilter.GetIncluded().ToArray();
			var excludedNomenclatures = nomenclaturesFilter.GetExcluded().ToArray();

			var productGroupsFilter = FilterViewModel.GetFilter<IncludeExcludeEntityWithHierarchyFilter<ProductGroup>>();
			var includedProductGroups = productGroupsFilter.GetIncluded().ToArray();
			var excludedProductGroups = productGroupsFilter.GetExcluded().ToArray();

			var counterpartiesFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<Domain.Client.Counterparty>>();
			var includedCounterparties = counterpartiesFilter.GetIncluded().ToArray();
			var excludedCounterparties = counterpartiesFilter.GetExcluded().ToArray();

			#region CounterpartyTypes

			var includedCounterpartyTypeElements = FilterViewModel.GetIncludedElements<CounterpartyType>();
			var excludedCounterpartyTypeElements = FilterViewModel.GetExcludedElements<CounterpartyType>();

			var includedCounterpartyTypes = includedCounterpartyTypeElements
				.Where(x => x is IncludeExcludeElement<CounterpartyType, CounterpartyType>)
				.Select(x => (x as IncludeExcludeElement<CounterpartyType, CounterpartyType>).Id)
				.ToArray();

			var excludedCounterpartyTypes = excludedCounterpartyTypeElements
				.Where(x => x is IncludeExcludeElement<CounterpartyType, CounterpartyType>)
				.Select(x => (x as IncludeExcludeElement<CounterpartyType, CounterpartyType>).Id)
				.ToArray();

			var includedCounterpartySubtypes = includedCounterpartyTypeElements
				.Where(x => x is IncludeExcludeElement<int, CounterpartySubtype>)
				.Select(x => (x as IncludeExcludeElement<int, CounterpartySubtype>).Id)
				.ToArray();

			var excludedCounterpartySubtypes = excludedCounterpartyTypeElements
				.Where(x => x is IncludeExcludeElement<int, CounterpartySubtype>)
				.Select(x => (x as IncludeExcludeElement<int, CounterpartySubtype>).Id)
				.ToArray();

			#endregion CounterpartyTypes

			var organizationsFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<Organization>>();
			var includedOrganizations = organizationsFilter.GetIncluded().ToArray();
			var excludedOrganizations = organizationsFilter.GetExcluded().ToArray();

			var discountReasonsFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<DiscountReason>>();
			var includedDiscountReasons = discountReasonsFilter.GetIncluded().ToArray();
			var excludedDiscountReasons = discountReasonsFilter.GetExcluded().ToArray();

			var subdivisionsFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<Subdivision>>();
			var includedSubdivisions = subdivisionsFilter.GetIncluded().ToArray();
			var excludedSubdivisions = subdivisionsFilter.GetExcluded().ToArray();

			var employeesFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<Employee>>("OrderAuthor");
			var includedEmployees = employeesFilter.GetIncluded().ToArray();
			var excludedEmployees = employeesFilter.GetExcluded().ToArray();

			var promotionalSetsFilter = FilterViewModel.GetFilter<IncludeExcludeEntityFilter<PromotionalSet>>();
			var includedPromotionalSets = promotionalSetsFilter.GetIncluded().ToArray();
			var excludedPromotionalSets = promotionalSetsFilter.GetExcluded().ToArray();

			var orderStatusesFilter = FilterViewModel.GetFilter<IncludeExcludeEnumFilter<OrderStatus>>();
			var includedOrderStatuses = orderStatusesFilter.GetIncluded().ToArray();
			var excludedOrderStatuses = orderStatusesFilter.GetExcluded().ToArray();

			var includedBoolParams = FilterViewModel.GetFilter<IncludeExcludeBoolParamsFilter>();

			#endregion Сбор параметров

			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;
			Employee authorAlias = null;
			Subdivision subdivisionAlias = null;
			PromotionalSet promotionalSetAlias = null;
			Domain.Client.Counterparty counterpartyAlias = null;
			CounterpartySubtype counterpartySubtypeAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			Organization organizationAlias = null;
			Phone orderContactPhoneAlias = null;

			CallCenterMotivationReportOrderItemNode resultNodeAlias = null;

			var query = _unitOfWork.Session.QueryOver(() => orderItemAlias)
				.Left.JoinAlias(() => orderItemAlias.PromoSet, () => promotionalSetAlias)
				.JoinEntityAlias(() => orderAlias, () => orderItemAlias.Order.Id == orderAlias.Id)
				.Left.JoinAlias(() => orderAlias.ContactPhone, () => orderContactPhoneAlias)
				.Left.JoinAlias(() => orderAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => authorAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => counterpartyAlias.CounterpartySubtype, () => counterpartySubtypeAlias)
				.Left.JoinAlias(() => orderAlias.Contract, () => counterpartyContractAlias)
				.Left.JoinAlias(() => counterpartyContractAlias.Organization, () => organizationAlias)
				.Inner.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias);

			#region filter parameters

			#region NomenclatureCategories

			if(includedNomenclatureCategories.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => nomenclatureAlias.Category),
					includedNomenclatureCategories));
			}

			if(excludedNomenclatureCategories.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => nomenclatureAlias.Category),
					excludedNomenclatureCategories)));
			}

			#endregion NomenclatureCategories

			#region Nomenclatures

			if(includedNomenclatures.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => nomenclatureAlias.Id),
					includedNomenclatures));
			}

			if(excludedNomenclatures.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => nomenclatureAlias.Id),
					excludedNomenclatures)));
			}

			#endregion Nomenclatures

			#region ProductGroups

			if(includedProductGroups.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => productGroupAlias.Id),
					includedProductGroups));
			}

			if(excludedProductGroups.Any())
			{
				query.Where(Restrictions.Disjunction()
					.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => productGroupAlias.Id),
						excludedProductGroups)))
					.Add(Restrictions.IsNull(Projections.Property(() => productGroupAlias.Id))));
			}

			#endregion ProductGroups

			#region Counterparties

			if(includedCounterparties.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => orderAlias.Client.Id),
					includedCounterparties));
			}

			if(excludedCounterparties.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => orderAlias.Client.Id),
					excludedCounterparties)));
			}

			#endregion Counterparties

			#region CounterpartyTypes

			if(includedCounterpartyTypes.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => counterpartyAlias.CounterpartyType),
					includedCounterpartyTypes));
			}

			if(excludedCounterpartyTypes.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => counterpartyAlias.CounterpartyType),
					excludedCounterpartyTypes)));
			}

			#endregion CounterpartyTypes

			#region CounterpartySubtypes

			if(includedCounterpartySubtypes.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => counterpartyAlias.CounterpartySubtype.Id),
					includedCounterpartySubtypes));
			}

			if(excludedCounterpartySubtypes.Any())
			{
				query.Where(Restrictions.Disjunction()
					.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => counterpartyAlias.CounterpartySubtype.Id),
						excludedCounterpartySubtypes)))
					.Add(Restrictions.IsNull(Projections.Property(() => counterpartyAlias.CounterpartySubtype.Id))));
			}

			#endregion CounterpartySubtypes

			#region Organizations

			if(includedOrganizations.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => counterpartyContractAlias.Organization.Id),
					includedOrganizations));
			}

			if(excludedOrganizations.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => counterpartyContractAlias.Organization.Id),
					excludedOrganizations)));
			}

			#endregion Organizations

			#region DiscountReasons

			if(includedDiscountReasons.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => orderItemAlias.DiscountReason.Id),
					includedDiscountReasons));
			}

			if(excludedDiscountReasons.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => orderItemAlias.DiscountReason.Id),
					excludedDiscountReasons)));
			}

			#endregion DiscountReasons

			#region Subdivisions

			if(includedSubdivisions.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => authorAlias.Subdivision.Id),
					includedSubdivisions));
			}

			if(excludedSubdivisions.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => authorAlias.Subdivision.Id),
					excludedSubdivisions)));
			}

			#endregion Subdivisions

			#region Employees

			if(includedEmployees.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => authorAlias.Id),
					includedEmployees));
			}

			if(excludedEmployees.Any())
			{
				query.Where(Restrictions.Not(Restrictions.In(
					Projections.Property(() => authorAlias.Id),
					excludedEmployees)));
			}

			#endregion Employees

			#region OrderAuthor

			var fullNameOrderAuthorProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT(?1, ' ', ?2, ' ', ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic)
			);

			var orderAuthorNameProjection = Projections.Conditional(
				Restrictions.IsNotNull(Projections.Property(() => authorAlias.LastName)),
				fullNameOrderAuthorProjection,
				Projections.Constant("Без автора заказа")
			);

			var orderAuthorIdProjection = Projections.Conditional(
				Restrictions.IsNull(Projections.Property(() => authorAlias.Id)),
				Projections.Constant(0),
				Projections.Property(() => authorAlias.Id));

			#endregion

			#region PromotionalSets

			if(includedPromotionalSets.Any())
			{
				query.Where(Restrictions.In(
					Projections.Property(() => promotionalSetAlias.Id),
					includedPromotionalSets));
			}

			if(excludedPromotionalSets.Any())
			{
				query.Where(Restrictions.Disjunction()
					.Add(Restrictions.Not(Restrictions.In(
						Projections.Property(() => promotionalSetAlias.Id),
						excludedPromotionalSets)))
					.Add(Restrictions.IsNull(Projections.Property(() => promotionalSetAlias.Id))));
			}

			#endregion PromotionalSets

			#region OrderStatuses

			if(includedOrderStatuses.Any())
			{
				query.WhereRestrictionOn(() => orderAlias.OrderStatus).IsIn(includedOrderStatuses);
			}

			if(excludedOrderStatuses.Any())
			{
				query.WhereRestrictionOn(() => orderAlias.OrderStatus).Not.IsIn(excludedOrderStatuses);
			}

			#endregion OrderStatuses

			#region BoolParams

			foreach(var param in includedBoolParams.FilteredElements)
			{
				if(!param.Include && !param.Exclude)
				{
					continue;
				}

				switch(param.Number)
				{
					case "is_self_delivery":
						query.Where(() => orderAlias.SelfDelivery == param.Include);
						break;
					case "is_first_client":
						Order orderMinAlias = null;
						var clientOneOrderStatuses = new[] { OrderStatus.Canceled, OrderStatus.DeliveryCanceled, OrderStatus.NotDelivered };
						var subQueryMinDeliveryDate = QueryOver.Of(() => orderMinAlias)
							.Where(() => orderMinAlias.Client.Id == counterpartyAlias.Id)
							.WhereRestrictionOn(() => orderMinAlias.OrderStatus)
							.Not.IsIn(clientOneOrderStatuses)
							.Select(Projections.Min(() => orderMinAlias.DeliveryDate));

						var minDateProjection = Projections.SubQuery(subQueryMinDeliveryDate);

						if(param.Include)
						{
							query.Where(Restrictions.Between(minDateProjection, StartDate, EndDate));
						}
						else
						{
							query.WhereNot(Restrictions.Between(minDateProjection, StartDate, EndDate));
						}
						break;
					default:
						throw new NotSupportedException(param.Number);
				}
			}

			#endregion BoolParams

			#endregion Filters

			var result = query
				.Where(() => !orderAlias.IsContractCloser)
				.And(Restrictions.Between(Projections.Property(() => orderAlias.DeliveryDate), StartDate, EndDate))
				.SelectList(list =>
					list.SelectGroup(() => orderItemAlias.Id)
						.Select(() => orderItemAlias.Id).WithAlias(() => resultNodeAlias.Id)
						.Select(() => orderItemAlias.Price).WithAlias(() => resultNodeAlias.Price)
						.Select(OrderProjections.GetOrderItemSumProjection()).WithAlias(() => resultNodeAlias.ActualSum)
						.Select(() => orderItemAlias.Count).WithAlias(() => resultNodeAlias.Count)
						.Select(() => orderItemAlias.ActualCount).WithAlias(() => resultNodeAlias.ActualCount)
						.Select(() => nomenclatureAlias.Id).WithAlias(() => resultNodeAlias.NomenclatureId)
						.Select(() => nomenclatureAlias.OfficialName).WithAlias(() => resultNodeAlias.NomenclatureOfficialName)
						.Select(() => nomenclatureAlias.MotivationUnitType).WithAlias(() => resultNodeAlias.MotivationUnitType)
						.Select(() => nomenclatureAlias.MotivationCoefficient).WithAlias(() => resultNodeAlias.MotivationCoefficient)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultNodeAlias.OrderDeliveryDate)
						.Select(() => productGroupAlias.Id).WithAlias(() => resultNodeAlias.ProductGroupId)
						.Select(ProductGroupProjections.GetProductGroupNameWithEnclosureProjection()).WithAlias(() => resultNodeAlias.ProductGroupName)
						.Select(orderAuthorIdProjection).WithAlias(() => resultNodeAlias.OrderAuthorId)
						.Select(orderAuthorNameProjection).WithAlias(() => resultNodeAlias.OrderAuthorName))
				.SetTimeout(0)
				.TransformUsing(Transformers.AliasToBean<CallCenterMotivationReportOrderItemNode>())
				.List<CallCenterMotivationReportOrderItemNode>();

			return result;
		}

		private IEnumerable<string> ValidateParameters()
		{
			if(StartDate == null
			   || StartDate == default(DateTime)
			   || EndDate == null
			   || EndDate == default(DateTime))
			{
				yield return "Заполните дату.";
			}

			if(StartDate > EndDate)
			{
				yield return "Начальная дата не может быть больше конечной";
			}

			var deltaTime = EndDate - StartDate;

			if(DateSlicingType == DateTimeSliceType.Day && deltaTime?.TotalDays >= 62)
			{
				yield return "Для разреза день нельзя выбрать интервал более 62х дней";
			}

			if((DateSlicingType == DateTimeSliceType.Week)
			   && (StartDate?.DayOfWeek == DayOfWeek.Monday ? deltaTime?.TotalDays / 7 >= 54 : deltaTime?.TotalDays / 7 > 54))
			{
				yield return "Для разреза неделя нельзя выбрать интервал более 54х недель";
			}

			var monthBetweenDates = 0;

			for(DateTime monthDate = StartDate.Value; monthDate < EndDate; monthDate = monthDate.AddMonths(1))
			{
				monthBetweenDates++;
			}

			if((DateSlicingType == DateTimeSliceType.Month)
			   && (StartDate?.Day == 1 ? monthBetweenDates >= 60 : monthBetweenDates > 60))
			{
				yield return "Для разреза месяц нельзя выбрать интервал более 60х месяцев";
			}
		}
	}
}
