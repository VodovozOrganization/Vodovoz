using TaxcomEdo.Contracts.Xml.Container.Entities.Card;

namespace TaxcomEdoApi.Library.Services.Interfaces;

/// <summary>
/// Интерфейс создателя карты документа
/// </summary>
public interface ICardCreator
{
	/// <summary>
	/// Создание карты документа
	/// </summary>
	/// <returns></returns>
	Card CreateCard();
}
