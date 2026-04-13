using System.Collections.Generic;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Services.Interfaces
{
	/// <summary>
	/// Подписыватель документов
	/// </summary>
	public interface ISignProcessor
	{
		/// <summary>
		/// Подписание документа
		/// </summary>
		/// <param name="containerDocument">Документ, подготовленный к упаковке в контейнер</param>
		/// <param name="document">Документ, добавленный в контейнер</param>
		/// <returns></returns>
		IList<IFileData> Sign(IContainerDocument containerDocument, IDocument document);
	}
}
