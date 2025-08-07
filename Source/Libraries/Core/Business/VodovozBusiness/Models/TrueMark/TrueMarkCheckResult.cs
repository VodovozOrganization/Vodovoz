using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkCheckResult
	{
		public TrueMarkWaterIdentificationCode Code { get; set; }

		/// <summary>
		/// В обороте
		/// </summary>
		public bool Introduced { get; set; }

		/// <summary>
		/// ИНН владельца кода
		/// </summary>
		public string OwnerInn { get; set; }

		/// <summary>
		/// Имя владельца кода
		/// </summary>
		public string OwnerName { get; set; }

		/// <summary>
		/// Дата окончания срока годности
		/// </summary>
		public DateTime? ExpirationDate { get; set; }
	}
}
