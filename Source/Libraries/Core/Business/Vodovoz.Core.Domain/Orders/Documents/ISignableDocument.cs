namespace Vodovoz.Core.Domain.Orders.Documents
{
	/// <summary>
	/// Интерфейс необходим для документов заказа, напротив которых должен быть крыжик
	/// "Без подписей и печати" в разделе "Документы" в диалоге заказа.
	/// </summary>
	public interface ISignableDocument
	{
		bool HideSignature { get; set; }
	}
}
