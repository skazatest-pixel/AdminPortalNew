using DTPortal.Common;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Utilities;
using EnterpriseGatewayPortal.Core.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DTPortal.Common.CommonResponse;

namespace DTPortal.Core.Services
{
    public class UserConsoleService : IUserConsoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserConsoleService> _logger;
        private readonly IPasswordHelper _passwordHelper;

        public UserConsoleService(IUnitOfWork unitOfWork, ILogger<UserConsoleService> logger, IPasswordHelper passwordHelper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _passwordHelper = passwordHelper;
        }

        public async Task<Response> ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var response = new Response();

            var userInDb = await _unitOfWork.Users.GetByIdAsync(userId);
            if (null == userInDb)
            {
                response.Success = false;
                response.Message = "No user found with ID";
                return response;
            }

            if (!userInDb.Status.Equals("NEW") && !userInDb.Status.Equals("ACTIVE"))
            {
                response.Success = false;
                response.Message = "User status is not ACTIVE/NEW";
                return response;
            }

            var passwordPolicy = await _unitOfWork.PasswordPolicy.GetByIdAsync(1);
            if (passwordPolicy == null)
            {
                response.Success = false;
                response.Message = "Internal server error";
                return response;
            }

            var isAccept = PasswordValidation.CheckPasswordComplexity(newPassword, passwordPolicy);
            if (false == isAccept)
            {
                response.Success = false;
                response.Message = String.Format(DTInternalConstants.PasswordPolicyMismatch,
                    passwordPolicy.MinimumPwdLength, passwordPolicy.MaximumPwdLength);
                return response;
            }

