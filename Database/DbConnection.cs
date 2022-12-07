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

    public QueryFactory QueryFactory => _qf;

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
            Log.Information($"Initialized database: {ConnectionString}");
        }
        _initialized = true;
    }

    public SqlKata.Query this[string table] => QueryFactory.Query(table);

    public ConnectionState ConnectionState => _qf.Connection.State;

    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            //
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~DbConnection() { Dispose(disposing: false); }
}
