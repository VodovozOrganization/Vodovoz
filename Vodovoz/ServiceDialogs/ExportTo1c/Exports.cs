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
			var exportData = new ExportData(UoW,dateStart,dateEnd);

			foreach (var order in orders)
			{
				exportData.AddOrder(order);
			}
			return exportData;
		}
	}
}

