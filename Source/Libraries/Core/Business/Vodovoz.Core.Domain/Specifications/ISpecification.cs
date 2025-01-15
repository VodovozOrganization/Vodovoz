namespace Core.Infrastructure.Specifications
{
	public interface ISpecification<in T>
	{
		bool IsSatisfiedBy(T entity);
	}
}
