
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text; 
using System.Threading.Tasks;
using DTPortal.Common;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Lookups;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Models.RegistrationAuthority;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.DTOs;
using DTPortal.Core.Enums;
using DTPortal.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static System.Formats.Asn1.AsnWriter;
using static DTPortal.Common.CommonResponse;

namespace DTPortal.Core.Services
{
    public class SessionService : ISessionService
    {
        private readonly ILogger<SessionService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheClient _cacheClient;

        private readonly IGlobalConfiguration _globalConfiguration;
        private readonly IConfiguration _configuration;
        private readonly MessageConstants Constants;
        private readonly ILogClient _LogClient;
        private readonly SSOConfig ssoConfig;
        private readonly HttpClient _client;
        private readonly IClientService _clientService;

        public SessionService(ILogger<SessionService> logger,
            IGlobalConfiguration globalConfiguration,
            ILogClient LogClient,
            IUnitOfWork unitOfWork, 
            ICacheClient cacheClient, 
            IConfiguration Configuration, 
            HttpClient httpClient, 
            IClientService clientService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _LogClient = LogClient;
            _cacheClient = cacheClient;
            _configuration = Configuration;

            _globalConfiguration = globalConfiguration;


            httpClient.BaseAddress = new Uri(_configuration["APIServiceLocations:IDPConfigurationBaseAddress"]);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _client = httpClient;

            // Get SSO Configuration
            ssoConfig = _globalConfiguration.GetSSOConfiguration();
            if (null == ssoConfig)
            {

                _logger.LogError("Get SSO Configuration failed");
                throw new NullReferenceException();
            }

            var errorConfiguration = _globalConfiguration.
                GetErrorConfiguration();
            if (null == errorConfiguration)
            {
                _logger.LogError("Get Error Configuration failed");
                throw new NullReferenceException();
            }

            Constants = errorConfiguration.Constants;
            if (null == Constants)
            {
                _logger.LogError("Get Error Configuration failed");
                throw new NullReferenceException();
            }

            _clientService = clientService;
        }

        public async Task<GetAllGlobalSessionsResponse> GetAllGlobalSession(int index, int count)
        {
            var response = new GetAllGlobalSessionsResponse();

            var task = await _cacheClient.GetAll<GlobalSession>("GlobalSession", index, count);
            if (task.Item2.Count == 0)
            {
                response.Success = false;
                response.Message = "No Sessions found";

                return response;
            }
            else
            {
                response.Success = true;
                response.NextIndex = task.nextIndex;
                response.GlobalSessions = task.Item2;

                return response;
            }
        }


        public async Task<GetAllUserSessionsResponse> GetAllIDPUserSessions(string input, int type)
        {
            var globalSessions = new List<GlobalSession>();
            var user = new UserTable();

            switch (type)
            {
                case (int)InputType.emailId:
                    {
                        user = await _unitOfWork.Users.GetUserbyEmailAsync(input);
                        if (null == user)
                        {
                            return new GetAllUserSessionsResponse("User details not found");
                        }
                        break;
                    }
                case (int)InputType.phoneno:
                    {
                        user = await _unitOfWork.Users.GetUserbyPhonenoAsync(input);
                        if (null == user)
                        {
                            return new GetAllUserSessionsResponse("User details not found");
                        }
                        break;
                    }
                case (int)InputType.userId:
                    {
                        user = await _unitOfWork.Users.GetUserbyUuidAsync(input);
                        if (null == user)
                        {
                            return new GetAllUserSessionsResponse("User details not found");
                        }
                        break;
                    }
                default:
                    {

                        return new GetAllUserSessionsResponse("Please check the input type");
                    }
                    ;
            }

            var isExists = await _cacheClient.Exists("UserSessions", user.Uuid);
            if (104 == isExists.retValue)
            {
                var userSessions = await _cacheClient.Get<IList<string>>("UserSessions", user.Uuid);
                if (userSessions.Count > 0)
                {
                    foreach (var item in userSessions)
                    {
                        var globalSession = await _cacheClient.Get<GlobalSession>("GlobalSession", item);
                        if (null != globalSession)
                        {
                            globalSessions.Add(globalSession);
                        }
                    }

                    return new GetAllUserSessionsResponse(globalSessions);
                }

                return new GetAllUserSessionsResponse("Session details not found");
            }
            else
            {
                return new GetAllUserSessionsResponse("Session details not found");
            }
        }

