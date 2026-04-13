using TaxcomEdoApi.Library.Models.Containers;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Factories
{
	/// <summary>
	/// Фабрика, для производства подписантов документов
	/// </summary>
	public interface ISignProcessorFactory
	{
		/// <summary>
		/// Подписыватель документа
		/// </summary>
		/// <param name="mode">Режим подписи</param>
		/// <returns></returns>
		ISignProcessor CreateSignProcessor(SignMode mode);
	}
}
