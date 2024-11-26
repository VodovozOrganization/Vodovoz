using Vodovoz;

namespace VodovozBusiness.Services.Subdivisions
{
	public interface ISubdivisionPermissionsService
	{
		void AddSubdiviionPermissions(Subdivision targer, Subdivision source);
		void ReplaceSubdivisionPermissions(Subdivision targer, Subdivision source);
	}
}
