using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Gamma.Utilities;
using Vodovoz.EntityRepositories.Counterparties;

namespace ExportTo1c.Library.Exporters
{
	public class CounterpartyChanges1cDataExporter : IDataExporterFor1c<CounterpartyChangesDto>
	{
		public XElement CreateXml(IList<CounterpartyChangesDto> sourceList, CancellationToken cancellationToken)
		{
			return new XElement("ФайлОбмена",
				new XAttribute("Тип", "Изменения в контрагентах за сутки"),
				CreateRows(sourceList, cancellationToken)
			);
		}

		private IList<XElement> CreateRows(IList<CounterpartyChangesDto> counterpartyChanges, CancellationToken cancellationToken)
		{
			var rows = new List<XElement>();

			foreach(var counterpartyChange in counterpartyChanges)
			{
				var row = new XElement
				(
					"Контрагент",
					new XAttribute("Инн", counterpartyChange.Inn),
					new XAttribute("Кпп", counterpartyChange.Kpp ?? ""),
					new XAttribute("ТипКонтрагента", counterpartyChange.CounterpartyType.GetEnumTitle()),
					new XAttribute("Сеть", counterpartyChange.IsChainStore),
					new XAttribute("ТипЗадолженности", counterpartyChange.CloseDeliveryDebtType?.GetEnumTitle() ?? ""),
					new XAttribute("ОткудаКлиент", counterpartyChange.CameFrom?.Name??""),
					new XAttribute("ОтсрочкаДнейПокупателям", counterpartyChange.DelayDaysForBuyers),
					new XAttribute("СтатусКонтрагентаВНалоговой", counterpartyChange.RevenueStatus?.GetEnumTitle() ?? "")
				);

				rows.Add(row);

				cancellationToken.ThrowIfCancellationRequested();
			}

			return rows;
		}
	}
}
