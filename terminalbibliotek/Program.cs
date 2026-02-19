using System.Data.SqlClient;

namespace terminalbibliotek;

class Program
{
    static void Main(string[] args)
    { 
        using var connection = new SqlConnection("Server=localhost,1433;Database=terminalbibliotek;User ID=sa;Password=Lösenord!;Encrypt=True;TrustServerCertificate=True;");
    }
}