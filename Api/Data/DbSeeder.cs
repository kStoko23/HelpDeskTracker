using Api.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(HelpDeskDbContext db)
    {
        if (await db.Users.AnyAsync())
            return;

        Randomizer.Seed = new Random(123);

        var now = DateTime.UtcNow;
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test1234!");

        // USERS
        var admin = new User
        {
            Email = "admin@test.com",
            Username = "admin",
            PasswordHash = passwordHash,
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = now
        };

        var userFaker = new Faker<User>("pl")
            .RuleFor(x => x.Email, f => f.Internet.Email().ToLower())
            .RuleFor(x => x.Username, f => f.Internet.UserName())
            .RuleFor(x => x.PasswordHash, _ => passwordHash)
            .RuleFor(x => x.Role, f => f.PickRandom(UserRole.Agent, UserRole.Client))
            .RuleFor(x => x.IsActive, f => f.Random.Bool(0.9f))
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(1).ToUniversalTime());

        var users = userFaker.Generate(40);
        users.Add(admin);

        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        var allUserIds = users.Select(x => x.Id).ToList();
        var agentIds = users
            .Where(x => x.Role is UserRole.Agent or UserRole.Admin)
            .Select(x => x.Id)
            .ToList();

        var clientIds = users
            .Where(x => x.Role == UserRole.Client)
            .Select(x => x.Id)
            .ToList();

        // TICKETS
        var ticketFaker = new Faker<Ticket>("pl")
            .RuleFor(x => x.Title, f => f.Hacker.Phrase())
            .RuleFor(x => x.Description, f => f.Lorem.Paragraphs(2))
            .RuleFor(x => x.Status, f => f.PickRandom<TicketStatus>())
            .RuleFor(x => x.Priority, f => f.PickRandom<TicketPriority>())
            .RuleFor(x => x.Category, f => f.PickRandom<TicketCategory>())
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(1).ToUniversalTime())
            .RuleFor(x => x.UpdatedAt, (f, x) =>
                f.Random.Bool(0.7f)
                    ? f.Date.Between(x.CreatedAt, now).ToUniversalTime()
                    : null)
            .RuleFor(x => x.CreatedById, f => f.PickRandom(allUserIds))
            .RuleFor(x => x.AssignedToId, (f, x) =>
                x.Status == TicketStatus.Open && f.Random.Bool(0.5f)
                    ? null
                    : f.PickRandom(agentIds))
            .RuleFor(x => x.ClosedAt, (f, x) =>
                x.Status == TicketStatus.Closed
                    ? f.Date.Between(x.CreatedAt, now).ToUniversalTime()
                    : null);

        var tickets = ticketFaker.Generate(250);

        db.Tickets.AddRange(tickets);
        await db.SaveChangesAsync();

        var ticketIds = tickets.Select(x => x.Id).ToList();

        // COMMENTS
        var commentFaker = new Faker<Comment>("pl")
            .RuleFor(x => x.Content, f => f.Lorem.Paragraph())
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(1).ToUniversalTime())
            .RuleFor(x => x.TicketId, f => f.PickRandom(ticketIds))
            .RuleFor(x => x.AuthorId, f => f.PickRandom(allUserIds))
            .RuleFor(x => x.IsInternal, f => f.Random.Bool(0.25f));

        var comments = commentFaker.Generate(700);

        db.Comments.AddRange(comments);
        await db.SaveChangesAsync();

        var commentIds = comments.Select(x => x.Id).ToList();

        // TICKET HISTORY
        var fields = new[] { "Status", "Priority", "Category", "AssignedToId" };

        var historyFaker = new Faker<TicketHistory>("pl")
            .RuleFor(x => x.TicketId, f => f.PickRandom(ticketIds))
            .RuleFor(x => x.ChangedById, f => f.PickRandom(agentIds))
            .RuleFor(x => x.ChangedAt, f => f.Date.Past(1).ToUniversalTime())
            .RuleFor(x => x.Field, f => f.PickRandom(fields))
            .RuleFor(x => x.OldValue, (f, x) => GenerateOldValue(f, x.Field, agentIds))
            .RuleFor(x => x.NewValue, (f, x) => GenerateNewValue(f, x.Field, agentIds));

        var histories = historyFaker.Generate(500);

        db.Set<TicketHistory>().AddRange(histories);
        await db.SaveChangesAsync();

        // ATTACHMENTS
        var attachmentFaker = new Faker<Attachment>("pl")
            .RuleFor(x => x.FileName, f => f.System.FileName(f.PickRandom("jpg", "png", "pdf", "txt", "docx", "xlsx", "zip")))
            .RuleFor(x => x.StoredPath, f => $"/uploads/{Guid.NewGuid()}{f.PickRandom(".jpg", ".png", ".pdf", ".txt", ".docx", ".xlsx", ".zip")}")
            .RuleFor(x => x.ContentType, f => f.PickRandom(
                "image/jpeg",
                "image/png",
                "application/pdf",
                "text/plain",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/zip"))
            .RuleFor(x => x.SizeBytes, f => f.Random.Long(10_000, 10_000_000))
            .RuleFor(x => x.UploadedAt, f => f.Date.Past(1).ToUniversalTime())
            .RuleFor(x => x.TicketId, f => f.Random.Bool(0.65f) ? f.PickRandom(ticketIds) : null)
            .RuleFor(x => x.CommentId, (f, x) => x.TicketId == null ? f.PickRandom(commentIds) : null);

        var attachments = attachmentFaker.Generate(120);

        db.Attachments.AddRange(attachments);
        await db.SaveChangesAsync();

        // REFRESH TOKENS
        var refreshTokenFaker = new Faker<RefreshToken>("pl")
            .RuleFor(x => x.TokenHash, f => f.Random.AlphaNumeric(64))
            .RuleFor(x => x.UserId, f => f.PickRandom(allUserIds))
            .RuleFor(x => x.CreatedAt, f => f.Date.Recent(30).ToUniversalTime())
            .RuleFor(x => x.ExpiresAt, (f, x) => x.CreatedAt.AddDays(f.Random.Int(1, 30)))
            .RuleFor(x => x.Revoked, f => f.Random.Bool(0.25f));

        var refreshTokens = refreshTokenFaker.Generate(60);

        db.Set<RefreshToken>().AddRange(refreshTokens);
        await db.SaveChangesAsync();
    }

    private static string GenerateOldValue(Faker f, string fieldName, List<long> agentIds)
    {
        return fieldName switch
        {
            "Status" => f.PickRandom<TicketStatus>().ToString(),
            "Priority" => f.PickRandom<TicketPriority>().ToString(),
            "Category" => f.PickRandom<TicketCategory>().ToString(),
            "AssignedToId" => f.PickRandom(agentIds).ToString(),
            _ => "Unknown"
        };
    }

    private static string GenerateNewValue(Faker f, string fieldName, List<long> agentIds)
    {
        return fieldName switch
        {
            "Status" => f.PickRandom<TicketStatus>().ToString(),
            "Priority" => f.PickRandom<TicketPriority>().ToString(),
            "Category" => f.PickRandom<TicketCategory>().ToString(),
            "AssignedToId" => f.PickRandom(agentIds).ToString(),
            _ => "Unknown"
        };
    }
}