using System.Collections.Generic;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Сгруппированная информация по клиенту,по которому есть непринятые УПД в ЭДО
	/// </summary>
	public class TimedOutDocFlowGrouppedNode
	{
		/// <summary>
		/// Клиент
		/// </summary>
		public CounterpartyEntity Client { get; set; }

		/// <summary>
		/// Организация
		/// </summary>
		public OrganizationEntity Organization { get; set; }

		/// <summary>
		/// Список документов, по которым есть непринятый УПД в ЭДО
		/// </summary>
		public IList<TimedOutDocFlowDocumentNode> Documents { get; set; } = new List<TimedOutDocFlowDocumentNode>();
	}

}
