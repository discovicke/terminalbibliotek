using System.Data.SqlClient;
using Dapper;

namespace terminalbibliotek;

class Program
{
    static void Main(string[] args)
    {
        using var connection =
            new SqlConnection(
                "Server=localhost,1433;Database=terminalbibliotek;User ID=sa;Password=Lösenord!;Encrypt=True;TrustServerCertificate=True;");
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
        
        if ((args[0] == "l" || args[0] == "list") && (args[1] == "a" || args[1] == "author"))
        {
            connection.Open();
            var listCommand = new SqlCommand("SELECT * FROM authors", connection);
            var reader = listCommand.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader["id"]}: {reader["name"]}");
            }
            connection.Close();
        }

        if ((args[0] == "a" && args[1] == "a") || (args[0] == "add" && args[1] == "author"))
        {
            connection.Open();
            var insertCommand = new SqlCommand("INSERT INTO authors (name) VALUES (@name)", connection);
            insertCommand.Parameters.AddWithValue("@name", args[2]);
            insertCommand.ExecuteNonQuery();
            connection.Close();
            Console.WriteLine($"Författare '{args[2]}' har lagts till!");
        }
        
    }


    private List<string> authors = new List<string>
    {
        "August Strindberg",
        "Selma Lagerlöf",
        "Kalle Anka",
        "Kajsa Anka",
        "Leif GW Persson",
        "Astrid Lindgren",
        "Tove Jansson",
        "Anders Andersson",
        "Nisse Pettersson",
        "Elsa Tokström",
        "Sune Hejåhå"
    };
}