using Microsoft.AspNetCore.Identity.UI.Services;

namespace ASP.NET_HW_20.Tools;

public class MockEmailSender : IEmailSender {
    private readonly ILogger<MockEmailSender> _logger;

    public MockEmailSender(ILogger<MockEmailSender> logger) {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string message) {
        _logger.LogInformation($"Mock email sent to: {email}");
        _logger.LogInformation($"Subject: {subject}");
        _logger.LogInformation($"Message: {message}");
        return Task.CompletedTask;
    }
}