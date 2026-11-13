using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Рассчитывает бонусы за 1 шт. товара в Mindbox
	/// </summary>
	public interface IMindboxBonusService
	{
		/// <summary>
		/// Рассчитывает бонусы по позициям. Ошибки Mindbox не приводят к исключению:
		/// для непросчитанных позиций значение в результате отсутствует.
		/// </summary>
		/// <param name="items">Позиции для расчёта: ключ товара в Mindbox и базовая цена за 1 шт.</param>
		/// <param name="cancellationToken">Токен отмены запроса</param>
		/// <returns>
		/// Словарь «ключ товара — количество бонусов». Значение <c>0</c> означает, что бонусы
		/// за позицию не начисляются. Отсутствие ключа в словаре означает, что позиция
		/// не была рассчитана (Mindbox не ответил или ответил ошибкой).
		/// </returns>
		Task<IReadOnlyDictionary<string, int?>> CalculateBonusesAsync(
			IReadOnlyCollection<MindboxBonusRequestItem> items,
			CancellationToken cancellationToken);
	}
}
