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
                ListItems(args, connection);
                break;
            case "a":
            case "add":
                AddItems(args, connection);
                break;
            case "r":
            case "remove":
                RemoveItems(args, connection);
                break;
            case "m":
            case "modify":
                ModifyItems(args, connection);
                break;
        }

        static void ListItems(string[] args, SqlConnection connection)
        {
            if (args.Length > 1)
            {
                switch (args[1].ToLowerInvariant())
                {
                    case "a":
                    case "authors":
                        ListAuthors(args, connection);
                        break;
                    case "b":
                    case "books":
                        ListBooks(args, connection);
                        break;
                    default:
                        Console.WriteLine($"'{args[1]}' hittades inte!");
                        break;
                }
            }
        }


        static void ListAuthors(string[] args, SqlConnection connection)
        {
            if (args.Length > 2 && (args[2].ToLowerInvariant() == "--books" || args[2].ToLowerInvariant() == "-b"))
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
        }

        static void ListBooks(string[] args, SqlConnection connection)
        {
            if (args.Length > 2 && (args[2].ToLowerInvariant() == "--authors" || args[2].ToLowerInvariant() == "-a"))
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
        }
    }

    static void AddItems(string[] args, SqlConnection connection)
    {
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
        }
    }

    static void RemoveItems(string[] args, SqlConnection connection)
    {
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
        }
    }

    static void ModifyItems(string[] args, SqlConnection connection)
    {
        if (args.Length > 2)
        {
            switch (args[1].ToLowerInvariant())
            {
                case "author":
                case "a":
                    ModifyAuthor(args, connection);
                    break;
                case "book":
                case "b":
                    ModifyBooks(args, connection);
                    break;
                default:
                    Console.WriteLine($"'{args[1]}' gick inte att ändra!");
                    break;
            }
        }
    }

    static void ModifyAuthor(string[] args, SqlConnection connection)
    {
        if (args.Length > 3)
        {
            switch (args[3].ToLowerInvariant())
            {
                case "set":
                case "s":
                    SetBirthYear(args, connection);
                    break;
                case "add":
                case "a":
                    AddBookToAuthor(args, connection);
                    break;
                case "remove":
                case "r":
                    RemoveBookFromAuthor(args, connection);
                    break;
            }
        }
    }

    static void SetBirthYear(string[] args, SqlConnection connection)
    {
        connection.Open();
        connection.Execute("UPDATE authors SET birth_year = @birth_year WHERE name = @name",
            new { birth_year = args[5], name = args[2] });
        connection.Close();
        Console.WriteLine($"{args[2]} har uppdaterats med {args[5]}!");
    }

    static void AddBookToAuthor(string[] args, SqlConnection connection)
    {
        connection.Open();
        var authId = connection.QueryFirst<int>("SELECT id FROM authors WHERE name = @name",
            new { name = args[2] });
        var bookId = connection.QueryFirst<int>("SELECT id FROM books WHERE name = @name",
            new { name = args[5] });
        connection.Execute("INSERT INTO book_author (book_id, author_id) VALUES (@book_id, @author_id)",
            new { book_id = bookId, author_id = authId });
        connection.Close();
    }

    static void RemoveBookFromAuthor(string[] args, SqlConnection connection)
    {
        connection.Open();
        var authId = connection.QueryFirst<int>("SELECT id FROM authors WHERE name = @name",
            new { name = args[2] });
        var bookId = connection.QueryFirst<int>("SELECT id FROM books WHERE name = @name",
            new { name = args[5] });
        connection.Execute(
            "DELETE FROM book_author WHERE book_id = @book_id AND author_id = @author_id",
            new { book_id = bookId, author_id = authId });
        connection.Close();
    }

    static void ModifyBooks(string[] args, SqlConnection connection)
    {
        if (args.Length > 4)
        {
            switch (args[4].ToLowerInvariant())
            {
                case "published":
                case "p":
                    SetPublishedToBook(args, connection);
                    break;
                case "genre":
                case "g":
                    SetGenreToBook(args, connection);
                    break;
                default:
                    Console.WriteLine($"'{args[4]}' har inte kunnat ändras!");
                    break;
            }
        }
    }

    static void SetPublishedToBook(string[] args, SqlConnection connection)
    {
        connection.Open();
        connection.Execute("UPDATE books SET published = @published WHERE name = @name",
            new { published = args[5], name = args[2] });
        connection.Close();
    }

    static void SetGenreToBook(string[] args, SqlConnection connection)
    {
        connection.Open();
        connection.Execute("UPDATE books SET genre = @genre WHERE name = @name",
            new { genre = args[5], name = args[2] });
    }
}
