using System.Data.SqlClient;

namespace terminalbibliotek;

class Program
{
    static void Main(string[] args)
    {
        using var connection = new SqlConnection("Server=localhost,1433;Database=terminalbibliotek;User ID=sa;Password=Lösenord!;Encrypt=True;TrustServerCertificate=True;");
        connection.Open();
        var command = new SqlCommand(@"
IF NOT EXISTS (
SELECT * FROM sys.tables WHERE name = 'authors') 
BEGIN
CREATE TABLE authors (
            id INT IDENTITY(1,1) PRIMARY KEY,
            name NVARCHAR(50));
END", connection);
        command.ExecuteNonQuery();
        connection.Close();
    }
}