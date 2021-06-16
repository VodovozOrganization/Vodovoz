using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.ViewModels.Reports.DeliveryAnalytics
{
	public class DeliveryAnalyticsReportNode
	{
		#region Свойства
		public int Id { get; set; }
		public string GeographicGroupName { get; set; }
		public string CityOrSuburb { get; set; }
		public string DistrictName { get; set; }
		public DateTime DayOfWeek { get; set; }
		public DateTime DeliveryDate { get; set; }

		#region Колонки адресов
		public int CountSmallOrdersOneMorning { get; set; }
		public decimal CountSmallOrders19LOneMorning { get; set; }
		public int CountBigOrdersOneMorning { get; set; }
		public decimal CountBigOrders19LOneMorning { get; set; }
		public int SumSmallAndBigOrdersOneMorning { get; set; }
		public decimal SumSmallAndBigOrders19LOneMorning { get; set; }
		public int CountSmallOrdersOneDay { get; set; }
		public decimal CountSmallOrders19LOneDay { get; set; }
		public int CountBigOrdersOneDay { get; set; }
		public decimal CountBigOrders19LOneDay { get; set; }
		public int SumSmallAndBigOrdersOneDay { get; set; }
		public decimal SumSmallAndBigOrders19LOneDay { get; set; }
		public int CountSmallOrdersOneEvening { get; set; }
		public decimal CountSmallOrders19LOneEvening { get; set; }
		public int CountBigOrdersOneEvening { get; set; }
		public decimal CountBigOrders19LOneEvening { get; set; }
		public int SumSmallAndBigOrdersOneEvening { get; set; }
		public decimal SumSmallAndBigOrders19LOneEvening { get; set; }
		public int CountSmallOrdersOneFinal { get; set; }
		public decimal CountSmallOrders19LOneFinal { get; set; }
		public int CountBigOrdersOneFinal { get; set; }
		public decimal CountBigOrders19LOneFinal { get; set; }
		public int SumSmallAndBigOrdersOneFinal { get; set; }
		public decimal SumSmallAndBigOrders19LOneFinal { get; set; }
		public int CountSmallOrdersTwoDay { get; set; }
		public decimal CountSmallOrders19LTwoDay { get; set; }
		public int CountBigOrdersTwoDay { get; set; }
		public decimal CountBigOrders19LTwoDay { get; set; }
		public int SumSmallAndBigOrdersTwoDay { get; set; }
		public decimal SumSmallAndBigOrders19LTwoDay { get; set; }
		public int CountSmallOrdersThreeDay { get; set; }
		public decimal CountSmallOrders19LThreeDay { get; set; }
		public int CountBigOrdersThreeDay { get; set; }
		public decimal CountBigOrders19LThreeDay { get; set; }
		public int SumSmallAndBigOrdersThreeDay { get; set; }
		public decimal SumSmallAndBigOrders19LThreeDay { get; set; }
		public int CountSmallOrdersFinal { get; set; }
		public decimal CountSmallOrders19LFinal { get; set; }
		public int CountBigOrdersFinal { get; set; }
		public decimal CountBigOrders19LFinal { get; set; }
		public int SumSmallAndBigOrdersFinal { get; set; }
		public decimal SumSmallAndBigOrders19LFinal { get; set; }
		#endregion
		#endregion

		public DeliveryAnalyticsReportNode() { }
		public DeliveryAnalyticsReportNode(IEnumerable<DeliveryAnalyticsReportNode> nodes, int count)
		{
			Id = count;
			GeographicGroupName = nodes.Select(x=>x.GeographicGroupName).FirstOrDefault();
			CityOrSuburb = nodes.Select(x=>x.CityOrSuburb).FirstOrDefault();
			DistrictName = nodes.Select(x=>x.DistrictName).FirstOrDefault();
			DayOfWeek = nodes.Select(x=>x.DayOfWeek).FirstOrDefault();
			DeliveryDate = nodes.Select(x=>x.DeliveryDate).FirstOrDefault();
			CountSmallOrdersOneMorning = nodes.Sum(x => x.CountSmallOrdersOneMorning);
			CountSmallOrders19LOneMorning = nodes.Sum(x => x.CountSmallOrders19LOneMorning);
			CountBigOrdersOneMorning = nodes.Sum(x => x.CountBigOrdersOneMorning);
			CountBigOrders19LOneMorning = nodes.Sum(x => x.CountBigOrders19LOneMorning);
			SumSmallAndBigOrdersOneMorning = CountBigOrdersOneMorning + CountSmallOrdersOneMorning;
			SumSmallAndBigOrders19LOneMorning = CountBigOrders19LOneMorning + CountSmallOrders19LOneMorning;
			CountSmallOrdersOneDay = nodes.Sum(x => x.CountSmallOrdersOneDay);
			CountSmallOrders19LOneDay = nodes.Sum(x => x.CountSmallOrders19LOneDay);
			CountBigOrdersOneDay = nodes.Sum(x => x.CountBigOrdersOneDay);
			CountBigOrders19LOneDay = nodes.Sum(x => x.CountBigOrders19LOneDay);
			SumSmallAndBigOrdersOneDay = CountBigOrdersOneDay + CountSmallOrdersOneDay;
			SumSmallAndBigOrders19LOneDay = CountBigOrders19LOneDay + CountSmallOrders19LOneDay;
			CountSmallOrdersOneEvening = nodes.Sum(x => x.CountSmallOrdersOneEvening);
			CountSmallOrders19LOneEvening = nodes.Sum(x => x.CountSmallOrders19LOneEvening);
			CountBigOrdersOneEvening = nodes.Sum(x => x.CountBigOrdersOneEvening);
			CountBigOrders19LOneEvening = nodes.Sum(x => x.CountBigOrders19LOneEvening);
			SumSmallAndBigOrdersOneEvening = CountBigOrdersOneEvening + CountSmallOrdersOneEvening;
			SumSmallAndBigOrders19LOneEvening = CountBigOrders19LOneEvening + CountSmallOrders19LOneEvening;
			CountSmallOrdersOneFinal = CountSmallOrdersOneMorning + CountSmallOrdersOneDay + CountSmallOrdersOneEvening;
			CountSmallOrders19LOneFinal = CountSmallOrders19LOneMorning + CountSmallOrders19LOneDay + CountSmallOrders19LOneEvening;
			CountBigOrdersOneFinal = CountBigOrdersOneMorning + CountBigOrdersOneDay + CountBigOrdersOneEvening;
			CountBigOrders19LOneFinal = CountBigOrders19LOneMorning + CountBigOrders19LOneDay + CountBigOrders19LOneEvening;
			SumSmallAndBigOrdersOneFinal = CountBigOrdersOneFinal + CountSmallOrdersOneFinal;
			SumSmallAndBigOrders19LOneFinal = CountBigOrders19LOneFinal + CountSmallOrders19LOneFinal;
			CountSmallOrdersTwoDay = nodes.Sum(x => x.CountSmallOrdersTwoDay);
			CountSmallOrders19LTwoDay = nodes.Sum(x => x.CountSmallOrders19LTwoDay);
			CountBigOrdersTwoDay = nodes.Sum(x => x.CountBigOrdersTwoDay);
			CountBigOrders19LTwoDay = nodes.Sum(x => x.CountBigOrders19LTwoDay);
			SumSmallAndBigOrdersTwoDay = CountBigOrdersTwoDay + CountSmallOrdersTwoDay;
			SumSmallAndBigOrders19LTwoDay = CountBigOrders19LTwoDay + CountSmallOrders19LTwoDay;
			CountSmallOrdersThreeDay  = nodes.Sum(x => x.CountSmallOrdersThreeDay);
			CountSmallOrders19LThreeDay = nodes.Sum(x => x.CountSmallOrders19LThreeDay);
			CountBigOrdersThreeDay = nodes.Sum(x => x.CountBigOrdersThreeDay);
			CountBigOrders19LThreeDay = nodes.Sum(x => x.CountBigOrders19LThreeDay);
			SumSmallAndBigOrdersThreeDay = CountBigOrdersThreeDay + CountSmallOrdersThreeDay;
			SumSmallAndBigOrders19LThreeDay = CountBigOrders19LThreeDay + CountSmallOrders19LThreeDay;
			CountSmallOrdersFinal = CountSmallOrdersOneFinal + CountSmallOrdersTwoDay + CountSmallOrdersThreeDay;
			CountSmallOrders19LFinal = CountSmallOrders19LOneFinal + CountSmallOrders19LTwoDay + CountSmallOrders19LThreeDay;
			CountBigOrdersFinal = CountBigOrdersOneFinal + CountBigOrdersTwoDay + CountBigOrdersThreeDay;
			CountBigOrders19LFinal = CountBigOrders19LOneFinal + CountBigOrders19LTwoDay + CountBigOrders19LThreeDay;
			SumSmallAndBigOrdersFinal = CountBigOrdersFinal + CountSmallOrdersFinal;
			SumSmallAndBigOrders19LFinal = CountBigOrders19LFinal + CountSmallOrders19LFinal;
		}

		public override string ToString()
		{
			return Id + ";" + GeographicGroupName + ";" + CityOrSuburb + ";" + DistrictName + ";" +
			       CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(DayOfWeek.DayOfWeek) + ";" + DeliveryDate.ToShortDateString() +  ";"
			       + CountSmallOrdersOneMorning + ";" + CountSmallOrders19LOneMorning.ToString("N0") + ";" +
			       CountBigOrdersOneMorning + ";" + CountBigOrders19LOneMorning.ToString("N0") + ";" + SumSmallAndBigOrdersOneMorning +
			       ";" + SumSmallAndBigOrders19LOneMorning.ToString("N0") + ";" + CountSmallOrdersOneDay + ";" +
			       CountSmallOrders19LOneDay.ToString("N0") + ";" +  CountBigOrdersOneDay + ";" + 
			       CountBigOrders19LOneDay.ToString("N0") + ";" + SumSmallAndBigOrdersOneDay + ";" +
			       SumSmallAndBigOrders19LOneDay.ToString("N0") + ";" + CountSmallOrdersOneEvening + ";" +
			       CountSmallOrders19LOneEvening.ToString("N0") + ";" +  CountBigOrdersOneEvening + ";" + 
			       CountBigOrders19LOneEvening.ToString("N0") + ";" + SumSmallAndBigOrdersOneEvening + ";" +  
			       SumSmallAndBigOrders19LOneEvening.ToString("N0") + ";" + CountSmallOrdersOneFinal + ";" +
			       CountSmallOrders19LOneFinal.ToString("N0") + ";" +  CountBigOrdersOneFinal + ";" + 
			       CountBigOrders19LOneFinal.ToString("N0") + ";" + SumSmallAndBigOrdersOneFinal + ";" +
			       SumSmallAndBigOrders19LOneFinal.ToString("N0") + ";" + CountSmallOrdersTwoDay + ";" +
			       CountSmallOrders19LTwoDay.ToString("N0") + ";" +  CountBigOrdersTwoDay + ";" + 
			       CountBigOrders19LTwoDay.ToString("N0") + ";" + SumSmallAndBigOrdersTwoDay + ";" +
			       SumSmallAndBigOrders19LTwoDay.ToString("N0") + ";" + CountSmallOrdersThreeDay + ";" +
			       CountSmallOrders19LThreeDay.ToString("N0") + ";" + CountBigOrdersThreeDay + ";" + 
			       CountBigOrders19LThreeDay.ToString("N0") + ";" + SumSmallAndBigOrdersThreeDay + ";" +
			       SumSmallAndBigOrders19LThreeDay.ToString("N0") + ";" + CountSmallOrdersFinal + ";" +
			       CountSmallOrders19LFinal.ToString("N0") + ";" +  CountBigOrdersFinal + ";" + 
			       CountBigOrders19LFinal.ToString("N0") + ";" + SumSmallAndBigOrdersFinal + ";" +
			       SumSmallAndBigOrders19LFinal.ToString("N0") + ";";
		}

		public override bool Equals(object obj)
		{
			return obj is DeliveryAnalyticsReportNode node && Id == node.Id && DistrictName == node.DistrictName;
		}

		public override int GetHashCode()
		{
			int hashCode = Id.GetHashCode();
			hashCode = 31 * hashCode + DistrictName.GetHashCode();
			return hashCode;
		}
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

			#region Колонки адресов

			Map(x => x.CountSmallOrdersOneMorning).Index(6).Name("М (.)");
			Map(x => x.CountSmallOrders19LOneMorning).Index(7).Name("М б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountBigOrdersOneMorning).Index(8).Name("К (.)");
			Map(x => x.CountBigOrders19LOneMorning).Index(9).Name("К б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.SumSmallAndBigOrdersOneMorning).Index(10).Name("И (.)");
			Map(x => x.SumSmallAndBigOrders19LOneMorning).Index(11).Name("И б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountSmallOrdersOneDay).Index(12).Name("М (.)");
			Map(x => x.CountSmallOrders19LOneDay).Index(13).Name("М б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountBigOrdersOneDay).Index(14).Name("К (.)");
			Map(x => x.CountBigOrders19LOneDay).Index(15).Name("К б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.SumSmallAndBigOrdersOneDay).Index(16).Name("И (.)");
			Map(x => x.SumSmallAndBigOrders19LOneDay).Index(17).Name("И б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountSmallOrdersOneEvening).Index(18).Name("М (.)");
			Map(x => x.CountSmallOrders19LOneEvening).Index(19).Name("М б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountBigOrdersOneEvening).Index(20).Name("К (.)");
			Map(x => x.CountBigOrders19LOneEvening).Index(21).Name("К б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.SumSmallAndBigOrdersOneEvening).Index(22).Name("И (.)");
			Map(x => x.SumSmallAndBigOrders19LOneEvening).Index(23).Name("И б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountSmallOrdersOneFinal).Index(24).Name("М (.)");
			Map(x => x.CountSmallOrders19LOneFinal).Index(25).Name("М б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountBigOrdersOneFinal).Index(26).Name("К (.)");
			Map(x => x.CountBigOrders19LOneFinal).Index(27).Name("К б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.SumSmallAndBigOrdersOneFinal).Index(28).Name("В (.)");
			Map(x => x.SumSmallAndBigOrders19LOneFinal).Index(29).Name("В б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountSmallOrdersTwoDay).Index(30).Name("М (.)");
			Map(x => x.CountSmallOrders19LTwoDay).Index(31).Name("М б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountBigOrdersTwoDay).Index(32).Name("К (.)");
			Map(x => x.CountBigOrders19LTwoDay).Index(33).Name("К б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.SumSmallAndBigOrdersTwoDay).Index(34).Name("И (.)");
			Map(x => x.SumSmallAndBigOrders19LTwoDay).Index(35).Name("И б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountSmallOrdersTwoDay).Index(36).Name("М (.)");
			Map(x => x.CountSmallOrders19LTwoDay).Index(37).Name("М б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountBigOrdersTwoDay).Index(38).Name("К (.)");
			Map(x => x.CountBigOrders19LTwoDay).Index(39).Name("К б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.SumSmallAndBigOrdersTwoDay).Index(40).Name("И (.)");
			Map(x => x.SumSmallAndBigOrders19LTwoDay).Index(41).Name("И б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountSmallOrdersThreeDay).Index(42).Name("М (.)");
			Map(x => x.CountSmallOrders19LThreeDay).Index(43).Name("М б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountBigOrdersThreeDay).Index(44).Name("К (.)");
			Map(x => x.CountBigOrders19LThreeDay).Index(45).Name("К б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.SumSmallAndBigOrdersThreeDay).Index(46).Name("И (.)");
			Map(x => x.SumSmallAndBigOrders19LThreeDay).Index(47).Name("И б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountSmallOrdersFinal).Index(48).Name("М (.)");
			Map(x => x.CountSmallOrders19LFinal).Index(49).Name("М б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.CountBigOrdersFinal).Index(50).Name("К (.)");
			Map(x => x.CountBigOrders19LFinal).Index(51).Name("К б.").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.SumSmallAndBigOrdersFinal).Index(52).Name("В (.)");
			Map(x => x.SumSmallAndBigOrders19LFinal).Index(53).Name("В б.").Default(0).TypeConverter<DecimalConverter>();
			#endregion
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
			return decimal.Round((decimal)value, 2).ToString("N0");
		}
	}
}