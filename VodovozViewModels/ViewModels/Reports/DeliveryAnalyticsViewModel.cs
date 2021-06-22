using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using Org.BouncyCastle.Math.EC.Rfc7748;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.ErrorReporting;
using QS.Navigation;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports.DeliveryAnalytics;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports
{
	public class DeliveryAnalyticsViewModel : TabViewModelBase
	{
		#region Поля
		private DateTime? startDeliveryDate;
		private DateTime? endDeliveryDate;
		private District _district;
		
		private DelegateCommand exportCommand;
		private DelegateCommand allStatusCommand;
		private DelegateCommand noneStatusCommand;
		private DelegateCommand showHelpCommand;
		
		private IEnumerable<DeliveryAnalyticsReportNode> _oneWaveMorning;
		private IEnumerable<DeliveryAnalyticsReportNode> _oneWaveDay;
		private IEnumerable<DeliveryAnalyticsReportNode> _oneWaveEvening;
		private IEnumerable<DeliveryAnalyticsReportNode> _twoWave;
		private IEnumerable<DeliveryAnalyticsReportNode> _threeWave;

		private readonly IInteractiveService _interactiveService;
		public IEntityAutocompleteSelectorFactory DistrictSelectorFactory;
		
		#endregion
		public DeliveryAnalyticsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IEntityAutocompleteSelectorFactory districtSelectorFactory)
			: base(interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			DistrictSelectorFactory = districtSelectorFactory ?? throw new ArgumentNullException(nameof(districtSelectorFactory));
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			Uow = unitOfWorkFactory.CreateWithoutRoot();
			Title = "Аналитика объёмов доставки";

			WaveList = new GenericObservableList<WaveNode>();
			WeekDayName = new GenericObservableList<WeekDayNodes>();
			GeographicGroupNodes =
				new GenericObservableList<GeographicGroupNode>(Uow.GetAll<GeographicGroup>().Select(x => new GeographicGroupNode(x))
					.ToList());
			WageDistrictNodes =
				new GenericObservableList<WageDistrictNode>(Uow.GetAll<WageDistrict>().Select(x => new WageDistrictNode(x)).ToList());

			foreach(var wave in Enum.GetValues(typeof(WaveNodes)))
			{
				var waveNode = new WaveNode {WaveNodes = (WaveNodes) wave, Selected = false};
				WaveList.Add(waveNode);
			}
			
			foreach(var week in Enum.GetValues(typeof(WeekDayName)))
			{
				if((WeekDayName)week == Domain.Sale.WeekDayName.Today) continue;
				var weekNode = new WeekDayNodes {WeekNameNode = (WeekDayName) week, Selected = false};
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
			get => startDeliveryDate;
			set
			{
				if(SetField(ref startDeliveryDate, value))
				{
					OnPropertyChanged(nameof(HasExportReport));
				}
			}
		}

		public DateTime? EndDeliveryDate
		{
			get => endDeliveryDate;
			set => SetField(ref endDeliveryDate, value);
		}

		public bool HasExportReport => StartDeliveryDate.HasValue;

		public GenericObservableList<GeographicGroupNode> GeographicGroupNodes { get; private set; }

		public GenericObservableList<WageDistrictNode> WageDistrictNodes { get; private set; }

		public GenericObservableList<WaveNode> WaveList { get; private set; }
		
		public GenericObservableList<WeekDayNodes> WeekDayName { get; set; }

		public string FileName { get; set; }
		#endregion

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
			GeographicGroup geographicGroupAlias = null;
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
				.Where(x => !x.IsService)
				.WhereRestrictionOn(x => x.OrderStatus)
				.Not.IsIn(new[] {OrderStatus.NewOrder, OrderStatus.Canceled, OrderStatus.WaitForPayment});

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

		private string GetCsvString()
		{
			var sb = new StringBuilder();
			sb.AppendLine(
				"Общие данные;Общие данные;Общие данные;Общие данные;Общие данные;Общие данные;1 Волна;1 Волна;1 Волна;1 Волна;1 Волна;" +
				"1 Волна;1 Волна;1 Волна;1 Волна;1 Волна;1 Волна;1 Волна;1 Волна;1 Волна;1 Волна;1 Волна;1 Волна;1 Волна;1 Волна;1 Волна;" +
				"1 Волна;1 Волна;1 Волна;1 Волна;2 Волна(Д);2 Волна(Д);2 Волна(Д);2 Волна(Д);2 Волна(Д);2 Волна(Д);3 Волна(В);3 Волна(В);" +
				"3 Волна(В);3 Волна(В);3 Волна(В);3 Волна(В);Всего по сектору;Всего по сектору;Всего по сектору;Всего по сектору;" +
				"Всего по сектору;Всего по сектору;");
			sb.AppendLine(" ; ; ; ; ; ;У;У;У;У;У;У;Д;Д;Д;Д;Д;Д;В;В;В;В;В;В;Итого волна;Итого волна;Итого волна;Итого волна;Итого волна;" +
			              "Итого волна;Итого волна;Итого волна;Итого волна;Итого волна;Итого волна;Итого волна;Итого волна;Итого волна;" +
			              "Итого волна;Итого волна;Итого волна;Итого волна;Всего по сектору;Всего по сектору;Всего по сектору;" +
			              "Всего по сектору;Всего по сектору;Всего по сектору;");
			sb.AppendLine("№;Ю/С;Ч;Сектор;Д.н.;Дата;М (.);М б.;К (.);К б;И (.);И б.;М (.);М б.;К (.);К б;И (.);И б.;" +
			              "М (.);М б.;К (.);К б;И (.);И б.;М (.);М б.;К (.);К б;И (.);И б.;М (.);М б.;К (.);К б;И (.);И б.;" +
			              "М (.);М б.;К (.);К б;И (.);И б.;М (.);М б.;К (.);К б;В (.);В б.;");
			
			var count = 1;
			var selectedDays = WeekDayName.Where(x => x.Selected).Select(x => x.WeekNameNode);
			var selectedWages = WageDistrictNodes.Where(x => x.Selected).Select(x => x.WageDistrict);
			var selectedWaves = WaveList.Where(x => x.Selected).Select(x => x.WaveNodes);

			var nodesCsv = new List<DeliveryAnalyticsReportNode>();
			if(selectedWaves.Any(x => x == WaveNodes.FirstWave)
			   || !selectedWaves.Any())
			{
				foreach(var reportNodes in _oneWaveMorning.Concat(_oneWaveDay).Concat(_oneWaveEvening)
					.GroupBy(x => new { x.GeographicGroupName, x.CityOrSuburb ,x.DistrictName, x.DayOfWeek.Date, x.DeliveryDate}))
				{
					var weekDayName = (WeekDayName) Enum.Parse(typeof(WeekDayName), reportNodes.Key.Date.DayOfWeek.ToString());
					if((selectedDays.Contains(weekDayName) || !selectedDays.Any())
					   && (selectedWages.Any(x=>x.Name == reportNodes.Key.CityOrSuburb) || !selectedWages.Any()))
					{
						nodesCsv.Add(new DeliveryAnalyticsReportNode(reportNodes, count));
					}
				}
			}
			
			if(selectedWaves.Any(x => x == WaveNodes.SecondWave)
			        || !selectedWaves.Any())
			{
				foreach(var reportNodes in _twoWave
					.GroupBy(x => new { x.GeographicGroupName, x.CityOrSuburb ,x.DistrictName, x.DayOfWeek.Date, x.DeliveryDate}))
				{
					var weekDayName = (WeekDayName) Enum.Parse(typeof(WeekDayName), reportNodes.Key.Date.DayOfWeek.ToString());
					if((selectedDays.Contains(weekDayName) || !selectedDays.Any())
					   && (selectedWages.Any(x=>x.Name == reportNodes.Key.CityOrSuburb) || !selectedWages.Any()))
					{
						nodesCsv.Add(new DeliveryAnalyticsReportNode(reportNodes, count));
					}
				}
			}
			
			if(selectedWaves.Any(x => x == WaveNodes.ThirdWave)
			        || !selectedWaves.Any())
			{
				foreach(var reportNodes in _threeWave
					.GroupBy(x => new { x.GeographicGroupName, x.CityOrSuburb ,x.DistrictName, x.DayOfWeek.Date, x.DeliveryDate}))
				{
					var weekDayName = (WeekDayName) Enum.Parse(typeof(WeekDayName), reportNodes.Key.Date.DayOfWeek.ToString());
					if((selectedDays.Contains(weekDayName) || !selectedDays.Any())
					   && (selectedWages.Any(x=>x.Name == reportNodes.Key.CityOrSuburb) || !selectedWages.Any()))
					{
						nodesCsv.Add(new DeliveryAnalyticsReportNode(reportNodes, count));
					}
				}
			}
			
			foreach(var groupNode in nodesCsv.OrderByDescending(x => x.GeographicGroupName)
				.ThenBy(x=>x.CityOrSuburb)
				.ThenBy(x=>x.DistrictName)
				.ThenBy(x => ((int)x.DayOfWeek.DayOfWeek + 6) % 7)
				.ThenBy(x => x.DeliveryDate)
				.GroupBy(x=> new
				{
					x.DistrictName, x.DayOfWeek
				}
				))
			{
				sb.AppendLine(count.ToString() + new DeliveryAnalyticsReportNode(groupNode, count));
					count++;
			}
			return sb.ToString();
		}

		#region Команды
		public DelegateCommand ExportCommand => exportCommand ?? (exportCommand = new DelegateCommand(
			() =>
			{
				try
				{
					UpdateNodes();
					if(string.IsNullOrWhiteSpace(FileName))
					{
						throw new InvalidOperationException($"Не был заполнен путь выгрузки: {nameof(FileName)}");
					}
					var exportString = GetCsvString();
					try
					{
						File.WriteAllText(FileName, exportString, Encoding.UTF8);
					}
					catch(IOException)
					{
						_interactiveService.ShowMessage(ImportanceLevel.Error,
							"Не удалось сохранить файл выгрузки. Возможно не закрыт предыдущий файл выгрузки", "Ошибка");
					}
				}
				catch(Exception e)
				{
					if (e.FindExceptionTypeInInner<TimeoutException>() != null)
					{
						_interactiveService.ShowMessage(
							ImportanceLevel.Error, "Превышен интервал ожидания выполнения запроса.\n Попробуйте уменьшить период");
					}
					else
					{
						throw;
					}
				}
			},
			() => !string.IsNullOrEmpty(FileName)
		));
		
		public DelegateCommand AllStatusCommand => allStatusCommand ?? (allStatusCommand = new DelegateCommand(
			() =>
			{
				foreach(var day in WeekDayName)
				{
					var item = (WeekDayNodes) day;
					item.Selected = true;
				}
			}, () => true
		));
		
		public DelegateCommand NoneStatusCommand => noneStatusCommand ?? (noneStatusCommand = new DelegateCommand(
			() =>
			{
				foreach(var day in WeekDayName)
				{
					var item = (WeekDayNodes) day;
					item.Selected = false;
				}
			}, () => true
		));
		
		public DelegateCommand ShowHelpCommand => showHelpCommand ?? (showHelpCommand = new DelegateCommand(
			() =>
			{
				_interactiveService.ShowMessage(ImportanceLevel.Info, $"Жду текст от Даши");
			}, () => true
		));
		#endregion
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
