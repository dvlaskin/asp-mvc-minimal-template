using System.Text.Encodings.Web;
using WebApp.Domain.Interfaces;

namespace WebApp.Domain.Extensions;

public static class EmailSenderExtensions
{
    public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
    {
        return emailSender.SendEmailAsync(
            email,
            "Confirm your email",
            $"Please confirm your account by clicking this link: <a href='{link}'>link</a>"
            );
    }
}