using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class EntitySelectionAutocompleteSelector<TEntity> : IEntitySelectionAutocompleteSelector<TEntity>, IDisposable
		where TEntity : class, IDomainObject
	{
		private readonly IUnitOfWork _uow;
		private readonly Func<IList<int>> _entityIdRestrictionFunc;
		private readonly Func<string, Expression<Func<TEntity, bool>>> _entityTitleComparerFunc;

		public EntitySelectionAutocompleteSelector(
			IUnitOfWork uow,
			Func<IList<int>> entityIdRestrictionFunc = null,
			Func<string, Expression<Func<TEntity, bool>>> entityTitleComparerFunc = null)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_entityIdRestrictionFunc = entityIdRestrictionFunc;
			_entityTitleComparerFunc = entityTitleComparerFunc;
		}

		public event EventHandler<AutocompleteUpdatedEventArgs> AutocompleteLoaded;

		public string GetTitle(object node) => node.GetTitle();

		public void LoadAutocompletion(string[] searchText, int takeCount)
		{
			var items = GetQuery(searchText, takeCount).ToList();

			AutocompleteLoaded?.Invoke(this, new AutocompleteUpdatedEventArgs(items));
		}

		public IList<TEntity> GetEntities()
		{
			return GetQuery().ToList();
		}

		private IQueryable<TEntity> GetQuery(string[] searchText = null, int? entitiesMaxCount = null)
		{
			var query = _uow.Session.Query<TEntity>();

			if(_entityIdRestrictionFunc != null)
			{
				query = query.Where(e => _entityIdRestrictionFunc.Invoke().Contains(e.Id));
			}

			if(searchText != null && _entityTitleComparerFunc != null)
			{
				foreach(var text in searchText)
				{
					query = query.Where(_entityTitleComparerFunc(text));
				}
			}

			if(entitiesMaxCount != null)
			{
				query = query.Take(entitiesMaxCount.Value);
			}

			return query;
		}

		public void Dispose()
		{

		}
	}

	public class SelectionDialogEntitiesLoader<TEntity> : ISelectionDialogEntitiesLoader<TEntity>, IDisposable
		where TEntity : class, IDomainObject
	{
		private readonly IUnitOfWork _uow;
		private readonly Func<IList<int>> _entityIdRestrictionFunc;

		public SelectionDialogEntitiesLoader(
			IUnitOfWork uow,
			Func<IList<int>> entityIdRestrictionFunc = null)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_entityIdRestrictionFunc = entityIdRestrictionFunc;
		}

		public IList<TEntity> GetEntities()
		{
			return GetQuery().ToList();
		}

		private IQueryable<TEntity> GetQuery()
		{
			var query = _uow.Session.Query<TEntity>();

			if(_entityIdRestrictionFunc != null)
			{
				query = query.Where(e => _entityIdRestrictionFunc.Invoke().Contains(e.Id));
			}

			return query;
		}

		public void Dispose()
		{

		}
	}
}
