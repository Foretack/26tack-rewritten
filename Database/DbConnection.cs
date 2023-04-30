using System.Collections.Concurrent;
using System.Data;
using System.Runtime.CompilerServices;
using Npgsql;
using SqlKata.Compilers;
using SqlKata.Execution;
using Tack.Models;
using Tack.Utils;

namespace Tack.Database;
internal abstract class DbConnection : Singleton
{
    private static readonly string ConnectionString =
        $"Host={AppConfig.DbHost};" +
        $"Username={AppConfig.DbUser};" +
        $"Password={AppConfig.DbPass};" +
        $"Database={AppConfig.DbName}";
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
                Logger = x => Log.Verbose("Query: {q}", x.Sql)
            };
            Log.Information("Initialized database");
            Time.DoEvery(TimeSpan.FromSeconds(3), Commit);
        }

        _ = QueryFactory ?? throw new NotImplementedException("This is impossible");
    }

    public void Enqueue(Func<QueryFactory, Task> task,
        [CallerFilePath] string path = default!,
        [CallerLineNumber] int lineNumber = default)
    {
        Log.Verbose("[{h}] Enqueued query at: {p}:{l} [{c}]", nameof(DbConnection), path, lineNumber, s_queryQueue.Count);
        s_queryQueue.Enqueue(task);
        if (s_queryQueue.Count > 100)
            Log.Warning("[{h}] Query queue is getting big! ({c})", nameof(DbConnection), s_queryQueue.Count);
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
        s_opInProgress = true;
        TResult result = await query(QueryFactory);
        s_opInProgress = false;
        return result;
    }

    protected SqlKata.Query this[string table] => QueryFactory.Query(table);

    private async Task Commit()
    {
        if (!s_opInProgress
            && s_queryQueue.TryDequeue(out Func<QueryFactory, Task>? f)
            && f is not null)
        {
            Log.Verbose("[{h}] Running query {i}", nameof(DbConnection), s_queryQueue.Count + 1);
            s_opInProgress = true;
            try
            {
                await f(QueryFactory).ConfigureAwait(false);
            }
            finally
            {
                s_opInProgress = false;
            }
        }
    }
}
