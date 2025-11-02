using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	/// <summary>
	/// Тип переноса
	/// </summary>
	public enum TransferType
	{
		/// <summary>
		/// Автоперенос согласован
		/// </summary>
		[Display(Name = "Автоперенос согласован")]
		AutoTransferApproved,
		/// <summary>
		/// Автоперенос не согласован
		/// Автоперенос н/согл
		/// </summary>
		[Display(Name = "Автоперенос н/согл")]
		AutoTransferNotApproved,
		/// <summary>
		/// Перенос клиентом
		/// </summary>
		[Display(Name = "Перенос клиентом")]
		TransferredByCounterparty
	}
}
