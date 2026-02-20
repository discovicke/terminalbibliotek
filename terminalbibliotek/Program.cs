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
        
        
        connection.Open();
        var bookCommand = new SqlCommand(@"
IF NOT EXISTS (
SELECT * FROM sys.tables WHERE name = 'books') 
BEGIN
CREATE TABLE books (
            id INT IDENTITY(1,1) PRIMARY KEY,
            name NVARCHAR(50),
            author_id INT REFERENCES authors(id));
END", connection);
        bookCommand.ExecuteNonQuery();
        connection.Close();
        
        connection.Open();
        var bridgeCommand = new SqlCommand(@"
IF NOT EXISTS (
SELECT * FROM sys.tables WHERE name = 'book_author')
BEGIN
CREATE TABLE book_author (
            book_id INT REFERENCES books(id) ON DELETE CASCADE,
            author_id INT REFERENCES authors(id) ON DELETE CASCADE,
            PRIMARY KEY (book_id, author_id));
END", connection);
        bridgeCommand.ExecuteNonQuery();
        connection.Close();
        
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: terminalbibliotek <action> <entity> [parameters]");
            Console.WriteLine("Actions: list (l), add (a), remove (r)");
            Console.WriteLine("Entities: author (a)");
            return;
        }
        
        if ((args[0] == "l" || args[0] == "list") && (args[1] == "a" || args[1] == "author"))
        {
            if (args.Length > 2 && (args[2] == "--books" || args[2] == "-b"))
            {
                connection.Open();
                var juncCommand = new SqlCommand(@"
SELECT 
    authors.name,
b.name AS book_name
FROM authors 
    JOIN book_author ba ON authors.id = ba.author_id
    JOIN books b ON ba.book_id = b.id", connection);
                var juncReader = juncCommand.ExecuteReader();

                while (juncReader.Read())
                {
                    Console.WriteLine($"{juncReader["name"]}: {juncReader["book_name"]}");
                }
                
                connection.Close();

                return;
            }
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

        if ((args[0] == "r" && args[1] == "a") || (args[0] == "remove" && args[1] == "author"))
        {
            connection.Open();
            var deleteCommand = new SqlCommand("DELETE FROM authors WHERE name = @name", connection);
            deleteCommand.Parameters.AddWithValue("@name", args[2]);
            deleteCommand.ExecuteNonQuery();
            connection.Close();
            Console.WriteLine($"Författare '{args[2]}' har tagits bort!");
        }

        if ((args[0] == "a" && args[1] == "b") || (args[0] == "add" && args[1] == "book"))
        {
            connection.Open();
            var insertCommand = new SqlCommand("INSERT INTO books (name) VALUES (@name)", connection);
            insertCommand.Parameters.AddWithValue("@name", args[2]);
            insertCommand.ExecuteNonQuery();
            connection.Close();
            Console.WriteLine($"Bok '{args[2]}' har lagts till!");
        }

        if ((args[0] == "r" && args[1] == "b") || (args[0] == "remove" && args[1] == "book"))
        {
            connection.Open();
            var deleteCommand = new SqlCommand("DELETE FROM books WHERE name = @name", connection);
            deleteCommand.Parameters.AddWithValue("@name", args[2]);
            deleteCommand.ExecuteNonQuery();
            connection.Close();
            Console.WriteLine($"Bok '{args[2]}' har tagits bort!");
        }

        if ((args[0] == "l" && args[1] == "b") || (args[0] == "list" && args[1] == "books"))
        {
            if (args.Length > 2 && (args[2] == "--authors" || args[2] == "-a"))
            {
                connection.Open();
                var juncCommand = new SqlCommand(@"
SELECT 
    books.name,
a.name AS author_name
FROM books 
    JOIN book_author ba ON books.id = ba.book_id
    JOIN authors a ON ba.book_id = a.id", connection);
                var juncReader = juncCommand.ExecuteReader();

                while (juncReader.Read())
                {
                    Console.WriteLine($"{juncReader["name"]}: {juncReader["author_name"]}");
                }
                
                connection.Close();

                return;
            }
            
            
            connection.Open();
            var listCommand = new SqlCommand("SELECT * FROM books", connection);
            var reader = listCommand.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader["id"]}: {reader["name"]}");
            }
            connection.Close();
        }

        if ((args[0] == "m" && args[1] == "a" && args[3] == "a" && args[4] == "b") ||
            (args[0] == "modify" && args[1] == "author" && args[3] == "add" && args[4] == "book"))
        {
            
            connection.Open();
            var authorlistCommand = new SqlCommand("SELECT id FROM authors WHERE name = @name", connection);
            authorlistCommand.Parameters.AddWithValue("@name", args[2]);
            var authResult = authorlistCommand.ExecuteScalar();

            if (authResult == null)
            {
                Console.WriteLine($"Författare '{args[2]}' hittades inte!");
                connection.Close();
                return;
            }
            
            var authorId = Convert.ToInt32(authResult);

            var booklistCommand = new SqlCommand("SELECT id FROM books WHERE name = @name", connection);
            booklistCommand.Parameters.AddWithValue("@name", args[5]);
            var bookResult = booklistCommand.ExecuteScalar();

            if (bookResult == null)
            {
                Console.WriteLine($"Bok '{args[5]}' hittades inte!");
                connection.Close();
                return;
            }
            var bookId = Convert.ToInt32(bookResult);
            
            var connectionCommand = new SqlCommand("INSERT INTO book_author (book_id, author_id) VALUES (@book_id, @author_id)", connection);
            connectionCommand.Parameters.AddWithValue("@book_id", bookId);
            connectionCommand.Parameters.AddWithValue("@author_id", authorId);
            connectionCommand.ExecuteNonQuery();
            connection.Close();
        }
        
        if ((args[0] == "m" && args[1] == "a" && args[3] == "r" && args[4] == "b") ||
            (args[0] == "modify" && args[1] == "author" && args[3] == "remove" && args[4] == "book"))
        {
            connection.Open();
            var authorlistCommand = new SqlCommand("SELECT id FROM authors WHERE name = @name", connection);
            authorlistCommand.Parameters.AddWithValue("@name", args[2]);
            var authResult = authorlistCommand.ExecuteScalar();

            if (authResult == null)
            {
                Console.WriteLine($"Författare '{args[2]}' hittades inte!");
                connection.Close();
                return;
            }
            
            var authorId = Convert.ToInt32(authResult);

            var booklistCommand = new SqlCommand("SELECT id FROM books WHERE name = @name", connection);
            booklistCommand.Parameters.AddWithValue("@name", args[5]);
            var bookResult = booklistCommand.ExecuteScalar();

            if (bookResult == null)
            {
                Console.WriteLine($"Bok '{args[5]}' hittades inte!");
                connection.Close();
                return;
            }
            var bookId = Convert.ToInt32(bookResult);
            
            var connectionId = new SqlCommand(@"DELETE FROM book_author WHERE  
                                              book_id = @bookId AND author_id = @authorId", connection);
            connectionId.Parameters.AddWithValue("@bookId", bookId);
            connectionId.Parameters.AddWithValue("@authorId", authorId);
            connectionId.ExecuteNonQuery();
            connection.Close();
        }
    }
}