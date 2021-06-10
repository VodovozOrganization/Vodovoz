using System;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.ViewModels.Reports.DeliveryAnalytics
{
	public class DeliveryAnalyticsReportNode
	{
		public int Id { get; set; }
		public string GeographicGroupName { get; set; }
		public string CityOrSuburb { get; set; }
		public string DistrictName { get; set; }
		public DateTime DayOfWeek { get; set; }
		public DateTime DeliveryDate { get; set; }
		public int CountSmallOrders { get; set; }
		public decimal CountSmallOrders19L { get; set; }
		public int CountBigOrders { get; set; }
		public decimal CountBigOrders19L { get; set; }
		public int SumSmallAndBigOrders { get; set; }
		public decimal SumSmallAndBigOrders19L { get; set; }
	}
	
	public sealed class DeliveryAnalyticsReportNodeMap : ClassMap<DeliveryAnalyticsReportNode>
	{
		public DeliveryAnalyticsReportNodeMap()
		{
			Map(x => x.Id).Index(0).Name("Номер");
			Map(x => x.GeographicGroupName).Index(1).Name("Сектор");
			Map(x => x.CityOrSuburb).Index(2).Name("Город/Пригород");
			Map(x => x.DistrictName).Index(3).Name("Район");
			Map(x => x.DayOfWeek).Index(4).Name("День недели").TypeConverter<DayOfWeekTimeConverter>();
			Map(x => x.DeliveryDate).Index(5).Name("Дата доставки").TypeConverter<DeliveryDateTimeConverter>();
			Map(x => x.CountSmallOrders).Index(6).Name("М (.)");
			Map(x => x.CountSmallOrders19L).Index(7).Name("М б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountBigOrders).Index(8).Name("К (.)");
			Map(x => x.CountBigOrders19L).Index(9).Name("К б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.SumSmallAndBigOrders).Index(10).Name("И (.)");
			Map(x => x.SumSmallAndBigOrders19L).Index(11).Name("И б.").Default(0).TypeConverter<DecimalConverter>();
		}
	}
	
	public class DayOfWeekTimeConverter : DefaultTypeConverter
	{
		public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
		{
			if (value is DateTime d) {
				return d.Day.ToString();
			}

			return string.Empty;
		}
	}
	
	public class DecimalConverter : DefaultTypeConverter
	{
		public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
		{
			return decimal.Round((decimal)value, 2).ToString();
		}
	}
}