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
        DbInitializer db = new DbInitializer();
        if (!db.Initialize(connection.ConnectionString))
        {
            Console.WriteLine("Databasen kunde inte initialiseras.");
            return;
        }

        switch (args[0].ToLowerInvariant())
        {
            case "l":
            case "list":
                if (args.Length > 1)
                {
                    if (args[1].ToLowerInvariant() == "a" || args[1].ToLowerInvariant() == "authors")
                    {
                        if (args.Length > 2 && (args[2] == "--books" || args[2] == "-b"))
                        {
                            connection.Open();
                            var bookAndAuthors = connection.Query(@"
                                SELECT 
                                a.name, 
                                b.name AS book_name
                                FROM book_author ba
                                JOIN authors a ON ba.author_id = a.id
                                JOIN books b ON ba.book_id = b.id
                                ");
                            foreach (var author in bookAndAuthors)
                            {
                                Console.WriteLine($"{author.name}: {author.book_name}");
                            }

                            connection.Close();
                            return;
                        }

                        connection.Open();
                        var authors = connection.Query("SELECT * FROM authors");
                        foreach (var author in authors)
                        {
                            Console.WriteLine($"{author.id}: {author.name} | {author.birth_year}");
                        }

                        connection.Close();
                        return;
                    }

                    if (args[1].ToLowerInvariant() == "b" || args[1].ToLowerInvariant() == "books")
                    {
                        if (args.Length > 2 && (args[2] == "--authors" || args[2] == "-a"))
                        {
                            connection.Open();
                            var authorAndBooks = connection.Query(@"
                                SELECT 
                                b.name, 
                                a.name AS author_name
                                FROM book_author ba
                                JOIN books b ON b.id = ba.book_id
                                JOIN authors a ON ba.author_id = a.id
                                ");
                            foreach (var book in authorAndBooks)
                            {
                                Console.WriteLine($"{book.name}: {book.author_name}");
                            }

                            connection.Close();
                            return;
                        }

                        connection.Open();
                        var listCommand = connection.Query("SELECT * FROM books");
                        foreach (var book in listCommand)
                        {
                            Console.WriteLine($"{book.id}: {book.name} | {book.published} | {book.genre}");
                        }

                        connection.Close();
                        return;
                    }
                }

                Console.WriteLine("Ogiltigt kommando. Använd 'list authors' eller 'list books'.");
                break;
            case "a":
            case "add":
                if (args[1].ToLowerInvariant() == "author" || args[1].ToLowerInvariant() == "a")
                {
                    connection.Open();
                    connection.Execute("INSERT INTO authors (name) VALUES (@name)", new { name = args[2] });
                    connection.Close();
                    Console.WriteLine($"Författare '{args[2]}' har lagts till!");
                    return;
                }

                if (args[1].ToLowerInvariant() == "book" || args[1].ToLowerInvariant() == "b")
                {
                    connection.Open();
                    connection.Execute("INSERT INTO books (name) VALUES (@name)", new { name = args[2] });
                    connection.Close();
                    Console.WriteLine($"Bok '{args[2]}' har lagts till!");
                    return;
                }

                break;
            case "r":
            case "remove":
                if (args[1].ToLowerInvariant() == "author" || args[1].ToLowerInvariant() == "a")
                {
                    connection.Open();
                    connection.Execute("DELETE FROM authors WHERE name = @name", new { name = args[2] });
                    connection.Close();
                    Console.WriteLine($"Författare '{args[2]}' har tagits bort!");
                    return;
                }

                if (args[1].ToLowerInvariant() == "book" || args[1].ToLowerInvariant() == "b")
                {
                    connection.Open();
                    connection.Execute("DELETE FROM books WHERE name = @name", new { name = args[2] });
                    connection.Close();
                    Console.WriteLine($"Bok '{args[2]}' har tagits bort!");
                    return;
                }

                break;
            case "m":
            case "modify":
                if (args[1].ToLowerInvariant() == "author" || args[1].ToLowerInvariant() == "a")
                {
                    if (args[3].ToLowerInvariant() == "set" && args[3].ToLowerInvariant() == "s" ||
                        args[4].ToLowerInvariant() == "born" && args[4].ToLowerInvariant() == "b")
                    {
                        connection.Open();
                        connection.Execute("UPDATE authors SET birth_year = @birth_year WHERE name = @name",
                            new { birth_year = args[5], name = args[4] });
                        connection.Close();
                        Console.WriteLine($"{args[4]} har uppdaterats med {args[5]}!}");
                    }

                    if (args[3].ToLowerInvariant() == "add" && args[3].ToLowerInvariant() == "a" ||
                        args[4].ToLowerInvariant() == "book" && args[4].ToLowerInvariant() == "b")
                    {
                        connection.Open();
                        var authId = connection.Query("SELECT id FROM authors WHERE name = @name",
                            new { name = args[2] });
                        var bookId = connection.Query("SELECT id FROM books WHERE name = @name",
                            new { name = args[5] });
                        connection.Execute("INSERT INTO book_author (book_id, author_id) VALUES (@book_id, @author_id)",
                            new { book_id = bookId, author_id = authId });
                        connection.Close();
                    }

                    if (args[3] == "remove" || args[3] == "r" &&
                        args[4] == "book" || args[4] == "b")
                    {
                        connection.Open();
                        var authId = connection.Query("SELECT id FROM authors WHERE name = @name",
                            new { name = args[2] });
                        var bookId = connection.Query("SELECT id FROM books WHERE name = @name",
                            new { name = args[5] });
                        connection.Execute("DELETE FROM book_author WHERE book_id = @book_id AND author_id = @author_id",
                            new { book_id = bookId, author_id = authId });
                        connection.Close();
                    }
                }
                
                break;
        }

        if ((args[0] == "m" && args[1] == "b" && args[3] == "s" && args[4] == "p") ||
            (args[0] == "modify" && args[1] == "book" && args[3] == "set" && args[4] == "published"))
        {
            if (!int.TryParse(args[5], out _))
            {
                return;
            }

            connection.Open();
            var booklistCommand = new SqlCommand("SELECT id FROM books WHERE name = @name", connection);
            booklistCommand.Parameters.AddWithValue("@name", args[2]);
            var bookResult = booklistCommand.ExecuteScalar();

            if (bookResult == null)
            {
                Console.WriteLine($"Bok '{args[2]}' hittades inte!");
                connection.Close();
                return;
            }

            var bookId = Convert.ToInt32(bookResult);
            var updateBookCommand = new SqlCommand(@"UPDATE books 
    SET published = @published
    WHERE name = @name", connection);
            updateBookCommand.Parameters.AddWithValue("@published", args[5]);
            updateBookCommand.Parameters.AddWithValue("@name", args[2]);
            updateBookCommand.ExecuteNonQuery();
            connection.Close();
        }

        if ((args[0] == "m" && args[1] == "b" && args[3] == "s" && args[4] == "g") ||
            (args[0] == "modify" && args[1] == "book" && args[3] == "set" && args[4] == "genre"))
        {
            connection.Open();
            var booklistCommand = new SqlCommand("SELECT id FROM books WHERE name = @name", connection);
            booklistCommand.Parameters.AddWithValue("@name", args[2]);
            var bookResult = booklistCommand.ExecuteScalar();

            if (bookResult == null)
            {
                Console.WriteLine($"Bok '{args[2]}' hittades inte!");
                connection.Close();
                return;
            }

            var updateBookCommand = new SqlCommand(@"UPDATE books 
    SET genre = @genre
    WHERE name = @name", connection);
            updateBookCommand.Parameters.AddWithValue("@genre", args[5]);
            updateBookCommand.Parameters.AddWithValue("@name", args[2]);
            updateBookCommand.ExecuteNonQuery();
            connection.Close();
        }
    }
}