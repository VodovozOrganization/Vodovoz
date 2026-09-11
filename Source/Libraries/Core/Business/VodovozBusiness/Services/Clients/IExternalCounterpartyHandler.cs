using QS.DomainModel.UoW;
using Vodovoz.Domain.Contacts;

namespace VodovozBusiness.Services.Clients
{
	/// <summary>
	/// Обработчик для работы с пользователями ИПЗ
	/// </summary>
	public interface IExternalCounterpartyHandler
	{
		/// <summary>
		/// Проверка на наличие пользователей клиентов физиков
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="phone">Телефон</param>
		/// <returns></returns>
		bool HasExternalCounterparties(IUnitOfWork uow, Phone phone);

		/// <summary>
		/// Удалить пользователей ИПЗ и связанные данные архивируемого телефона в текущей транзакции.
		/// Сохранение и завершение транзакции выполняет вызывающая сторона.
		/// </summary>
		/// <param name="uow">Текущая единица работы.</param>
		/// <param name="phone">Архивируемый телефон.</param>
		void DeleteExternalCounterpartiesForArchivedPhone(IUnitOfWork uow, Phone phone);
	}
}
