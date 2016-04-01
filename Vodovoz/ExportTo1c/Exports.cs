using System;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Generic;
using System.Collections;
using Vodovoz.Domain;
using QSBusinessCommon.Domain;
using Vodovoz.Domain.Store;
using Vodovoz.Repository;
using QSOrmProject;
using Vodovoz.ExportTo1c;

namespace Vodovoz.ExportTo1c
{
	public static class Exports
	{		
		public static void TestExport()
		{
			XmlWriter writer = XmlWriter.Create("test.xml", new XmlWriterSettings
				{
					OmitXmlDeclaration = true,
					Indent = true,
					Encoding = System.Text.Encoding.UTF8,
					NewLineChars = "\r\n"
				});
			var testExchange = GetSalesFor(DateTime.Now.AddDays(-20),DateTime.Now);
			testExchange.ToXml().WriteTo(writer);

			writer.Close();
		}

		public static ExportData GetSalesFor(DateTime dateStart, DateTime dateEnd)
		{
			var UoW = UnitOfWorkFactory.CreateWithoutRoot();
			var orders = OrderRepository.GetCompleteOrdersBetweenDates(UoW, dateStart, dateEnd);
			var exportData = new ExportData(UoW);
			exportData.Version = "2.0";
			exportData.ExportDate = DateTime.Now;
			exportData.StartPeriodDate = dateStart;
			exportData.EndPeriodDate = dateEnd;
			exportData.SourceName = "Торговля+Склад, редакция 9.2";
			exportData.DestinationName = "БухгалтерияПредприятия";
			exportData.ConversionRulesId = "70e9dbac-59df-44bb-82c6-7d4f30d37c74";
			exportData.Comment = "";
			foreach (var order in orders)
			{
				exportData.AddOrder(order);
			}
			return exportData;
		}
	}
}

