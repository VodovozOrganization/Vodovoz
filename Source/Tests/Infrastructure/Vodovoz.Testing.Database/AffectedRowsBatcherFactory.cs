using NHibernate;
using NHibernate.AdoNet;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Testing.Database
{
	/// <summary>
	/// Фабрика батчера как на проде, через которую тест может подменить число изменённых строк.
	/// </summary>
	public class AffectedRowsBatcherFactory : IBatcherFactory
	{
		/// <summary>
		/// Подмена результата ExecuteNonQuery: (команда, реальное число строк) → число строк для вызывающего кода.
		/// </summary>
		public static Func<DbCommand, int, int> AffectedRows { get; set; }

		public IBatcher CreateBatcher(ConnectionManager connectionManager, IInterceptor interceptor) =>
			new Batcher(new GenericBatchingBatcherFactory().CreateBatcher(connectionManager, interceptor));

		private static int Adjust(DbCommand command, int rows) => AffectedRows?.Invoke(command, rows) ?? rows;

		private sealed class Batcher : IBatcher
		{
			private readonly IBatcher _inner;

			public Batcher(IBatcher inner)
			{
				_inner = inner;
			}

			public bool HasOpenResources => _inner.HasOpenResources;

			public int BatchSize
			{
				get => _inner.BatchSize;
				set => _inner.BatchSize = value;
			}

			public int ExecuteNonQuery(DbCommand cmd) => Adjust(cmd, _inner.ExecuteNonQuery(cmd));

			public async Task<int> ExecuteNonQueryAsync(DbCommand cmd, CancellationToken cancellationToken) =>
				Adjust(cmd, await _inner.ExecuteNonQueryAsync(cmd, cancellationToken));

			public DbCommand PrepareQueryCommand(CommandType commandType, SqlString sql, SqlType[] parameterTypes) =>
				_inner.PrepareQueryCommand(commandType, sql, parameterTypes);

			public DbCommand PrepareCommand(CommandType commandType, SqlString sql, SqlType[] parameterTypes) =>
				_inner.PrepareCommand(commandType, sql, parameterTypes);

			public Task<DbCommand> PrepareCommandAsync(CommandType commandType, SqlString sql, SqlType[] parameterTypes, CancellationToken cancellationToken) =>
				_inner.PrepareCommandAsync(commandType, sql, parameterTypes, cancellationToken);

			public void CloseCommand(DbCommand cmd, DbDataReader reader) => _inner.CloseCommand(cmd, reader);

			public void CloseReader(DbDataReader reader) => _inner.CloseReader(reader);

			public DbCommand PrepareBatchCommand(CommandType commandType, SqlString sql, SqlType[] parameterTypes) =>
				_inner.PrepareBatchCommand(commandType, sql, parameterTypes);

			public Task<DbCommand> PrepareBatchCommandAsync(CommandType commandType, SqlString sql, SqlType[] parameterTypes, CancellationToken cancellationToken) =>
				_inner.PrepareBatchCommandAsync(commandType, sql, parameterTypes, cancellationToken);

			public void AddToBatch(IExpectation expectation) => _inner.AddToBatch(expectation);

			public Task AddToBatchAsync(IExpectation expectation, CancellationToken cancellationToken) =>
				_inner.AddToBatchAsync(expectation, cancellationToken);

			public void ExecuteBatch() => _inner.ExecuteBatch();

			public Task ExecuteBatchAsync(CancellationToken cancellationToken) => _inner.ExecuteBatchAsync(cancellationToken);

			public void CloseCommands() => _inner.CloseCommands();

			public DbDataReader ExecuteReader(DbCommand cmd) => _inner.ExecuteReader(cmd);

			public Task<DbDataReader> ExecuteReaderAsync(DbCommand cmd, CancellationToken cancellationToken) =>
				_inner.ExecuteReaderAsync(cmd, cancellationToken);

			public void AbortBatch(Exception e) => _inner.AbortBatch(e);

			public void CancelLastQuery() => _inner.CancelLastQuery();

			public void Dispose() => _inner.Dispose();
		}
	}
}
