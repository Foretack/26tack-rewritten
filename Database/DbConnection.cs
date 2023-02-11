using System.Data;
using System.Runtime.CompilerServices;
using Npgsql;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Tack.Database;
internal abstract class DbConnection : IDisposable
{
    private static readonly string ConnectionString =
        $"Host={AppConfigLoader.Config.DbHost};" +
        $"Username={AppConfigLoader.Config.DbUser};" +
        $"Password={AppConfigLoader.Config.DbPass};" +
        $"Database={AppConfigLoader.Config.DbName}";
    private static readonly SemaphoreSlim _operationLock = new(1);

    protected QueryFactory QueryFactory => _qf;

    private static bool _initialized;
    private static NpgsqlConnection _connection;
    private static PostgresCompiler _compiler;
    private static QueryFactory _qf;

    protected DbConnection()
    {
        if (!_initialized)
        {
            _connection = new(ConnectionString);
            _compiler = new();
            _connection.Open();
            _qf = new QueryFactory(_connection, _compiler)
            {
                Logger = x => Log.Verbose("Query: {q}", x.RawSql)
            };
            Log.Information("Initialized database");
        }

        _initialized = true;
    }

    public async Task<TResult> Enqueue<TResult>(string table, Func<SqlKata.Query, TResult> query, int retryDelayMs = 1000,
    [CallerFilePath] string path = default!,
    [CallerLineNumber] int lineNumber = default)
    {
        int delayMs = await BlockOperation(retryDelayMs, path, lineNumber);

        await _operationLock.WaitAsync().ConfigureAwait(false);
        Log.Verbose("[DB] Operation in progress. Locking Semaphore...");

        TResult result = query.Invoke(QueryFactory.Query(table));

        _ = _operationLock.Release();
        Log.Debug("| [DB] Operation finished. Semaphore released \n | Total delay: {total}ms", delayMs);

        return result;
    }
    public async Task<TResult> Enqueue<TResult>(string table, Func<SqlKata.Query, Task<TResult>> query, int retryDelayMs = 1000,
    [CallerFilePath] string path = default!,
    [CallerLineNumber] int lineNumber = default)
    {
        int delayMs = await BlockOperation(retryDelayMs, path, lineNumber);

        await _operationLock.WaitAsync().ConfigureAwait(false);
        Log.Verbose("[DB] Operation in progress. Locking Semaphore...");

        TResult result = await query.Invoke(QueryFactory.Query(table));

        _ = _operationLock.Release();
        Log.Debug("| [DB] Operation finished. Semaphore released \n| Total delay: {total}ms", delayMs);

        return result;
    }
    public async Task<TResult> Enqueue<TResult>(Func<SqlKata.Query, Task<TResult>> query, int retryDelayMs = 1000,
    [CallerFilePath] string path = default!,
    [CallerLineNumber] int lineNumber = default)
    {
        int delayMs = await BlockOperation(retryDelayMs, path, lineNumber);

        await _operationLock.WaitAsync().ConfigureAwait(false);
        Log.Verbose("[DB] Operation in progress. Locking Semaphore...");

        TResult result = await query.Invoke(QueryFactory.Query());

        _ = _operationLock.Release();
        Log.Debug("| [DB] Operation finished. Semaphore released \n| Total delay: {total}ms", delayMs);

        return result;
    }
    public async Task<int> Enqueue(string sql, int retryDelayMs = 1000,
    [CallerFilePath] string path = default!,
    [CallerLineNumber] int lineNumber = default)
    {
        int delayMs = await BlockOperation(retryDelayMs, path, lineNumber);

        await _operationLock.WaitAsync().ConfigureAwait(false);
        Log.Verbose("[DB] Operation in progress. Locking Semaphore...");

        int result = await QueryFactory.StatementAsync(sql);

        _ = _operationLock.Release();
        Log.Debug("| [DB] Operation finished. Semaphore released \n| Total delay: {total}ms", delayMs);

        return result;
    }

    private async Task<int> BlockOperation(int retryDelayMs, string path, int lineNumber)
    {
        int delayCount = 0;

        while (_operationLock.CurrentCount == 0)
        {
            delayCount++;
            if (delayCount % 100 == 0)
            {
                Log.Error("[DB] Aborting operation at [{path}:{line}]: Delayed for too long ({time}ms)", path, lineNumber, retryDelayMs * delayCount);
                throw new TimeoutException("Operation delayed for too long. Aborting...");
            }
            else if (delayCount % 10 == 0)
            {
                Log.Warning("[DB] Operation at {path}:{line} is taking too much time! ({time}ms)", path, lineNumber, retryDelayMs * delayCount);
            }

            await Task.Delay(retryDelayMs);
        }

        Log.Verbose("[DB] Now running: {path}:{line}", path, lineNumber);

        return delayCount * retryDelayMs;
    }

    protected SqlKata.Query this[string table] => QueryFactory.Query(table);

    public ConnectionState ConnectionState => _qf.Connection.State;
    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~DbConnection() { Dispose(disposing: false); }
}
