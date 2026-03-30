using DTPortal.Core.Domain.Services.Communication;
using System.Threading.Tasks;

namespace DTPortal.Core.Utilities
{
    public interface IEmailSender
    {
        Task<int> SendEmail(Message message);
        public bool TestSmtpConnection(EmailConfiguration emailConfiguration);
    }
}
