using System;
namespace Vodovoz.Domain.Permissions
{
	/// <summary>
	/// Сущность которая обазательно относится к какому либо подразделению
	/// </summary>
	public interface ISubdivisionEntity
	{
		Subdivision RelatedToSubdivision { get; }
	}
}
