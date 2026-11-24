using Xunit;

namespace Vodovoz.Testing.Database
{
	/// <summary>
	/// Параметризованный тест с БД: пропускается локально, если тестовый сервер запустить нельзя. В CI пропуска нет.
	/// </summary>
	public sealed class DatabaseTheoryAttribute : TheoryAttribute
	{
		public DatabaseTheoryAttribute()
		{
			if(!MariaDbServer.IsCi)
			{
				Skip = MariaDbServer.UnavailableReason;
			}
		}
	}
}
