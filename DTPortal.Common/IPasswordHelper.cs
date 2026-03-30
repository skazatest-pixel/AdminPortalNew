namespace EnterpriseGatewayPortal.Core.Utilities
{
	public interface IPasswordHelper
	{
		string HashPassword(string password);
		bool VerifyPassword(string storedHash, string enteredPassword);
	}
}