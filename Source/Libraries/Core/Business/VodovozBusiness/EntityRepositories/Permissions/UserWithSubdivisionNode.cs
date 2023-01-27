using QS.Project.Domain;

namespace Vodovoz.EntityRepositories.Permissions
{
	public class UserWithSubdivisionNode
	{
		public UserBase User { get; set; }
		public Subdivision Subdivision { get; set; }
		public string SubdivisionName { get; set; }
	}
}
