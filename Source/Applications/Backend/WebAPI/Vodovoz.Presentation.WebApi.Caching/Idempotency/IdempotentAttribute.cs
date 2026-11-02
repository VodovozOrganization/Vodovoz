using System;

namespace Vodovoz.Presentation.WebApi.Caching.Idempotency
{
	/// <summary>
	/// Отмечает метод API, к которому применяется идемпотентность по заголовку <see cref="IdempotencyRequestHeadersNames.IdempotencyKey"/>:
	/// повторный запрос с тем же ключом получает сохранённый ответ первого запроса без повторного выполнения метода
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class IdempotentAttribute : Attribute
	{
	}
}
