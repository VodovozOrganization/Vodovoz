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
		private readonly Func<IList<int>> _selectRestriction;
		private readonly Func<string, Expression<Func<TEntity, bool>>> _entityTitleFunc;

		public EntitySelectionAutocompleteSelector(
			IUnitOfWork uow,
			Func<IList<int>> selectRestriction = null,
			Func<string, Expression<Func<TEntity, bool>>> entityTitleFunc = null)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_selectRestriction = selectRestriction;
			_entityTitleFunc = entityTitleFunc;
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

			if(_selectRestriction != null)
			{
				query = query.Where(e => _selectRestriction.Invoke().Contains(e.Id));
			}

			if(searchText != null && _entityTitleFunc != null)
			{
				foreach(var text in searchText)
				{
					var expression = _entityTitleFunc(text);

					query = query.Where(expression);
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
}
