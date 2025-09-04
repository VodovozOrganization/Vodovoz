namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class BookkeeppingPermissions
	{
		public static class Work1S
		{
			/// <summary>
			/// Доступ к вкладке Работа с 1С
			/// </summary>
			public static string HasAccessTo1sWork => "has_access_to_1s_work";
		}
	}
}
