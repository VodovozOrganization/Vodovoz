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
	public interface IDataExporterFor1cFactory
	{
		IDataExporterFor1c<Order> CreateOrders1cDataExporter(
			Export1cMode export1CMode, 
			Organization organization, 
			DateTime startDate,
			DateTime endDate,
			IProgressBarDisplayable progressBarDisplayable = null);
		
		IDataExporterFor1c<CounterpartyChangesDto> CreateCounterpartyChanges1cDataExporter();

		IDataExporterFor1c<OrderTo1cExport> CreateApi1cChangesExporter(DateTime exportDate);
	}
}
