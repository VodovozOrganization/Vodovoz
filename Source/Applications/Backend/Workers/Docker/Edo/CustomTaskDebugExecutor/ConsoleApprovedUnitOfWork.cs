using NHibernate;
using QS.DomainModel.Config;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CustomTaskDebugExecutor
{
	public class ConsoleApprovedUnitOfWork : IUnitOfWork
	{
		private readonly IUnitOfWork _uow;

		public ConsoleApprovedUnitOfWork(IUnitOfWorkFactory uowFactory)
		{
			_uow = uowFactory.CreateWithoutRoot();
		}

		internal ConsoleApprovedUnitOfWork(IUnitOfWork uow)
		{
			_uow = uow;
		}

		public void Commit()
		{
			Console.WriteLine();
			Console.Write("Подтверждаем сохранение? (y/n): ");
			var key = Console.ReadKey();
			Console.WriteLine();
			if(key.KeyChar != 'y')
			{
				Console.WriteLine("Отменяем сохранение");
				_uow.Dispose();
				throw new InvalidOperationException("Отмена сохранения");
			}

			Console.WriteLine("Подтверждаем сохранение");
			_uow.Commit();
		}

		public async Task CommitAsync(CancellationToken cancellationToken = default)
		{
			Console.WriteLine();
			Console.Write("Подтверждаем сохранение? (y/n): ");
			var key = Console.ReadKey();
			Console.WriteLine();
			if(key.KeyChar != 'y')
			{
				Console.WriteLine("Отменяем сохранение");
				_uow.Dispose();
				throw new InvalidOperationException("Отмена сохранения");
			}

			Console.WriteLine("Подтверждаем сохранение");
			await _uow.CommitAsync(cancellationToken);
		}

		#region No changes

		public UnitOfWorkTitle ActionTitle => _uow.ActionTitle;

		public ISession Session => _uow.Session;

		public object RootObject => _uow.RootObject;

		public bool IsNew => _uow.IsNew;

		public bool IsAlive => _uow.IsAlive;

		public bool HasChanges => _uow.HasChanges;

		public event EventHandler<EntityUpdatedEventArgs> SessionScopeEntitySaved
		{
			add
			{
				_uow.SessionScopeEntitySaved += value;
			}

			remove
			{
				_uow.SessionScopeEntitySaved -= value;
			}
		}

		public void Delete(object entity)
		{
			_uow.Delete(entity);
		}

		public Task DeleteAsync(object entity, CancellationToken cancellationToken = default)
		{
			return _uow.DeleteAsync(entity, cancellationToken);
		}

		public void Dispose()
		{
			_uow.Dispose();
		}

		public IQueryable<T> GetAll<T>() where T : IDomainObject
		{
			return _uow.GetAll<T>();
		}

		public T GetById<T>(int id) where T : IDomainObject
		{
			return _uow.GetById<T>(id);
		}

		public object GetById(Type clazz, int id)
		{
			return _uow.GetById(clazz, id);
		}

		public void OpenTransaction()
		{
			_uow.OpenTransaction();
		}

		public IQueryOver<T, T> Query<T>() where T : class
		{
			return _uow.Query<T>();
		}

		public IQueryOver<T, T> Query<T>(Expression<Func<T>> alias) where T : class
		{
			return _uow.Query(alias);
		}

		public void RaiseSessionScopeEntitySaved(object[] entities)
		{
			_uow.RaiseSessionScopeEntitySaved(entities);
		}

		public void Save()
		{
			_uow.Save();
		}

		public void Save(object entity, bool orUpdate = true)
		{
			_uow.Save(entity, orUpdate);
		}

		public Task SaveAsync()
		{
			return _uow.SaveAsync();
		}

		public Task SaveAsync(object entity, bool orUpdate = true, CancellationToken cancellationToken = default)
		{
			return _uow.SaveAsync(entity, orUpdate, cancellationToken);
		}

		IList<T> IUnitOfWork.GetById<T>(int[] ids)
		{
			return _uow.GetById<T>(ids);
		}

		IList<T> IUnitOfWork.GetById<T>(IEnumerable<int> ids)
		{
			return _uow.GetById<T>(ids);
		}

		T IUnitOfWork.GetInSession<T>(T origin)
		{
			return _uow.GetInSession(origin);
		}

		#endregion No changes
	}
}
