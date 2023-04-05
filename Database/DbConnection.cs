using System.Collections.Concurrent;
using System.Data;
using System.Runtime.CompilerServices;
using Npgsql;
using SqlKata.Compilers;
using SqlKata.Execution;
using Tack.Utils;

namespace Tack.Database;
internal abstract class DbConnection
{
    private static readonly string ConnectionString =
        $"Host={AppConfigLoader.Config.DbHost};" +
        $"Username={AppConfigLoader.Config.DbUser};" +
        $"Password={AppConfigLoader.Config.DbPass};" +
        $"Database={AppConfigLoader.Config.DbName}";
    private static readonly ConcurrentQueue<Func<QueryFactory, Task>> s_queryQueue = new();
    private static readonly SemaphoreSlim s_operationLock = new(1);

    public ConnectionState ConnectionState => QueryFactory.Connection.State;

    protected QueryFactory QueryFactory { get; }

    private bool s_opInProgress = false;

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
            Time.DoEvery(TimeSpan.FromSeconds(15), Commit);
        }

        _ = QueryFactory ?? throw new NotImplementedException("This is impossible");
    }

    public void Enqueue(Func<QueryFactory, Task> task,
        [CallerFilePath] string path = default!,
        [CallerLineNumber] int lineNumber = default)
    {
        Log.Verbose("[{h}] Enqueued query at: {p}:{l} [{c}]", nameof(DbConnection), path, lineNumber, s_queryQueue.Count);
        s_queryQueue.Enqueue(task);
    }

    public async Task<TResult> Enqueue<TResult>(string table, Func<SqlKata.Query, TResult> query,
    [CallerFilePath] string path = default!,
    [CallerLineNumber] int lineNumber = default)
    {
        await s_operationLock.WaitAsync().ConfigureAwait(false);
        Log.Verbose("[{h}] Running query at: {p}:{l}", nameof(DbConnection), path, lineNumber);
        TResult result = query.Invoke(QueryFactory.Query(table));
        _ = s_operationLock.Release();
        return result;
    }

    public async Task<TResult> ValueStatement<TResult>(Func<QueryFactory, Task<TResult>> query,
        [CallerFilePath] string path = default!,
        [CallerLineNumber] int lineNumber = default)
    {
        const int wait = 100;
        int waitedMs = 0;
        while (s_opInProgress)
        {
            await Task.Delay(wait);
            waitedMs += wait;
            if (waitedMs % (wait * 50) == 0)
            {
                Log.Error("[{h}] Value statement took too long: {p}:{l}", nameof(DbConnection), path, lineNumber);
                throw new TimeoutException("Operation took too long");
            }
        }

        return await query(QueryFactory);
    }

    protected SqlKata.Query this[string table] => QueryFactory.Query(table);

    private async Task Commit()
    {
        if (!s_opInProgress
            && s_queryQueue.TryDequeue(out Func<QueryFactory, Task>? f)
            && f is not null)
        {
            s_opInProgress = true;
            await f(QueryFactory);
            s_opInProgress = false;
        }
    }
}
