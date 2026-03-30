using Microsoft.AspNetCore.Identity;
using System;

namespace EnterpriseGatewayPortal.Core.Utilities
{
	public class PasswordHelper : IPasswordHelper
	{
		private readonly IPasswordHasher<object> _passwordHasher;

		public PasswordHelper(IPasswordHasher<object> passwordHasher)
		{
			_passwordHasher = passwordHasher;
		}

		public string HashPassword(string password)
		{
			if (string.IsNullOrWhiteSpace(password))
				throw new ArgumentException("Password cannot be null or empty.", nameof(password));

			return _passwordHasher.HashPassword(null, password);
		}

		public bool VerifyPassword(string storedHash, string enteredPassword)
		{
			if (string.IsNullOrWhiteSpace(storedHash))
				throw new ArgumentException("Stored hash cannot be null or empty.", nameof(storedHash));

			if (string.IsNullOrWhiteSpace(enteredPassword))
				return false;

			var result = _passwordHasher.VerifyHashedPassword(
				null,
				storedHash,
				enteredPassword);

			return result == PasswordVerificationResult.Success ||
				   result == PasswordVerificationResult.SuccessRehashNeeded;
		}
	}

}
