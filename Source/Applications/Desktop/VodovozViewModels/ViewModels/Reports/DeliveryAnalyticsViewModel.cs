using ClosedXML.Report;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.EntitySelector;
using QS.Utilities.Debug;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports.DeliveryAnalytics;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports
{
	public class DeliveryAnalyticsViewModel : TabViewModelBase
	{
		private const string _templatePath = @".\Reports\Logistic\DeliveryAnalyticsReport.xlsx";
		private DeliveryAnalyticsReport _report;

		private DateTime? _startDeliveryDate;
		private DateTime? _endDeliveryDate;
		private District _district;

		private bool _isLoadingData;
		private string _progress = string.Empty;
		private const string _dataLoaded = "Выгрузка завершена.";
		private const string _error = "Произошла ошибка.";

		private DelegateCommand _exportCommand;
		private DelegateCommand _allStatusCommand;
		private DelegateCommand _noneStatusCommand;
		private DelegateCommand _showHelpCommand;

		private IEnumerable<DeliveryAnalyticsReportNode> _oneWaveMorning;
		private IEnumerable<DeliveryAnalyticsReportNode> _oneWaveDay;
		private IEnumerable<DeliveryAnalyticsReportNode> _oneWaveEvening;
		private IEnumerable<DeliveryAnalyticsReportNode> _twoWave;
		private IEnumerable<DeliveryAnalyticsReportNode> _threeWave;

		private readonly IInteractiveService _interactiveService;
		private readonly IDistrictJournalFactory _districtJournalFactory;
		public IEntityAutocompleteSelectorFactory DistrictSelectorFactory;
		public string LoadingData = "Идет выгрузка данных...";

		public DeliveryAnalyticsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IDistrictJournalFactory districtJournalFactory)
			: base(interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_districtJournalFactory = districtJournalFactory ?? throw new ArgumentNullException(nameof(districtJournalFactory));

			var filter = new DistrictJournalFilterViewModel { Status = DistrictsSetStatus.Active };

			DistrictSelectorFactory = _districtJournalFactory.CreateDistrictAutocompleteSelectorFactory(filter, true);
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			Uow = unitOfWorkFactory.CreateWithoutRoot();
			Title = "Аналитика объёмов доставки";

			WaveList = new GenericObservableList<WaveNode>();
			WeekDayName = new GenericObservableList<WeekDayNodes>();
			GeographicGroupNodes = new GenericObservableList<GeographicGroupNode>();

			WageDistrictNodes = new GenericObservableList<WageDistrictNode>();

			foreach(var wage in Uow.GetAll<WageDistrict>().Select(x => x).ToList())
			{
				var wageNode = new WageDistrictNode(wage);
				wageNode.Selected = true;
				WageDistrictNodes.Add(wageNode);
			}

			foreach(var geographic in Uow.GetAll<GeoGroup>().Select(x => x).ToList())
			{
				var geographicNode = new GeographicGroupNode(geographic);
				geographicNode.Selected = true;
				GeographicGroupNodes.Add(geographicNode);
			}

			foreach(var wave in Enum.GetValues(typeof(WaveNodes)))
			{
				var waveNode = new WaveNode { WaveNodes = (WaveNodes)wave, Selected = true };
				WaveList.Add(waveNode);
			}

			foreach(var week in Enum.GetValues(typeof(WeekDayName)))
			{
				if((WeekDayName)week == Domain.Sale.WeekDayName.Today) continue;
				var weekNode = new WeekDayNodes { WeekNameNode = (WeekDayName)week, Selected = true };
				WeekDayName.Add(weekNode);
			}
		}

		#region Свойства

		public IUnitOfWork Uow;

		public District District
		{
			get => _district;
			set => SetField(ref _district, value);
		}

		public DateTime? StartDeliveryDate
		{
			get => _startDeliveryDate;
			set
			{
				if(SetField(ref _startDeliveryDate, value))
				{
					OnPropertyChanged(nameof(HasExportReport));
				}
			}
		}

		public DateTime? EndDeliveryDate
		{
			get => _endDeliveryDate;
			set => SetField(ref _endDeliveryDate, value);
		}

		public bool HasExportReport => StartDeliveryDate.HasValue;

		public GenericObservableList<GeographicGroupNode> GeographicGroupNodes { get; private set; }

		public GenericObservableList<WageDistrictNode> WageDistrictNodes { get; private set; }

		public GenericObservableList<WaveNode> WaveList { get; private set; }

		public GenericObservableList<WeekDayNodes> WeekDayName { get; set; }

		public string FileName { get; set; }

		public string Progress
		{
			get => _progress;
			set => SetField(ref _progress, value);
		}
		#endregion

		#region Методы

		private void UpdateNodes()
		{
			#region Все к базовому запросу

			var createDate4 = DateTime.Parse(EndDeliveryDate.Value.ToShortDateString() + " 4:00:00");
			var createDate12 = DateTime.Parse(EndDeliveryDate.Value.ToShortDateString() + " 12:00:00");
			var createDate18 = DateTime.Parse(EndDeliveryDate.Value.ToShortDateString() + " 18:00:00");

			var selectedGeographicGroup = GeographicGroupNodes.Where(x => x.Selected).Select(x => x.GeographicGroup);
			var selectedWages = WageDistrictNodes.Where(x => x.Selected).Select(x => x.WageDistrict);

			DeliveryAnalyticsReportNode resultAlias = null;
			District districtAlias = null;
			WageDistrict wageDistrictAlias = null;
			GeoGroup geographicGroupAlias = null;
			Order orderAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			DeliverySchedule deliveryScheduleAlias = null;

			var query = Uow.Session.QueryOver(() => orderAlias)
				.Inner.JoinAlias(x => x.DeliveryPoint, () => deliveryPointAlias)
				.Inner.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
				.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geographicGroupAlias)
				.Left.JoinAlias(() => districtAlias.WageDistrict, () => wageDistrictAlias)
				.Left.JoinAlias(x => x.DeliverySchedule, () => deliveryScheduleAlias)
				.Left.JoinAlias(x => x.OrderItems, () => orderItemAlias)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias);

			query.Where(x => !x.SelfDelivery)
				.Where(x => !x.IsContractCloser)
				.Where(x => x.OrderAddressType != OrderAddressType.Service)
				.WhereRestrictionOn(x => x.OrderStatus)
				.Not.IsIn(new[] { OrderStatus.NewOrder, OrderStatus.Canceled, OrderStatus.WaitForPayment });

			if(StartDeliveryDate.HasValue)
			{
				query.Where(x => x.DeliveryDate >= StartDeliveryDate);
			}

			if(EndDeliveryDate.HasValue)
			{
				query.Where(x => x.DeliveryDate <= EndDeliveryDate);
			}

			if(District != null)
			{
				query.Where(() => districtAlias.Id == District.Id);
			}

			if(selectedGeographicGroup.Any())
			{
				query.Where(Restrictions.In(Projections.Property(() => geographicGroupAlias.Id),
					selectedGeographicGroup.Select(x => x.Id).ToArray()));
			}

			#endregion

			#region Разделение запросов по волнам

			var oneWaveMorningQuery = query.Clone();
			var oneWaveEveningQuery = query.Clone();
			var oneWaveDayQuery = query.Clone();
			var twoWaveQuery = query.Clone();
			var threeWaveQuery = query.Clone();

			oneWaveMorningQuery.Where(x => deliveryScheduleAlias.From < createDate12.TimeOfDay);
			oneWaveDayQuery.Where(x =>
				deliveryScheduleAlias.From >= createDate12.TimeOfDay && deliveryScheduleAlias.From < createDate18.TimeOfDay);


			if(!selectedWages.Any() || selectedWages.Count() == 2)
			{
				oneWaveMorningQuery.Where(x =>
					(x.CreateDate < createDate4 && wageDistrictAlias.Name == "Город") || (wageDistrictAlias.Name == "Пригород"));
				oneWaveDayQuery.Where(x =>
					(x.CreateDate < createDate4 && wageDistrictAlias.Name == "Город") || wageDistrictAlias.Name == "Пригород");
				oneWaveEveningQuery.Where(x =>
					deliveryScheduleAlias.From >= createDate18.TimeOfDay && wageDistrictAlias.Name == "Пригород");
				twoWaveQuery.Where(x =>
					x.CreateDate >= createDate4 && x.CreateDate < createDate12 && deliveryScheduleAlias.From < createDate18.TimeOfDay &&
					wageDistrictAlias.Name == "Город");
				threeWaveQuery.Where(x =>
					((x.CreateDate >= createDate12) || (x.CreateDate >= createDate4 && x.CreateDate < createDate12 &&
														deliveryScheduleAlias.From >= createDate18.TimeOfDay) ||
					 (x.CreateDate < createDate4 && deliveryScheduleAlias.From >= createDate18.TimeOfDay)) &&
					wageDistrictAlias.Name == "Город");
			}

			else if(selectedWages.Any(x => x.Name == "Город"))
			{
				oneWaveMorningQuery.Where(x =>
					x.CreateDate < createDate4 && wageDistrictAlias.Name == "Город");
				oneWaveDayQuery.Where(x =>
					x.CreateDate < createDate4 &&
					wageDistrictAlias.Name == "Город");
				twoWaveQuery.Where(x =>
					x.CreateDate >= createDate4 && x.CreateDate < createDate12 && deliveryScheduleAlias.From < createDate18.TimeOfDay &&
					wageDistrictAlias.Name == "Город");
				threeWaveQuery.Where(x =>
					(x.CreateDate >= createDate12 && wageDistrictAlias.Name == "Город") ||
					(x.CreateDate >= createDate4 && x.CreateDate < createDate12 && deliveryScheduleAlias.From >= createDate18.TimeOfDay &&
					 wageDistrictAlias.Name == "Город") ||
					(x.CreateDate < createDate4 && deliveryScheduleAlias.From >= createDate18.TimeOfDay) &&
					wageDistrictAlias.Name == "Город");
			}

			else if(selectedWages.Any(x => x.Name == "Пригород"))
			{
				oneWaveMorningQuery.Where(x => wageDistrictAlias.Name == "Пригород");
				oneWaveDayQuery.Where(x => wageDistrictAlias.Name == "Пригород");
				oneWaveEveningQuery.Where(x =>
					deliveryScheduleAlias.From >= createDate18.TimeOfDay && wageDistrictAlias.Name == "Пригород");
			}

			#endregion

			#region Подсчёт заказов и бутылей

			var bottleSmallCountSubquery = QueryOver.Of(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Where(Restrictions.Lt(Projections.Sum(() => orderItemAlias.Count), 5))
				.Select(Projections.Sum(() => orderItemAlias.Count));

			var bootleBigCountSubquery = QueryOver.Of(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Where(Restrictions.Ge(Projections.Sum(() => orderItemAlias.Count), 5))
				.Select(Projections.Sum(() => orderItemAlias.Count));

			Order orderAlias2 = null;
			var bigCountOrders = QueryOver.Of(() => orderItemAlias)
				.Left.JoinAlias(() => orderItemAlias.Order, () => orderAlias2)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.Where(Restrictions.Ge(Projections.Sum(() => orderItemAlias.Count), 5))
				.Select(Projections.CountDistinct(() => orderAlias2.Id));

			var nullSmallCountOrders = QueryOver.Of(() => orderAlias2)
				.Left.JoinAlias(() => orderAlias2.OrderItems, () => orderItemAlias)
				.Where(Restrictions.IsNull(Projections.Property(() => orderItemAlias.Id)))
				.Where(() => orderAlias.Id == orderAlias2.Id)
				.Select(Projections.CountDistinct(() => orderAlias2.Id));

			var notNullSmallCountOrders = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Order, () => orderAlias2)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Where(Restrictions.Lt(Projections.Sum(() => orderItemAlias.Count), 5))
				.Select(Projections.CountDistinct(() => orderAlias2.Id));

			var ordersWith19L = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Order, () => orderAlias2)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Distinct(Projections.Property(() => orderAlias2.Id)));

			var notNullSmallCountOrdersWithoutWater = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Order, () => orderAlias2)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category != NomenclatureCategory.water || nomenclatureAlias.TareVolume != TareVolume.Vol19L)
				.WithSubquery.WhereNotExists(ordersWith19L)
				.Select(Projections.CountDistinct(() => orderAlias2.Id));
			#endregion

			#region Составление волн

			var selectedWaves = WaveList.Where(x => x.Selected).Select(x => x.WaveNodes);

			if(selectedWaves.Any(x => x == WaveNodes.FirstWave)
			|| selectedWaves.Count() == 3
			|| !selectedWaves.Any())
			{
				_oneWaveMorning = oneWaveMorningQuery
					.SelectList(list => list
						.Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => geographicGroupAlias.Name).WithAlias(() => resultAlias.GeographicGroupName)
						.Select(() => wageDistrictAlias.Name).WithAlias(() => resultAlias.CityOrSuburb)
						.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DayOfWeek)
						.SelectSubQuery(nullSmallCountOrders).WithAlias(() => resultAlias.NullCountSmallOrdersOneMorning)
						.SelectSubQuery(notNullSmallCountOrders).WithAlias(() => resultAlias.NotNullCountSmallOrdersOneMorning)
						.SelectSubQuery(notNullSmallCountOrdersWithoutWater).WithAlias(() => resultAlias.NotNullCountSmallOrdersOneMorningWithoutWater)
						.SelectSubQuery(bottleSmallCountSubquery).WithAlias(() => resultAlias.CountSmallOrders19LOneMorning)
						.SelectSubQuery(bigCountOrders).WithAlias(() => resultAlias.CountBigOrdersOneMorning)
						.SelectSubQuery(bootleBigCountSubquery).WithAlias(() => resultAlias.CountBigOrders19LOneMorning))
					.TransformUsing(Transformers.AliasToBean<DeliveryAnalyticsReportNode>())
					.List<DeliveryAnalyticsReportNode>().Distinct();

				_oneWaveDay = oneWaveDayQuery
					.SelectList(list => list
						.Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => geographicGroupAlias.Name).WithAlias(() => resultAlias.GeographicGroupName)
						.Select(() => wageDistrictAlias.Name).WithAlias(() => resultAlias.CityOrSuburb)
						.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DayOfWeek)
						.SelectSubQuery(nullSmallCountOrders).WithAlias(() => resultAlias.NullCountSmallOrdersOneDay)
						.SelectSubQuery(notNullSmallCountOrders).WithAlias(() => resultAlias.NotNullCountSmallOrdersOneDay)
						.SelectSubQuery(notNullSmallCountOrdersWithoutWater).WithAlias(() => resultAlias.NotNullCountSmallOrdersOneDayWithoutWater)
						.SelectSubQuery(bottleSmallCountSubquery).WithAlias(() => resultAlias.CountSmallOrders19LOneDay)
						.SelectSubQuery(bigCountOrders).WithAlias(() => resultAlias.CountBigOrdersOneDay)
						.SelectSubQuery(bootleBigCountSubquery).WithAlias(() => resultAlias.CountBigOrders19LOneDay))
					.TransformUsing(Transformers.AliasToBean<DeliveryAnalyticsReportNode>())
					.List<DeliveryAnalyticsReportNode>().Distinct();

				_oneWaveEvening = oneWaveEveningQuery
					.SelectList(list => list
						.Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => geographicGroupAlias.Name).WithAlias(() => resultAlias.GeographicGroupName)
						.Select(() => wageDistrictAlias.Name).WithAlias(() => resultAlias.CityOrSuburb)
						.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DayOfWeek)
						.SelectSubQuery(nullSmallCountOrders).WithAlias(() => resultAlias.NullCountSmallOrdersOneEvening)
						.SelectSubQuery(notNullSmallCountOrders).WithAlias(() => resultAlias.NotNullCountSmallOrdersOneEvening)
						.SelectSubQuery(notNullSmallCountOrdersWithoutWater).WithAlias(() => resultAlias.NotNullCountSmallOrdersOneEveningWithoutWater)
						.SelectSubQuery(bottleSmallCountSubquery).WithAlias(() => resultAlias.CountSmallOrders19LOneEvening)
						.SelectSubQuery(bigCountOrders).WithAlias(() => resultAlias.CountBigOrdersOneEvening)
						.SelectSubQuery(bootleBigCountSubquery).WithAlias(() => resultAlias.CountBigOrders19LOneEvening))
					.TransformUsing(Transformers.AliasToBean<DeliveryAnalyticsReportNode>())
					.List<DeliveryAnalyticsReportNode>().Distinct();
			}

			if(selectedWaves.Any(x => x == WaveNodes.SecondWave)
			   || selectedWaves.Count() == 3
			   || !selectedWaves.Any())
			{
				_twoWave = twoWaveQuery
					.SelectList(list => list
						.Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => geographicGroupAlias.Name).WithAlias(() => resultAlias.GeographicGroupName)
						.Select(() => wageDistrictAlias.Name).WithAlias(() => resultAlias.CityOrSuburb)
						.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DayOfWeek)
						.SelectSubQuery(nullSmallCountOrders).WithAlias(() => resultAlias.NullCountSmallOrdersTwoDay)
						.SelectSubQuery(notNullSmallCountOrders).WithAlias(() => resultAlias.NotNullCountSmallOrdersTwoDay)
						.SelectSubQuery(notNullSmallCountOrdersWithoutWater).WithAlias(() => resultAlias.NotNullCountSmallOrdersTwoDayWithoutWater)
						.SelectSubQuery(bottleSmallCountSubquery).WithAlias(() => resultAlias.CountSmallOrders19LTwoDay)
						.SelectSubQuery(bigCountOrders).WithAlias(() => resultAlias.CountBigOrdersTwoDay)
						.SelectSubQuery(bootleBigCountSubquery).WithAlias(() => resultAlias.CountBigOrders19LTwoDay))
					.TransformUsing(Transformers.AliasToBean<DeliveryAnalyticsReportNode>())
					.List<DeliveryAnalyticsReportNode>().Distinct();
			}

			if(selectedWaves.Any(x => x == WaveNodes.ThirdWave)
			   || selectedWaves.Count() == 3
			   || !selectedWaves.Any())
			{
				_threeWave = threeWaveQuery
					.SelectList(list => list
						.Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => geographicGroupAlias.Name).WithAlias(() => resultAlias.GeographicGroupName)
						.Select(() => wageDistrictAlias.Name).WithAlias(() => resultAlias.CityOrSuburb)
						.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DayOfWeek)
						.SelectSubQuery(nullSmallCountOrders).WithAlias(() => resultAlias.NullCountSmallOrdersThreeDay)
						.SelectSubQuery(notNullSmallCountOrders).WithAlias(() => resultAlias.NotNullCountSmallOrdersThreeDay)
						.SelectSubQuery(notNullSmallCountOrdersWithoutWater).WithAlias(() => resultAlias.NotNullCountSmallOrdersThreeDayWithoutWater)
						.SelectSubQuery(bottleSmallCountSubquery).WithAlias(() => resultAlias.CountSmallOrders19LThreeDay)
						.SelectSubQuery(bigCountOrders).WithAlias(() => resultAlias.CountBigOrdersThreeDay)
						.SelectSubQuery(bootleBigCountSubquery).WithAlias(() => resultAlias.CountBigOrders19LThreeDay))
					.TransformUsing(Transformers.AliasToBean<DeliveryAnalyticsReportNode>())
					.List<DeliveryAnalyticsReportNode>().Distinct();
			}

			#endregion
		}

		private void GenerateReport()
		{
			_report = new DeliveryAnalyticsReport()
			{
				CreationDate = DateTime.Now,
				StartDate = StartDeliveryDate ?? DateTime.Now,
				EndDate = EndDeliveryDate ?? DateTime.Now,
				SelectedFilters = GetSelectedFilters()
			};
			var rows = new List<DeliveryAnalyticsReportRow>();

			var count = 1;
			var selectedDays = WeekDayName.Where(x => x.Selected).Select(x => x.WeekNameNode);
			var selectedWages = WageDistrictNodes.Where(x => x.Selected).Select(x => x.WageDistrict);
			var selectedWaves = WaveList.Where(x => x.Selected).Select(x => x.WaveNodes);

			var nodesCsv = new List<DeliveryAnalyticsReportNode>();
			if(selectedWaves.Any(x => x == WaveNodes.FirstWave)
			   || !selectedWaves.Any())
			{
				foreach(var reportNodes in _oneWaveMorning.Concat(_oneWaveDay).Concat(_oneWaveEvening)
					.GroupBy(x => new { x.GeographicGroupName, x.CityOrSuburb, x.DistrictName, x.DayOfWeek.Date, x.DeliveryDate }))
				{
					var weekDayName = (WeekDayName)Enum.Parse(typeof(WeekDayName), reportNodes.Key.Date.DayOfWeek.ToString());
					if((selectedDays.Contains(weekDayName) || !selectedDays.Any())
					   && (selectedWages.Any(x => x.Name == reportNodes.Key.CityOrSuburb) || !selectedWages.Any()))
					{
						nodesCsv.Add(new DeliveryAnalyticsReportNode(reportNodes, count));
					}
				}
			}

			if(selectedWaves.Any(x => x == WaveNodes.SecondWave)
			   || !selectedWaves.Any())
			{
				foreach(var reportNodes in _twoWave
					.GroupBy(x => new { x.GeographicGroupName, x.CityOrSuburb, x.DistrictName, x.DayOfWeek.Date, x.DeliveryDate }))
				{
					var weekDayName = (WeekDayName)Enum.Parse(typeof(WeekDayName), reportNodes.Key.Date.DayOfWeek.ToString());
					if((selectedDays.Contains(weekDayName) || !selectedDays.Any())
					   && (selectedWages.Any(x => x.Name == reportNodes.Key.CityOrSuburb) || !selectedWages.Any()))
					{
						nodesCsv.Add(new DeliveryAnalyticsReportNode(reportNodes, count));
					}
				}
			}

			if(selectedWaves.Any(x => x == WaveNodes.ThirdWave)
			   || !selectedWaves.Any())
			{
				foreach(var reportNodes in _threeWave
					.GroupBy(x => new { x.GeographicGroupName, x.CityOrSuburb, x.DistrictName, x.DayOfWeek.Date, x.DeliveryDate }))
				{
					var weekDayName = (WeekDayName)Enum.Parse(typeof(WeekDayName), reportNodes.Key.Date.DayOfWeek.ToString());
					if((selectedDays.Contains(weekDayName) || !selectedDays.Any())
					   && (selectedWages.Any(x => x.Name == reportNodes.Key.CityOrSuburb) || !selectedWages.Any()))
					{
						nodesCsv.Add(new DeliveryAnalyticsReportNode(reportNodes, count));
					}
				}
			}

			foreach(var groupNode in nodesCsv
				.OrderByDescending(x => x.GeographicGroupName)
				.ThenBy(x => x.CityOrSuburb)
				.ThenBy(x => x.DistrictName)
				.ThenBy(x => ((int)x.DayOfWeek.DayOfWeek + 6) % 7)
				.ThenBy(x => x.DeliveryDate)
				.GroupBy(x => new
				{
					x.DistrictName,
					x.DayOfWeek
				}
				))
			{
				rows.Add(new DeliveryAnalyticsReportRow(new DeliveryAnalyticsReportNode(groupNode, count)));
				count++;
			}
			_report.Rows = rows;
		}

		private string GetSelectedFilters()
		{
			var result = "";
			if(District != null)
			{
				result += $"район - {District.DistrictName}, ";
			}
			result += "часть -";
			if(GeographicGroupNodes.Any(x => x.Selected) && GeographicGroupNodes.Count(x => x.Selected) < GeographicGroupNodes.Count)
			{
				foreach(var group in GeographicGroupNodes.Where(x => x.Selected))
				{
					result += $" {group},";
				}
			}
			else
			{
				result += " город и пригород,";
			}
			result += " зарплатный район -";
			if(WageDistrictNodes.Any(x => x.Selected) && WageDistrictNodes.Count(x => x.Selected) < WageDistrictNodes.Count)
			{
				foreach(var dist in WageDistrictNodes.Where(x => x.Selected))
				{
					result += $" {dist},";
				}
			}
			else
			{
				result += " север и юг,";
			}
			result += " день недели -";
			if(WeekDayName.Any(x => x.Selected) && WeekDayName.Count(x => x.Selected) < WeekDayName.Count)
			{
				foreach(var day in WeekDayName.Where(x => x.Selected))
				{
					result += $" {day},";
				}
			}
			else
			{
				result += " все,";
			}
			result += " волна -";
			if(WaveList.Any(x => x.Selected) && WaveList.Count(x => x.Selected) < WaveList.Count)
			{
				foreach(var wave in WaveList.Where(x => x.Selected))
				{
					result += $" {wave},";
				}
			}
			else
			{
				result += " все";
			}

			return result.TrimEnd(',');
		}

		#endregion

		#region Команды
		public DelegateCommand ExportCommand => _exportCommand ?? (_exportCommand = new DelegateCommand(
			() =>
			{
				try
				{
					UpdateNodes();
					if(string.IsNullOrWhiteSpace(FileName))
					{
						throw new InvalidOperationException($"Не был заполнен путь выгрузки: {nameof(FileName)}");
					}
					GenerateReport();
					try
					{
						var template = new XLTemplate(_templatePath);

						template.AddVariable(_report);
						template.Generate();

						template.SaveAs(FileName);
					}
					catch(IOException)
					{
						Progress = _error;
						_interactiveService.ShowMessage(ImportanceLevel.Error,
							"Не удалось сохранить файл выгрузки. Возможно не закрыт предыдущий файл выгрузки", "Ошибка");
					}
				}
				catch(Exception e)
				{
					if(e.FindExceptionTypeInInner<TimeoutException>() != null)
					{
						Progress = _error;
						_interactiveService.ShowMessage(
							ImportanceLevel.Error, "Превышен интервал ожидания выполнения запроса.\n Попробуйте уменьшить период");
					}
					else
					{
						Progress = _error;
						throw;
					}
				}
			},
			() => !string.IsNullOrEmpty(FileName)
		));

		public DelegateCommand AllStatusCommand => _allStatusCommand ?? (_allStatusCommand = new DelegateCommand(
			() =>
			{
				foreach(var day in WeekDayName)
				{
					var item = (WeekDayNodes)day;
					item.Selected = true;
				}
			}, () => true
		));

		public DelegateCommand NoneStatusCommand => _noneStatusCommand ?? (_noneStatusCommand = new DelegateCommand(
			() =>
			{
				foreach(var day in WeekDayName)
				{
					var item = (WeekDayNodes)day;
					item.Selected = false;
				}
			}, () => true
		));

		public DelegateCommand ShowHelpCommand => _showHelpCommand ?? (_showHelpCommand = new DelegateCommand(
			() =>
			{
				_interactiveService.ShowMessage(ImportanceLevel.Info, $"Жду текст от Даши");
			}, () => true
		));
		#endregion
	}

	public class DeliveryAnalyticsReport
	{
		public DateTime CreationDate { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public IEnumerable<DeliveryAnalyticsReportRow> Rows { get; set; }
		public string SelectedFilters { get; set; }
	}

	public class DeliveryAnalyticsReportRow
	{
		public string GeographicGroupName;
		public string CityOrSuburb;
		public string DistrictName;
		public string DayOfWeek;
		public string DeliveryDate;

		public int CountSmallOrdersOneMorning;
		public int CountSmallOrders19LOneMorning;
		public int CountBigOrdersOneMorning;
		public int CountBigOrders19LOneMorning;
		public int SumSmallAndBigOrdersOneMorning;
		public int SumSmallAndBigOrders19LOneMorning;

		public int CountSmallOrdersOneDay;
		public int CountSmallOrders19LOneDay;
		public int CountBigOrdersOneDay;
		public int CountBigOrders19LOneDay;
		public int SumSmallAndBigOrdersOneDay;
		public int SumSmallAndBigOrders19LOneDay;

		public int CountSmallOrdersOneEvening;
		public int CountSmallOrders19LOneEvening;
		public int CountBigOrdersOneEvening;
		public int CountBigOrders19LOneEvening;
		public int SumSmallAndBigOrdersOneEvening;
		public int SumSmallAndBigOrders19LOneEvening;

		public int CountSmallOrdersOneFinal;
		public int CountSmallOrders19LOneFinal;
		public int CountBigOrdersOneFinal;
		public int CountBigOrders19LOneFinal;
		public int SumSmallAndBigOrdersOneFinal;
		public int SumSmallAndBigOrders19LOneFinal;

		public int CountSmallOrdersTwoDay;
		public int CountSmallOrders19LTwoDay;
		public int CountBigOrdersTwoDay;
		public int CountBigOrders19LTwoDay;
		public int SumSmallAndBigOrdersTwoDay;
		public int SumSmallAndBigOrders19LTwoDay;

		public int CountSmallOrdersThreeDay;
		public int CountSmallOrders19LThreeDay;
		public int CountBigOrdersThreeDay;
		public int CountBigOrders19LThreeDay;
		public int SumSmallAndBigOrdersThreeDay;
		public int SumSmallAndBigOrders19LThreeDay;

		public int CountSmallOrdersFinal;
		public int CountSmallOrders19LFinal;
		public int CountBigOrdersFinal;
		public int CountBigOrders19LFinal;
		public int SumSmallAndBigOrdersFinal;
		public int SumSmallAndBigOrders19LFinal;

		public DeliveryAnalyticsReportRow(DeliveryAnalyticsReportNode node)
		{
			GeographicGroupName = node.GeographicGroupName == "Север" ? "С" : "Ю";
			CityOrSuburb = node.CityOrSuburb == "Город" ? "Г" : "П";
			DistrictName = node.DistrictName;
			DayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[(int)node.DayOfWeek.DayOfWeek];
			DeliveryDate = node.DeliveryDate.ToShortDateString();

			CountSmallOrdersOneMorning = node.CountSmallOrdersOneMorning;
			CountSmallOrders19LOneMorning = (int)node.CountSmallOrders19LOneMorning;
			CountBigOrdersOneMorning = node.CountBigOrdersOneMorning;
			CountBigOrders19LOneMorning = (int)node.CountBigOrders19LOneMorning;
			SumSmallAndBigOrdersOneMorning = node.SumSmallAndBigOrdersOneMorning;
			SumSmallAndBigOrders19LOneMorning = (int)node.SumSmallAndBigOrders19LOneMorning;
			CountSmallOrdersOneDay = node.CountSmallOrdersOneDay;
			CountSmallOrders19LOneDay = (int)node.CountSmallOrders19LOneDay;
			CountBigOrdersOneDay = node.CountBigOrdersOneDay;
			CountBigOrders19LOneDay = (int)node.CountBigOrders19LOneDay;
			SumSmallAndBigOrdersOneDay = node.SumSmallAndBigOrdersOneDay;
			SumSmallAndBigOrders19LOneDay = (int)node.SumSmallAndBigOrders19LOneDay;
			CountSmallOrdersOneEvening = node.CountSmallOrdersOneEvening;
			CountSmallOrders19LOneEvening = (int)node.CountSmallOrders19LOneEvening;
			CountBigOrdersOneEvening = node.CountBigOrdersOneEvening;
			CountBigOrders19LOneEvening = (int)node.CountBigOrders19LOneEvening;
			SumSmallAndBigOrdersOneEvening = node.SumSmallAndBigOrdersOneEvening;
			SumSmallAndBigOrders19LOneEvening = (int)node.SumSmallAndBigOrders19LOneEvening;
			CountSmallOrdersOneFinal = node.CountSmallOrdersOneFinal;
			CountSmallOrders19LOneFinal = (int)node.CountSmallOrders19LOneFinal;
			CountBigOrdersOneFinal = node.CountBigOrdersOneFinal;
			CountBigOrders19LOneFinal = (int)node.CountBigOrders19LOneFinal;
			SumSmallAndBigOrdersOneFinal = node.SumSmallAndBigOrdersOneFinal;
			SumSmallAndBigOrders19LOneFinal = (int)node.SumSmallAndBigOrders19LOneFinal;
			CountSmallOrdersTwoDay = node.CountSmallOrdersTwoDay;
			CountSmallOrders19LTwoDay = (int)node.CountSmallOrders19LTwoDay;
			CountBigOrdersTwoDay = node.CountBigOrdersTwoDay;
			CountBigOrders19LTwoDay = (int)node.CountBigOrders19LTwoDay;
			SumSmallAndBigOrdersTwoDay = node.SumSmallAndBigOrdersTwoDay;
			SumSmallAndBigOrders19LTwoDay = (int)node.SumSmallAndBigOrders19LTwoDay;
			CountSmallOrdersThreeDay = node.CountSmallOrdersThreeDay;
			CountSmallOrders19LThreeDay = (int)node.CountSmallOrders19LThreeDay;
			CountBigOrdersThreeDay = node.CountBigOrdersThreeDay;
			CountBigOrders19LThreeDay = (int)node.CountBigOrders19LThreeDay;
			SumSmallAndBigOrdersThreeDay = node.SumSmallAndBigOrdersThreeDay;
			SumSmallAndBigOrders19LThreeDay = (int)node.SumSmallAndBigOrders19LThreeDay;
			CountSmallOrdersFinal = node.CountSmallOrdersFinal;
			CountSmallOrders19LFinal = (int)node.CountSmallOrders19LFinal;
			CountBigOrdersFinal = node.CountBigOrdersFinal;
			CountBigOrders19LFinal = (int)node.CountBigOrders19LFinal;
			SumSmallAndBigOrdersFinal = node.SumSmallAndBigOrdersFinal;
			SumSmallAndBigOrders19LFinal = (int)node.SumSmallAndBigOrders19LFinal;
		}
	}

	public enum WaveNodes
	{
		[Display(Name = "1 Волна")]
		FirstWave,
		[Display(Name = "2 Волна")]
		SecondWave,
		[Display(Name = "3 Волна")]
		ThirdWave
	}
}
