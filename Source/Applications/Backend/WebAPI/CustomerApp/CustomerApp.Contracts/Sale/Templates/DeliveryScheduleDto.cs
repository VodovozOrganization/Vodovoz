using System;

namespace CustomerApp.Contracts.Sale.Templates
{
	/// <summary>
	/// Информация по интервалу доставки
	/// </summary>
	public class DeliveryScheduleDto : IEquatable<DeliveryScheduleDto>
	{
		/// <summary>
		/// Идентификатор интервала доставки
		/// </summary>
		public int ErpId { get; set; }
		/// <summary>
		/// Название интервала
		/// </summary>
		public string IntervalName { get; set; }

		public static DeliveryScheduleDto Create(int erpId, string intervalName) =>
			new DeliveryScheduleDto
			{
				ErpId = erpId,
				IntervalName = intervalName
			};

		public bool Equals(DeliveryScheduleDto other)
		{
			if(other is null)
			{
				return false;
			}

			if(ReferenceEquals(this, other))
			{
				return true;
			}

			return ErpId == other.ErpId;
		}

		public override bool Equals(object obj)
		{
			if(obj is null)
			{
				return false;
			}

			if(ReferenceEquals(this, obj))
			{
				return true;
			}

			if(obj.GetType() != GetType())
			{
				return false;
			}

			return Equals((DeliveryScheduleDto)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (ErpId * 397) ^ (IntervalName != null ? IntervalName.GetHashCode() : 0);
			}
		}
	}
}
