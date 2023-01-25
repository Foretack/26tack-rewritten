using System.Data;
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
            _qf = new QueryFactory(_connection, _compiler);
            _qf.Logger = x => Log.Verbose("Query: {q}", x.RawSql);
            Log.Information("Initialized database");
        }
        _initialized = true;
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
