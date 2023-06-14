using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using SqlServerBulkCopy;

var connString = "Data Source=.\\SQLEXPRESS;Initial Catalog=TestDB;Integrated Security=True;Trust Server Certificate=true;";
var sw = Stopwatch.StartNew();
try
{
    var rs = new BulkTable[]
    {
        new BulkTable(){Id=1,Name="One"},
        new BulkTable(){Id=2,Name="Two"},
    };

    using var conn = new SqlConnection(connString);
    conn.Open();
    using var tran = conn.BeginTransaction(IsolationLevel.Serializable);
    using var bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, tran);
    bulkCopy.BulkCopyTimeout = 60;
    bulkCopy.DestinationTableName = "BulkTable";
    bulkCopy.WriteToServer(rs.ToDataTable());
    tran.Commit();

    sw.Stop();
    Console.WriteLine($"finish {sw.Elapsed.TotalMilliseconds:#,##0}ms");
}
catch (SqlException ex)
{
    sw.Stop();
    Console.WriteLine(ex.Message);
}
catch (Exception ex)
{
    sw.Stop();
    Console.WriteLine(ex.Message);
}
