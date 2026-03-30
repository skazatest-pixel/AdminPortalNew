using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Utilities
{
    public class TokenManager : ITokenManager
    {
        // Initialize logger.
        private readonly ILogger<TokenManager> _logger;
        private readonly IPKIServiceClient _pkiServiceClient;
        private readonly IPKILibrary _pkiLibrary;
        private readonly IUnitOfWork _unitOfWork;
        private readonly SSOConfig ssoConfig;
        private readonly idp_configuration idpConfiguration;
        private readonly IGlobalConfiguration _globalConfiguration;
        public JWTConfig _config { get; set; }
        public TokenManager(JWTConfig config, ILogger<TokenManager> logger,
            IPKIServiceClient pkiServiceClient, IPKILibrary pkiLibrary,
            IUnitOfWork unitOfWork, IGlobalConfiguration globalConfiguration)
        {
            _config = config;
            _pkiServiceClient = pkiServiceClient;
            _logger = logger;
            _pkiLibrary = pkiLibrary;
            _unitOfWork = unitOfWork;
            _globalConfiguration = globalConfiguration;

            // Get SSO Configuration
            ssoConfig = _globalConfiguration.GetSSOConfiguration();
            if (null == ssoConfig)
            {
                _logger.LogError("Get SSO Configuration failed");
                throw new NullReferenceException();
            }

            idpConfiguration = _globalConfiguration.GetIDPConfiguration();
            if (null == idpConfiguration)
            {
                _logger.LogError("Get IDP Configuration failed");
                throw new NullReferenceException();
            }
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Generate a JWToken
        public string GenerateSecKeyJWT(string userName, string nonce)
        {
            _logger.LogInformation("-->GenerateToken");

            // Generate a security key from secret key
            var securityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_config.SecretKey));

            // Create Token Handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // Create Token Parameters
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Create Claims
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userName),
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(JwtRegisteredClaimNames.Nonce, nonce),
                }),
                Expires = DateTime.UtcNow.AddMinutes(_config.ExpiryInMins),
                Issuer = _config.Issuer,
                Audience = _config.Audience,
                SigningCredentials = new SigningCredentials(securityKey,
                _config.Algorithm)
            };

            // Create Token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            _logger.LogInformation("<-->GenerateToken");
            // Return Token as a string
            return tokenHandler.WriteToken(token);
        }

        // Validate JWToken
        public bool ValidateSecKeyJWT(string token)
        {
            _logger.LogInformation("-->ValidateToken");

            // Generate a security key from secret key
            var securityKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_config.SecretKey));

            // Create Token Handler
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                // Validate Token
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = _config.Issuer,
                    ValidAudience = _config.Audience,
                    IssuerSigningKey = securityKey
                }, out SecurityToken validatedToken);
            }
            catch (SecurityTokenValidationException error)
            {
                _logger.LogError("ValidateToken Failed: {0}",
                    error.Message);
                return false;
            }

            _logger.LogInformation("<--ValidateToken");
            return true;
        }

        public bool VerifyJWTToken(string jwtToken,
            string issuer,
            string audience,
            string certificate,
            bool validateIssuer = true,
            bool validateAud = true,
            bool expiry = true
            )
        {
            _logger.LogDebug("-->VerifyJWTToken");
            // Local variable declaration
            X509Certificate2 cert;

            // Validate input parameters
            if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(certificate))
            {
                _logger.LogError("Invalid input parameter");
                return false;
            }

            _logger.LogDebug("Issuer: {0}, Audience: {1}",
                issuer, audience);

            try
            {
                cert = new X509Certificate2(Convert.FromBase64String
                    (@certificate));

                RsaSecurityKey rsaSecurityKey = new RsaSecurityKey(
                    cert.GetRSAPublicKey());

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = validateIssuer,
                    ValidateAudience = validateAud,
                    ValidateLifetime = expiry,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = rsaSecurityKey,
                    CryptoProviderFactory = new CryptoProviderFactory()
                    {
                        CacheSignatureProviders = false
                    }
                };

                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(jwtToken, validationParameters,
                    out var validatedSecurityToken);
            }
            catch (Exception error)
            {
                _logger.LogError("VerifyJWTToken Failed: {0}", error.Message);
                return false;
            }

            _logger.LogDebug("<--VerifyJWTToken");
            return true;
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        public bool VerifyHSJWTToken(string jwtToken,
    string issuer,
    string audience,
    string secretKey,
    bool validateIssuer = true,
    bool validateAud = true,
    bool expiry = true
    )
        {
            _logger.LogDebug("-->VerifyHSJWTToken");

            if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(secretKey))
            {
                _logger.LogError("Invalid input parameter");
                return false;
            }
            _logger.LogDebug("Issuer: {0}, Audience: {1}",
                issuer, audience);
            try
            {
                var securityKey = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(secretKey));
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = securityKey
                };
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(jwtToken, validationParameters,
                    out var validatedSecurityToken);
            }
            catch (Exception error)
            {
                _logger.LogError("VerifyHSJWTToken Failed: {0}", error.Message);
                return false;
            }
            _logger.LogDebug("<--VerifyHSJWTToken");
            return true;
        }

        public string GetJSONWebTokenClaims(string token)
        {
            _logger.LogDebug("-->GetJSONWebTokenClaims");

            // Local variable declaration
            string result = null;

            // Validate input parameters
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Invalid input parameter");
                return result;
            }

            // Create Token Handler
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var str = tokenHandler.ReadJwtToken(token);
                if (null == str)
                {
                    _logger.LogError("Failed to read JWT Token");
                    return result;
                }

                result = str.Claims.FirstOrDefault(c => c.Type ==
                JwtRegisteredClaimNames.Sub).Value;
            }
            catch (Exception error)
            {
                _logger.LogError("GetJSONWebTokenClaims failed:{0}", error.Message);
                return null;
            }

            _logger.LogDebug("-->GetJSONWebTokenClaims");
            return result;
        }

        //public async Task<bool> ValidateDeviceRegistrationToken(
        //    string token)
        //{
        //    _logger.LogDebug("-->ValidateDeviceRegistrationToken");
        //    // Local variable declaration
        //    X509Certificate2 cert;

        //    // Validate input parameters
        //    if (string.IsNullOrEmpty(token))
        //    {
        //        _logger.LogError("Invalid input parameter");
        //        return false;
        //    }

        //    try
        //    {
        //        if (false == ssoConfig.sso_config.remoteSigning)
        //        {
        //            // For Testing Only
        //            cert = new X509Certificate2(@"publickey.crt");
        //        }
        //        else
        //        {
        //            // Get Active IDP certificate
        //            var certificate = await _unitOfWork.Certificates.
        //                GetActiveCertificateAsync();
        //            if (null == certificate)
        //            {
        //                _logger.LogError("GetActiveCertificate Failed");
        //                return false;
        //            }

        //            cert = new X509Certificate2(
        //                Convert.FromBase64String(@certificate.Data));
        //        }

        //        RsaSecurityKey rsaSecurityKey = new RsaSecurityKey(
        //            cert.GetRSAPublicKey());

        //        var openidconnect = JsonConvert.DeserializeObject<OpenIdConnect>(
        //            idpConfiguration.openidconnect.ToString());
        //        var validationParameters = new TokenValidationParameters
        //        {
        //            ValidateIssuer = false,
        //            ValidateAudience = false,
        //            ValidateLifetime = true,
        //            ValidateIssuerSigningKey = true,
        //            ValidIssuer = DTInternalConstants.DTPortalClientId,
        //            ValidAudience = openidconnect.issuer,
        //            IssuerSigningKey = rsaSecurityKey,
        //            CryptoProviderFactory = new CryptoProviderFactory()
        //            {
        //                CacheSignatureProviders = false
        //            }
        //        };

        //        var handler = new JwtSecurityTokenHandler();
        //        handler.ValidateToken(token, validationParameters,
        //            out var validatedSecurityToken);
        //    }
        //    catch (Exception error)
        //    {
        //        _logger.LogError("ValidateDeviceRegistrationToken Failed: {0}",
        //            error.Message);
        //        return false;
        //    }

        //    _logger.LogDebug("<--ValidateDeviceRegistrationToken");
        //    return true;
        //}

    }
}
