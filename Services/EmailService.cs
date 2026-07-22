using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ProsocAPI.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string username, string password, string phoneNumber, string roleName);
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
        Task SendAdhesionConfirmationEmailAsync(string toEmail, string affilieName, string codeAdhesion, string typeAdhesion);
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string username, string password, string phoneNumber, string roleName)
        {
            var subject = "Bienvenue sur Prosoc Platform";

            var htmlBody = GenerateWelcomeEmailHtml(username, password, phoneNumber, toEmail, roleName);

            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var subject = "Réinitialisation de votre mot de passe - Prosoc Platform";

            var htmlBody = GeneratePasswordResetEmailHtml(resetToken);

            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendAdhesionConfirmationEmailAsync(string toEmail, string affilieName, string codeAdhesion, string typeAdhesion)
        {
            var subject = "Confirmation d'adhésion - Prosoc Platform";

            var htmlBody = GenerateAdhesionConfirmationEmailHtml(affilieName, codeAdhesion, typeAdhesion);

            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                var smtpServer = emailSettings["SmtpServer"];
                var port = int.Parse(emailSettings["Port"] ?? "587");
                var senderEmail = emailSettings["SenderEmail"];
                var password = emailSettings["Password"];
                var senderName = emailSettings["SenderName"];

                using var smtpClient = new SmtpClient(smtpServer, port)
                {
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        private string GenerateWelcomeEmailHtml(string username, string password, string phoneNumber, string email, string roleName)
        {
            return $@"
<!DOCTYPE html>
<html lang='fr'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Bienvenue sur Prosoc</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background-color: #f5f5f5;
            margin: 0;
            padding: 0;
            line-height: 1.6;
        }}
        .email-wrapper {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
        }}
        .header {{
            background-color: #232f3e;
            padding: 30px 40px;
            text-align: center;
        }}
        .header-logo {{
            color: #ffffff;
            font-size: 28px;
            font-weight: 600;
            letter-spacing: 1px;
            margin: 0;
        }}
        .header-logo .highlight {{
            color: #ff9900;
        }}
        .content {{
            padding: 40px;
            color: #232f3e;
        }}
        .title {{
            font-size: 24px;
            font-weight: 600;
            color: #232f3e;
            margin: 0 0 20px 0;
        }}
        .greeting {{
            font-size: 16px;
            color: #232f3e;
            margin: 0 0 20px 0;
        }}
        .message {{
            font-size: 16px;
            color: #666666;
            margin: 0 0 30px 0;
            line-height: 1.7;
        }}
        .highlight-box {{
            background-color: #f8f9fa;
            border-left: 4px solid #ff9900;
            padding: 20px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .highlight-box-title {{
            font-weight: 600;
            color: #232f3e;
            margin: 0 0 10px 0;
        }}
        .info-section {{
            background-color: #f8f9fa;
            padding: 25px;
            border-radius: 8px;
            margin: 25px 0;
        }}
        .info-section-title {{
            color: #232f3e;
            font-size: 18px;
            font-weight: 600;
            margin: 0 0 15px 0;
        }}
        .credential-row {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 8px 0;
            border-bottom: 1px solid #e1e5e9;
        }}
        .credential-row:last-child {{
            border-bottom: none;
        }}
        .credential-label {{
            font-weight: 600;
            color: #232f3e;
            min-width: 120px;
        }}
        .credential-value {{
            color: #666666;
            font-family: 'Courier New', monospace;
            background-color: #ffffff;
            padding: 4px 8px;
            border-radius: 4px;
            border: 1px solid #e1e5e9;
        }}
        .warning-box {{
            background-color: #fff3cd;
            border: 1px solid #ffeaa7;
            color: #856404;
            padding: 15px;
            border-radius: 4px;
            margin: 20px 0;
            font-size: 14px;
        }}
        .button {{
            display: inline-block;
            background-color: #ff9900;
            color: #ffffff;
            text-decoration: none;
            padding: 12px 30px;
            border-radius: 6px;
            font-weight: 600;
            font-size: 16px;
            transition: background-color 0.3s ease;
        }}
        .button:hover {{
            background-color: #e68900;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px 40px;
            text-align: center;
            border-top: 1px solid #e1e5e9;
        }}
        .footer-text {{
            color: #666666;
            font-size: 12px;
            margin: 5px 0;
        }}
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <div class='header'>
            <h1 class='header-logo'>PROSOC<span class='highlight'>.</span></h1>
        </div>
        
        <div class='content'>
            <h2 class='title'>Bienvenue sur Prosoc Platform !</h2>
            
            <p class='greeting'>Cher(e) Agent,</p>
            
            <p class='message'>
                Votre compte agent a été créé avec succès sur la plateforme Prosoc. 
                Vous êtes maintenant {roleName} et pouvez accéder à toutes les fonctionnalités réservées à votre rôle.
            </p>
            
            <div class='highlight-box'>
                <p class='highlight-box-title'>En tant qu'agent, vous pourrez :</p>
                <ul style='margin: 10px 0; padding-left: 25px; color: #666666;'>
                    <li style='margin: 5px 0;'>Gérer les adhésions et les affiliés</li>
                    <li style='margin: 5px 0;'>Effectuer des collectes et retraits</li>
                    <li style='margin: 5px 0;'>Consulter vos commissions et targets</li>
                    <li style='margin: 5px 0;'>Accéder aux rapports et statistiques</li>
                    <li style='margin: 5px 0;'>Communiquer avec vos clients</li>
                </ul>
            </div>
            
            <div class='info-section'>
                <h3 class='info-section-title'>Vos identifiants de connexion</h3>
                <div class='credential-row'>
                    <span class='credential-label'>Email :</span>
                    <span class='credential-value'>{email}</span>
                </div>
                <div class='credential-row'>
                    <span class='credential-label'>Nom d'utilisateur :</span>
                    <span class='credential-value'>{username}</span>
                </div>
                <div class='credential-row'>
                    <span class='credential-label'>Téléphone :</span>
                    <span class='credential-value'>{phoneNumber}</span>
                </div>
                <div class='credential-row'>
                    <span class='credential-label'>Mot de passe :</span>
                    <span class='credential-value'>{password}</span>
                </div>
            </div>
            
            <div class='warning-box'>
                <strong>Important :</strong> Pour des raisons de sécurité, vous devrez <strong>obligatoirement changer votre mot de passe</strong> lors de votre première connexion.
            </div>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='https://prosoc.kansaconsulting.com' class='button'>Se connecter maintenant</a>
            </div>
            
            <p style='margin-top: 30px; font-size: 14px; color: #666666;'>
                Vous pouvez vous connecter en utilisant votre <strong>email</strong>, votre <strong>nom d'utilisateur</strong> ou votre <strong>numéro de téléphone</strong>.
            </p>
        </div>
        
        <div class='footer'>
            <p class='footer-text'>Cet email a été envoyé automatiquement par Prosoc Platform.</p>
            <p class='footer-text'>© 2025 Prosoc. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GeneratePasswordResetEmailHtml(string resetToken)
        {
            return $@"
<!DOCTYPE html>
<html lang='fr'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Réinitialisation de mot de passe</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background-color: #f5f5f5;
            margin: 0;
            padding: 0;
            line-height: 1.6;
        }}
        .email-wrapper {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
        }}
        .header {{
            background-color: #232f3e;
            padding: 30px 40px;
            text-align: center;
        }}
        .header-logo {{
            color: #ffffff;
            font-size: 28px;
            font-weight: 600;
            letter-spacing: 1px;
            margin: 0;
        }}
        .header-logo .highlight {{
            color: #ff9900;
        }}
        .content {{
            padding: 40px;
            color: #232f3e;
        }}
        .title {{
            font-size: 24px;
            font-weight: 600;
            color: #232f3e;
            margin: 0 0 20px 0;
        }}
        .message {{
            font-size: 16px;
            color: #666666;
            margin: 0 0 30px 0;
            line-height: 1.7;
        }}
        .token-box {{
            background-color: #f8f9fa;
            border: 2px dashed #ff9900;
            padding: 20px;
            text-align: center;
            margin: 20px 0;
            border-radius: 8px;
        }}
        .token {{
            font-family: 'Courier New', monospace;
            font-size: 18px;
            font-weight: bold;
            color: #232f3e;
            background-color: #ffffff;
            padding: 10px;
            border-radius: 4px;
            border: 1px solid #e1e5e9;
            display: inline-block;
            margin: 10px 0;
        }}
        .warning-box {{
            background-color: #fff3cd;
            border: 1px solid #ffeaa7;
            color: #856404;
            padding: 15px;
            border-radius: 4px;
            margin: 20px 0;
            font-size: 14px;
        }}
        .button {{
            display: inline-block;
            background-color: #ff9900;
            color: #ffffff;
            text-decoration: none;
            padding: 12px 30px;
            border-radius: 6px;
            font-weight: 600;
            font-size: 16px;
            transition: background-color 0.3s ease;
        }}
        .button:hover {{
            background-color: #e68900;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px 40px;
            text-align: center;
            border-top: 1px solid #e1e5e9;
        }}
        .footer-text {{
            color: #666666;
            font-size: 12px;
            margin: 5px 0;
        }}
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <div class='header'>
            <h1 class='header-logo'>PROSOC<span class='highlight'>.</span></h1>
        </div>
        
        <div class='content'>
            <h2 class='title'>Réinitialisation de votre mot de passe</h2>
            
            <p class='message'>
                Vous avez demandé la réinitialisation de votre mot de passe. Utilisez le code ci-dessous pour procéder à la réinitialisation.
            </p>
            
            <div class='token-box'>
                <p style='margin: 0 0 10px 0; font-weight: 600;'>Votre code de réinitialisation :</p>
                <div class='token'>{resetToken}</div>
            </div>
            
            <div class='warning-box'>
                <strong>Important :</strong> Ce code est valable pendant 30 minutes. Ne partagez ce code avec personne.
            </div>
            
            <p style='margin-top: 30px; font-size: 14px; color: #666666;'>
                Si vous n'avez pas demandé cette réinitialisation, ignorez cet email. Votre mot de passe restera inchangé.
            </p>
        </div>
        
        <div class='footer'>
            <p class='footer-text'>Cet email a été envoyé automatiquement par Prosoc Platform.</p>
            <p class='footer-text'>© 2026 Prosoc. Tous droits réservés.</p>
        </div>
    </div>
