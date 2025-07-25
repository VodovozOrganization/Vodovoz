using System.Threading;
using System.Threading.Tasks;

namespace TrueMark.Codes.Pool
{
	/// <summary>
	/// Интерфейс для работы с пулом кодов
	/// </summary>
	public interface ITrueMarkCodesPool
	{
		/// <summary>
		/// Положить код
		/// </summary>
		/// <param name="codeId">Id кода</param>
		void PutCode(int codeId);
		/// <summary>
		/// Асинхронный метод вставки кода в пул
		/// </summary>
		/// <param name="codeId">Id кода</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns></returns>
		Task PutCodeAsync(int codeId, CancellationToken cancellationToken);
		/// <summary>
		/// Взятие кода
		/// </summary>
		/// <param name="gtin">Джитин</param>
		/// <returns>Id кода</returns>
		int TakeCode(string gtin);
		/// <summary>
		/// Асинхронный метод забора кода из пула
		/// </summary>
		/// <param name="gtin">Джитин</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Id кода</returns>
		Task<int> TakeCode(string gtin, CancellationToken cancellationToken);
	}
}
