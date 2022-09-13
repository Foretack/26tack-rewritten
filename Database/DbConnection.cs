using Npgsql;
using Serilog;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Tack.Database;
internal abstract class DbConnection : IDisposable
{
    private const string ConnectionString = $"Host={Config.Host};Username={Config.DbUsername};Password={Config.Password};Database={Config.DatabaseName}";

    public QueryFactory QueryFactory { get; protected set; }
    public HttpClient Requests { get; private set; } = new();

    private NpgsqlConnection Connection { get; set; } = new(ConnectionString);
    private PostgresCompiler Compiler { get; set; } = new();

    protected DbConnection(int logLevel = 1)
    {
        QueryFactory = new QueryFactory(Connection, Compiler);
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
                Connection.Close();
                Connection.Dispose();
                Requests.Dispose();
                QueryFactory.Dispose();
            }

            QueryFactory = default!;
            Compiler = default!;
            Requests = default!;
            Connection = default!;
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
