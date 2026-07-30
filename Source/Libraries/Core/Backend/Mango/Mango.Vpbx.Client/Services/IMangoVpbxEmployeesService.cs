using Mango.Core.Dto.Vpbx.Requests;
using Mango.Core.Dto.Vpbx.Responses;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mango.Vpbx.Client.Services
{
	/// <summary>
	/// Сервис управления сотрудниками ВАТС Манго и их составом в группах.
	/// Каждый метод соответствует одному методу API ВАТС и не содержит дополнительной логики.
	/// При неуспешном ответе бросается <see cref="Exceptions.MangoVpbxApiException"/>.
	/// </summary>
	/// <remarks>
	/// Сервис не выполняет повторов запросов и не ограничивает их частоту. За это отвечает вызывающий код.
	/// При планировании нагрузки нужно учитывать лимиты API ВАТС:
	/// не более 10 запросов в секунду по продукту, для запроса списка сотрудников - не более 1 запроса в 2 секунды.
	/// При превышении лимита бросается исключение с признаком
	/// <see cref="Exceptions.MangoVpbxApiException.IsRateLimitExceeded"/> - такой запрос можно повторить после паузы.
	/// Повторять запрос, завершившийся кодом результата 3XXX, нельзя:
	/// второй неверный запрос в течение 2 минут блокирует доступ ко всему API на 2 минуты.
	/// </remarks>
	public interface IMangoVpbxEmployeesService
	{
		/// <summary>
		/// Запрашивает всех сотрудников ВАТС
		/// </summary>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Сотрудники ВАТС</returns>
		Task<IReadOnlyList<VpbxUser>> GetAllUsersAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Запрашивает сотрудника ВАТС по внутреннему номеру
		/// </summary>
		/// <param name="extension">Внутренний номер сотрудника</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Сотрудник ВАТС. Пустой список, если сотрудник с указанным номером не найден</returns>
		Task<IReadOnlyList<VpbxUser>> GetUserAsync(long extension, CancellationToken cancellationToken);

		/// <summary>
		/// Создаёт сотрудника ВАТС
		/// </summary>
		/// <param name="request">Данные создаваемого сотрудника</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Id созданного сотрудника</returns>
		Task<long> CreateMemberAsync(CreateVpbxMemberRequest request, CancellationToken cancellationToken);

		/// <summary>
		/// Удаляет сотрудника ВАТС
		/// </summary>
		/// <param name="userId">
		/// Id сотрудника. Соответствует полю general.user_id в ответе <see cref="GetUserAsync"/>
		/// </param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		Task DeleteMemberAsync(string userId, CancellationToken cancellationToken);

		/// <summary>
		/// Запрашивает все группы ВАТС вместе с их составом
		/// </summary>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Группы ВАТС</returns>
		Task<IReadOnlyList<VpbxGroup>> GetAllGroupsAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Запрашивает состав группы ВАТС по Id
		/// </summary>
		/// <param name="groupId">Id группы. Если не указан, возвращаются все группы ВАТС</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Группы ВАТС. Пустой список, если группа с указанным Id не найдена</returns>
		Task<IReadOnlyList<VpbxGroup>> GetGroupsAsync(long groupId, CancellationToken cancellationToken);

		/// <summary>
		/// Заменяет состав группы ВАТС
		/// </summary>
		/// <param name="groupId">Id группы</param>
		/// <param name="operatorIds">
		/// Id сотрудников, которые должны состоять в группе. Полностью заменяют текущий состав,
		/// поэтому передавать нужно всех сотрудников группы, а не только добавляемых
		/// </param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		Task UpdateGroupOperatorsAsync(long groupId, IEnumerable<long> operatorIds, CancellationToken cancellationToken);
	}
}
