using DTPortal.Common;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services.Communication;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Utilities
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public EmailSender(ILogger<EmailSender> logger, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        private async Task<MimeMessage> CreateEmailMessage(Message message)
        {
            _logger.LogDebug("-->CreateEmailMessage");

            if (message == null)
            {
                _logger.LogError("Invalid Input Parameter");
                return null;
            }

            var emailMessage = new MimeMessage();

            try
            {
                var emailSettings = await _unitOfWork.SMTP.GetByIdAsync(1);
                if (emailSettings == null || string.IsNullOrWhiteSpace(emailSettings.FromEmailAddr))
                {
                    _logger.LogError("Failed to get email settings or sender address is missing");
                    return null;
                }

                emailMessage.From.Add(MailboxAddress.Parse(emailSettings.FromEmailAddr));
                emailMessage.To.AddRange(message.To);
                emailMessage.Subject = message.Subject ?? string.Empty;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = message.Content ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateEmailMessage Failed : {0}", ex.Message);
                return null;
            }

            _logger.LogDebug("<--CreateEmailMessage");
            return emailMessage;
        }

        private async Task<int> Send(MimeMessage mailMessage)
        {
            _logger.LogDebug("-->Send");
            int result = -1;

            if (mailMessage == null)
            {
                _logger.LogError("Invalid Input Parameter");
                return result;
            }

            using (var client = new SmtpClient())
            {
                try
                {
                    //var encKey = await _unitOfWork.EncDecKeys.GetByIdAsync(24);
                    //if (encKey?.Key1 == null)
                    //{
                    //    _logger.LogError("Failed to get keys");
                    //    return result;
                    //}

                    //string encryptionPassword = Encoding.UTF8.GetString(encKey.Key1);

                    var smtp = await _unitOfWork.SMTP.GetByIdAsync(1);
                    if (smtp == null)
                    {
                        _logger.LogError("Failed to get SMTP settings");
                        return result;
                    }

                    //string? decryptedPasswd = EncryptionLibrary.DecryptText(smtp.SmtpPwd ?? string.Empty, encryptionPassword, "appshield3.0");
                    string decryptedPasswd = PKIMethods.Instance.PKIDecryptSecureWireData(smtp.SmtpPwd);
                    if (decryptedPasswd == null)
                    {
                        _logger.LogError("Failed to decrypt text");
                        return result;
                    }

                    var mailConfig = new EmailConfiguration
                    {
                        SmtpServer = smtp.SmtpHost ?? string.Empty,
                        Port = smtp.SmtpPort,
                        UserName = smtp.SmtpUserName ?? string.Empty,
                        Password = decryptedPasswd
                    };

                    SecureSocketOptions socketOptions;
                    switch (mailConfig.Port)
                    {
                        case 465:
                            socketOptions = SecureSocketOptions.SslOnConnect;
                            break;
                        case 587:
                            socketOptions = SecureSocketOptions.StartTls;
                            break;
                        case 25:
                            socketOptions = SecureSocketOptions.None;
                            break;
                        default:
                            socketOptions = smtp.RequiresSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None;
                            break;
                    }

                    await client.ConnectAsync(mailConfig.SmtpServer, mailConfig.Port, socketOptions);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    if (mailConfig.Port != 25 && !string.IsNullOrWhiteSpace(mailConfig.UserName))
                    {
                        await client.AuthenticateAsync(mailConfig.UserName, mailConfig.Password);
                    }

                    await client.SendAsync(mailMessage);
                    result = 0;
                }
                catch (Exception error)
                {
                    _logger.LogError("Failed to send Mail: {0}", error.Message);
                }
                finally
                {
                    if (client.IsConnected)
                    {
                        await client.DisconnectAsync(true);
                    }
                }
            }

            _logger.LogDebug("<--Send");
            return result;
        }

        public bool TestSmtpConnection(EmailConfiguration emailConfiguration)
        {
            _logger.LogDebug("-->TestSmtpConnection");
            bool result = false;

            if (emailConfiguration == null)
            {
                _logger.LogError("Invalid Input Parameter");
                return result;
            }

            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(emailConfiguration.SmtpServer, emailConfiguration.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(emailConfiguration.UserName, emailConfiguration.Password);
                    result = true;
                }
                catch (Exception error)
                {
                    _logger.LogError("Failed to TestSmtpConnection: {0}", error.Message);
                }
                finally
                {
                    if (client.IsConnected)
                    {
                        client.Disconnect(true);
                    }
                }
            }

            _logger.LogDebug("<--TestSmtpConnection");
            return result;
        }

        public async Task<int> SendEmail(Message message)
        {
            _logger.LogDebug("-->SendEmail");
            int result = -1;

            if (message == null)
            {
                _logger.LogError("Invalid Input Parameter");
                return result;
            }

            var emailMessage = await CreateEmailMessage(message);
            if (emailMessage == null)
            {
                _logger.LogError("CreateEmailMessage Failed");
                return result;
            }

            result = await Send(emailMessage);
            if (0 != result)
            {
                _logger.LogError("Send Email Failed");
                return result;
            }

            _logger.LogDebug("<--SendEmail");
            return 0;
        }
    }
}