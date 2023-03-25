using WebApp.Domain.Interfaces;

namespace WebApp.Services;

public class FakeEmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        await Task.Run(() =>
        {
            System.Console.WriteLine("=> Send Email:");
            System.Console.WriteLine($"=> email: {email}");
            System.Console.WriteLine($"=> subject: {subject}");
            System.Console.WriteLine($"=> message: {message}");
        });

    }
}