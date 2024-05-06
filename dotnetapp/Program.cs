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
        List<(string, string)> sheetQueryList = new List<(string, string)>();
        
        using (ExcelPackage package = new ExcelPackage(sourceXlsx)) {
            // Iterate over each worksheet
            foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets) {
                Console.WriteLine($"*********************Sheet {worksheet.Name}:*********************");
                // Get the dimensions of the worksheet
                int rowCount = worksheet.Dimension.Rows;
                int columnCount = worksheet.Dimension.Columns;
                string tableName = $"{worksheet.Name}_{Path.GetFileNameWithoutExtension(sourceXlsx.Name)}";

                string createTableQuery = @$"
                  CREATE TABLE IF NOT EXISTS {tableName}
                    (id INTEGER PRIMARY KEY AUTOINCREMENT
                    , sheet TEXT
                ";
                createTableQuery += $", {string.Join(", ", worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column].Select(c => $"\"{c.Text}\" TEXT\n").ToList())}";
                createTableQuery += ");";

                string insertQuery = $"INSERT INTO {tableName} ("
                                    + string.Join(", ", worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column].Select(c => "\""+c.Text+"\"").ToList())
                                    + ")\nVALUES\n";
                List<string> rowList = new List<string>();
                // Iterate over each row
                // Start from row 2 to skip headers 
                for (int row = 2; row <= rowCount; row++) {
                  List<string> valueList = new List<string>();
                  // Iterate over each cell in the row
                  for (int col = 1; col <= columnCount; col++) {
                      valueList.Add(worksheet.Cells[row, col].Text);
                  }
                  if (!valueList.All(s => string.IsNullOrEmpty(s))) {
                    rowList.Add($"({string.Join(",", valueList.Select(v => string.IsNullOrEmpty(v) ? "NULL" : $"'{v}'"))})");
                  }
                }
                insertQuery += $"{string.Join(",", rowList)};";
                sheetQueryList.Add((createTableQuery, insertQuery));
            }
        }
        // foreach ((string createSql, string insertSql) q in sheetQueryList) {
        //   Console.WriteLine(q.createSql);
        //   Console.WriteLine(q.insertSql);
        // }

        string connectionString = $"Data Source={Path.Join(rootDir, "airflow.db")}";
        string testQuery = "SELECT * FROM dag;";
        try {
          foreach ((string createSql, string insertSql) in sheetQueryList) {
            // Console.WriteLine(q.createSql);
            // Console.WriteLine(q.insertSql);

            using(SqliteConnection conn = new SqliteConnection(connectionString)) {
              conn.Open();
              using (SqliteCommand cmd = new SqliteCommand(createSql, conn)) {
                Console.WriteLine($"Running:\n{createSql}");
                int rowsAffected = cmd.ExecuteNonQuery();
              }
              using (SqliteCommand cmd = new SqliteCommand(insertSql, conn)) {
                Console.WriteLine($"Running:\n{insertSql}");
                int rowsAffected = cmd.ExecuteNonQuery();
              }
            }
          }
          // using (SqliteConnection conn = new SqliteConnection(connectionString)) {
          //   conn.Open();
          //   var cmd = conn.CreateCommand();
          //   cmd.CommandText = testQuery;
          //   using (var reader = cmd.ExecuteReader()) {
          //     while (reader.Read()) {
          //       var name = reader.GetString(0);
          //       // Console.WriteLine($"Hello, {name}!");
          //     }
          //   }
          // }
        }
        catch (Exception ex) {
          Console.WriteLine($"ERROR: {ex.Message}");
        }
    }
}
