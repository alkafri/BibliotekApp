using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

class Program
{
    static void Main()
    {
        using (var db = new AppDbContext())
        {
            db.Database.Migrate();

            bool running = true;
            while (running)
            {
                Console.WriteLine("\n=== Welcome to BibliotekApp ===");
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. Exit");
                Console.Write("Choose: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": Register(db); break;
                    case "2": Login(db); break;
                    case "3": running = false; break;
                    default: Console.WriteLine("Invalid option!"); break;
                }
            }
        }
        Console.WriteLine("Program exited.");
    }

    // Register new user
    static void Register(AppDbContext db)
    {
        Console.Write("Enter username: ");
        string username = Console.ReadLine();
        Console.Write("Enter password: ");
        string password = Console.ReadLine();
        Console.Write("Is this an admin? (yes/no): ");
        bool isAdmin = Console.ReadLine().ToLower() == "yes";

        var user = new User { Username = username, Password = password, IsAdmin = isAdmin };
        db.Users.Add(user);
        db.SaveChanges();
        Console.WriteLine($"User {username} registered successfully.");
    }

    // Login function
    static void Login(AppDbContext db)
    {
        Console.Write("Username: ");
        string username = Console.ReadLine();
        Console.Write("Password: ");
        string password = Console.ReadLine();

        var user = db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        if (user == null) { Console.WriteLine("Login failed!"); return; }

        Console.WriteLine($"Login successful! Welcome {username}.");
        if (user.IsAdmin) AdminMenu(db);
        else UserMenu(db, user);
    }

    // Admin menu
    static void AdminMenu(AppDbContext db)
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine("\n--- Admin Menu ---");
            Console.WriteLine("1. Add Book");
            Console.WriteLine("2. Remove Book");
            Console.WriteLine("3. Update Book");
            Console.WriteLine("4. Show All Books");
            Console.WriteLine("5. Search and Sort Books");
            Console.WriteLine("6. Logout");
            Console.Write("Choose: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": AddBook(db); break;
                case "2": RemoveBook(db); break;
                case "3": UpdateBook(db); break;
                case "4": ShowBooks(db); break;
                case "5": SearchSortBooks(db); break;
                case "6": running = false; break;
                default: Console.WriteLine("Invalid option!"); break;
            }
        }
    }

    // User menu
    static void UserMenu(AppDbContext db, User user)
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine("\n--- User Menu ---");
            Console.WriteLine("1. Borrow Book");
            Console.WriteLine("2. Return Book");
            Console.WriteLine("3. Show All Books");
            Console.WriteLine("4. Search and Sort Books");
            Console.WriteLine("5. Logout");
            Console.Write("Choose: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": BorrowBook(db, user); break;
                case "2": ReturnBook(db, user); break;
                case "3": ShowBooks(db); break;
                case "4": SearchSortBooks(db); break;
                case "5": running = false; break;
                default: Console.WriteLine("Invalid option!"); break;
            }
        }
    }

    // CRUD book methods
    static void AddBook(AppDbContext db)
    {
        Console.Write("Book Title: "); string title = Console.ReadLine();
        Console.Write("Author: "); string author = Console.ReadLine();
        Console.Write("ISBN: "); string isbn = Console.ReadLine();

        var book = new Book { Title = title, Author = author, ISBN = isbn };
        db.Books.Add(book);
        db.SaveChanges();
        Console.WriteLine($"Book '{title}' added.");
    }

    static void RemoveBook(AppDbContext db)
    {
        Console.Write("Book ID to remove: "); int id = int.Parse(Console.ReadLine());
        var book = db.Books.FirstOrDefault(b => b.Id == id);
        if (book == null) { Console.WriteLine("Book not found!"); return; }
        db.Books.Remove(book);
        db.SaveChanges();
        Console.WriteLine($"Book '{book.Title}' removed.");
    }

    static void UpdateBook(AppDbContext db)
    {
        Console.Write("Book ID to update: "); int id = int.Parse(Console.ReadLine());
        var book = db.Books.FirstOrDefault(b => b.Id == id);
        if (book == null) { Console.WriteLine("Book not found!"); return; }

        Console.Write("New Title: "); book.Title = Console.ReadLine();
        Console.Write("New Author: "); book.Author = Console.ReadLine();
        Console.Write("New ISBN: "); book.ISBN = Console.ReadLine();

        db.SaveChanges();
        Console.WriteLine($"Book '{book.Title}' updated.");
    }

    static void ShowBooks(AppDbContext db)
    {
        var books = db.Books.ToList();
        if (books.Count == 0) { Console.WriteLine("No books."); return; }

        Console.WriteLine("\n--- Books List ---");
        foreach (var b in books)
        {
            // Check availability
            var borrowed = db.Borrowings.Any(br => br.BookId == b.Id && br.ReturnDate == null);
            string status = borrowed ? "Borrowed" : "Available";
            Console.WriteLine($"ID: {b.Id}, Title: {b.Title}, Author: {b.Author}, ISBN: {b.ISBN}, Status: {status}");
        }
    }

    static void BorrowBook(AppDbContext db, User user)
    {
        Console.Write("Book ID to borrow: "); int id = int.Parse(Console.ReadLine());
        var book = db.Books.FirstOrDefault(b => b.Id == id);
        if (book == null) { Console.WriteLine("Book not found!"); return; }

        bool borrowed = db.Borrowings.Any(br => br.BookId == id && br.ReturnDate == null);
        if (borrowed) { Console.WriteLine("Book already borrowed."); return; }

        var borrow = new Borrowing { UserId = user.Id, BookId = id, BorrowDate = DateTime.Now };
        db.Borrowings.Add(borrow);
        db.SaveChanges();
        Console.WriteLine($"You borrowed '{book.Title}'.");
    }

    static void ReturnBook(AppDbContext db, User user)
    {
        Console.Write("Book ID to return: "); int id = int.Parse(Console.ReadLine());
        var borrow = db.Borrowings.FirstOrDefault(br => br.BookId == id && br.UserId == user.Id && br.ReturnDate == null);
        if (borrow == null) { Console.WriteLine("Borrowing not found!"); return; }

        borrow.ReturnDate = DateTime.Now;
        db.SaveChanges();
        Console.WriteLine("Book returned.");
    }

    // Search and sort using manual algorithms (Bubble Sort)
    static void SearchSortBooks(AppDbContext db)
    {
        var books = db.Books.ToList();

        Console.Write("Enter keyword to search: ");
        string keyword = Console.ReadLine().ToLower();

        // Manual search
        List<Book> results = new List<Book>();
        foreach (var b in books)
        {
            if (b.Title.ToLower().Contains(keyword) || b.Author.ToLower().Contains(keyword) || b.ISBN.ToLower().Contains(keyword))
                results.Add(b);
        }

        if (results.Count == 0) { Console.WriteLine("No books found."); return; }

        // Bubble sort by Title
        for (int i = 0; i < results.Count - 1; i++)
        {
            for (int j = 0; j < results.Count - i - 1; j++)
            {
                if (string.Compare(results[j].Title, results[j + 1].Title) > 0)
                {
                    var temp = results[j];
                    results[j] = results[j + 1];
                    results[j + 1] = temp;
                }
            }
        }

        Console.WriteLine("\n--- Search & Sorted Results ---");
        foreach (var b in results)
        {
            var borrowed = db.Borrowings.Any(br => br.BookId == b.Id && br.ReturnDate == null);
            string status = borrowed ? "Borrowed" : "Available";
            Console.WriteLine($"ID: {b.Id}, Title: {b.Title}, Author: {b.Author}, ISBN: {b.ISBN}, Status: {status}");
        }
    }
}