        //public async Task<GetAllUserSessionsResponse> GetAllRAUserSessions(string input, int type)
        //{
        //    var globalSessions = new List<GlobalSession>();
        //    var raSubscriber = new SubscriberView();

        //    switch (type)
        //    {
        //        case (int)InputType.emailId:
        //            {
        //                raSubscriber = await _unitOfWork.Subscriber.GetSubscriberInfoByEmail(input);
        //                if (null == raSubscriber)
        //                {
        //                    return new GetAllUserSessionsResponse("Subscriber session details not found");
        //                }
        //                break;
        //            }
        //        case (int)InputType.phoneno:
        //            {
        //                raSubscriber = await _unitOfWork.Subscriber.GetSubscriberInfoByPhone(input);
        //                if (null == raSubscriber)
        //                {
        //                    return new GetAllUserSessionsResponse("Subscriber session details not found");
        //                }
        //                break;
        //            }
        //        case (int)InputType.NIN:
        //            {
        //                raSubscriber = await _unitOfWork.Subscriber.GetSubscriberInfobyDocType(input);
        //                if (null == raSubscriber)
        //                {
        //                    return new GetAllUserSessionsResponse("Subscriber session details not found");
        //                }
        //                break;
        //            }
        //        case (int)InputType.Passport:
        //            {
        //                raSubscriber = await _unitOfWork.Subscriber.GetSubscriberInfobyDocType(input);
        //                if (null == raSubscriber)
        //                {
        //                    return new GetAllUserSessionsResponse("Subscriber session details not found");
        //                }
        //                break;
        //            }
        //        default:
        //            {

        //                return new GetAllUserSessionsResponse("Please check the input type");
        //            }
        //            ;
        //    }

        //    var isExists = await _cacheClient.Exists("UserSessions", raSubscriber.SubscriberUid);
        //    if (104 == isExists.retValue)
        //    {
        //        var userSessions = await _cacheClient.Get<IList<string>>("UserSessions", raSubscriber.SubscriberUid);
        //        if (userSessions.Count > 0)
        //        {
        //            foreach (var item in userSessions)
        //            {
        //                var globalSession = await _cacheClient.Get<GlobalSession>("GlobalSession", item);
        //                if (null != globalSession)
        //                {
        //                    globalSessions.Add(globalSession);
        //                }
        //            }

        //            return new GetAllUserSessionsResponse(globalSessions);
        //        }

        //        return new GetAllUserSessionsResponse("Session details not found");
        //    }
        //    else
        //    {
        //        return new GetAllUserSessionsResponse("Session details not found");
        //    }
        //}

