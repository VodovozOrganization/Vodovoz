using System;
using System.Xml;
using QS.DomainModel.UoW;
using Vodovoz.Repository;

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
			var orders = OrderRepository.GetOrdersToExport1c8(UoW, dateStart, dateEnd);
			var exportData = new ExportData(UoW,dateStart,dateEnd);

			foreach (var order in orders)
			{
				exportData.AddOrder(order);
			}
			return exportData;
		}
	}
}

