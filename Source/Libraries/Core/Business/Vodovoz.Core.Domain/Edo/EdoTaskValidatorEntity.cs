using System;
using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Edo
{
	public class EdoTaskValidatorEntity : IEquatable<EdoTaskValidatorEntity>
	{
		public virtual string Name { get; set; }
		public virtual EdoValidationImportance Importance { get; set; }
		public virtual string Message { get; set; }
		public virtual string Description { get; set; }
		public virtual string Recommendation { get; set; }

		public override bool Equals(object obj)
		{
			return Equals(obj as EdoTaskValidatorEntity);
		}

		public bool Equals(EdoTaskValidatorEntity other)
		{
			return !(other is null) &&
				   Name == other.Name &&
				   Importance == other.Importance &&
				   Message == other.Message &&
				   Description == other.Description &&
				   Recommendation == other.Recommendation;
		}

		public override int GetHashCode()
		{
			int hashCode = -689312363;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
			hashCode = hashCode * -1521134295 + Importance.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Message);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Recommendation);
			return hashCode;
		}

		public override string ToString()
		{
			return $"EdoTaskValidator: {Name}";
		}
	}
}
