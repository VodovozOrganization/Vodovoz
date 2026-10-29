namespace Vodovoz.Core.Domain.Organizations
{
	public interface IUsnModeOrganization
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		int Id { get; }
		/// <summary>
		/// Упрощенное налогообложение
		/// </summary>
		bool IsUsnMode { get; }
	}
}
