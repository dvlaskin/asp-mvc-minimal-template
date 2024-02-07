using WebApp.Domain.Interfaces;

namespace WebApp.Services;

public class FakeEmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        await Task.Run(() =>
        {
            Console.WriteLine("=> Send Email:");
            Console.WriteLine($"=> email: {email}");
            Console.WriteLine($"=> subject: {subject}");
            Console.WriteLine($"=> message: {message}");
        });

    }
}