using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.Nodes
{
	/// <summary>
	/// Данные по пользователю ИПЗ
	/// </summary>
	public class ExternalCounterpartyNode
	{
		/// <summary>
		/// Id контрагента
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Id пользователя из ИПЗ (МП, сайт)
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Номер телефона в формате "(XXX) XXX - XX - XX"
		/// </summary>
		public string Phone { get; set; }
		/// <summary>
		/// Откуда пользователь
		/// </summary>
		public CounterpartyFrom CounterpartyFrom { get; set; }
	}
}
