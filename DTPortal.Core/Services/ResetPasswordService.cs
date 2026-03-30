using DTPortal.Common;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
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
    public class ResetPasswordService : IResetPasswordService
    {
        private readonly ILogger<ResetPasswordService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheClient _cacheClient;
        private readonly IEmailSender _emailSender;
        private readonly IPasswordHelper _passwordHelper;

        public ResetPasswordService(ILogger<ResetPasswordService> logger,
        IUnitOfWork unitOfWork, ICacheClient cacheClient,
            IEmailSender emailSender, IPasswordHelper passwordHelper)
        {
            _logger = logger;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
            _cacheClient = cacheClient;
            _passwordHelper = passwordHelper;
        }

        private string GenerateRandomOTP(int iOTPLength)
        {
            _logger.LogDebug("-->GenerateRandomOTP");

            if (0 == iOTPLength)
            {
                _logger.LogError("Invalid Input Parameter");
                return null;
            }

            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6",
                "7", "8", "9", "0" };

            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();

            try
            {
                for (int i = 0; i < iOTPLength; i++)
                {
                    int p = rand.Next(0, saAllowedCharacters.Length);
                    sTempChars = saAllowedCharacters[rand.Next(0,
                        saAllowedCharacters.Length)];
                    sOTP += sTempChars;
                }
            }
            catch (Exception error)
            {
                _logger.LogError("GenerateRandomOTP failed: {0}", error.Message);
                return null;
            }

            _logger.LogDebug("<--GenerateRandomOTP");
            return sOTP;
        }

        //public async Task<GetAllUserSecurityQueResponse> GetUserSecurityQuestions(int userId)
        //{
        //    GetAllUserSecurityQueResponse response = new GetAllUserSecurityQueResponse();

        //    var user = await _unitOfWork.Users.GetByIdAsync(userId);
        //    if (null == user)
        //    {
        //        response.Success = false;
        //        response.Message = "User not found";
        //        return response;
        //    }

        //    var userSecQues = await _unitOfWork.UserSecurityQue.GetAllUserSecQueAnsAsync(user.Id);
        //    if (userSecQues.Count() == 0)
        //    {
        //        response.Success = false;
        //        response.Message = "User Security Questions not found";
        //        return response;
        //    }

        //    var SecQueList = new List<SecurityQuestions>();
        //    foreach (var item in userSecQues)
        //    {
        //        var secQue = new SecurityQuestions()
        //        {
        //            Id = item.Id,
        //            Question = item.Question
        //        };

        //        SecQueList.Add(secQue);
        //    }

        //    response.Success = true;
        //    response.Result = SecQueList;
        //    return response;
        //}

        //public async Task<ValidateUserSecQueResponse> ValidateUserSecurityQuestions(ValidateUserSecQueRequest request)
        //{
        //    ValidateUserSecQueResponse response = new ValidateUserSecQueResponse();
        //    GetAuthNSessRes responseObj = new GetAuthNSessRes();

        //    var userId = await _unitOfWork.Users.GetUserbyUuidAsync(request.uuid);
        //    if (null == userId)
        //    {
        //        response.Success = false;
        //        response.Message = "User Security Questions/Answers not matched";
        //        return response;
        //    }

        //    var userSecQueAns = await _unitOfWork.UserSecurityQue.GetAllUserSecQueAnsAsync(userId.Id);
        //    int i = 0;

        //    foreach (var item in request.secQueAns)
        //    {
        //        foreach (var item1 in userSecQueAns)
        //        {
        //            if (item.secQue == item1.Question)
        //            {
        //                if (item.answer != item1.Answer)
        //                {
        //                    response.Success = false;
        //                    response.Message = "User Security Questions/Answers not matched";
        //                    return response;
        //                }
        //                i++;
        //            }
        //        }
        //    }

        //    if (i != request.secQueAns.Count)
        //    {
        //        response.Success = false;
        //        response.Message = "Security Questions/Answers not matched with User";
        //        return response;
        //    }

        //    var tempAuthNSessId = EncryptionLibrary.KeyGenerator.GetUniqueKey();

        //    TemporarySession temporarySession = new TemporarySession
        //    {
        //        TemporarySessionId = tempAuthNSessId,
        //        UserId = request.uuid,
        //        PrimaryAuthNSchemeList = new List<string>() { "PASSWORD" },
        //        AuthNSuccessList = new List<string>(),
        //        IpAddress = "NOT_AVAILABLE",
        //        MacAddress = "NOT_AVAILABLE"
        //    };

        //    var task = await _cacheClient.Add("TemporarySession", tempAuthNSessId,
        //            temporarySession);
        //    if (0 != task.retValue)
        //    {
        //        _logger.LogError("_cacheClient.Add failed");
        //        response.Success = false;
        //        if (!string.IsNullOrEmpty(task.errorMsg))
        //            response.Message = task.errorMsg;
        //        return response;
        //    }

        //    response.Success = true;
        //    response.TemporarySession = tempAuthNSessId;

        //    return response;
        //}

        public async Task<ValidateUserSecQueResponse> SendEmailOTP(string uuid, string tempsession)
        {
            var response = new ValidateUserSecQueResponse();

            var user = await _unitOfWork.Users.GetUserbyUuidAsync(uuid);
            if (null == user)
            {
                _logger.LogError("GetUserbyUuidAsync failed, not found");
                response.Success = false;
                response.Message = "User not found";
                return response;
            }

            if (null != tempsession)
            {
                var isExists = _cacheClient.KeyExists("TemporarySession", tempsession);
                if (104 == isExists.retValue)
                {
                    var res = await _cacheClient.Remove("TemporarySession", tempsession);
                    if (0 != res.retValue)
                    {
                        _logger.LogError("_cacheClient.Remove failed");
                        response.Success = false;
                        response.Message = "Internal server error";
                        return response;
                    }
                }
            }

            var otp = GenerateRandomOTP(6);

            var tempAuthNSessId = EncryptionLibrary.KeyGenerator.GetUniqueKey();

            TemporarySession temporarySession = new TemporarySession
            {
                TemporarySessionId = tempAuthNSessId,
                UserId = uuid,
                PrimaryAuthNSchemeList = new List<string>() { "EMAIL_OTP" },
                AuthNSuccessList = new List<string>(),
                IpAddress = "NOT_AVAILABLE",
                MacAddress = "NOT_AVAILABLE",
                AdditionalValue = otp
            };

            var task = await _cacheClient.Add("TemporarySession", tempAuthNSessId,
                    temporarySession);
            if (0 != task.retValue)
            {
                _logger.LogError("_cacheClient.Add failed");
                response.Success = false;
                if (!string.IsNullOrEmpty(task.errorMsg))
                    response.Message = task.errorMsg;
                return response;
            }

            response.Success = true;
            response.TemporarySession = tempAuthNSessId;

            var mailBody = "<p>Hi " + user.FullName + ",</p>" +
               "<p>Below is the OTP for email verification:</p>" +
               "<p>OTP: " + otp + "</p>";

            var message = new Message(new string[]
            {
            user.MailId
            },
            "IDP OTP",
            mailBody
            );

            try
            {
                await _emailSender.SendEmail(message);
            }
            catch
            {
                response.Success = false;
                response.Message = "Internal server error";
            }

            return response;
        }

        public async Task<Response> ResetPassword(ResetPasswordRequest request)
        {
            _logger.LogDebug("-->ResetPassword");

            Response response = new Response();

            //var isExists = await _cacheClient.Exists("TemporarySession",
            //    request.TemporarySession);
            //if (104 != isExists.retValue)
            //{
            //    response.Success = false;
            //    response.Message = "Temporary session expired/does not exists";
            //    return response;
            //}

            //var tempSession = await _cacheClient.Get<TemporarySession>("TemporarySession",
            //    request.TemporarySession);
            //if (null == tempSession)
            //{
            //    response.Success = false;
            //    response.Message = "Temporary session expired/does not exists";
            //    return response;
            //}

            //if (tempSession.PrimaryAuthNSchemeList.Count == 0 &&
            //    tempSession.PrimaryAuthNSchemeList[0].Equals("EMAIL_OTP"))
            //{
            //    if (request.otp != tempSession.AdditionalValue)
            //    {
            //        response.Success = false;
            //        response.Message = "Incorrect OTP";
            //        return response;
            //    }
            //}

            var userId = await _unitOfWork.Users.GetUserbyUuidAsync(request.uuid);
            if (null == userId)
            {
                response.Success = false;
                response.Message = "No user found with username";
                return response;
            }

            var passwordPolicy = await _unitOfWork.PasswordPolicy.GetByIdAsync(1);
            if (passwordPolicy == null)
            {
                response.Success = false;
                response.Message = "Internal server error";
                return response;
            }

            var isAccept = PasswordValidation.CheckPasswordComplexity(request.newPassword,
                passwordPolicy);
            if (false == isAccept)
            {
                response.Success = false;
                response.Message = String.Format(DTInternalConstants.PasswordPolicyMismatch,
                    passwordPolicy.MinimumPwdLength, passwordPolicy.MaximumPwdLength);
                return response;
            }

            if (!string.IsNullOrEmpty(userId.AuthData))
            {
                var passwordsInDb = userId.AuthData.Split(',');
                if (passwordsInDb.Count() > 0)
                {
                    if (passwordPolicy.PasswordHistory > 0)
                    {
                        for (int i = 0; i < passwordPolicy.PasswordHistory && i < passwordsInDb.Count(); i++)
                        {
                            if (!string.IsNullOrEmpty(passwordsInDb[i]))
                            {
                                try
                                {
                                    bool isMatch = _passwordHelper.VerifyPassword(passwordsInDb[i], request.newPassword);
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
            }

            var hashedNewPassword = _passwordHelper.HashPassword(request.newPassword);

            var userAuthDatainDb = await _unitOfWork.UserAuthData.GetUserAuthDataAsync(userId.Uuid, "PASSWORD");
            if (null == userAuthDatainDb)
            {
                // Handle missing auth data case if necessary
            }
            else
            {
                bool isSameAsCurrent = false;
                try
                {
                    isSameAsCurrent = _passwordHelper.VerifyPassword(userAuthDatainDb.AuthData, request.newPassword);
                }
                catch (Exception error)
                {
                    _logger.LogError("Password verification failed: {0}", error.Message);
                    response.Success = false;
                    response.Message = "Internal error verifying password";
                    return response;
                }

                if (isSameAsCurrent)
                {
                    response.Success = false;
                    response.Message = "New password and Old password is same";
                    return response;
                }

                userAuthDatainDb.AuthData = hashedNewPassword;
                _unitOfWork.UserAuthData.Update(userAuthDatainDb);
            }

            int res = await _unitOfWork.SaveAsync();
            if (1 != res)
            {
                response.Success = false;
                response.Message = "Internal error occurred";
                return response;
            }

            var UserpasswordDetail = await _unitOfWork.UserLoginDetail.GetUserLoginDetailAsync(
                userId.Id.ToString());
            if (null == UserpasswordDetail)
            {
                var userPasswordDetail = new UserLoginDetail
                {
                    UserId = userId.Id.ToString(),
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
                UserpasswordDetail.WrongPinCount = 0;
                UserpasswordDetail.IsReversibleEncryption = false;

                _unitOfWork.UserLoginDetail.Update(UserpasswordDetail);

                res = await _unitOfWork.SaveAsync();
                if (1 != res)
                {
                    response.Success = false;
                    response.Message = "Internal error occurred";
                    return response;
                }
            }

            if (string.IsNullOrEmpty(userId.AuthData))
            {
                userId.AuthData = hashedNewPassword;
            }
            else
            {
                var password = userId.AuthData.Split(',').ToList();
                if (password.Count() == passwordPolicy.PasswordHistory)
                {
                    password.Remove(password[0]);
                    userId.AuthData = string.Join(',', password);
                }
                userId.AuthData = string.Format("{0},{1}", userId.AuthData, hashedNewPassword);
            }

            try
            {
                _unitOfWork.Users.Update(userId);
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                response.Success = false;
                response.Message = "Internal server error";
                return response;
            }

            response.Success = true;
            response.Message = "User password reset successfully";

            return response;
        }
    }
}