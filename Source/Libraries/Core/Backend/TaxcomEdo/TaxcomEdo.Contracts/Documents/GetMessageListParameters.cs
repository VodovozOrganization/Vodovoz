using System;

namespace TaxcomEdo.Contracts.Documents
{
	public class GetMessageListParameters
	{
		/// <summary>
		/// Дата, с которой будет идти выборка
		/// </summary>
		public DateTime? Date { get; set; }
		/// <summary>
		/// Фильтр по направлению документооборота: Входящий и Исходящий
		/// </summary>
		public string Direction { get; set; }
		/// <summary>
		/// С прослеживаемостью в ЧЗ или без
		/// </summary>
		public bool WithTracing { get; set; }
		/// <summary>
		/// При значении true в xml ответа в блок AdditionalData будет записан признак принадлежности к пакету следующего вида:
		/// AdditionalParameter с Name="GroupID" Value "[GuidGroup]", где [GuidGroup] - GUID группы, в которую входит транзакция
		/// </summary>
		public bool WithGroupInfo { get; set; }
	}
}
