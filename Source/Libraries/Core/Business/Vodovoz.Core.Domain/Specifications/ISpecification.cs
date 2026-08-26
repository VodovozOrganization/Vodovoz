namespace Vodovoz.Core.Domain.Specifications
{
	public interface ISpecification<in T>
	{
		bool IsSatisfiedBy(T entity);
	}
	
	public interface ISpecification<in T1, in T2> : ISpecificationTwoArgs
	{
		bool IsSatisfiedBy(T1 entity1, T2 entity2);
	}
	
	public interface ISpecificationTwoArgs
	{
		bool IsSatisfiedBy(object entity1, object entity2);
	}
}