        public async Task<GetAllClientSessionsResponse> GetAllClientSessions(string clientId)
        {
            var response = new GetAllClientSessionsResponse();
            var globalSessions = new List<GlobalSession>();

            var isExists = await _clientService.IsClientExistsAsync(clientId);
            if (false == isExists)
            {
                response.Success = false;
                response.Message = "No client found with given ID";
                return response;
            }

            var result = await _cacheClient.Exists("ClientSessions", clientId);
            if (104 == result.retValue)
            {
                var clientSessions = await _cacheClient.Get<IList<string>>("ClientSessions", clientId);
                foreach (var item in clientSessions)
                {
                    var globalSession = await _cacheClient.Get<GlobalSession>("GlobalSession", item);
                    if (null != globalSession)
                    {
                        if (!globalSessions.Contains(globalSession))
                        {
                            globalSessions.Add(globalSession);
                        }
                    }
                }

                if (globalSessions.Count > 0)
                {
                    response.Success = true;
                    response.GlobalSessions = globalSessions;
                    return response;
                }
                else
                {
                    response.Success = false;
                    response.Message = "No active sessions found for client";
                    return response;
                }

            }
            else
            {
                response.Success = false;
                response.Message = "No active sessions found for client";
                return response;
            }
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        public async Task<SessionResponse> GetGlobalSession(string sessionId)
        {
            /*
                        var isExists = await _cacheClient.Exists("GlobalSession", sessionId);
                        if(104 != isExists.retValue)
                        {
                            return new SessionResponse("No globalsession is found");
                        }
            */
            var result = await _cacheClient.Get<GlobalSession>("GlobalSession", sessionId);
            if (null == result)
            {
                return new SessionResponse("No globalsession is found");
            }

            result.LastAccessTime = DateTime.UtcNow.Ticks.ToString();

            var cacheRes = await _cacheClient.Add("GlobalSession", sessionId, result);
            if (0 != cacheRes.retValue)
            {
                return new SessionResponse("Internal server error");
            }

            return new SessionResponse(result);
        }


        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~



        //public async Task<Response> LogoutUser(LogoutUserRequest request)
        //{

        //    _logger.LogDebug("-->LogoutUser");

        //    // Variable declaration
        //    Response response = new Response();

        //    // Validate input
        //    if (string.IsNullOrEmpty(request.GlobalSession))
        //    {
        //        _logger.LogError(Constants.InvalidArguments);
        //        response.Success = false;
        //        response.Message = Constants.InvalidArguments;

        //        return response;
        //    }
        //    /*
        //                var isExists = await _cacheClient.Exists(CacheNames.GlobalSession,
        //                    request.GlobalSession);
        //                if (CacheCodes.KeyExist != isExists.retValue)
        //                {
        //                    _logger.LogError("GlobalSession expired/not exists");
        //                    response.Success = false;
        //                    response.Message = "GlobalSession expired/not exists";

        //                    return response;
        //                }
        //    */
        //    var globalSession = await _cacheClient.Get<GlobalSession>
        //        (CacheNames.GlobalSession, request.GlobalSession);
        //    if (null == globalSession)
        //    {
        //        _logger.LogError(Constants.SessionMismatch);
        //        response.Success = false;
        //        response.Message = Constants.SessionMismatch;

        //        return response;
        //    }

        //    var task = await _cacheClient.Remove(CacheNames.GlobalSession,
        //        request.GlobalSession);
        //    if (0 != task.retValue)
        //    {
        //        _logger.LogError("GlobalSession Remove failed");
        //        response.Success = false;
        //        response.Message = Constants.InternalError;
        //        return response;
        //    }

        //    var isExists = await _cacheClient.Exists(CacheNames.UserSessions,
        //        globalSession.UserId);
        //    if (CacheCodes.KeyExist == isExists.retValue)
        //    {
        //        var userSessions = await _cacheClient.Get<IList<string>>(
        //            CacheNames.UserSessions,
        //            globalSession.UserId);
        //        if (null == userSessions)
        //        {
        //            _logger.LogError(Constants.SessionMismatch);
        //            response.Success = false;
        //            response.Message = Constants.SessionMismatch;

        //            return response;
        //        }

        //        var isRemoved = userSessions.Remove(globalSession.GlobalSessionId);
        //        if (false == isRemoved)
        //        {
        //            _logger.LogError(Constants.SessionMismatch);
        //            response.Success = false;
        //            response.Message = Constants.SessionMismatch;

        //            return response;
        //        }

        //        var result = await _cacheClient.Add(CacheNames.UserSessions,
        //            globalSession.UserId,
        //            userSessions);
        //        if (0 != result.retValue)
        //        {
        //            _logger.LogError("UserSessions Add failed");
        //            response.Success = false;
        //            response.Message = Constants.InternalError;
        //            return response;
        //        }
        //    }
        //    else
        //    {
        //        response.Success = false;
        //        response.Message = Constants.SessionsNotFound;
        //        return response;
        //    }

        //    response.Success = true;

        //    var appList = string.Join(" ", globalSession.ClientId);
        //    var clientNameList = string.Empty;
        //    var clientList = new List<string>();

        //    foreach (var item in globalSession.ClientId)
        //    {
        //        var appName = await _unitOfWork.Client.GetClientByAppNameAsync(
        //            item);

        //        clientList.Add(appName.ClientId);

        //    }

        //    TemporarySession tempSession = new TemporarySession();
        //    Clientdetails clientdetails = new Clientdetails()
        //    {
        //        ClientId = string.Join(" ", clientList),
        //        AppName = appList
        //    };

        //    tempSession.CoRelationId = globalSession.CoRelationId;
        //    tempSession.AuthNStartTime = DateTime.Now.ToString("s");
        //    tempSession.Clientdetails = clientdetails;

        //    // Send central, service log message
        //    var logResponse =await _LogClient.SendAuthenticationLogMessage(
        //                tempSession,
        //                globalSession.UserId,
        //                LogClientServices.SubscriberLogOut,
        //                LogClientServices.SubscriberLogOut,
        //                LogClientServices.Success,
        //                LogClientServices.Business,
        //                false
        //                );
        //    if (false == logResponse.Success)
        //    {
        //        _logger.LogError("SendAuthenticationLogMessage failed: " +
        //            "{0}", logResponse.Message);
        //        response.Success = false;
        //        response.Message = Constants.InternalError;
        //        return response;
        //    }

        //    _logger.LogDebug("<--LogoutUser failed");
        //    return response;
        //}

        public async Task<Response> ValidateAccessToken(string accessToken)
        {
            _logger.LogDebug("--->ValidateSession");

            // Variable declaration
            Response response = new Response();

            // Validate input
            if (null == accessToken)
            {
                _logger.LogError("Invalid input parameter");
                response.Success = false;
                response.Message = "Invalid input parameter";

                return response;
            }
            /*
                        var isExists = await _cacheClient.Exists("AccessToken",
                            accessToken);
                        if (CacheCodes.KeyExist != isExists.retValue)
                        {
                            _logger.LogError("_cacheClient.Exists failed, AccessToken " +
                                "not found");
                            response.Success = false;
                            response.Message = "AccessToken expired/not exists";

                            return response;
                        }
            */
            var AccessToken = await _cacheClient.Get<Accesstoken>("AccessToken", accessToken);
            if (null == AccessToken)
            {
                _logger.LogError("_cacheClient.Exists failed, AccessToken " +
                    "not found");
                response.Success = false;
                response.Message = "AccessToken expired/not exists";

                return response;
            }


            if (_configuration.GetValue<string>("IDP_TYPE").Equals("INTERNAL"))
            {
                _logger.LogInformation("IDP_TYPE: INTERNAL");

                var isExists = await _cacheClient.Exists("GlobalSession",
                    AccessToken.GlobalSessionId);
                if (CacheCodes.KeyExist != isExists.retValue)
                {
                    _logger.LogError("_cacheClient.Exists failed, GlobalSession " +
                        "not found");
                    response.Success = false;
                    response.Message = "GlobalSession expired/not exists";

                    return response;
                }

                var sessInCache = await _cacheClient.Get<GlobalSession>("GlobalSession",
                    AccessToken.GlobalSessionId);
                if (null == sessInCache)
                {
                    _logger.LogError("_cacheClient.Exists failed, GlobalSession " +
                        "not found");
                    response.Success = false;
                    response.Message = "GlobalSession expired/not exists";

                    return response;
                }

                long loginTicks = long.Parse(sessInCache.LoggedInTime);
                // Compare session timeout
                TimeSpan sessionTime =
                    TimeSpan.FromTicks(DateTime.UtcNow.Ticks - loginTicks);
                //var sessionTime = DateTime.Now - Convert.ToDateTime(sessInCache.LoggedInTime);
                if (sessionTime.TotalMinutes >= ssoConfig.sso_config.session_timeout)
                {
                    // Remove global session
                    var CacheRes = await _cacheClient.Remove("GlobalSession",
                         AccessToken.GlobalSessionId);
                    if (0 != CacheRes.retValue)
                    {
                        _logger.LogError("_cacheClient.Remove failed, GlobalSession " +
                                "remove failed");

                        response.Success = false;
                        response.Message = "Internal server error";

                        return response;
                    }



                    // globalsession 
                    isExists = await _cacheClient.Exists(CacheNames.UserSessions,
                        sessInCache.UserId);
                    if (CacheCodes.KeyExist == isExists.retValue)
                    {
                        var userSessions = await _cacheClient.Get<IList<string>>
                            (CacheNames.UserSessions, sessInCache.UserId);

                        if (userSessions.Count > 0)
                        {
                            //var res = await _cacheClient.Remove(
                            //    CacheNames.GlobalSession,
                            //    userSessions.First());
                            //if (0 != res.retValue)
                            //{
                            //    _logger.LogError("GlobalSession Remove failed");
                            //    response.Success = false;
                            //    response.Message = Constants.InternalError;
                            //    return response;
                            //}

                            var res = await _cacheClient.Remove(CacheNames.UserSessions,
                                sessInCache.UserId);
                            if (0 != res.retValue)
                            {
                                _logger.LogError("UserSessions Remove failed");
                                response.Success = false;
                                response.Message = Constants.InternalError;
                                return response;
                            }
                        }
                    }





                    _logger.LogError("_cacheClient.Exists failed, GlobalSession " +
                            "not found");
                    response.Success = false;
                    response.Message = "GlobalSession expired/not exists";

                    return response;
                }

                _logger.LogInformation("LAST ACCESS TIME: {0}", sessInCache.LastAccessTime);

                long lastAccessTicks = long.Parse(sessInCache.LastAccessTime);
                // Compare ideal timeout
                TimeSpan idealTime =
                    TimeSpan.FromTicks(DateTime.UtcNow.Ticks - lastAccessTicks);

                //var idealTime = DateTime.Now - Convert.ToDateTime(sessInCache.LastAccessTime);  
                if (idealTime.TotalMinutes >= ssoConfig.sso_config.ideal_timeout)
                {
                    _logger.LogInformation("SESSION REACHED IDEAL TIMEOUT: {0}",
                        idealTime.TotalMinutes);

                    // Remove global session
                    var cacheRes = await _cacheClient.Remove("GlobalSession",
                        AccessToken.GlobalSessionId);
                    if (0 != cacheRes.retValue)
                    {
                        _logger.LogError("_cacheClient.Remove failed, GlobalSession " +
                                "remove failed");

                        response.Success = false;
                        response.Message = "Internal server error";

                        return response;
                    }


                    // globalsession 
                    isExists = await _cacheClient.Exists(CacheNames.UserSessions,
                        sessInCache.UserId);
                    if (CacheCodes.KeyExist == isExists.retValue)
                    {
                        var userSessions = await _cacheClient.Get<IList<string>>
                            (CacheNames.UserSessions, sessInCache.UserId);

                        if (userSessions.Count > 0)
                        {
                            //    var res = await _cacheClient.Remove(
                            //        CacheNames.GlobalSession,
                            //        userSessions.First());
                            //    if (0 != res.retValue)
                            //    {
                            //        _logger.LogError("GlobalSession Remove failed");
                            //        response.Success = false;
                            //        response.Message = Constants.InternalError;
                            //        return response;
                            //    }

                            var res = await _cacheClient.Remove(CacheNames.UserSessions, sessInCache.UserId);
                            if (0 != res.retValue)
                            {
                                _logger.LogError("UserSessions Remove failed");
                                response.Success = false;
                                response.Message = Constants.InternalError;
                                return response;
                            }
                        }
                    }






                    _logger.LogError("_cacheClient.Exists failed, GlobalSession " +
                            "REMOVED, as timeout has been reached");
                    response.Success = false;
                    response.Message = "GlobalSession expired/not exists";

                    return response;
                }

                _logger.LogInformation("session is active");

                sessInCache.LastAccessTime = DateTime.UtcNow.Ticks.ToString();

                var cacheres = await _cacheClient.Add("GlobalSession",
                    AccessToken.GlobalSessionId,
                    sessInCache);
                if (0 != cacheres.retValue)
                {
                    _logger.LogError("_cacheClient.Add failed, GlobalSession");
                    response.Success = false;
                    response.Message = "Internal server error";

                    return response;
                }
            }
            response.Success = true;
            response.Message = "";

            _logger.LogInformation("ValidateSession Response:{0}", response);
            _logger.LogDebug("<--ValidateSession");
            return response;
        }

        public async Task<Response> ValidateAccessTokenSession(string accessToken)
        {
            _logger.LogDebug("--->ValidateSession");

            // Variable declaration
            Response response = new Response();

            // Validate input
            if (null == accessToken)
            {
                _logger.LogError("Invalid input parameter");
                response.Success = false;
                response.Message = "Invalid input parameter";

                return response;
            }

            var isExists = await _cacheClient.Exists("AccessToken",
                accessToken);
            if (CacheCodes.KeyExist != isExists.retValue)
            {
                _logger.LogError("_cacheClient.Exists failed, AccessToken " +
                    "not found");
                response.Success = false;
                response.Message = "AccessToken expired/not exists";

                return response;
            }

            var AccessToken = await _cacheClient.Get<Accesstoken>("AccessToken", accessToken);
            if (null == AccessToken)
            {
                _logger.LogError("_cacheClient.Exists failed, AccessToken " +
                    "not found");
                response.Success = false;
                response.Message = "AccessToken expired/not exists";

                return response;
            }


            if (_configuration.GetValue<string>("IDP_TYPE").Equals("INTERNAL"))
            {
                _logger.LogInformation("IDP_TYPE: INTERNAL");

                isExists = await _cacheClient.Exists("GlobalSession",
                    AccessToken.GlobalSessionId);
                if (CacheCodes.KeyExist != isExists.retValue)
                {
                    _logger.LogError("_cacheClient.Exists failed, GlobalSession " +
                        "not found");
                    response.Success = false;
                    response.Message = "GlobalSession expired/not exists";

                    return response;
                }

                var sessInCache = await _cacheClient.Get<GlobalSession>("GlobalSession",
                    AccessToken.GlobalSessionId);
                if (null == sessInCache)
                {
                    _logger.LogError("_cacheClient.Exists failed, GlobalSession " +
                        "not found");
                    response.Success = false;
                    response.Message = "GlobalSession expired/not exists";

                    return response;
                }

                // Compare session timeout
                var sessionTime = DateTime.Now - Convert.ToDateTime(sessInCache.LoggedInTime);
                if (sessionTime.TotalMinutes >= ssoConfig.sso_config.session_timeout)
                {
                    // Remove global session
                    var CacheRes = await _cacheClient.Remove("GlobalSession",
                         AccessToken.GlobalSessionId);
                    if (0 != CacheRes.retValue)
                    {
                        _logger.LogError("_cacheClient.Remove failed, GlobalSession " +
                                "remove failed");

                        response.Success = false;
                        response.Message = "Internal server error";

                        return response;
                    }



                    // globalsession 
                    isExists = await _cacheClient.Exists(CacheNames.UserSessions,
                        sessInCache.UserId);
                    if (CacheCodes.KeyExist == isExists.retValue)
                    {
                        var userSessions = await _cacheClient.Get<IList<string>>
                            (CacheNames.UserSessions, sessInCache.UserId);

                        if (userSessions.Count > 0)
                        {
                            //var res = await _cacheClient.Remove(
                            //    CacheNames.GlobalSession,
                            //    userSessions.First());
                            //if (0 != res.retValue)
                            //{
                            //    _logger.LogError("GlobalSession Remove failed");
                            //    response.Success = false;
                            //    response.Message = Constants.InternalError;
                            //    return response;
                            //}

                            var res = await _cacheClient.Remove(CacheNames.UserSessions,
                                sessInCache.UserId);
                            if (0 != res.retValue)
                            {
                                _logger.LogError("UserSessions Remove failed");
                                response.Success = false;
                                response.Message = Constants.InternalError;
                                return response;
                            }
                        }
                    }

                    // globalsession 
                    isExists = await _cacheClient.Exists(CacheNames.UserSessions,
                        sessInCache.UserId);
                    if (CacheCodes.KeyExist == isExists.retValue)
                    {

                        _logger.LogError("_cacheClient.Exists failed, GlobalSession " +
                                "not found");
                        response.Success = false;
                        response.Message = "GlobalSession expired/not exists";

                        return response;
                    }
                }

                _logger.LogInformation("LAST ACCESS TIME: {0}", sessInCache.LastAccessTime);

                // Compare ideal timeout
                var idealTime = DateTime.Now - Convert.ToDateTime(sessInCache.LastAccessTime);
                if (idealTime.TotalMinutes >= ssoConfig.sso_config.ideal_timeout)
                {
                    _logger.LogInformation("SESSION REACHED IDEAL TIMEOUT: {0}",
                        idealTime.TotalMinutes);

                    // Remove global session
                    var cacheRes = await _cacheClient.Remove("GlobalSession",
                        AccessToken.GlobalSessionId);
                    if (0 != cacheRes.retValue)
                    {
                        _logger.LogError("_cacheClient.Remove failed, GlobalSession " +
                                "remove failed");

                        response.Success = false;
                        response.Message = "Internal server error";

                        return response;
                    }



                    var userSessions = await _cacheClient.Get<IList<string>>
                        (CacheNames.UserSessions, sessInCache.UserId);

                    if (userSessions.Count > 0)
                    {
                        //var res = await _cacheClient.Remove(
                        //    CacheNames.GlobalSession,
                        //    userSessions.First());
                        //if (0 != res.retValue)
                        //{
                        //    _logger.LogError("GlobalSession Remove failed");
                        //    response.Success = false;
                        //    response.Message = Constants.InternalError;
                        //    return response;
                        //}

                        var res = await _cacheClient.Remove(CacheNames.UserSessions, sessInCache.UserId);
                        if (0 != res.retValue)
                        {
                            _logger.LogError("UserSessions Remove failed");
                            response.Success = false;
                            response.Message = Constants.InternalError;
                            return response;
                        }
                    }

                    _logger.LogError("_cacheClient.Exists failed, GlobalSession " +
                            "REMOVED, as timeout has been reached");
                    response.Success = false;
                    response.Message = "GlobalSession expired/not exists";

                    return response;
                }

                _logger.LogInformation("session is active");

                //sessInCache.LastAccessTime = DateTime.Now.ToString();

                //var cacheres = await _cacheClient.Add("GlobalSession",
                //    AccessToken.GlobalSessionId,
                //    sessInCache);
                //if (0 != cacheres.retValue)
                //{
                //    _logger.LogError("_cacheClient.Add failed, GlobalSession");
                //    response.Success = false;
                //    response.Message = "Internal server error";

                //    return response;
                //}
            }
            response.Success = true;
            response.Message = "";

            _logger.LogInformation("ValidateSession Response:{0}", response);
            _logger.LogDebug("<--ValidateSession");
            return response;
        }



        //api implementation functions
        public async Task<GetAllUserSessionsResponse> GetIDPUserSessions(string input, int type)
        {


            var apiRes = await GetSessionAsync(input, type);
            var sessionRes = JsonConvert.DeserializeObject<GetAllUserSessionsResponse>(apiRes.Result.ToString());
            //user
            if (null == sessionRes)
            {
                return new GetAllUserSessionsResponse("User session details not found");
            }
            if (!sessionRes.Success)
            {

                return new GetAllUserSessionsResponse(sessionRes.Message);
            }
            else
            {
                return sessionRes;
            }


        }

        
        public async Task<GetAllUserSessionsResponse> GetRAUserSessions(string input, int type)
        {

            try
            {

                var apiResponse = await GetSessionAsync(input, type);

                if (!apiResponse.Success || apiResponse.Result == null)
                {

                    return new GetAllUserSessionsResponse(apiResponse.Message);
                }
                else
                {
                    var sessions = JsonConvert.DeserializeObject<IList<GlobalSession>>(apiResponse.Result.ToString());
                    return new GetAllUserSessionsResponse(sessions);
                }
            }
            catch (Exception ex) {

            }
            return null;

        }
       
        public async Task<APIResponse> GetSessionAsync(string input, int type)
        {

            try
            {
                APIResponse sessionRes = null;
                HttpResponseMessage response = await _client.GetAsync($"api/Session/get-session/{input}/{type}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    APIResponse apiResponse = JsonConvert.DeserializeObject<APIResponse>(await response.Content.ReadAsStringAsync());

                    if (apiResponse.Success)
                    {

                        //sessionRes = JsonConvert.DeserializeObject<APIResponse>(apiResponse.Result.ToString());
                        return JsonConvert.DeserializeObject<APIResponse>(apiResponse.Result.ToString());
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return apiResponse;
                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                       $"with status code={response.StatusCode}");

                    return new APIResponse("contact the admin");
                }
            }
            catch (Exception)
            {
                _logger.LogError("Failed to Get Session configuration");

                //return new GetAllUserSessionsResponse("Failed to Get Session Configuration");
            }
            return null;
        }
        
        public async Task<Response> LogoutSession(LogoutSession request)
        {
            try
            {

                var jsonContent = new StringContent(JsonConvert.SerializeObject(request),
                            Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("api/Session/revoke-session", jsonContent);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var apiResponse = JsonConvert.DeserializeObject<Response>(await response.Content.ReadAsStringAsync());
                    if (apiResponse.Success)
                    {
                        return apiResponse;
                    }
                    else
                    {
                        _logger.LogError(apiResponse.Message);
                        return apiResponse;
                    }
                }
                else
                {
                    _logger.LogError($"The request with uri={response.RequestMessage.RequestUri} failed " +
                       $"with status code={response.StatusCode}");

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return null;
        }





    }
}
