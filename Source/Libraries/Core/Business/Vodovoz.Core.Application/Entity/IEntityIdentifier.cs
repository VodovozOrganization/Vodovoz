namespace Vodovoz.Core.Application.Entity
{
	public interface IEntityIdentifier
	{
		bool IsNewEntity { get; }
		object Id { get; }
	}
}
