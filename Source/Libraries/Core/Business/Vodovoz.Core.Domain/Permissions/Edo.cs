namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// ЭДО
	/// </summary>
	public static partial class Edo
	{
		/// <summary>
		/// Разрешено закрывать ЭДО задачу по Тендеру
		/// </summary>
		public static string CanCloseTenderEdoTask => nameof(CanCloseTenderEdoTask);
	}
}
