namespace Vodovoz.Permissions
{
	/// <summary>
	/// Права недовозы
	/// </summary>
	public static partial class Order
	{
		public static class UndeliveredOrder
		{
			public static string CanEditUndeliveries => "can_edit_undeliveries";
			public static string CanCloseUndeliveries => "can_close_undeliveries";
			public static string CanChangeUndeliveryProblemSource => "can_change_undelivery_problem_source";
		}
	}
}
