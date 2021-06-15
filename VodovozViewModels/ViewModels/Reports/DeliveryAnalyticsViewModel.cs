using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NHibernate.Criterion;
using NHibernate.Transform;
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
		private readonly IInteractiveService _interactiveService;
		public IEntityAutocompleteSelectorFactory DistrictSelectorFactory;
		private DateTime? startDeliveryDate = DateTime.Today;
		private DateTime? endDeliveryDate = DateTime.Today.AddDays(1).AddMinutes(-1);
		private DateTime? _dayOfWeek;
		private DelegateCommand exportCommand = null;
		private bool isLoadingData;

		public DeliveryAnalyticsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IEntityAutocompleteSelectorFactory districtSelectorFactory)
			: base(interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			DistrictSelectorFactory = districtSelectorFactory ?? throw new ArgumentNullException(nameof(districtSelectorFactory));
			if(unitOfWorkFactory == null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			Uow = unitOfWorkFactory.CreateWithoutRoot();
			Title = "Аналитика объёмов доставки";

			WaveList = new GenericObservableList<WaveNode>();
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
		}

		#region Поля и свойства

		public IUnitOfWork Uow;

		private District _district;

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
					OnPropertyChanged(nameof(HasRunReport));
				}
			}
		}

		public DateTime? EndDeliveryDate
		{
			get => endDeliveryDate;
			set => SetField(ref endDeliveryDate, value);
		}

		public DateTime? DayOfWeek
		{
			get => _dayOfWeek;
			set
			{
				if(SetField(ref _dayOfWeek, value))
				{
					OnPropertyChanged(nameof(HasRunReport));
				}
			}
		}

		public bool IsLoadingData
		{
			get => isLoadingData;
			set
			{
				if(isLoadingData != value)
				{
					isLoadingData = value;
					OnPropertyChanged(nameof(HasRunReport));
				}
			}
		}

		public bool HasRunReport => StartDeliveryDate.HasValue && !IsLoadingData;

		public GenericObservableList<GeographicGroupNode> GeographicGroupNodes { get; private set; }

		public GenericObservableList<WageDistrictNode> WageDistrictNodes { get; private set; }

		public GenericObservableList<WaveNode> WaveList { get; private set; }

		public string FileName { get; set; }

		public GenericObservableList<DeliveryAnalyticsReportNode> NodesList { get; } =
			new GenericObservableList<DeliveryAnalyticsReportNode>();

		private IEnumerable<DeliveryAnalyticsReportNode> _oneWaveMorning;
		private IEnumerable<DeliveryAnalyticsReportNode> _oneWaveDay;
		private IEnumerable<DeliveryAnalyticsReportNode> _oneWaveEvening;
		private IEnumerable<DeliveryAnalyticsReportNode> _twoWave;
		private IEnumerable<DeliveryAnalyticsReportNode> _threeWave;
		
		#endregion

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
		
		private void UpdateNodes()
		{
			var createDate4 = DateTime.Parse(StartDeliveryDate.Value.ToShortDateString() + " 4:00:00");
			var createDate12 = DateTime.Parse(StartDeliveryDate.Value.ToShortDateString() + " 12:00:00");
			var createDate18 = DateTime.Parse(StartDeliveryDate.Value.ToShortDateString() + " 18:00:00");
			
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
				.Left.JoinAlias(x => x.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
				.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geographicGroupAlias)
				.Left.JoinAlias(() => districtAlias.WageDistrict, () => wageDistrictAlias)
				.Left.JoinAlias(x => x.DeliverySchedule, () => deliveryScheduleAlias)
				.Inner.JoinAlias(x => x.OrderItems, () => orderItemAlias)
				.Inner.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias);
		
			query.Where(x => !x.SelfDelivery)
				.Where(x => !x.IsContractCloser)
				.Where(x => !x.IsService)
				.WhereRestrictionOn(x => x.OrderStatus)
				.Not.IsIn(new[] {OrderStatus.NewOrder, OrderStatus.Canceled, OrderStatus.WaitForPayment});

			if (StartDeliveryDate.HasValue) {
				query.Where(x => x.DeliveryDate >= StartDeliveryDate);
			}
		
			if (EndDeliveryDate.HasValue) {
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

			var _oneWaveMorningQuery = query.Clone();
			var _oneWaveEveningQuery = query.Clone();
			var _oneWaveDayQuery = query.Clone();
			var _twoWaveQuery = query.Clone();
			var _threeWaveQuery = query.Clone();
			if(selectedWages.Any(x => x.Name == "Город") || !selectedWages.Any())
			{
				_oneWaveMorningQuery.Where(x => x.CreateDate < createDate4 && deliveryScheduleAlias.From < createDate12.TimeOfDay);
				_oneWaveDayQuery.Where(x => x.CreateDate < createDate4 && (deliveryScheduleAlias.From >= createDate12.TimeOfDay && deliveryScheduleAlias.From < createDate18.TimeOfDay));
				_twoWaveQuery.Where(x => x.CreateDate >= createDate4 && x.CreateDate < createDate12 && deliveryScheduleAlias.From < createDate18.TimeOfDay);
				_threeWaveQuery.Where(x =>
					(x.CreateDate >= createDate12) ||
					(x.CreateDate >= createDate4 && x.CreateDate < createDate12 && deliveryScheduleAlias.From >= createDate18.TimeOfDay) ||
					(x.CreateDate < createDate4 && deliveryScheduleAlias.From >= createDate18.TimeOfDay));
			}
			
			if(selectedWages.Any(x => x.Name == "Пригород") || !selectedWages.Any())
			{
				_oneWaveEveningQuery.Where(x=>deliveryScheduleAlias.From >= createDate18.TimeOfDay);
				_oneWaveDayQuery.Where(x => deliveryScheduleAlias.From >= createDate12.TimeOfDay && deliveryScheduleAlias.From < createDate18.TimeOfDay);
				_oneWaveMorningQuery.Where(x => x.CreateDate < createDate12);
			}
			
			var bottleSmallCountSubquery = QueryOver.Of(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.And(x=>x.Count < 5)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count));
			
			var bootleBigCountSubquery = QueryOver.Of(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.And(x=>x.Count >= 5)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count));
			
			var bigCountOrders = QueryOver.Of(() => orderAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderAlias.DeliverySchedule, () => deliveryScheduleAlias)
				.Where(() => orderItemAlias.Count >= 5).ToRowCountQuery();

			var smallCountOrders = QueryOver.Of(() => orderAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderAlias.DeliverySchedule, () => deliveryScheduleAlias)
				.Where(() => orderItemAlias.Count < 5).ToRowCountQuery();

			#region SelectWaves

			_oneWaveMorning = _oneWaveMorningQuery
				.SelectList(list => list
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => geographicGroupAlias.Name).WithAlias(() => resultAlias.GeographicGroupName)
					.Select(() => wageDistrictAlias.Name).WithAlias(() => resultAlias.CityOrSuburb)
					.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
					.Select(() => orderAlias.CreateDate).WithAlias(() => resultAlias.DayOfWeek)
					.SelectSubQuery(smallCountOrders).WithAlias(() => resultAlias.CountSmallOrdersOneMorning)
					.SelectSubQuery(bigCountOrders).WithAlias(() => resultAlias.CountBigOrdersOneMorning)
					.SelectSubQuery(bottleSmallCountSubquery).WithAlias(() => resultAlias.CountSmallOrders19LOneMorning)
					.SelectSubQuery(bootleBigCountSubquery).WithAlias(() => resultAlias.CountBigOrders19LOneMorning))
				.TransformUsing(Transformers.AliasToBean<DeliveryAnalyticsReportNode>())
				.List<DeliveryAnalyticsReportNode>().Distinct();
			
			_oneWaveDay = _oneWaveDayQuery
				.SelectList(list => list
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => geographicGroupAlias.Name).WithAlias(() => resultAlias.GeographicGroupName)
					.Select(() => wageDistrictAlias.Name).WithAlias(() => resultAlias.CityOrSuburb)
					.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
					.Select(() => orderAlias.CreateDate).WithAlias(() => resultAlias.DayOfWeek)
					.SelectSubQuery(smallCountOrders).WithAlias(() => resultAlias.CountSmallOrdersOneDay)
					.SelectSubQuery(bigCountOrders).WithAlias(() => resultAlias.CountBigOrdersOneDay)
					.SelectSubQuery(bottleSmallCountSubquery).WithAlias(() => resultAlias.CountSmallOrders19LOneDay)
					.SelectSubQuery(bootleBigCountSubquery).WithAlias(() => resultAlias.CountBigOrders19LOneDay))
				.TransformUsing(Transformers.AliasToBean<DeliveryAnalyticsReportNode>())
				.List<DeliveryAnalyticsReportNode>().Distinct();
			
			_oneWaveEvening = _oneWaveEveningQuery
				.SelectList(list => list
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => geographicGroupAlias.Name).WithAlias(() => resultAlias.GeographicGroupName)
					.Select(() => wageDistrictAlias.Name).WithAlias(() => resultAlias.CityOrSuburb)
					.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
					.Select(() => orderAlias.CreateDate).WithAlias(() => resultAlias.DayOfWeek)
					.SelectSubQuery(smallCountOrders).WithAlias(() => resultAlias.CountSmallOrdersOneEvening)
					.SelectSubQuery(bigCountOrders).WithAlias(() => resultAlias.CountBigOrdersOneEvening)
					.SelectSubQuery(bottleSmallCountSubquery).WithAlias(() => resultAlias.CountSmallOrders19LOneEvening)
					.SelectSubQuery(bootleBigCountSubquery).WithAlias(() => resultAlias.CountBigOrders19LOneEvening))
				.TransformUsing(Transformers.AliasToBean<DeliveryAnalyticsReportNode>())
				.List<DeliveryAnalyticsReportNode>().Distinct();
			
			_twoWave = _twoWaveQuery
				.SelectList(list => list
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => geographicGroupAlias.Name).WithAlias(() => resultAlias.GeographicGroupName)
					.Select(() => wageDistrictAlias.Name).WithAlias(() => resultAlias.CityOrSuburb)
					.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
					.Select(() => orderAlias.CreateDate).WithAlias(() => resultAlias.DayOfWeek)
					.SelectSubQuery(smallCountOrders).WithAlias(() => resultAlias.CountSmallOrdersOneMorning)
					.SelectSubQuery(bigCountOrders).WithAlias(() => resultAlias.CountBigOrdersOneMorning)
					.SelectSubQuery(bottleSmallCountSubquery).WithAlias(() => resultAlias.CountSmallOrders19LOneMorning)
					.SelectSubQuery(bootleBigCountSubquery).WithAlias(() => resultAlias.CountBigOrders19LOneMorning))
				.TransformUsing(Transformers.AliasToBean<DeliveryAnalyticsReportNode>())
				.List<DeliveryAnalyticsReportNode>().Distinct();
			
			_threeWave = _threeWaveQuery
				.SelectList(list => list
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => geographicGroupAlias.Name).WithAlias(() => resultAlias.GeographicGroupName)
					.Select(() => wageDistrictAlias.Name).WithAlias(() => resultAlias.CityOrSuburb)
					.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
					.Select(() => orderAlias.CreateDate).WithAlias(() => resultAlias.DayOfWeek)
					.SelectSubQuery(smallCountOrders).WithAlias(() => resultAlias.CountSmallOrdersOneMorning)
					.SelectSubQuery(bigCountOrders).WithAlias(() => resultAlias.CountBigOrdersOneMorning)
					.SelectSubQuery(bottleSmallCountSubquery).WithAlias(() => resultAlias.CountSmallOrders19LOneMorning)
					.SelectSubQuery(bootleBigCountSubquery).WithAlias(() => resultAlias.CountBigOrders19LOneMorning))
				.TransformUsing(Transformers.AliasToBean<DeliveryAnalyticsReportNode>())
				.List<DeliveryAnalyticsReportNode>().Distinct();
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
			foreach(var reportNodes in _oneWaveMorning.Concat(_oneWaveDay).Concat(_oneWaveEvening).Concat(_twoWave).Concat(_threeWave).Distinct()
				.GroupBy(x => new {x.GeographicGroupName, x.CityOrSuburb, x.DistrictName, x.DayOfWeek.Date, x.DeliveryDate})
				.OrderByDescending(x=>x.Key.GeographicGroupName).ThenBy(x=>x.Key.CityOrSuburb))
			{
				sb.AppendLine(new DeliveryAnalyticsReportNode(reportNodes, count).ToString());
				count++;
			}
			return sb.ToString();
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
