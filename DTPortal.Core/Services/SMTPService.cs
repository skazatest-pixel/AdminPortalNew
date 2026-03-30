using DTPortal.Common;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Utilities;
using MailKit.Net.Smtp;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class SMTPService : ISMTPService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SMTPService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Smtp> GetSMTPSettingsAsync(int id)
        {
            var smtpindb = await _unitOfWork.SMTP.GetByIdAsync(id);

            // Get EncryptionKey

            //var EncKey = await _unitOfWork.EncDecKeys.GetByIdAsync(24);
            //if (null == EncKey)
            //{
            //}

            //string encryptionPassword = Encoding.UTF8.GetString(EncKey.Key1);
            string decryptedPasswd = PKIMethods.Instance.PKIDecryptSecureWireData(smtpindb.SmtpPwd);
            //var DecryptedPasswd = EncryptionLibrary.DecryptText(smtpindb.SmtpPwd,
            //    encryptionPassword, "appshield3.0");

            smtpindb.SmtpPwd = decryptedPasswd;

            return smtpindb;
        }

        public async Task<SMTPResponse> UpdateSMTPSettingsAsync(Smtp smtp)
        {
            // Get EncryptionKey
            //var EncKey = await _unitOfWork.EncDecKeys.GetByIdAsync(24);
            //if (null == EncKey)
            //{
            //}

            //string encryptionPassword = Encoding.UTF8.GetString(EncKey.Key1);

            var EncryptedPasswd = PKIMethods.Instance.PKICreateSecureWireData(smtp.SmtpPwd);

            smtp.SmtpPwd = EncryptedPasswd;

            try
            {
                _unitOfWork.SMTP.Update(smtp);
                await _unitOfWork.SaveAsync();

                return new SMTPResponse(smtp);
            }
            catch (Exception)
            {
                // Log the exception 
                return new SMTPResponse("An error occurred while updating the SMTP settings. Please contact the admin.");
            }
        }

        public async Task<SMTPResponse> TestSMTPConnectionAsync(Smtp smtp)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtp.SmtpHost, smtp.SmtpPort, smtp.RequiresSsl);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(smtp.SmtpUserName, smtp.SmtpPwd);

                    return new SMTPResponse(smtp);
                }
            }
            catch (Exception)
            {
                var smtperror = string.Format("An error occurred while testing the SMTP connection. Please contact the admin");
                // Log the exception 
                return new SMTPResponse(smtperror);
            }
        }
    }
}
