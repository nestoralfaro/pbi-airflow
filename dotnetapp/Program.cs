using OfficeOpenXml;
using Microsoft.Data.Sqlite;
using System.Data;
namespace dotnetapp;

class Program
{
    static void Main(string[] args)
    {
        const string rootDir = "/Users/nestoralfaro/Desktop/pbi-scenario-two/";
        FileInfo sourceXlsx = new FileInfo($"/Users/nestoralfaro/Downloads/Portfolio_BI_CaseStudy_Data.xlsx");
        using (ExcelPackage package = new ExcelPackage(sourceXlsx)) {
          Console.WriteLine($"rows: {package.Workbook.Worksheets[0].Dimension.Rows}");
        }

        string connectionString = $"Data Source={Path.Join(rootDir, "airflow.db")}";
        string testQuery = "SELECT * FROM dag;";
        try {
          using (SqliteConnection conn = new SqliteConnection(connectionString)) {
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = testQuery;
            using (var reader = cmd.ExecuteReader()) {
              while (reader.Read()) {
                var name = reader.GetString(0);
                Console.WriteLine($"Hello, {name}!");
              }
            }
          }
            
        }
        catch (Exception ex) {
          Console.WriteLine($"ERROR: {ex.Message}");
        }
    }
}
