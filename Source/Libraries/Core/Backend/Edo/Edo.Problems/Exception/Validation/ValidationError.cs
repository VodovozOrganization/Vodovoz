namespace Edo.Problems.Exception.Validation
{
	public class ValidationError<TEntity>
		where TEntity : class
	{
		public virtual string Name { get; }
		public virtual string Description { get; }
		public virtual string Recommendation { get; }
	}
}
