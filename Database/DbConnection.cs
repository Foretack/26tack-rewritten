using System.Data;
using System.Runtime.CompilerServices;
using Npgsql;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Tack.Database;
internal abstract class DbConnection
{
    private static readonly string ConnectionString =
        $"Host={AppConfigLoader.Config.DbHost};" +
        $"Username={AppConfigLoader.Config.DbUser};" +
        $"Password={AppConfigLoader.Config.DbPass};" +
        $"Database={AppConfigLoader.Config.DbName}";
    private static readonly SemaphoreSlim _operationLock = new(1);

    protected QueryFactory QueryFactory { get; }

    protected DbConnection()
    {
        if (QueryFactory is null)
        {
            var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            QueryFactory = new QueryFactory(conn, new PostgresCompiler())
            {
                Logger = x => Log.Verbose("Query: {q}", x.RawSql)
            };
            Log.Information("Initialized database");
        }

        _ = QueryFactory ?? throw new NotImplementedException("This is impossible");
    }

    public async Task<TResult> Enqueue<TResult>(string table, Func<SqlKata.Query, TResult> query,
    [CallerFilePath] string path = default!,
    [CallerLineNumber] int lineNumber = default)
    {
        await _operationLock.WaitAsync().ConfigureAwait(false);
        Log.Verbose("[{h}] Running query at: {p}:{l}", nameof(DbConnection), path, lineNumber);
        TResult result = query.Invoke(QueryFactory.Query(table));
        _ = _operationLock.Release();
        return result;
    }
    public async Task<TResult> Enqueue<TResult>(string table, Func<SqlKata.Query, Task<TResult>> query, 
        [CallerFilePath] string path = default!,
        [CallerLineNumber] int lineNumber = default)
    {
        await _operationLock.WaitAsync().ConfigureAwait(false);
        Log.Verbose("[{h}] Running query at: {p}:{l}", nameof(DbConnection), path, lineNumber);
        TResult result = await query.Invoke(QueryFactory.Query(table));
        _ = _operationLock.Release();
        return result;
    }
    public async Task<TResult> Enqueue<TResult>(Func<SqlKata.Query, Task<TResult>> query, 
        [CallerFilePath] string path = default!,
        [CallerLineNumber] int lineNumber = default)
    {
        await _operationLock.WaitAsync().ConfigureAwait(false);
        Log.Verbose("[{h}] Running query at: {p}:{l}", nameof(DbConnection), path, lineNumber);
        TResult result = await query.Invoke(QueryFactory.Query());
        _ = _operationLock.Release();
        return result;
    }
    public async Task<int> Enqueue(string sql, 
        [CallerFilePath] string path = default!,
        [CallerLineNumber] int lineNumber = default)
    {
        await _operationLock.WaitAsync().ConfigureAwait(false);
        Log.Verbose("[{h}] Running query at: {p}:{l}", nameof(DbConnection), path, lineNumber);
        int result = await QueryFactory.StatementAsync(sql);
        _ = _operationLock.Release();
        return result;
    }

    protected SqlKata.Query this[string table] => QueryFactory.Query(table);

    public ConnectionState ConnectionState => QueryFactory.Connection.State;
}
