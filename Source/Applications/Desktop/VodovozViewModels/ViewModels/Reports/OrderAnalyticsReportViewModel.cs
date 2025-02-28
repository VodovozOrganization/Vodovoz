using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Utilities.Debug;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.WageCalculation;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Reports
{
	public class OrderAnalyticsReportViewModel : UowDialogViewModelBase
    {
        private readonly IInteractiveService interactiveService;
        private readonly Encoding encoding = Encoding.GetEncoding(1251);

        #region Help

        private const string HELP = "<b>Сохранение в файл</b>: " +
                                    "Для выгрузки данных, нажмите на кнопку \"Экспорт\" выберите папку и файл, куда будут сохранены данные. " +
                                    "Если нужно сохранить в новый файл, то укажите его имя в верней строке и нажмите " +
                                    "кнопку \"Сохранить\"\n\n" +
                                    "<b>В отчет не попадают</b>:\n" +
                                    "- самовывозы;\n" +
                                    "- закрывающие документы;\n" +
                                    "- выезды мастера;\n" +
                                    "- заказы закрытые без доставки;\n" +
                                    "- заказы в статусах: Новый, Отменен, Ожидание оплаты;\n" +
                                    "- без наших фур";

        #endregion

        private bool hasExportReport;
        private bool isLoadingData;
        private string progress = string.Empty;
        private const string DataLoaded = "Загрузка завершена.";
        private const string Error = "Произошла ошибка.";
        private DateTime? startDeliveryDate = DateTime.Today;
        private DateTime? endDeliveryDate = DateTime.Today;
        private DateTime? startCreationDate;
        private DateTime? endCreationDate;
        private DelegateCommand exportCommand = null;
        private DelegateCommand helpCommand = null;
        private DelegateCommand runReportCommand = null;
        public string LoadingData = "Идет загрузка данных...";

        public OrderAnalyticsReportViewModel(
            IUnitOfWorkFactory unitOfWorkFactory,
            IInteractiveService interactiveService,
            INavigationManager navigationManager) : base(unitOfWorkFactory, navigationManager)
        {
            this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

            if (unitOfWorkFactory == null) {
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            }

            Title = "Отчет аналитика заказов";
        }
        
        public DateTime? StartDeliveryDate
        {
            get => startDeliveryDate;
            set 
            {
                if (SetField(ref startDeliveryDate, value)) 
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
        
        public DateTime? StartCreationDate
        {
            get => startCreationDate;
            set 
            {
                if (SetField(ref startCreationDate, value)) 
                {
                    OnPropertyChanged(nameof(HasRunReport));
                }
            } 
        }
        
        public DateTime? EndCreationDate
        {
            get => endCreationDate;
            set => SetField(ref endCreationDate, value);
        }
        
        public bool HasExportReport
        {
	        get => hasExportReport;
	        set => SetField(ref hasExportReport, value);
        }
        
        public bool IsLoadingData
        {
	        get => isLoadingData;
	        set
	        {
		        if (isLoadingData != value)
		        {
			        isLoadingData = value;
			        OnPropertyChanged(nameof(HasRunReport));
		        }
	        }
        }
        
        public string Progress
        {
	        get => progress;
	        set => SetField(ref progress, value);
        }

        public string FileName { get; set; }

        public bool HasRunReport => (StartCreationDate.HasValue || StartDeliveryDate.HasValue) && !IsLoadingData;
        
        public GenericObservableList<OrderAnalyticsReportNode> NodesList { get; } =
	        new GenericObservableList<OrderAnalyticsReportNode>();
        
        public DelegateCommand ExportCommand => exportCommand ?? (exportCommand = new DelegateCommand(
            () => 
            {
                try
                {
                    CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
	                    Delimiter = ";", TrimOptions = TrimOptions.Trim
                    };
                    configuration.RegisterClassMap<OrderAnalyticsReportNodeMap>();

                    if (!FileName.EndsWith(".csv"))
                    {
	                    FileName = $"{FileName}.csv";
                    }

                    using (var writer = new StreamWriter($"{FileName}", false, encoding))
                    using (var csv = new CsvWriter(writer, configuration))
                    {
                        csv.WriteRecords(NodesList);
                    }
                }
                catch (Exception e) {
                    interactiveService.ShowMessage(ImportanceLevel.Error, $"При выгрузке произошла ошибка.\n{e.Message}");
                }
            },
            () => !string.IsNullOrEmpty(FileName)
        ));
        
        public DelegateCommand HelpCommand => helpCommand ?? (helpCommand = new DelegateCommand(
            () => 
            {
                interactiveService.ShowMessage(ImportanceLevel.Info, HELP, "Справка");
            },
            () => true
        ));
        
        public DelegateCommand RunReportCommand => runReportCommand ?? (runReportCommand = new DelegateCommand(
	        () => 
	        {
		        try
		        {
			        var list = Task.Run(UpdateNodes);

			        foreach (var item in list.Result)
			        {
				        NodesList.Add(item);
			        }

			        Progress = DataLoaded;
			        HasExportReport = true;
		        }
		        catch (Exception ex)
		        {
			        Progress = Error;
			        if (ex.FindExceptionTypeInInner<TimeoutException>() != null)
			        {
				        interactiveService.ShowMessage(
					        ImportanceLevel.Error, "Превышен интервал ожидания выполнения запроса.\n Попробуйте уменьшить период");
			        }
			        else
			        {
				        throw;
			        }
		        }
		        finally
		        {
			        IsLoadingData = false;
		        }
	        },
	        () => true
        ));

        private IEnumerable<OrderAnalyticsReportNode> UpdateNodes()
        {
	        HasExportReport = false;
	        NodesList.Clear();
            
            OrderAnalyticsReportNode resultAlias = null;
            Order orderAlias = null;
            Order orderAlias2 = null;
            DeliverySchedule deliveryScheduleAlias = null;
            DeliveryPoint deliveryPointAlias = null;
            District districtAlias = null;
            DistrictsSet districtsSetAlias = null;
            WageDistrict wageDistrictAlias = null;
            RouteListItem routeListItemAlias = null;
            RouteListItem routeListItemAlias2 = null;
            RouteList routeListAlias = null;
            Car carAlias = null;
            CarVersion carVersionAlias = null;
            CarModel carModelAlias = null;
            Employee driverAlias = null;
            Employee forwarderAlias = null;
            OrderItem orderItemAlias = null;
            Nomenclature nomenclatureAlias = null;
            GeoGroup geographicGroupAlias = null;

            var query = UoW.Session.QueryOver(() => orderAlias)
                .JoinAlias(x => x.DeliverySchedule, () => deliveryScheduleAlias)
                .JoinAlias(x => x.DeliveryPoint, () => deliveryPointAlias)
                .JoinEntityAlias(() => routeListItemAlias, () => routeListItemAlias.Order.Id == orderAlias.Id, JoinType.LeftOuterJoin)
                .Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
                .Left.JoinAlias(() => districtAlias.GeographicGroup, () => geographicGroupAlias)
                .Left.JoinAlias(() => districtAlias.WageDistrict, () => wageDistrictAlias)
                .JoinAlias(() => districtAlias.DistrictsSet, () => districtsSetAlias)
                .Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
                .Left.JoinAlias(() => routeListAlias.Car, () => carAlias)
                .Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
                .Left.JoinAlias(() => routeListAlias.Forwarder, () => forwarderAlias)
                .Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
                .JoinEntityAlias(() => carVersionAlias,
	                () => carVersionAlias.Car.Id == carAlias.Id
		                && carVersionAlias.StartDate <= routeListAlias.Date
		                && (carVersionAlias.EndDate == null || carVersionAlias.EndDate >= routeListAlias.Date),
	                JoinType.LeftOuterJoin);

            query.Where(x => !x.SelfDelivery)
	             .Where(x => !x.IsContractCloser)
                 .Where(x => x.OrderAddressType != OrderAddressType.Service)
                 .WhereRestrictionOn(x => x.OrderStatus)
                    .Not.IsIn(new[]{OrderStatus.NewOrder, OrderStatus.Canceled, OrderStatus.WaitForPayment})
	             .Where(() => carModelAlias.Id == null || carModelAlias.CarTypeOfUse != CarTypeOfUse.Truck)
                 .Where(() => districtsSetAlias.Status == DistrictsSetStatus.Active)
	             .WithSubquery.WhereNotExists(
		             QueryOver.Of(() => orderAlias2)
			             .JoinEntityAlias(() => routeListItemAlias2, () => routeListItemAlias2.Order.Id == orderAlias2.Id, JoinType.LeftOuterJoin)
			             .Where(o => o.Id == orderAlias.Id)
			             .And(o => o.OrderStatus == OrderStatus.Closed)
			             .And(() => routeListItemAlias2.RouteList == null)
			             .Select(x => x.Id));

            if (StartDeliveryDate.HasValue) {
                query.Where(x => x.DeliveryDate >= StartDeliveryDate);
            }

            if (EndDeliveryDate.HasValue) {
                query.Where(x => x.DeliveryDate <= EndDeliveryDate);
            }
            
            if (StartCreationDate.HasValue) {
                query.Where(x => x.CreateDate >= StartCreationDate);
            }
            
            if (EndCreationDate.HasValue) {
	            query.Where(x => x.CreateDate.Value.Date <= EndCreationDate.Value.AddDays(1).AddMilliseconds(-1));
            }
			
            var bottleCountSubquery = QueryOver.Of(() => orderItemAlias)
                .Where(() => orderAlias.Id == orderItemAlias.Order.Id)
                .JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
                .Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
                .Select(Projections.Sum(() => orderItemAlias.Count)); 
            
            var orderSumSubquery = QueryOver.Of(() => orderItemAlias)
	            .Where(() => orderAlias.Id == orderItemAlias.Order.Id)
	            .Select(Projections.Sum(() => (orderItemAlias.Count * orderItemAlias.Price) - orderItemAlias.DiscountMoney)); 
            
            var bottleSumSubquery = QueryOver.Of(() => orderItemAlias)
	            .Where(() => orderAlias.Id == orderItemAlias.Order.Id)
	            .JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
	            .Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
	            .Select(Projections.Sum(() => (orderItemAlias.Count * orderItemAlias.Price) - orderItemAlias.DiscountMoney));

            var result = query
                .SelectList(list => list
                    .Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
                    .Select(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverLastName)
                    .Select(() => driverAlias.Name).WithAlias(() => resultAlias.DriverName)
                    .Select(() => driverAlias.Patronymic).WithAlias(() => resultAlias.DriverPatronymic)
                    .Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.OrderStatus)
                    .Select(() => routeListItemAlias.Status).WithAlias(() => resultAlias.RouteListItemStatus)
                    .Select(() => routeListAlias.Id).WithAlias(() => resultAlias.RouteListId)
                    .Select(() => carModelAlias.Name).WithAlias(() => resultAlias.CarModelName)
                    .Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarRegNumber)
                    .Select(() => carModelAlias.CarTypeOfUse).WithAlias(() => resultAlias.CarTypeOfUse)
                    .Select(() => carVersionAlias.CarOwnType).WithAlias(() => resultAlias.CarOwnType)
                    .Select(() => driverAlias.IsDriverForOneDay).WithAlias(() => resultAlias.IsDriverForOneDay)
                    .SelectSubQuery(bottleCountSubquery).WithAlias(() => resultAlias.Bottles19LCount)
                    .Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.Address)
                    .Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.DistrictName)
                    .Select(() => geographicGroupAlias.Name).WithAlias(() => resultAlias.GeographicGroupName)
                    .Select(() => wageDistrictAlias.Name).WithAlias(() => resultAlias.CityOrSuburb)
                    .Select(() => deliveryScheduleAlias.From).WithAlias(() => resultAlias.DeliveryScheduleFrom)
                    .Select(() => deliveryScheduleAlias.To).WithAlias(() => resultAlias.DeliveryScheduleTo)
                    .Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
                    .Select(() => orderAlias.CreateDate).WithAlias(() => resultAlias.CreationDate)
                    .Select(() => routeListItemAlias.DriverWage).WithAlias(() => resultAlias.DriverWage)
                    .Select(() => forwarderAlias.LastName).WithAlias(() => resultAlias.ForwarderLastName)
                    .Select(() => forwarderAlias.Name).WithAlias(() => resultAlias.ForwarderName)
                    .Select(() => forwarderAlias.Patronymic).WithAlias(() => resultAlias.ForwarderPatronymic)
                    .SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.OrderSum)
                    .SelectSubQuery(bottleSumSubquery).WithAlias(() => resultAlias.Bottles19LSum)
                )
                .TransformUsing(Transformers.AliasToBean<OrderAnalyticsReportNode>())
                .List<OrderAnalyticsReportNode>();

            return result;
        }
    }

    public class OrderAnalyticsReportNode
    {
        public int Id { get; set; }
        public string DriverLastName { get; set; }
        public string DriverName { get; set; }
        public string DriverPatronymic { get; set; }
        public string DriverFullName => $"{DriverLastName} {DriverName} {DriverPatronymic}";
        public OrderStatus OrderStatus { get; set; }
        public RouteListItemStatus? RouteListItemStatus { get; set; }
        public int? RouteListId { get; set; }
        public string CarModelName { get; set; }
        public string CarRegNumber { get; set; }
        public CarTypeOfUse? CarTypeOfUse { get; set; }
        public CarOwnType? CarOwnType { get; set; }
        public bool? IsDriverForOneDay { get; set; }
        public decimal Bottles19LCount { get; set; }
        public string Address { get; set; }
        public string DistrictName { get; set; }
        public string GeographicGroupName { get; set; }
        public string CityOrSuburb { get; set; }
        public TimeSpan DeliveryScheduleFrom { get; set; }
        public TimeSpan DeliveryScheduleTo { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime CreationDate { get; set; }
        public decimal DriverWage { get; set; }
        public string ForwarderLastName { get; set; }
        public string ForwarderName { get; set; }
        public string ForwarderPatronymic { get; set; }
        public string ForwarderFullName => $"{ForwarderLastName} {ForwarderName} {ForwarderPatronymic}";
        public decimal OrderSum { get; set; }
        public decimal Bottles19LSum { get; set; }
        public decimal Bottles19LAvgPrice => Bottles19LCount > 0 ? Bottles19LSum / Bottles19LCount: 0;

    }
    
    public sealed class OrderAnalyticsReportNodeMap : ClassMap<OrderAnalyticsReportNode>
    {
        public OrderAnalyticsReportNodeMap()
        {
            Map(x => x.Id).Index(0).Name("Номер заказа");
            Map(x => x.RouteListId).Index(1).Name("Номер МЛ");
            Map(x => x.DriverFullName).Index(2).Name("ФИО водителя");
            Map(x => x.OrderStatus).Index(3).Name("Статус заказа").TypeConverter<NullableEnumConverter>();
            Map(x => x.RouteListItemStatus).Index(4).Name("Статус адреса").TypeConverter<NullableEnumConverter>();
            Map(x => x.CarModelName).Index(5).Name("Модель авто");
            Map(x => x.CarRegNumber).Index(6).Name("Номер авто");
            Map(x => x.CarTypeOfUse).Index(7).Name("Тип авто").TypeConverter<NullableEnumConverter>();
            Map(x => x.CarOwnType).Index(8).Name("Принадлежность авто").TypeConverter<NullableBooleanConverter>();
            Map(x => x.IsDriverForOneDay).Index(9).Name("Разовый водитель?").TypeConverter<NullableBooleanConverter>();
            Map(x => x.Bottles19LCount).Index(10).Name("19л бутылей").Default(0).TypeConverter<DecimalConverter>();
            Map(x => x.Address).Index(11).Name("Адрес");
            Map(x => x.DistrictName).Index(12).Name("Район");
            Map(x => x.GeographicGroupName).Index(13).Name("Часть города");
            Map(x => x.CityOrSuburb).Index(14).Name("Город/Пригород");
            Map(x => x.DeliveryScheduleFrom).Index(15).Name("Интервал от");
            Map(x => x.DeliveryScheduleTo).Index(16).Name("Интервал до");
            Map(x => x.DeliveryDate).Index(17).Name("Дата доставки").TypeConverter<DeliveryDateTimeConverter>();
            Map(x => x.CreationDate).Index(18).Name("Дата создания заказа").TypeConverter<CreationDateTimeConverter>();
            Map(x => x.DriverWage).Index(19).Name("ЗП водителя за адрес").TypeConverter<DecimalConverter>();
            Map(x => x.ForwarderFullName).Index(20).Name("ФИО экспедитора").Default("Без экспедитора");
            Map(x => x.OrderSum).Index(21).Name("Общая сумма заказа").TypeConverter<DecimalConverter>();
            Map(x => x.Bottles19LSum).Index(22).Name("Сумма за 19л бутыли").TypeConverter<DecimalConverter>();
            Map(x => x.Bottles19LAvgPrice).Index(23).Name("Cредняя стоимость 19л в заказе").TypeConverter<DecimalConverter>();
        }
    }
    
    public class DeliveryDateTimeConverter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value is DateTime d) {
                return d.ToShortDateString();
            }

            return string.Empty;
        }
    }
    
    public class CreationDateTimeConverter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value is DateTime d) {
                return d.ToString(CultureInfo.CurrentCulture);
            }

            return string.Empty;
        }
    }
    
    public class NullableBooleanConverter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
	        if (value is bool b)
	        {
		        return b ? "Да" : "Нет";
	        }

            return string.Empty;
        }
    }
    
    public class NullableEnumConverter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
	        if (value is Enum en)
	        {
		        return en.GetEnumTitle();
	        }
	        
	        return string.Empty;
        }
    }
    
    public class DecimalConverter : DefaultTypeConverter
    {
	    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
	    {
		    return $"{decimal.Round((decimal)value, 2)}";
	    }
    }
}
