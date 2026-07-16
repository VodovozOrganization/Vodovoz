using Vodovoz.Core.Domain.StoredEmails;

namespace Vodovoz.Core.Domain.Orders.OrdersWithoutShipment
{
	/// <summary>
	/// Кастомный документ с возможностью повторной отправки
	/// </summary>
	public interface ICustomResendTemplateEmailableDocument : IEmailableDocument
	{
		/// <summary>
		/// Получить шаблон письма для повторной отправки документа
		/// </summary>
		/// <returns></returns>
		EmailTemplateEntity GetResendDocumentEmailTemplate();
	}
}
