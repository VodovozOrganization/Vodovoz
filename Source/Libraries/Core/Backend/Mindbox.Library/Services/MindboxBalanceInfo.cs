using System;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Бонусный баланс клиента из Mindbox
	/// </summary>
	public sealed class MindboxBalanceInfo
	{
		public bool IsAvailable { get; set; }
		public int Balance { get; set; }
		public int Pending { get; set; }
		public int? ExpiringAmount { get; set; }
		public DateTime? ExpiringDate { get; set; }

		public static MindboxBalanceInfo Zero()
		{
			return new MindboxBalanceInfo { IsAvailable = true };
		}

		public static MindboxBalanceInfo Unavailable()
		{
			return new MindboxBalanceInfo { IsAvailable = false };
		}
	}
}
