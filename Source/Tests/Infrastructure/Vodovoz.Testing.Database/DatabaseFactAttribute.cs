using Xunit;

namespace Vodovoz.Testing.Database
{
	/// <summary>
	/// Тест с БД: пропускается локально, если тестовый сервер запустить нельзя. В CI пропуска нет, тест падает.
	/// </summary>
	public sealed class DatabaseFactAttribute : FactAttribute
	{
		public DatabaseFactAttribute()
		{
			if(!MariaDbServer.IsCi)
			{
				Skip = MariaDbServer.UnavailableReason;
			}
		}
	}
}
