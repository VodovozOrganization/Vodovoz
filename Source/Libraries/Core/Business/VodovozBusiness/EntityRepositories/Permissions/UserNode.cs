namespace Vodovoz.EntityRepositories.Permissions
{
	public class UserNode
	{
		public int UserId { get; set; }
		public string UserName { get; set; }
		public string UserSubdivision { get; set; }
		public bool IsDeactivatedUser { get; set; }
	}
}
