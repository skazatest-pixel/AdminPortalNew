using Amazon.Runtime.Internal;
using DTPortal.Common;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Lookups;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using EnterpriseGatewayPortal.Core.Utilities;
using Fido2NetLib.Development;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Core.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly ILocalJWTManager _tokenManager;
        private readonly ILogger<UserManagementService> _logger;
        private IConfiguration _configuration;
        private readonly IConfigurationService _configurationService;
        private readonly IGlobalConfiguration _globalConfiguration;
        private readonly IMCValidationService _mcValidationService;
        private readonly IPasswordHelper _passwordHelper;

        public UserManagementService(
            IUnitOfWork unitOfWork,
            IEmailSender emailSender,
            ILocalJWTManager tokenManager,
            ILogger<UserManagementService> logger,
            IGlobalConfiguration globalConfiguration,
            IConfigurationService configurationService,
            IMCValidationService mcValidationService,
            IConfiguration configuration,
            IPasswordHelper passwordHelper
            )
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _tokenManager = tokenManager;
            _logger = logger;
            _configurationService = configurationService;
            _globalConfiguration = globalConfiguration;
            _mcValidationService = mcValidationService;
            _configuration = configuration;
            _passwordHelper = passwordHelper;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ-abcdefghijklmnopqrstuvwxyz@#$0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static byte[] FromBase64Url(string base64Url)
        {
            string padded = base64Url.Length % 4 == 0
                ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
            string base64 = padded.Replace("_", "/")
                                  .Replace("-", "+");
            return Convert.FromBase64String(base64);
        }

        public async Task<IEnumerable<RoleLookupItem>> GetRoleLookupsAsync()
        {
            return await _unitOfWork.Roles.GetRoleLookupItemsAsync();
        }
        public static string GenerateSecurePassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";

            StringBuilder result = new StringBuilder();
            byte[] buffer = new byte[sizeof(uint)];

            using (var rng = RandomNumberGenerator.Create())
            {
                while (result.Length < length)
                {
                    rng.GetBytes(buffer);
                    uint num = BitConverter.ToUInt32(buffer, 0);

                    result.Append(validChars[(int)(num % (uint)validChars.Length)]);
                }
            }

            return result.ToString();
        }
        public async Task<UserResponse> RevertUser(UserTable user, int level)
        {
            var userAuthDatainDb = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(
                user.Uuid, "PASSWORD");
            if (null == userAuthDatainDb)
            {
                _logger.LogError("GetUserAuthDataAsync Failed: not found: {0}", user.Id);
                return new UserResponse("An error occurred while creating user, Please contact admin");
            }

            if (level == 2)
            {
                var userLoginDetailInDb = await _unitOfWork.UserLoginDetail.GetUserLoginDetailAsync(user.Id.ToString());
                if (null == userLoginDetailInDb)
                {
                    _logger.LogError("GetUserAuthDataAsync Failed: not found: {0}", user.Id);
                    return new UserResponse("An error occurred while creating user, Please contact admin");
                }

                try
                {
                    _unitOfWork.UserLoginDetail.Remove(userLoginDetailInDb);
                    await _unitOfWork.SaveAsync();
                }
                catch (Exception error)
                {
                    _logger.LogError("Autdata remove Failed:{0}", error.Message);
                    return new UserResponse("An error occurred while creating user, Please contact admin");
                }
            }
            if (level <= 2)
            {
                try
                {
                    _unitOfWork.UserAuthData.Remove(userAuthDatainDb);
                    await _unitOfWork.SaveAsync();

                    _unitOfWork.Users.Remove(user);
                    await _unitOfWork.SaveAsync();
                }
                catch (Exception error)
                {
                    _logger.LogError("UserAuthData remove Failed: not found: {0}", error.Message);
                    return new UserResponse("An error occurred while creating user, Please contact admin");
                }
            }

            _logger.LogInformation("USER DATA REVERTED SUCCESS:{0}", user.Id);
            return new UserResponse("An error occurred while creating user, Please contact admin");
        }

        public async Task<UserResponse> AddUserAsync(UserTable user, string password,
            bool makerCheckerFlag = false)
        {
            if (null == user.Uuid)
            {
                return new UserResponse("Username not received");
            }

            if (await _unitOfWork.Users.IsUserExistsWithEmail(user))
            {
                return new UserResponse("User emailid already exists");
            }

            if (await _unitOfWork.Users.IsUserExistsWitMobile(user))
            {
                return new UserResponse("User phone number already exists");
            }

            if (string.IsNullOrEmpty(password))
            {
                password = GenerateSecurePassword(10);
            }

            var isEnabled = await _mcValidationService.IsMCEnabled(
                ActivityIdConstants.UserActivityId);

            if (false == makerCheckerFlag && true == isEnabled)
            {
                var userRequest = new UserRequest();

                userRequest.user = user;
                userRequest.password = password;

                var response = await _mcValidationService.IsCheckerApprovalRequired(
                    ActivityIdConstants.UserActivityId,
                    "CREATE", user.CreatedBy,
                    JsonConvert.SerializeObject(userRequest));
                if (!response.Success)
                {
                    _logger.LogError("CheckApprovalRequired Failed");
                    return new UserResponse(response.Message);
                }
                if (response.Result)
                {
                    return new UserResponse(user, "Your request sent for approval");
                }
            }

            try
            {
                user.Uuid = Guid.NewGuid().ToString();

                var hashedPassword = _passwordHelper.HashPassword(password);

                UserAuthDatum userAuthData = new UserAuthDatum
                {
                    UserId = user.Uuid,
                    AuthScheme = AuthNSchemeConstants.PASSWORD,
                    AuthData = hashedPassword,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    FailedLoginAttempts = 0,
                    CreatedBy = user.CreatedBy,
                    UpdatedBy = user.UpdatedBy,
                    Status = "ACTIVE"
                };

                user.Status = "NEW";
                user.CreatedDate = DateTime.Now;
                user.ModifiedDate = DateTime.Now;
                user.LastLoginTime = DateTime.UtcNow;
                user.CurrentLoginTime = DateTime.Now;
                user.Hash = "not implemented";

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception error)
            {
                _logger.LogError("AddUserAsync Failed: {0}", error.Message);
                return new UserResponse("An error occurred while creating the user." +
                    " Please contact the admin.");
            }

            var userId = await _unitOfWork.Users.GetUserbyUuidAsync(user.Uuid);
            if (null == userId)
            {
                return new UserResponse("An error occurred while creating the user." +
                    " Please contact the admin.");
            }

            var userPasswordDetail = new UserLoginDetail
            {
                UserId = userId.Id.ToString(),
                IsReversibleEncryption = false,
                WrongPinCount = 0,
                WrongCodeCount = 0,
                DeniedCount = 0,
                IsScrambled = false,
                PriAuthSchId = 64
            };

            try
            {
                await _unitOfWork.UserLoginDetail.AddAsync(userPasswordDetail);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception)
            {
                return await RevertUser(user, 1);
            }

            var mailTemplate = await _unitOfWork.SMTP.GetByIdAsync(1);
            if (null == mailTemplate)
            {
                _logger.LogError("smtp details not found");
                return new UserResponse("An error occurred while creating the user." +
                " Please contact the admin.");
            }

            var portalLink = _configuration.GetValue<string>("PortalLink");
            if (null == portalLink)
            {
                portalLink = string.Empty;
            }
            var mailBody = " <p>Hi " + user.FullName + ",</p>" +
                                    "<p>" + mailTemplate.Template + "</p>" +
                                    "<p>Login ID: " + user.MailId + " (or) " + user.MobileNo + "</p>" +
                                    "<p>Password: " + password + "</p>" +
                                    "<br/>" +
                                    "<p>Portal login URL: <a href='" + portalLink + "'>" + portalLink + "</a></p>";

            var message = new Message(new string[]
            {
            user.MailId
            },
            mailTemplate.MailSubject,
            mailBody
            );

            try
            {
                await _emailSender.SendEmail(message);
                return new UserResponse(user, "User created successfully");
            }
            catch
            {
                var userresp = await RevertUser(user, 2);
                return new UserResponse(user, "Unable to send email");
            }

        }

        public async Task<PaginatedList<UserTable>> ListUsersAsync(int offset, int count)
        {
            _logger.LogInformation("ListUsersAsync");
            return await _unitOfWork.Users.GetAllUsersWithRolesAsync(offset, count);
        }

        public async Task<List<UserTable>> ListUsersAsync()
        {
            _logger.LogInformation("ListUsersAsync");
            return await _unitOfWork.Users.GetAllUsersWithRolesAsync();
        }

        public async Task<UserTable> GetUserAsync(int id)
        {
            return await _unitOfWork.Users.GetUserByIdWithRoleAsync(id);
        }

        public async Task<string> GetUserStatusAsync(int id)
        {
            var userInDb = await _unitOfWork.Users.GetByIdAsync(id);
            if (null == userInDb)
            {
                return null;
            }

            return userInDb.Status;
        }

        public async Task<UserTable> GetUserAsyncByName(string name)
        {
            return await _unitOfWork.Users.GetUserbyNameAsync(name);
        }

        public async Task<bool> GetUserFido2StatusAsync(string id)
        {
            var userAuthData = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(id, "FIDO2");
            if (null == userAuthData)
            {
                return false;
            }
            return true;
        }

        public async Task<UserTable> GetUserAsyncByUid(string uid)
        {
            return await _unitOfWork.Users.GetUserbyUuidAsync(uid);
        }

        public async Task<UserTable> GetUserAsyncByEmail(string email)
        {
            return await _unitOfWork.Users.GetUserbyEmailAsync(email);
        }

        public async Task<UserTable> GetUserAsyncByPhone(string phone)
        {
            return await _unitOfWork.Users.GetUserbyPhonenoAsync(phone);
        }

        public async Task<List<string>> SearchUserAsyncByEmail(string email)
        {
            return await _unitOfWork.Users.SearchUserbyEmailAsync(email);
        }

        public async Task<List<string>> SearchUserAsyncByPhone(string phone)
        {
            return await _unitOfWork.Users.SearchUserbyPhoneAsync(phone);
        }

        public async Task<UserResponse> UpdateUserAsync(UserTable user,
            bool makerCheckerFlag = false)
        {
            var userInDb = await _unitOfWork.Users.GetByIdAsync(user.Id);
            if (null == userInDb)
            {
                _logger.LogError("user not found");
                return new UserResponse("user not found");
            }

            if (user.AuthScheme.Equals("FIDO2"))
            {
                var isExists = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(user.Uuid, "FIDO2");
                if (null == isExists)
                {
                    return new UserResponse("User FIDO2 device not registered");
                }
            }

            var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.UserActivityId);

            if (false == makerCheckerFlag && true == isEnabled)
            {
                _logger.LogError("in makerchecker");
                var istrue = user.Role.UserTables.Remove(user);
                _logger.LogError("user.Role.UserTables.Remove removed");

                var response = await _mcValidationService.IsCheckerApprovalRequired(
                    ActivityIdConstants.UserActivityId,
                    "UPDATE", user.UpdatedBy,
                    JsonConvert.SerializeObject(user));
                if (!response.Success)
                {
                    _logger.LogError("CheckApprovalRequired Failed");
                    return new UserResponse(response.Message);
                }
                if (response.Result)
                {
                    return new UserResponse(user, "Your request sent for approval");
                }
            }

            if (userInDb.MailId != user.MailId)
            {
                if (await _unitOfWork.Users.IsUserExistsWithEmail(user))
                {
                    return new UserResponse("User emailid already exists");
                }
            }
            if (userInDb.MobileNo != user.MobileNo)
            {
                if (await _unitOfWork.Users.IsUserExistsWitMobile(user))
                {
                    return new UserResponse("User phone number already exists");
                }
            }

            userInDb.UpdatedBy = user.UpdatedBy;
            userInDb.ModifiedDate = DateTime.Now;
            userInDb.Gender = user.Gender;
            userInDb.MailId = user.MailId;
            userInDb.MobileNo = user.MobileNo;
            userInDb.Dob = user.Dob;
            userInDb.RoleId = user.RoleId;
            userInDb.FullName = user.FullName;
            userInDb.AuthScheme = user.AuthScheme;

            try
            {
                _unitOfWork.Users.Update(userInDb);
                await _unitOfWork.SaveAsync();
                return new UserResponse(user, "User updated successfully");
            }
            catch
            {
                _logger.LogError("An error occurred while updating the user. Please contact the admin.");
                return new UserResponse("An error occurred while updating the user. Please contact the admin.");
            }
        }

        public async Task<UserResponse> VerifyToken(string jwtToken, string issuer, string audience,
            bool validateIssuer = true, bool validateAudience = true, bool validateExpiry = true)
        {
            var isTrue = true;
            if (false == isTrue)
            {
                return new UserResponse("Token validation failed. Please contact the admin.");
            }

            var claims = _tokenManager.GetJSONWebTokenClaims(jwtToken);
            if (null == claims)
            {
                return new UserResponse("An error occurred while updating the user. Please contact the admin.");
            }

            var userInDb = await _unitOfWork.Users.GetUserbyUuidAsync(claims);
            if (null == userInDb)
            {
                return new UserResponse("An error occurred while updating the user. Please contact the admin.");
            }

            if (userInDb.Status.Equals("NEW"))
            {

            }
            else
            {
                return new UserResponse("User status is not NEW. Please contact the admin.");
            }

            return new UserResponse(userInDb);
        }

        public async Task<UserResponse> VerifyDeviceRegistrationToken(string jwtToken)
        {
            var isTrue = _tokenManager.ValidateSecKeyJWT(jwtToken);
            if (false == isTrue)
            {
                return new UserResponse("Token validation failed. Please contact the admin.");
            }

            var claims = _tokenManager.GetJSONWebTokenClaims(jwtToken);
            if (null == claims)
            {
                return new UserResponse("An error occurred while updating the user. Please contact the admin.");
            }

            var userInDb = await _unitOfWork.Users.GetUserbyUuidAsync(claims);
            if (null == userInDb)
            {
                return new UserResponse("An error occurred while updating the user. Please contact the admin.");
            }

            return new UserResponse(userInDb);
        }

        public async Task<UserResponse> SaveUserAsync(UserTable user, string authData)
        {
            var userInDb = await _unitOfWork.Users.GetUserbyUuidAsync(user.Uuid);
            if (null == userInDb)
            {
                return new UserResponse("An error occurred while updating the user. Please contact the admin.");
            }

            var userAuthDataInDb = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(
                user.Uuid, "FIDO2");
            if (null != userAuthDataInDb)
            {
                return new UserResponse("User is already provisioned with Fido2 device");
            }

            UserAuthDatum userAuthData = new UserAuthDatum
            {
                UserId = user.Uuid,
                AuthScheme = "FIDO2",
                AuthData = authData,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                CreatedBy = "sysadmin",
                UpdatedBy = "sysadmin",
                FailedLoginAttempts = 0,
                Status = "ACTIVE",
                Istemporary = false,
            };

            try
            {
                await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                return new UserResponse("Internal server error");
            }

            userInDb.Status = "ACTIVE";

            try
            {
                _unitOfWork.Users.Update(userInDb);
                await _unitOfWork.SaveAsync();

                return new UserResponse(userInDb);
            }
            catch
            {
                return new UserResponse("An error occurred while updating the user. Please contact the admin.");
            }
        }

        public async Task<UserResponse> SendTempDeviceLinkAsync(
            string user_id,
            string authData,
            DateTime? expiry
            )
        {
            var userInDb = await _unitOfWork.Users.GetUserbyUuidAsync(user_id);
            if (null == userInDb)
            {
                return new UserResponse("An error occurred while updating the user. Please contact the admin.");
            }

            var userAuthDataInDb = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(
            userInDb.Uuid,
            "FIDO2");
            if (null == userAuthDataInDb)
            {
                UserAuthDatum userAuthData = new UserAuthDatum();

                if (expiry.HasValue)
                {
                    userAuthData.UserId = userInDb.Uuid;
                    userAuthData.AuthScheme = "FIDO2";
                    userAuthData.AuthData = "NA";
                    userAuthData.CreatedDate = DateTime.Now;
                    userAuthData.ModifiedDate = DateTime.Now;
                    userAuthData.CreatedBy = "sysadmin";
                    userAuthData.UpdatedBy = "sysadmin";
                    userAuthData.FailedLoginAttempts = 0;
                    userAuthData.Status = "HOLD";
                    userAuthData.Istemporary = true;
                    userAuthData.Expiry = expiry;

                    try
                    {
                        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                        await _unitOfWork.SaveAsync();

                    }
                    catch
                    {
                        return new UserResponse("Internal server error");
                    }
                }
                else
                {
                    userAuthData.UserId = userInDb.Uuid;
                    userAuthData.AuthScheme = "FIDO2";
                    userAuthData.AuthData = "NA";
                    userAuthData.CreatedDate = DateTime.Now;
                    userAuthData.ModifiedDate = DateTime.Now;
                    userAuthData.CreatedBy = "sysadmin";
                    userAuthData.UpdatedBy = "sysadmin";
                    userAuthData.FailedLoginAttempts = 0;
                    userAuthData.Status = "HOLD";
                    userAuthData.Istemporary = false;
                    userAuthData.Expiry = expiry;

                    try
                    {
                        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                        await _unitOfWork.SaveAsync();
                    }
                    catch
                    {
                        return new UserResponse("Internal server error");
                    }
                }
            }
            else
            {
                if (userAuthDataInDb.Expiry.HasValue && expiry.HasValue)
                {
                    return new UserResponse("User already has a temporary fido2 device");
                }

                UserAuthDatum userAuthData = new UserAuthDatum();

                if (expiry.HasValue)
                {
                    userAuthData.UserId = userInDb.Uuid;
                    userAuthData.AuthScheme = "FIDO2";
                    userAuthData.AuthData = "NA";
                    userAuthData.CreatedDate = DateTime.Now;
                    userAuthData.ModifiedDate = DateTime.Now;
                    userAuthData.CreatedBy = "sysadmin";
                    userAuthData.UpdatedBy = "sysadmin";
                    userAuthData.FailedLoginAttempts = 0;
                    userAuthData.Status = "HOLD";
                    userAuthData.Istemporary = true;
                    userAuthData.Expiry = expiry;

                    try
                    {
                        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                        await _unitOfWork.SaveAsync();
                    }
                    catch
                    {
                        return new UserResponse("Internal server error");
                    }
                }
                else
                {
                    userAuthDataInDb.Status = "EXPIRED";

                    try
                    {
                        _unitOfWork.UserAuthData.Update(userAuthDataInDb);
                        await _unitOfWork.SaveAsync();
                    }
                    catch
                    {
                        return new UserResponse("Internal server error");
                    }

                    userAuthData.UserId = userInDb.Uuid;
                    userAuthData.AuthScheme = "FIDO2";
                    userAuthData.AuthData = "NA";
                    userAuthData.CreatedDate = DateTime.Now;
                    userAuthData.ModifiedDate = DateTime.Now;
                    userAuthData.CreatedBy = "sysadmin";
                    userAuthData.UpdatedBy = "sysadmin";
                    userAuthData.FailedLoginAttempts = 0;
                    userAuthData.Status = "HOLD";
                    userAuthData.Istemporary = false;
                    userAuthData.Expiry = expiry;

                    try
                    {
                        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                        await _unitOfWork.SaveAsync();

                    }
                    catch
                    {
                        return new UserResponse("Internal server error");
                    }
                }
            }

            var idpConfiguration = _globalConfiguration.GetIDPConfiguration();
            if (null == idpConfiguration)
            {
                _logger.LogError("Get IDP Configuration failed");
                throw new NullReferenceException();
            }

            var openidconnect = JsonConvert.DeserializeObject<OpenIdConnect>
                (idpConfiguration.openidconnect.ToString());

            var tokenPayload = new JWTokenDTO();
            tokenPayload.exp = (Int32)(DateTime.UtcNow.AddMinutes(60).Subtract
                (new DateTime(1970, 1, 1))).TotalSeconds;
            tokenPayload.sub = userInDb.Uuid;
            tokenPayload.auth_time = (Int32)(DateTime.UtcNow.Subtract(new DateTime
                (1970, 1, 1))).TotalSeconds;
            tokenPayload.iat = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)))
                .TotalSeconds;
            tokenPayload.at_hash = string.Empty;
            tokenPayload.iss = DTInternalConstants.DTPortalClientId;
            tokenPayload.aud = openidconnect.issuer;
            tokenPayload.nonce = AuthNSchemeConstants.FIDO2;

            var octets = Encoding.ASCII.GetBytes(userInDb.Uuid);
            var hash = SHA256.Create().ComputeHash(octets);
            tokenPayload.at_hash = WebEncoders.Base64UrlEncode(hash[..(hash.Length / 2)]);

            var token = _tokenManager.GenerateSecKeyJWT(userInDb.Uuid, "fido2");
            if (string.IsNullOrEmpty(token))
            {
                return new UserResponse("An error occurred while creating the user. Please contact the admin.");
            }

            var issuer = _configuration.GetValue<string>("fido2:origin");
            if (null == issuer)
            {
                _logger.LogError("Registration url not found in settings file");
                return new UserResponse("An error occurred while creating the user. Please contact the admin.");
            }

            var basePath = _configuration.GetValue<string>("BasePath");
            if (null == basePath)
            {
                _logger.LogError("Path not found in settings file");
                return new UserResponse("An error occurred while creating the user. Please contact the admin.");
            }

            var url = string.Format("{0}{1}/Registration?Request_code={2}" +
                "&Request_for={3}&Request_type=1", issuer, basePath, token, user_id);

            var mailTemplate = await _unitOfWork.SMTP.GetByIdAsync(1);
            if (null == mailTemplate)
            {
                _logger.LogError("smtp details not found");
                return new UserResponse("An error occurred while creating the user. Please contact the admin.");
            }

            var mailBody = "<p>" + mailTemplate.Template + ":</p>" +
               "<p>" + url + "</p>" +
               "<p><i>This link is active for 30 minutes.</i></p>";

            var message = new Message(new string[]
            {
            userInDb.MailId
            },
            mailTemplate.MailSubject,
            mailBody
            );

            try
            {
                await _emailSender.SendEmail(message);
                return new UserResponse(userInDb);
            }
            catch
            {
                return new UserResponse(userInDb);
            }
        }

        public async Task<UserResponse> IntiateFido2DeviceAsync(
            string user_id,
            string authData,
            DateTime? expiry
            )
        {
            var userInDb = await _unitOfWork.Users.GetUserbyUuidAsync(user_id);
            if (null == userInDb)
            {
                return new UserResponse("An error occurred while updating the user. Please contact the admin.");
            }

            var userAuthDataInDb = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(
            userInDb.Uuid,
            "FIDO2");
            if (null == userAuthDataInDb)
            {
                UserAuthDatum userAuthData = new UserAuthDatum();

                if (expiry.HasValue)
                {
                    userAuthData.UserId = userInDb.Uuid;
                    userAuthData.AuthScheme = "FIDO2";
                    userAuthData.AuthData = "NA";
                    userAuthData.CreatedDate = DateTime.Now;
                    userAuthData.ModifiedDate = DateTime.Now;
                    userAuthData.CreatedBy = "sysadmin";
                    userAuthData.UpdatedBy = "sysadmin";
                    userAuthData.FailedLoginAttempts = 0;
                    userAuthData.Status = "HOLD";
                    userAuthData.Istemporary = true;
                    userAuthData.Expiry = expiry;

                    try
                    {
                        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                        await _unitOfWork.SaveAsync();

                    }
                    catch (Exception error)
                    {
                        _logger.LogError("Add User Auth data Failed: {0}", error.Message);
                        return new UserResponse("Internal server error");
                    }
                }
                else
                {
                    userAuthData.UserId = userInDb.Uuid;
                    userAuthData.AuthScheme = "FIDO2";
                    userAuthData.AuthData = "NA";
                    userAuthData.CreatedDate = DateTime.Now;
                    userAuthData.ModifiedDate = DateTime.Now;
                    userAuthData.CreatedBy = "sysadmin";
                    userAuthData.UpdatedBy = "sysadmin";
                    userAuthData.FailedLoginAttempts = 0;
                    userAuthData.Status = "HOLD";
                    userAuthData.Istemporary = false;
                    userAuthData.Expiry = expiry;

                    try
                    {
                        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                        await _unitOfWork.SaveAsync();
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("Add user auth data Failed: {0}", error.Message);
                        return new UserResponse("Internal server error");
                    }
                }
            }
            else
            {
                if (userAuthDataInDb.Expiry.HasValue && expiry.HasValue)
                {
                    return new UserResponse("User already has a temporary fido2 device");
                }

                UserAuthDatum userAuthData = new UserAuthDatum();

                if (expiry.HasValue)
                {
                    userAuthData.UserId = userInDb.Uuid;
                    userAuthData.AuthScheme = "FIDO2";
                    userAuthData.AuthData = "NA";
                    userAuthData.CreatedDate = DateTime.Now;
                    userAuthData.ModifiedDate = DateTime.Now;
                    userAuthData.CreatedBy = "sysadmin";
                    userAuthData.UpdatedBy = "sysadmin";
                    userAuthData.FailedLoginAttempts = 0;
                    userAuthData.Status = "HOLD";
                    userAuthData.Istemporary = true;
                    userAuthData.Expiry = expiry;

                    try
                    {
                        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                        await _unitOfWork.SaveAsync();

                    }
                    catch (Exception error)
                    {
                        _logger.LogError("Add User Auth data Failed: {0}", error.Message);
                        return new UserResponse("Internal server error");
                    }
                }
                else
                {
                    userAuthDataInDb.Status = "EXPIRED";

                    try
                    {
                        _unitOfWork.UserAuthData.Update(userAuthDataInDb);
                        await _unitOfWork.SaveAsync();
                    }
                    catch
                    {
                        return new UserResponse("Internal server error");
                    }

                    userAuthData.UserId = userInDb.Uuid;
                    userAuthData.AuthScheme = "FIDO2";
                    userAuthData.AuthData = "NA";
                    userAuthData.CreatedDate = DateTime.Now;
                    userAuthData.ModifiedDate = DateTime.Now;
                    userAuthData.CreatedBy = "sysadmin";
                    userAuthData.UpdatedBy = "sysadmin";
                    userAuthData.FailedLoginAttempts = 0;
                    userAuthData.Status = "HOLD";
                    userAuthData.Istemporary = false;
                    userAuthData.Expiry = expiry;


                    try
                    {
                        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                        await _unitOfWork.SaveAsync();

                    }
                    catch (Exception error)
                    {
                        _logger.LogError("Add User Auth data Failed: {0}", error.Message);
                        return new UserResponse("Internal server error");
                    }
                }
            }

            return new UserResponse(userInDb);
        }

        public async Task<UserResponse> RegisterTempDeviceAsync(
            string user_id,
            string authData
            )
        {
            var userInDb = await _unitOfWork.Users.GetUserbyUuidAsync(user_id);
            if (null == userInDb)
            {
                return new UserResponse("An error occurred while updating the user. Please contact the admin.");
            }

            var userAuthDataInDb = await _unitOfWork.UserAuthData.GetUserTempAuthDataAsync(
                userInDb.Uuid,
                "FIDO2");
            if (null == userAuthDataInDb)
            {
                return new UserResponse("An error occurred while updating the user. Please contact the admin.");
            }

            if (userAuthDataInDb.Expiry.HasValue)
            {
                var userAuthData = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(
                    userInDb.Uuid,
                    "FIDO2");
                if (null != userAuthData)
                {
                    userAuthData.Status = "INACTIVE";
                    try
                    {
                        _unitOfWork.UserAuthData.Update(userAuthData);
                        await _unitOfWork.SaveAsync();
                    }
                    catch
                    {
                        return new UserResponse("An error occurred while updating the user. Please contact the admin.");
                    }
                }
            }

            if (userAuthDataInDb.AuthData != "NA")
            {
                var decryptedPassword = userAuthDataInDb.AuthData;

                var credential = JsonConvert.DeserializeObject<StoredCredential>
                    (decryptedPassword);

                var newAuthData = JsonConvert.DeserializeObject<StoredCredential>
                    (authData);

                if (credential.AaGuid == newAuthData.AaGuid)
                {
                    return new UserResponse("This device is already active");
                }
            }

            userAuthDataInDb.AuthData = authData;
            userAuthDataInDb.Status = "ACTIVE";

            try
            {
                _unitOfWork.UserAuthData.Update(userAuthDataInDb);
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                return new UserResponse("Internal server error");
            }

            if (!userAuthDataInDb.Expiry.HasValue)
            {
                var userInactiveAuthData = await _unitOfWork.UserAuthData.
                    GetUserInactiveAuthDataAsync(
                    userInDb.Uuid,
                    "FIDO2");
                if (null != userInactiveAuthData)
                {
                    userInactiveAuthData.Status = "EXPIRED";

                    try
                    {
                        _unitOfWork.UserAuthData.Update(userInactiveAuthData);
                        await _unitOfWork.SaveAsync();

                        return new UserResponse(userInDb);
                    }
                    catch
                    {
                        return new UserResponse("Internal server error");
                    }
                }
            }

            return new UserResponse(userInDb);
        }

        public async Task<UserResponse> RegisterUserFido2DeviceAsync(
            string user_id,
            string authData,
            DateTime? expiry
            )
        {
            var userInDb = await _unitOfWork.Users.GetUserbyUuidAsync(user_id);
            if (null == userInDb)
            {
                return new UserResponse("An error occurred while updating the user. Please contact the admin.");
            }

            var userAuthDataInDb = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(
            userInDb.Uuid,
            "FIDO2");
            if (null == userAuthDataInDb)
            {
                UserAuthDatum userAuthData = new UserAuthDatum();

                if (expiry.HasValue)
                {
                    userAuthData.UserId = userInDb.Uuid;
                    userAuthData.AuthScheme = "FIDO2";
                    userAuthData.AuthData = authData;
                    userAuthData.CreatedDate = DateTime.Now;
                    userAuthData.ModifiedDate = DateTime.Now;
                    userAuthData.CreatedBy = "sysadmin";
                    userAuthData.UpdatedBy = "sysadmin";
                    userAuthData.FailedLoginAttempts = 0;
                    userAuthData.Status = "ACTIVE";
                    userAuthData.Istemporary = true;
                    userAuthData.Expiry = expiry;

                    try
                    {
                        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                        await _unitOfWork.SaveAsync();
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("Add User Auth data Failed: {0}", error.Message);
                        return new UserResponse("Internal server error");
                    }
                }
                else
                {
                    userAuthData.UserId = userInDb.Uuid;
                    userAuthData.AuthScheme = "FIDO2";
                    userAuthData.AuthData = authData;
                    userAuthData.CreatedDate = DateTime.Now;
                    userAuthData.ModifiedDate = DateTime.Now;
                    userAuthData.CreatedBy = "sysadmin";
                    userAuthData.UpdatedBy = "sysadmin";
                    userAuthData.FailedLoginAttempts = 0;
                    userAuthData.Status = "ACTIVE";
                    userAuthData.Istemporary = false;
                    userAuthData.Expiry = expiry;

                    try
                    {
                        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                        await _unitOfWork.SaveAsync();
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("Add User Auth data Failed: {0}", error.Message);
                        return new UserResponse("Internal server error");
                    }
                }
            }
            else
            {
                if (userAuthDataInDb.Expiry.HasValue && expiry.HasValue)
                {
                    return new UserResponse("User already has a temporary fido2 device");
                }

                UserAuthDatum userAuthData = new UserAuthDatum();

                if (expiry.HasValue)
                {
                    userAuthData.UserId = userInDb.Uuid;
                    userAuthData.AuthScheme = "FIDO2";
                    userAuthData.AuthData = authData;
                    userAuthData.CreatedDate = DateTime.Now;
                    userAuthData.ModifiedDate = DateTime.Now;
                    userAuthData.CreatedBy = "sysadmin";
                    userAuthData.UpdatedBy = "sysadmin";
                    userAuthData.FailedLoginAttempts = 0;
                    userAuthData.Status = "ACTIVE";
                    userAuthData.Istemporary = true;
                    userAuthData.Expiry = expiry;

                    try
                    {
                        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                        await _unitOfWork.SaveAsync();
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("Add User Auth data Failed: {0}", error.Message);
                        return new UserResponse("Internal server error");
                    }
                }
                else
                {
                    userAuthDataInDb.Status = "EXPIRED";

                    try
                    {
                        _unitOfWork.UserAuthData.Update(userAuthDataInDb);
                        await _unitOfWork.SaveAsync();
                    }
                    catch
                    {
                        return new UserResponse("Internal server error");
                    }

                    userAuthData.UserId = userInDb.Uuid;
                    userAuthData.AuthScheme = "FIDO2";
                    userAuthData.AuthData = authData;
                    userAuthData.CreatedDate = DateTime.Now;
                    userAuthData.ModifiedDate = DateTime.Now;
                    userAuthData.CreatedBy = "sysadmin";
                    userAuthData.UpdatedBy = "sysadmin";
                    userAuthData.FailedLoginAttempts = 0;
                    userAuthData.Status = "ACTIVE";
                    userAuthData.Istemporary = false;
                    userAuthData.Expiry = expiry;

                    try
                    {
                        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
                        await _unitOfWork.SaveAsync();
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("Add User Auth data Failed: {0}", error.Message);
                        return new UserResponse("Internal server error");
                    }
                }
            }

            return new UserResponse(userInDb);
        }

        public async Task<UserResponse> DeleteUserAsync(int id, string updatedBy,
            bool makerCheckerFlag = false)
        {
            var userInDB = new UserTable();
            userInDB = await _unitOfWork.Users.GetByIdAsync(id);
            if (userInDB == null)
            {
                return new UserResponse("User not found");
            }

            var isEnabled = await _mcValidationService.IsMCEnabled(ActivityIdConstants.UserActivityId);

            if (false == makerCheckerFlag && true == isEnabled)
            {
                var response = await _mcValidationService.IsCheckerApprovalRequired(ActivityIdConstants.UserActivityId,
                    "DELETE", updatedBy,
                    JsonConvert.SerializeObject(userInDB));
                if (!response.Success)
                {
                    _logger.LogError("CheckApprovalRequired Failed");
                    return new UserResponse(response.Message);
                }
                if (response.Result)
                {
                    return new UserResponse(userInDB, "Your request sent for approval");
                }
            }
            try
            {
                userInDB.Status = "DELETED";
                userInDB.UpdatedBy = updatedBy;
                userInDB.ModifiedDate = DateTime.Now;

                _unitOfWork.Users.Update(userInDB);
                await _unitOfWork.SaveAsync();

                return new UserResponse(userInDB, "User deleted successfully");
            }
            catch (Exception)
            {
                return new UserResponse($"An error occurred while deleting the user. Please contact the admin.");
            }
        }

        public async Task<UserResponse> DeactivateUserAsync(int id)
        {
            var userInDB = await _unitOfWork.Users.GetByIdAsync(id);
            if (userInDB == null)
            {
                return new UserResponse("User not found");
            }

            userInDB.Status = "DEACTIVATED";

            try
            {
                _unitOfWork.Users.Update(userInDB);
                await _unitOfWork.SaveAsync();

                return new UserResponse(userInDB);
            }
            catch (Exception)
            {
                return new UserResponse($"An error occurred while deleting the user. Please contact the admin.");
            }
        }

        public async Task<UserResponse> ActivateUserAsync(int id)
        {
            var userInDB = await _unitOfWork.Users.GetByIdAsync(id);
            if (userInDB == null)
            {
                return new UserResponse("User not found");
            }

            userInDB.Status = "ACTIVE";

            try
            {
                _unitOfWork.Users.Update(userInDB);
                await _unitOfWork.SaveAsync();

                return new UserResponse(userInDB);
            }
            catch (Exception)
            {
                return new UserResponse($"An error occurred while deleting the user. Please contact the admin.");
            }
        }

        public async Task<UserResponse> UpdateUserStatus(int id, bool isApproved,
            string reason = null)
        {
            var userInDB = await _unitOfWork.Users.GetByIdAsync(id);
            if (userInDB == null)
            {
                return new UserResponse("Role not found");
            }

            if (isApproved)
            {
                userInDB.Status = "ACTIVE";
            }
            else
            {
                userInDB.Status = "BLOCKED";
            }
            try
            {
                _unitOfWork.Users.Update(userInDB);
                await _unitOfWork.SaveAsync();

                return new UserResponse(userInDB);
            }
            catch (Exception)
            {
                return new UserResponse($"An error occurred while changing state of the client. Please contact the admin.");
            }
        }

        public async Task<UserResponse> AdminResetPassword(int userId)
        {
            _logger.LogInformation("--->AdminResetPassword");

            var userInDb = await _unitOfWork.Users.GetByIdAsync(userId);
            if (null == userInDb)
            {
                _logger.LogError("User not found");
                return new UserResponse($"User not found");
            }

            var userAuthDataInDb = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(userInDb.Uuid, "PASSWORD");
            if (null == userAuthDataInDb)
            {
                _logger.LogError("User password details not found");
                return new UserResponse($"User password details not found");
            }

            var newRandomPswd = GenerateSecurePassword(10);

            try
            {
                var hashedPassword = _passwordHelper.HashPassword(newRandomPswd);
                userAuthDataInDb.AuthData = hashedPassword;
            }
            catch (Exception error)
            {
                _logger.LogError(error.ToString());
                return new UserResponse("Internal error");
            }

            try
            {
                _unitOfWork.UserAuthData.Update(userAuthDataInDb);
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                _logger.LogError("UserAuthData.Update failed");
                return new UserResponse($"An error occurred while reset password of user. Please contact the admin.");
            }

            var mailTemplate = await _unitOfWork.SMTP.GetByIdAsync(1);
            if (null == mailTemplate)
            {
                _logger.LogError("smtp details not found");
                return new UserResponse("An error occurred while creating the user. Please contact the admin.");
            }
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            var mailBody = " <p>Hi " + user.FullName + ",</p>" +
                                    "<p>" + mailTemplate.Template + "</p>" +
                                    "<p>Login ID: " + user.MailId + " (or) " + user.MobileNo + "</p>" +
                                    "<p>Password: " + newRandomPswd + "</p>" +
                                    "<br/>";
            var message = new Message(new string[]
            {
            userInDb.MailId
            },
            mailTemplate.MailSubject,
            mailBody
            );

            try
            {
                await _emailSender.SendEmail(message);
                return new UserResponse(userInDb, "User password sent to email successfully");
            }
            catch
            {
                _logger.LogError("Unable to send email");
                var userresp = await RevertUser(userInDb, 2);
                return new UserResponse(userInDb, "Unable to send email");
            }
        }
    }
}