            if (!string.IsNullOrEmpty(userInDb.AuthData))
            {
                var passwordsInDb = userInDb.AuthData.Split(',');
                if (passwordsInDb.Length > 0 && passwordPolicy.PasswordHistory > 0)
                {
                    for (int i = 0; i < passwordPolicy.PasswordHistory && i < passwordsInDb.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(passwordsInDb[i]))
                        {
                            try
                            {
                                bool isMatch = _passwordHelper.VerifyPassword(passwordsInDb[i], newPassword);

                                if (isMatch)
                                {
                                    response.Success = false;
                                    response.Message = String.Format("New password matches one of the last {0} passwords",
                                        passwordPolicy.PasswordHistory);
                                    return response;
                                }
                            }
                            catch (Exception error)
                            {
                                _logger.LogError("History verification failed: {0}", error.Message);
                            }
                        }
                    }
                }
            }

            var currentAuthData = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(userInDb.Uuid, "PASSWORD");

            bool isOldPasswordCorrect = false;
            try
            {
                isOldPasswordCorrect = _passwordHelper.VerifyPassword(currentAuthData.AuthData, oldPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError("Password verification failed: {0}", ex.Message);
                response.Success = false;
                response.Message = "Error verifying old password";
                return response;
            }

            if (!isOldPasswordCorrect)
            {
                response.Success = false;
                response.Message = "Old password is not matched";
                return response;
            }

            bool isNewSameAsCurrent = _passwordHelper.VerifyPassword(currentAuthData.AuthData, newPassword);
            if (isNewSameAsCurrent)
            {
                response.Success = false;
                response.Message = "New password and Old password is same";
                return response;
            }

            var hashedNewPassword = _passwordHelper.HashPassword(newPassword);

            var UserpasswordDetail = await _unitOfWork.UserLoginDetail.GetUserLoginDetailAsync(userId.ToString());
            if (null == UserpasswordDetail)
            {
                var userPasswordDetail = new UserLoginDetail
                {
                    UserId = userInDb.Id.ToString(),
                    IsReversibleEncryption = false,
                    WrongPinCount = 0,
                    WrongCodeCount = 0,
                    DeniedCount = 0,
                    IsScrambled = false,
                    PriAuthSchId = 64,
                    LastAuthData = hashedNewPassword
                };

                try
                {
                    await _unitOfWork.UserLoginDetail.AddAsync(userPasswordDetail);
                    await _unitOfWork.SaveAsync();
                }
                catch
                {
                    response.Success = false;
                    response.Message = "Internal server error";
                    return response;
                }
            }
            else
            {
                UserpasswordDetail.LastAuthData = hashedNewPassword;
                UserpasswordDetail.IsReversibleEncryption = false;

                _unitOfWork.UserLoginDetail.Update(UserpasswordDetail);
                try
                {
                    await _unitOfWork.SaveAsync();
                }
                catch
                {
                    response.Success = false;
                    response.Message = "Internal error occurred";
                    return response;
                }
            }

            var userAuthData = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(userInDb.Uuid, "PASSWORD");

            userAuthData.AuthData = hashedNewPassword;
            userAuthData.ModifiedDate = DateTime.Now;

            _unitOfWork.UserAuthData.Update(userAuthData);

            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                response.Success = false;
                response.Message = "Internal error occurred";
                return response;
            }

            if (string.IsNullOrEmpty(userInDb.AuthData))
            {
                userInDb.AuthData = hashedNewPassword;
            }
            else
            {
                var passwordHistoryList = userInDb.AuthData.Split(',').ToList();

                if (passwordHistoryList.Count >= passwordPolicy.PasswordHistory)
                {
                    passwordHistoryList.RemoveAt(0);
                }

                passwordHistoryList.Add(hashedNewPassword);
                userInDb.AuthData = string.Join(",", passwordHistoryList);
            }

            if (userInDb.Status.Equals("NEW"))
            {
                userInDb.Status = "ACTIVE";
            }

            try
            {
                _unitOfWork.Users.Update(userInDb);
                await _unitOfWork.SaveAsync();

                response.Success = true;
                return response;
            }
            catch (Exception)
            {
                response.Success = false;
                response.Message = "Internal error occurred";
                return response;
            }
        }

        public async Task<UserResponse> UpdateProfile(UserTable user)
        {
            var userInDb = await _unitOfWork.Users.GetByIdAsync(user.Id);
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

            if (user.AuthScheme.Equals("FIDO2"))
            {
                var isExists = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(user.Uuid, "FIDO2");
                if (null == isExists)
                {
                    return new UserResponse("User FIDO2 device not registered");
                }
            }

            if (null == user.FullName)
            {
                return new UserResponse("FullName cannot be empty");
            }
            userInDb.UpdatedBy = user.UpdatedBy;
            userInDb.ModifiedDate = DateTime.Now;
            userInDb.Gender = user.Gender;
            userInDb.MailId = user.MailId;
            userInDb.MobileNo = user.MobileNo;
            userInDb.Dob = user.Dob;
            userInDb.RoleId = user.RoleId;
            userInDb.FullName = user.FullName;

            try
            {
                _unitOfWork.Users.Update(userInDb);
                await _unitOfWork.SaveAsync();
                return new UserResponse(user, "User updated successfully");
            }
            catch
            {
                return new UserResponse("An error occurred while updating the user. Please contact the admin.");
            }
        }

        //public async Task<bool> IsUserProvisioned(UserAuthDatum userAuthData)
        //{
        //    var isExists = await _unitOfWork.UserAuthData.IsUserAuthDataExists(userAuthData.UserId, "CHANGE_PASSWORD");
        //    if (false == isExists)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        //public async Task<UserAuthDataResponse> ProvisionUser(UserAuthDatum userAuthData)
        //{
        //    var isExists = await _unitOfWork.UserAuthData.IsUserAuthDataExists(userAuthData.UserId, "CHANGE_PASSWORD");
        //    if (false == isExists)
        //    {
        //        userAuthData.CreatedDate = DateTime.Now;
        //        userAuthData.ModifiedDate = DateTime.Now;

        //        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
        //    }
        //    else
        //    {
        //        var userAuthDatainDb = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(userAuthData.UserId, "CHANGE_PASSWORD");
        //        if (null == userAuthDatainDb)
        //        {
        //            return new UserAuthDataResponse("Provision user failed, Please contact admin");
        //        }

        //        userAuthDatainDb.AuthData = userAuthData.AuthData;
        //        userAuthDatainDb.ModifiedDate = DateTime.Now;
        //        userAuthDatainDb.FailedLoginAttempts = userAuthData.FailedLoginAttempts;

        //        _unitOfWork.UserAuthData.Update(userAuthData);
        //    }
        //    try
        //    {
        //        await _unitOfWork.SaveAsync();
        //        return new UserAuthDataResponse(userAuthData);
        //    }
        //    catch
        //    {
        //        return new UserAuthDataResponse("Provision user failed, Please contact admin");
        //    }
        //}

        public async Task<UserTable> GetUserAsync(int id)
        {
            return await _unitOfWork.Users.GetUserByIdWithRoleAsync(id);
        }

        //public async Task<UserSecurityQueResponse> UpdateUserSecurityQnsAns(UserSecurityQue userSecurityQue)
        //{
        //    var userSecQueinDb = await _unitOfWork.UserSecurityQue.GetByIdAsync(userSecurityQue.Id);
        //    if (null == userSecQueinDb)
        //    {
        //        return new UserSecurityQueResponse("No user security question/answer found with given ID. Please contact the admin.");
        //    }

        //    userSecQueinDb.Question = userSecurityQue.Question;
        //    userSecQueinDb.Answer = userSecurityQue.Answer;
        //    userSecQueinDb.UpdatedBy = "sysadmin";
        //    userSecQueinDb.ModifiedDate = DateTime.Now;

        //    try
        //    {
        //        _unitOfWork.UserSecurityQue.Update(userSecQueinDb);
        //        await _unitOfWork.SaveAsync();

        //        return new UserSecurityQueResponse(userSecQueinDb);
        //    }
        //    catch (Exception)
        //    {
        //        return new UserSecurityQueResponse("An error occurred while updating the user security question/answer. Please contact the admin.");
        //    }
        //}

        //public async Task<IEnumerable<UserSecurityQue>> GetAllUserSecurityQnsAns(int userId)
        //{
        //    return await _unitOfWork.UserSecurityQue.GetAllUserSecQueAnsAsync(userId);
        //}

        //public async Task<UserSecurityQueResponse> DeleteUserSecurityQnsAns(UserSecurityQue userSecurityQue)
        //{
        //    var userSecQueinDb = await _unitOfWork.UserSecurityQue.GetByIdAsync(userSecurityQue.Id);
        //    if (null == userSecQueinDb)
        //    {
        //        return new UserSecurityQueResponse("Could not delete user security question/answer. Please contact admin");
        //    }

        //    _unitOfWork.UserSecurityQue.Remove(userSecQueinDb);

        //    try
        //    {
        //        await _unitOfWork.SaveAsync();
        //        return new UserSecurityQueResponse(userSecurityQue);
        //    }
        //    catch
        //    {
        //        return new UserSecurityQueResponse("Could not delete user security question/answer. Please contact admin");
        //    }
        //}

        //public async Task<UserAuthDataResponse> GetUserAuthDataAsync(UserAuthDatum userAuthData)
        //{
        //    var data = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(userAuthData.UserId, userAuthData.AuthScheme);
        //    if (data == null)
        //    {
        //        return new UserAuthDataResponse("User Authdata Not Found");
        //    }
        //    else
        //    {
        //        userAuthData.AuthData = data.AuthData;
        //        return new UserAuthDataResponse(userAuthData);
        //    }
        //}

        //public async Task<UserAuthDataResponse> ProvisionExternalUser(UserAuthDatum userAuthData)
        //{
        //    var isExists = await _unitOfWork.UserAuthData.IsUserAuthDataExists(userAuthData.UserId, userAuthData.AuthScheme);
        //    if (false == isExists)
        //    {
        //        userAuthData.CreatedDate = DateTime.Now;
        //        userAuthData.ModifiedDate = DateTime.Now;
        //        userAuthData.CreatedBy = "sysadmin";
        //        userAuthData.UpdatedBy = "sysadmin";
        //        userAuthData.FailedLoginAttempts = 0;
        //        userAuthData.Status = "ACTIVE";
        //        userAuthData.Istemporary = false;
        //        await _unitOfWork.UserAuthData.AddAsync(userAuthData);
        //    }
        //    else
        //    {
        //        var userAuthDatainDb = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(userAuthData.UserId, userAuthData.AuthScheme);
        //        if (null == userAuthDatainDb)
        //        {
        //            return new UserAuthDataResponse("Provision user failed, Please contact admin");
        //        }

        //        userAuthDatainDb.AuthData = userAuthData.AuthData;
        //        userAuthDatainDb.ModifiedDate = DateTime.Now;
        //        userAuthDatainDb.FailedLoginAttempts = userAuthData.FailedLoginAttempts;

        //        _unitOfWork.UserAuthData.Update(userAuthDatainDb);
        //    }

        //    try
        //    {
        //        await _unitOfWork.SaveAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        return new UserAuthDataResponse(e.Message);
        //    }

        //    return new UserAuthDataResponse(userAuthData);
        //}

        //public async Task<UserSecurityQueResponse> CreateUserSecurityQnsAns(UserSecurityQue userSecurityQue)
        //{
        //    var userSecInDb = await _unitOfWork.UserSecurityQue.GetAllUserSecQueAnsAsync((int)userSecurityQue.UserId);
        //    if (userSecInDb.Count() == 2)
        //    {
        //        return new UserSecurityQueResponse("User security questions already provisioned");
        //    }

        //    userSecurityQue.CreatedDate = DateTime.Now;
        //    userSecurityQue.ModifiedDate = DateTime.Now;

        //    await _unitOfWork.UserSecurityQue.AddAsync(userSecurityQue);

        //    try
        //    {
        //        await _unitOfWork.SaveAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Add User Sec qns failed: {0}", ex.Message);
        //        return new UserSecurityQueResponse("An error occurred while creating the user security question/answer. Please contact the admin.");
        //    }

        //    var userSecCount = await _unitOfWork.UserSecurityQue.GetAllUserSecQueAnsAsync((int)userSecurityQue.UserId);
        //    if (userSecCount.Count() >= 2)
        //    {
        //        var userInDb = await _unitOfWork.Users.GetByIdAsync((int)userSecurityQue.UserId);
        //        if (null == userInDb)
        //        {
        //            return new UserSecurityQueResponse("An error occurred while creating the user security question/answer. Please contact the admin.");
        //        }

        //        if (userInDb.AuthScheme == "FIDO2")
        //        {
        //            userInDb.Status = "SET_FIDO2";
        //        }
        //        else
        //        {
        //            userInDb.Status = "ACTIVE";
        //        }

        //        try
        //        {
        //            _unitOfWork.Users.Update(userInDb);
        //            await _unitOfWork.SaveAsync();
        //            return new UserSecurityQueResponse(userSecurityQue);
        //        }
        //        catch
        //        {
        //            return new UserSecurityQueResponse("An error occurred while creating the user security question/answer. Please contact the admin.");
        //        }
        //    }
        //    return new UserSecurityQueResponse(userSecurityQue);
        //}
        public async Task<Response> UpdateLastLoginTimeAsync(int userId)
        {
            var response = new Response();

            var userInDb = await _unitOfWork.Users.GetByIdAsync(userId);

            if (userInDb == null)
            {
                response.Success = false;
                response.Message = "User not found";
                return response;
            }

            try
            {
                userInDb.LastLoginTime = DateTime.UtcNow;
                // userInDb.ModifiedDate = DateTime.Now;

                _unitOfWork.Users.Update(userInDb);
                await _unitOfWork.SaveAsync();

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating LastLoginTime: {0}", ex.Message);
                response.Success = false;
                response.Message = "Failed to update login time";
                return response;
            }
        }
        public async Task<ServiceResult> VerifyUserPassword(string email, string password)
        {
            var userInDb = await _unitOfWork.Users.GetUserByIdWithRoleByEmailAsync(email);
            if (null == userInDb)
            {
                return new ServiceResult(false, "No user found with the email");
            }

            var userAuthData = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(userInDb.Uuid, "PASSWORD");
            if (null == userAuthData)
            {
                return new ServiceResult(false, "No password data found for the user");
            }

            bool isCorrect = false;
            if (userInDb.Status.ToLower() == "deactivated")
            {
                return new ServiceResult(false, "User is Deactivated, Please contact Admin");
            }
            try
            {
                isCorrect = _passwordHelper.VerifyPassword(userAuthData.AuthData, password);
            }
            catch (Exception error)
            {
                _logger.LogError("VerifyPassword failed, found exception: {0}", error.Message);
                return new ServiceResult(false, "Password verification failed due to internal error");
            }

            if (!isCorrect)
            {
                return new ServiceResult(false, "Password is incorrect");
            }

            return new ServiceResult(true, "Password is correct", userInDb);
        }
    }
}