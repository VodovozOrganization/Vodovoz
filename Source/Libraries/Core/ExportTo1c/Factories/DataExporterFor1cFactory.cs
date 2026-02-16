using System;
using ExportTo1c.Library.Exporters;
using QS.Dialog;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;

namespace ExportTo1c.Library.Factories
{
	public class DataExporterFor1cFactory : IDataExporterFor1cFactory
	{
		public IDataExporterFor1c<Order> CreateOrders1cDataExporter(
			Export1cMode export1CMode,
			Organization organization,
			DateTime startDate,
			DateTime endDate,
			IProgressBarDisplayable progressBarDisplayable = null)
		{
			switch(export1CMode)
			{
				case Export1cMode.ComplexAutomation:
					return new ComplexAutomationCashless1cDataExporter(organization, startDate, endDate, progressBarDisplayable);
				case Export1cMode.Retail:
					return new Retail1cDataExporter(organization, startDate, endDate, progressBarDisplayable);
				default:
					throw new ArgumentException("Неизвестный тип выгрузки");
			}
		}
		
		public IDataExporterFor1c<CounterpartyChangesDto> CreateCounterpartyChanges1cDataExporter() => new CounterpartyChanges1cDataExporter();
		
		public IDataExporterFor1c<OrderTo1cExport> CreateApi1cChangesExporter(DateTime exportDate) => new Api1cChangesExporter(exportDate);
	}
}
