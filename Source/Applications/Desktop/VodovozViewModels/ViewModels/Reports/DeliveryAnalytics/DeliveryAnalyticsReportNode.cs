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
		public int NullCountSmallOrdersOneMorning { get; set; }
		public int NotNullCountSmallOrdersOneMorning { get; set; }
		public int NotNullCountSmallOrdersOneMorningWithoutWater { get; set; }
		public int CountSmallOrdersOneMorning { get; set; }
		public decimal CountSmallOrders19LOneMorning { get; set; }
		public int CountBigOrdersOneMorning { get; set; }
		public decimal CountBigOrders19LOneMorning { get; set; }
		public int SumSmallAndBigOrdersOneMorning { get; set; }
		public decimal SumSmallAndBigOrders19LOneMorning { get; set; }
		public int NullCountSmallOrdersOneDay { get; set; }
		public int NotNullCountSmallOrdersOneDay { get; set; }
		
		public int NotNullCountSmallOrdersOneDayWithoutWater { get; set; }
		public int CountSmallOrdersOneDay { get; set; }
		public decimal CountSmallOrders19LOneDay { get; set; }
		public int CountBigOrdersOneDay { get; set; }
		public decimal CountBigOrders19LOneDay { get; set; }
		public int SumSmallAndBigOrdersOneDay { get; set; }
		public decimal SumSmallAndBigOrders19LOneDay { get; set; }
		public int NullCountSmallOrdersOneEvening { get; set; }
		public int NotNullCountSmallOrdersOneEvening { get; set; }
		public int NotNullCountSmallOrdersOneEveningWithoutWater { get; set; }
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
		public int NullCountSmallOrdersTwoDay { get; set; }
		public int NotNullCountSmallOrdersTwoDay { get; set; }
		public int NotNullCountSmallOrdersTwoDayWithoutWater { get; set; }
		public int CountSmallOrdersTwoDay { get; set; }
		public decimal CountSmallOrders19LTwoDay { get; set; }
		public int CountBigOrdersTwoDay { get; set; }
		public decimal CountBigOrders19LTwoDay { get; set; }
		public int SumSmallAndBigOrdersTwoDay { get; set; }
		public decimal SumSmallAndBigOrders19LTwoDay { get; set; }
		public int NullCountSmallOrdersThreeDay { get; set; }
		public int NotNullCountSmallOrdersThreeDay { get; set; }
		public int NotNullCountSmallOrdersThreeDayWithoutWater { get; set; }
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
			
			NullCountSmallOrdersOneMorning = nodes.Sum(x => x.NullCountSmallOrdersOneMorning);
			NotNullCountSmallOrdersOneMorning = nodes.Sum(x => x.NotNullCountSmallOrdersOneMorning);
			NotNullCountSmallOrdersOneMorningWithoutWater = nodes.Sum(x => x.NotNullCountSmallOrdersOneMorningWithoutWater);
			CountSmallOrdersOneMorning = NullCountSmallOrdersOneMorning + NotNullCountSmallOrdersOneMorning +
			                             NotNullCountSmallOrdersOneMorningWithoutWater;
			CountSmallOrders19LOneMorning = nodes.Sum(x => x.CountSmallOrders19LOneMorning);
			CountBigOrdersOneMorning = nodes.Sum(x => x.CountBigOrdersOneMorning);
			CountBigOrders19LOneMorning = nodes.Sum(x => x.CountBigOrders19LOneMorning);
			SumSmallAndBigOrdersOneMorning = CountBigOrdersOneMorning + CountSmallOrdersOneMorning;
			SumSmallAndBigOrders19LOneMorning = CountBigOrders19LOneMorning + CountSmallOrders19LOneMorning;
			
			NullCountSmallOrdersOneDay = nodes.Sum(x => x.NullCountSmallOrdersOneDay);
			NotNullCountSmallOrdersOneDay = nodes.Sum(x => x.NotNullCountSmallOrdersOneDay);
			NotNullCountSmallOrdersOneDayWithoutWater = nodes.Sum(x => x.NotNullCountSmallOrdersOneDayWithoutWater);
			CountSmallOrdersOneDay = NullCountSmallOrdersOneDay + NotNullCountSmallOrdersOneDay + NotNullCountSmallOrdersOneDayWithoutWater;
			CountSmallOrders19LOneDay = nodes.Sum(x => x.CountSmallOrders19LOneDay);
			CountBigOrdersOneDay = nodes.Sum(x => x.CountBigOrdersOneDay);
			CountBigOrders19LOneDay = nodes.Sum(x => x.CountBigOrders19LOneDay);
			SumSmallAndBigOrdersOneDay = CountBigOrdersOneDay + CountSmallOrdersOneDay;
			SumSmallAndBigOrders19LOneDay = CountBigOrders19LOneDay + CountSmallOrders19LOneDay;
			
			NullCountSmallOrdersOneEvening = nodes.Sum(x => x.NullCountSmallOrdersOneEvening);
			NotNullCountSmallOrdersOneEvening = nodes.Sum(x => x.NotNullCountSmallOrdersOneEvening);
			NotNullCountSmallOrdersOneEveningWithoutWater = nodes.Sum(x => x.NotNullCountSmallOrdersOneEveningWithoutWater);
			CountSmallOrdersOneEvening = NullCountSmallOrdersOneEvening + NotNullCountSmallOrdersOneEvening +
			                             NotNullCountSmallOrdersOneEveningWithoutWater;
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
			
			NullCountSmallOrdersTwoDay = nodes.Sum(x => x.NullCountSmallOrdersTwoDay);
			NotNullCountSmallOrdersTwoDay = nodes.Sum(x => x.NotNullCountSmallOrdersTwoDay);
			NotNullCountSmallOrdersTwoDayWithoutWater = nodes.Sum(x => x.NotNullCountSmallOrdersTwoDayWithoutWater);
			CountSmallOrdersTwoDay = NotNullCountSmallOrdersTwoDay + NullCountSmallOrdersTwoDay + NotNullCountSmallOrdersTwoDayWithoutWater;
			CountSmallOrders19LTwoDay = nodes.Sum(x => x.CountSmallOrders19LTwoDay);
			CountBigOrdersTwoDay = nodes.Sum(x => x.CountBigOrdersTwoDay);
			CountBigOrders19LTwoDay = nodes.Sum(x => x.CountBigOrders19LTwoDay);
			SumSmallAndBigOrdersTwoDay = CountBigOrdersTwoDay + CountSmallOrdersTwoDay;
			SumSmallAndBigOrders19LTwoDay = CountBigOrders19LTwoDay + CountSmallOrders19LTwoDay;
			
			NullCountSmallOrdersThreeDay = nodes.Sum(x => x.NullCountSmallOrdersThreeDay);
			NotNullCountSmallOrdersThreeDay = nodes.Sum(x => x.NotNullCountSmallOrdersThreeDay);
			NotNullCountSmallOrdersThreeDayWithoutWater = nodes.Sum(x => x.NotNullCountSmallOrdersThreeDayWithoutWater);
			CountSmallOrdersThreeDay = NotNullCountSmallOrdersThreeDay + NullCountSmallOrdersThreeDay + NotNullCountSmallOrdersThreeDayWithoutWater;
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
			return ";" + GeographicGroupName + ";" + CityOrSuburb + ";" + DistrictName + ";" +
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
			return obj is DeliveryAnalyticsReportNode node && Id == node.Id;
		}

		public override int GetHashCode()
		{
			int hashCode = Id.GetHashCode();;
			return hashCode;
		}
	}
}