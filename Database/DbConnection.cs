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

    public QueryFactory QueryFactory { get; protected set; }
    public HttpClient Requests { get; private set; } = new();

    private NpgsqlConnection _connection = new(ConnectionString);
    private PostgresCompiler _compiler = new();

    protected DbConnection(int logLevel = 1)
    {
        QueryFactory = new QueryFactory(_connection, _compiler);
        QueryFactory.Logger = compiled =>
        {
            switch (logLevel)
            {
                // Fatal
                case 5:
                    Log.Fatal(compiled.ToString());
                    break;
                // Error
                case 4:
                    Log.Error(compiled.ToString());
                    break;
                // Warning
                case 3:
                    Log.Warning(compiled.ToString());
                    break;
                // Information
                case 2:
                    Log.Information(compiled.ToString());
                    break;
                // Debug (Default)
                default:
                case 1:
                    Log.Debug(compiled.ToString());
                    break;
                // Verbose
                case 0:
                    Log.Verbose(compiled.ToString());
                    break;
            }
        };
    }

    public SqlKata.Query this[string table] => QueryFactory.Query(table);

    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _connection.Close();
                _connection.Dispose();
                Requests.Dispose();
                QueryFactory.Dispose();
            }

            QueryFactory = default!;
            _compiler = default!;
            Requests = default!;
            _connection = default!;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~DbConnection() { Dispose(disposing: false); }
}