</body>
    </html>";
        }

        private string GenerateAdhesionConfirmationEmailHtml(string affilieName, string codeAdhesion, string typeAdhesion)
        {
            return $@"
<!DOCTYPE html>
<html lang='fr'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Confirmation d'adhésion - Prosoc Platform</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
        }}
        .email-wrapper {{
            max-width: 600px;
            margin: 20px auto;
            background: white;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px 40px;
            text-align: center;
        }}
        .header-logo {{
            font-size: 28px;
            font-weight: bold;
            margin: 0;
        }}
        .highlight {{
            color: #ffd700;
        }}
        .content {{
            padding: 40px;
        }}
        .title {{
            color: #333;
            font-size: 24px;
            margin-bottom: 20px;
            text-align: center;
        }}
        .message {{
            color: #666;
            font-size: 16px;
            margin-bottom: 20px;
            line-height: 1.6;
        }}
        .info-box {{
            background: #f8f9fa;
            border-left: 4px solid #667eea;
            padding: 20px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .info-item {{
            margin: 10px 0;
            display: flex;
            justify-content: space-between;
        }}
        .info-label {{
            font-weight: 600;
            color: #333;
        }}
        .info-value {{
            color: #667eea;
            font-weight: bold;
        }}
        .success-box {{
            background: #d4edda;
            border: 1px solid #c3e6cb;
            border-radius: 4px;
            padding: 20px;
            margin: 20px 0;
            text-align: center;
        }}
        .success-title {{
            color: #155724;
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 10px;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px 40px;
            text-align: center;
            border-top: 1px solid #e1e5e9;
        }}
        .footer-text {{
            color: #666666;
            font-size: 12px;
            margin: 5px 0;
        }}
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <div class='header'>
            <h1 class='header-logo'>PROSOC<span class='highlight'>.</span></h1>
        </div>
        
        <div class='content'>
            <h2 class='title'>Félicitations ! Votre adhésion est confirmée</h2>
            
            <p class='message'>
                Cher(e) {affilieName},
            </p>
            
            <p class='message'>
                Nous sommes ravis de vous accueillir parmi nos affiliés. Votre adhésion a été enregistrée avec succès et vous pouvez désormais bénéficier de tous nos services.
            </p>
            
            <div class='info-box'>
                <div class='info-item'>
                    <span class='info-label'>Code d'adhésion :</span>
                    <span class='info-value'>{codeAdhesion}</span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Type d'adhésion :</span>
                    <span class='info-value'>{typeAdhesion}</span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Date d'adhésion :</span>
                    <span class='info-value'>{DateTime.Now:dd/MM/yyyy}</span>
                </div>
            </div>
            
            <div class='success-box'>
                <div class='success-title'>🎉 Bienvenue dans la famille Prosoc !</div>
                <p>Votre couverture est maintenant active et vous pouvez accéder à tous nos services de santé mutualiste.</p>
            </div>
            
            <p class='message'>
                Conservez précieusement votre code d'adhésion, il vous sera demandé pour toutes vos démarches.
            </p>
            
            <p class='message'>
                Pour toute question ou assistance, n'hésitez pas à contacter notre service client.
            </p>
        </div>
        
        <div class='footer'>
            <p class='footer-text'>Cet email a été envoyé automatiquement par Prosoc Platform.</p>
            <p class='footer-text'>© 2026 Prosoc. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}