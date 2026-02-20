using Microsoft.Data.SqlClient;
using Dapper;

namespace terminalbibliotek;

public class DbInitializer
{
    public bool Initialize(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);

        connection.Execute(@"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'authors')
BEGIN
CREATE TABLE authors (
 id INT IDENTITY(1,1) PRIMARY KEY,
 name NVARCHAR(50),
 birth_year INT)
END");

        connection.Execute(@"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'books')
BEGIN
CREATE TABLE books (
 id INT IDENTITY(1,1) PRIMARY KEY,
 name NVARCHAR(50),
 author_id INT REFERENCES authors(id),
 published INT,
 genre NVARCHAR(50)
)
END");

        connection.Execute(@"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'book_author')
BEGIN
CREATE TABLE book_author (
 book_id INT REFERENCES books(id) ON DELETE CASCADE,
 author_id INT REFERENCES authors(id) ON DELETE CASCADE,
 PRIMARY KEY (book_id, author_id)
)
END");

        return true;
    }
}