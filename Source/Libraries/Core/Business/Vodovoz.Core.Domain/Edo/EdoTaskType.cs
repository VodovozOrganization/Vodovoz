namespace Vodovoz.Core.Domain.Edo
{
	public enum EdoTaskType
	{
		/// <summary>
		/// Задача отправки документов по заказу
		/// </summary>
		Order,

		/// <summary>
		/// Задача отправки документов от поставщика
		/// </summary>
		Transfer,

		/// <summary>
		/// Задача отправки чека
		/// </summary>
		Receipt,

		/// <summary>
		/// Задача сохранения кодов
		/// </summary>
		SaveCode
	}
}
