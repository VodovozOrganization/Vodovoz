using System;
using System.Collections.Generic;

namespace Edo.Transfer
{
	public class TransferDirection : IEquatable<TransferDirection>
	{
		public int FromOrganizationId { get; set; }
		public int ToOrganizationId { get; set; }

		public TransferDirection(int fromOrganizationId, int toOrganizationId)
		{
			FromOrganizationId = fromOrganizationId;
			ToOrganizationId = toOrganizationId;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as TransferDirection);
		}

		public bool Equals(TransferDirection other)
		{
			return !(other is null) &&
				   FromOrganizationId == other.FromOrganizationId &&
				   ToOrganizationId == other.ToOrganizationId;
		}

		public override int GetHashCode()
		{
			int hashCode = -2067352899;
			hashCode = hashCode * -1521134295 + FromOrganizationId.GetHashCode();
			hashCode = hashCode * -1521134295 + ToOrganizationId.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(TransferDirection left, TransferDirection right)
		{
			return EqualityComparer<TransferDirection>.Default.Equals(left, right);
		}

		public static bool operator !=(TransferDirection left, TransferDirection right)
		{
			return !(left == right);
		}
	}
}
