using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	public interface IStagingTrueMarkCodeFactory
	{
		/// <summary>
		/// Создание группового кода ЧЗ для промежуточного хранения на основе кода TrueMarkWaterCode
		/// </summary>
		/// <param name="parsedCode">Код, полученный в результате парсинга строки (TrueMarkWaterCode)</param>
		/// <param name="relatedDocumentType">Тип связанного документа</param>
		/// <param name="relatedDocumentId">Id связанного документа</param>
		/// <param name="orderItemId">Номер строки заказа</param>
		/// <returns>Групповой код ЧЗ для промежуточного хранения</returns>
		StagingTrueMarkCode CreateGroupCodeFromParsedCode(TrueMarkWaterCode parsedCode, StagingTrueMarkCodeRelatedDocumentType relatedDocumentType, int relatedDocumentId, int? orderItemId = null);

		/// <summary>
		/// Создание группового кода ЧЗ для промежуточного хранения на основе статуса экземпляра продукта, полученного из ЧЗ
		/// </summary>
		/// <param name="productInstanceStatus">Статус экземляра продукта в ЧЗ</param>
		/// <param name="relatedDocumentType">Тип связанного документа</param>
		/// <param name="relatedDocumentId">Id связанного документа</param>
		/// <param name="orderItemId">Номер строки заказа</param>
		/// <returns>Групповой код ЧЗ для промежуточного хранения</returns>
		StagingTrueMarkCode CreateGroupCodeFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus, StagingTrueMarkCodeRelatedDocumentType relatedDocumentType, int relatedDocumentId, int? orderItemId = null);

		/// <summary>
		/// Создание индивидуального кода ЧЗ для промежуточного хранения на основе кода TrueMarkWaterCode
		/// </summary>
		/// <param name="parsedCode">Код, полученный в результате парсинга строки (TrueMarkWaterCode)</param>
		/// <param name="relatedDocumentType">Тип связанного документа</param>
		/// <param name="relatedDocumentId">Id связанного документа</param>
		/// <param name="orderItemId">Номер строки заказа</param>
		/// <returns>Индивидуальный код ЧЗ для промежуточного хранения</returns>
		StagingTrueMarkCode CreateIdentificationCodeFromParsedCode(TrueMarkWaterCode parsedCode, StagingTrueMarkCodeRelatedDocumentType relatedDocumentType, int relatedDocumentId, int? orderItemId = null);

		/// <summary>
		/// Создание индивидуального кода ЧЗ для промежуточного хранения на основе статуса экземпляра продукта, полученного из ЧЗ
		/// </summary>
		/// <param name="productInstanceStatus">Статус экземляра продукта в ЧЗ</param>
		/// <param name="relatedDocumentType">Тип связанного документа</param>
		/// <param name="relatedDocumentId">Id связанного документа</param>
		/// <param name="orderItemId">Номер строки заказа</param>
		/// <returns>Индивидуальный код ЧЗ для промежуточного хранения</returns>
		StagingTrueMarkCode CreateIdentificationCodeFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus, StagingTrueMarkCodeRelatedDocumentType relatedDocumentType, int relatedDocumentId, int? orderItemId = null);

		/// <summary>
		/// Создание транспортного кода ЧЗ для промежуточного хранения на отсканированной строки кода
		/// </summary>
		/// <param name="rawCode">Строка кода</param>
		/// <param name="relatedDocumentType">Тип связанного документа</param>
		/// <param name="relatedDocumentId">Id связанного документа</param>
		/// <param name="orderItemId">Номер строки заказа</param>
		/// <returns>Транспортный код ЧЗ для промежуточного хранения</returns>
		StagingTrueMarkCode CreateTransportCodeFromRawCode(string rawCode, StagingTrueMarkCodeRelatedDocumentType relatedDocumentType, int relatedDocumentId, int? orderItemId = null);
	}
}
