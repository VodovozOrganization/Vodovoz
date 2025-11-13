using ExportTo1c.Library.Exporters;
using System;
using Vodovoz.EntityRepositories.Orders;

namespace ExportTo1c.Library.Factories
{
	public class DataExporterFor1cFactory : IDataExporterFor1cFactory
	{
		public IDataExporterFor1c Create1cDataExporter(Export1cMode export1CMode)
		{
			switch(export1CMode)
			{
				case Export1cMode.ComplexAutomation:
					return new ComplexAutomationCashless1cDataExporter();
				case Export1cMode.Retail:
					return new Retail1cDataExporter();
				default:
					throw new ArgumentException("Неизвестный тип выгрузки");
			}
		}
	}
}
