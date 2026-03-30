using DocumentFormat.OpenXml.Spreadsheet;
using DTPortal.Core.Constants;
using DTPortal.Core.Domain.Models;
using DTPortal.Core.Domain.Repositories;
using DTPortal.Core.Domain.Services;
using DTPortal.Core.Domain.Services.Communication;
using DTPortal.Core.Utilities;
using DTPortal.Web.Constants;
using DTPortal.Web.Enums;
using DTPortal.Web.ExtensionMethods;
using DTPortal.Web.Helper;
using DTPortal.Web.Models;
using DTPortal.Web.ViewModel.Login;
using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [AllowAnonymous]
    public class LoginController : BaseController
    {
        private readonly IRoleManagementService _roleActivityService;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationService _configurationService;
        private readonly IUserConsoleService _userConsoleService;
        private readonly ILogger<LoginController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        //private readonly IGlobalConfiguration _globalConfiguration;
        private readonly OpenID openIDHelper;
        private readonly MessageConstants Constants;
        private readonly OIDCConstants OIDCConstants;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIpRestriction _iPRestriction;
        private readonly IGlobalConfiguration _globalConfiguration;
        public LoginController(IConfigurationService configurationService,
            ILogger<LoginController> logger,
            IUnitOfWork unitOfWork,
            ILogClient logClient,
            IUserConsoleService userConsoleService,
            IIpRestriction ipRestriction,
            IConfiguration configuration,
            IGlobalConfiguration globalConfiguration,
            IRoleManagementService roleActivityService,
            IHttpClientFactory httpClientFactory) : base(logClient)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _iPRestriction = ipRestriction;
            _configurationService = configurationService;
            _userConsoleService = userConsoleService;
            _roleActivityService = roleActivityService;
            _globalConfiguration = globalConfiguration;
            _httpClientFactory = httpClientFactory;
            openIDHelper = new OpenID(_configuration, _httpClientFactory, globalConfiguration);
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
            OIDCConstants = errorConfiguration.OIDCConstants;
            if (null == OIDCConstants)
            {
                _logger.LogError("Get Error Configuration failed");
                throw new NullReferenceException();
            }
        }


        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    _logger.LogInformation("Login  : User has valid session");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogInformation("Login  : User has invalid session redirect to login");
                    TempData.Clear();

                    var state = Guid.NewGuid().ToString("N");
                    var nonce = Guid.NewGuid().ToString("N");

                    HttpContext.Session.SetString("Nonce", nonce);
                    HttpContext.Session.SetString("state", state);

                    return View();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Login  Exception :  {0}", e.Message);
                _logger.LogError(e, "Login  Exception ");
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login ", LogMessageType.SUCCESS.ToString(), "Login  : " + e.Message);
                ViewBag.error = "Something Went wrong";
                ViewBag.error_description = e.Message;
                return View("Errorp");
            }
        }
        //[HttpGet]
        //public async Task<IActionResult> CallBack()
        //{
        //    try
        //    {

        //        if (!string.IsNullOrEmpty(Request.Query["error"]) && !string.IsNullOrEmpty(Request.Query["error_description"]))
        //        {
        //            ViewBag.error = Request.Query["error"].ToString();
        //            ViewBag.error_description = Request.Query["error_description"].ToString();
        //            return View("Errorp");
        //        }

        //        string code = Request.Query["code"].ToString();
        //        if (string.IsNullOrEmpty(code))
        //        {
        //            ViewBag.error = "Invalid code";
        //            ViewBag.error_description = "The code value is empty string or null";
        //            return View("Errorp");
        //        }

        //        JObject TokenResponse = openIDHelper.GetAccessToken(code).Result;
        //        if (TokenResponse.ContainsKey("error") && TokenResponse.ContainsKey("error_description"))
        //        {
        //            ViewBag.error = TokenResponse["error"].ToString();
        //            ViewBag.error_description = TokenResponse["error_description"].ToString();
        //            return View("Errorp");
        //        }


        //        _logger.LogInformation("Login Callback  : get access_token from idp successfully");

        //        UserSessionObj user = new UserSessionObj();

        //        var isOpenId = _configuration.GetValue<bool>("OpenId_Connect");

        //        var ID_Token = "";
        //        if (isOpenId)
        //        {
        //            ID_Token = TokenResponse["id_token"].ToString();
        //            if (string.IsNullOrEmpty(ID_Token))
        //            {
        //                ViewBag.error = "Invalid code";
        //                ViewBag.error_description = "The ID_Token value is empty string or null";
        //                return View("Errorp");
        //            }
        //        }

        //        var accessToken = TokenResponse["access_token"].ToString();
        //        if (string.IsNullOrEmpty(accessToken))
        //        {
        //            ViewBag.error = "Invalid code";
        //            ViewBag.error_description = "The code value is empty string or null";
        //            return View("Errorp");
        //        }

        //        if (isOpenId == true)
        //        {
        //            //code for openid connect

        //            //validate id_token and get cliam values from  id_token
        //            ClaimsPrincipal userObj = openIDHelper.ValidateIdentityToken(ID_Token);
        //            if (userObj == null)
        //            {
        //                ViewBag.error = "Something went wrong ";
        //                ViewBag.error_description = "Claim Object getting null value";
        //                return View("Errorp");
        //            }
        //            _logger.LogInformation("Login Callback  : get userinfo from id_token successfully");

        //            //get nonce value from session which is send from idp login url
        //            var Nonce = HttpContext.Session.GetString("Nonce");
        //            if (string.IsNullOrEmpty(Nonce))
        //            {
        //                ViewBag.error = "Something went wrong ";
        //                ViewBag.error_description = "Nonce value not found";
        //                return View("Errorp");
        //            }

        //            //validate nonce value is matched with our nonce value
        //            //which is send from login url
        //            var nonce = userObj.FindFirst("nonce")?.Value ?? "";
        //            if (!string.Equals(nonce, Nonce)) throw new Exception("invalid nonce");

        //            var daesClaim = userObj.FindFirst("daes_claims")?.Value ?? "";
        //            UserObj userdata = JsonConvert.DeserializeObject<UserObj>(daesClaim);
        //            userdata.sub = userObj.FindFirst("sub")?.Value ?? "";

        //            user.Uuid = userdata.suid;
        //            user.fullname = userdata.name;
        //            user.dob = userdata.birthdate;
        //            user.mailId = userdata.email;
        //            user.mobileNo = userdata.phone;
        //            user.sub = int.Parse(userdata.sub);
        //        }
        //        else
        //        {
        //            //code for oauth
        //            JObject userObj = openIDHelper.GetUserInfo(accessToken).Result;
        //            if (userObj.ContainsKey("error") && userObj.ContainsKey("error_description"))
        //            {
        //                ViewBag.error = userObj["error"].ToString();
        //                ViewBag.error_description = userObj["error_description"].ToString();
        //                return View("Errorp");
        //            }
        //            _logger.LogInformation("Login Callback  : get userinfo from idp successfully");

        //            user.Uuid = userObj["uuid"].ToString();
        //            user.fullname = userObj["name"].ToString();
        //            user.dob = userObj["birthdate"].ToString();
        //            user.mailId = userObj["email"].ToString();
        //            user.mobileNo = userObj["phone_number"].ToString();
        //            user.sub = int.Parse(userObj["sub"].ToString());
        //        }

        //        var userInDb = await _userConsoleService.GetUserAsync(user.sub);
        //        if (userInDb == null)
        //        {
        //            _logger.LogError("Login Callback : get user details failed");
        //            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get user info");
        //            return NotFound();
        //        }
        //        _logger.LogInformation("Login Callback  : get user details successfully");
        //        var roleInDb = await _roleActivityService.GetRoleAsync((int)userInDb.RoleId);
        //        if (roleInDb == null)
        //        {
        //            _logger.LogError("Login Callback : get user role details failed");
        //            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get user role info");
        //            return NotFound();
        //        }
        //        _logger.LogInformation("Login Callback  : get user role details successfully");
        //        var rolesList = GetAccessibleModuleList(roleInDb.Id, roleInDb.RoleActivities).Result;
        //        var isUserChecker = IsCheckerRole(roleInDb.Id, roleInDb.RoleActivities).Result;

        //        var kycAdminRoleId = _configuration.GetValue<string>("KycRoleId");
        //        var isKycAdmin = false;
        //        if (!string.IsNullOrEmpty(kycAdminRoleId))
        //        {
        //            if (userInDb.RoleId.ToString() == kycAdminRoleId)
        //            {
        //                rolesList.Add(new Claim(ClaimTypes.Role, "KYC Admin"));
        //                isKycAdmin = true;
        //            }
        //        }

        //        var identity = new ClaimsIdentity(new[] {
        //            new Claim("Access_Token", accessToken),
        //            new Claim(ClaimTypes.Name, user.fullname),
        //            new Claim(ClaimTypes.NameIdentifier, user.Uuid),
        //            new Claim("LastLoginTime",userInDb.LastLoginTime.ToString().Replace("/","-")),
        //            new Claim(ClaimTypes.Email,user.mailId),
        //            new Claim("UserRoleID",userInDb.RoleId.ToString()),
        //            new Claim(ClaimTypes.UserData,user.sub.ToString()),
        //            new Claim("IsUserChecker",isUserChecker.ToString()),
        //            new Claim("UserStatus",userInDb.Status.ToString()),
        //        }, CookieAuthenticationDefaults.AuthenticationScheme);

        //        if (isOpenId)
        //        {
        //            identity.AddClaim(new Claim("ID_Token", ID_Token));
        //        }

        //        if (rolesList.Count > 0)
        //            identity.AddClaims(rolesList);

        //        var principal = new ClaimsPrincipal(identity);

        //        var properties = new AuthenticationProperties();
        //        properties.IsPersistent = true;
        //        properties.AllowRefresh = false;

        //        //get session timeout from AdminPortal_SSOConfig db
        //        var configInDB = await _configurationService.GetConfigurationAsync<adminportal_config>("AdminPortal_SSOConfig");
        //        if (configInDB == null)
        //        {
        //            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get AdminPortal SSO configuration");
        //            return NotFound();
        //        }
        //        int SessionTimeOut = configInDB.session_timeout;
        //        if (0 == SessionTimeOut)
        //            SessionTimeOut = 30;

        //        properties.ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(SessionTimeOut));
        //        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        //        _logger.LogInformation("Login Callback  :  Login successfully");
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.SUCCESS.ToString(), "User login successfully", null, user.fullname);

        //        if (userInDb.Status == "NEW")
        //        {
        //            return RedirectToAction("ChangePassword", "UserDashboard", new { id = user.sub, isFirstLogin = true });
        //        }
        //        else if (userInDb.Status == "CHANGE_PASSWORD")
        //        {
        //            return RedirectToAction("SetSecurityQuestion", "UserDashboard");
        //        }
        //        else if (userInDb.Status == "SET_FIDO2")
        //        {
        //            return RedirectToAction("RegisterFido2", "Registration", new { id = user.sub });
        //        }
        //        else if (isKycAdmin == true)
        //        {
        //            return RedirectToAction("Index", "KycDashboard");
        //        }
        //        else
        //        {
        //            return RedirectToAction("Index", "Dashboard");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError("Login Callback Exception :  {0}", e.Message);
        //        _logger.LogError(e, "Login Callback Exception ");
        //        SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.SUCCESS.ToString(), "Login Callback : " + e.Message);
        //        ViewBag.error = "Internal Error";
        //        ViewBag.error_description = "Something went wrong!";
        //        return View("Errorp");
        //    }
        //}

        [NonAction]
        private async Task<List<Claim>> GetAccessibleModuleList(int roleId, IEnumerable<RoleActivity> roleActivities = null)
        {
            var activityLookupItems = await _roleActivityService.GetActivityLookupItemsAsync();
            if (activityLookupItems == null)
            {
                _logger.LogError("Login Callback : get all activity list failed");
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get all activities list");
                throw new Exception("Fail to get role activity");
            }
            var activityListItems = new List<Claim>();

            var roleIdList = _configuration.GetSection("RoleIds").Get<List<int>>();

            if (roleIdList.Count != 0 && !roleIdList.Contains(roleId))
            {
                activityLookupItems = activityLookupItems.Where(a => !a.IsCritical).ToList();
            }

            if (roleActivities != null)
            {
                foreach (var activity in activityLookupItems)
                {
                    var ActivityName = roleActivities.Any(x => x.ActivityId == activity.Id && (bool)x.IsEnabled) ? activity.DisplayName : "";
                    if (!String.IsNullOrEmpty(ActivityName))
                    {
                        if (ActivityName == "Reports")
                        {
                            ActivityName = ActivityName + "_" + activity.Id;
                            activityListItems.Add(new Claim(ClaimTypes.Role, ActivityName));
                        }
                        else
                        {
                            activityListItems.Add(new Claim(ClaimTypes.Role, ActivityName));
                        }
                    }
                    var isChecker = roleActivities.Any(x => x.ActivityId == activity.Id && (bool)x.IsEnabled && x.IsChecker == true);
                    if (activity.McSupported && isChecker)
                    {
                        var CheckerActivityName = activity.DisplayName + " Checker";
                        activityListItems.Add(new Claim(ClaimTypes.Role, CheckerActivityName));
                    }
                    if (!String.IsNullOrEmpty(ActivityName))
                    {
                        var Name = "";
                        var isParentExsist = activityLookupItems.Any(x => x.Id == activity.ParentId);
                        if (isParentExsist)
                            Name = activityLookupItems.First(x => x.Id == activity.ParentId).DisplayName;

                        var isTitleAdded = activityListItems.Any(x => x.Value == Name);
                        if (!isTitleAdded && Name != "")
                            activityListItems.Add(new Claim(ClaimTypes.Role, Name));
                    }
                }
            }
            else
            {

                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Get Accessible Module List", LogMessageType.FAILURE.ToString(), "Fail to get activity info");
            }


            return activityListItems;
        }

        [NonAction]
        private async Task<bool> IsCheckerRole(int roleId, IEnumerable<RoleActivity> roleActivities = null)
        {
            var activityLookupItems = await _roleActivityService.GetActivityLookupItemsAsync();
            if (activityLookupItems == null)
            {
                _logger.LogError("Login Callback : get all activity list failed");
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get all activities list");
                throw new Exception("Fail to get role activity");
            }
            var IsCheckerRole = false;
            var TotalCheckerActivity = 0;

            var roleIdList = _configuration.GetSection("RoleIds").Get<List<int>>();

            if (roleIdList.Count != 0 && !roleIdList.Contains(roleId))
            {
                activityLookupItems = activityLookupItems.Where(a => !a.IsCritical).ToList();
            }

            if (roleActivities != null)
            {
                foreach (var activity in activityLookupItems)
                {

                    var isChecker = roleActivities.Any(x => x.ActivityId == activity.Id && x.IsChecker == true);
                    if (activity.McSupported && isChecker)
                    {
                        TotalCheckerActivity = TotalCheckerActivity + 1;
                    }

                }
            }
            else
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Get User Role Type", LogMessageType.FAILURE.ToString(), "Fail to get activity info");
            }

            if (TotalCheckerActivity != 0)
                IsCheckerRole = true;

            return IsCheckerRole;
        }
        //public IActionResult ForgotPassword(string id, int userType)
        //{
        //    return RedirectToAction("ValidateSecurityQuestion", "UserDashboard", new { id = id, type = userType });
        //}
        //public IActionResult ForgotPassword(string id, int UserType)
        //{
        //    try
        //    {
        //        //_logger.LogDebug("--> ForgotPassword");
        //        //var ForgotPasswordUrl = string.Format(_configuration["ForgotPasswordUrl"],
        //        //                                    id, UserType);
        //        //_logger.LogDebug("<-- ForgotPassword");
        //        //return Redirect(ForgotPasswordUrl);
        //        return RedirectToAction("ValidateSecurityQuestion", "UserDashboard", new { id = id, type = UserType });
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError("SendPushNotification: {0}", e.Message);
        //        _logger.LogDebug("<--SendPushNotification");
        //        return StatusCode(500, "Internal Server Error");
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeLanguage(string culture, string returnUrl)
        {
            // 1. Set the language cookie
            Response.Cookies.Append(
      CookieRequestCultureProvider.DefaultCookieName,
      CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
      new CookieOptions
      {
          Secure = true,                      // Always enforce HTTPS
          HttpOnly = true,                    // Prevent JS access
          SameSite = SameSiteMode.Lax,        // Safe default
          MaxAge = TimeSpan.FromDays(30)      // Reduce lifetime
      }
  );
            // 2. Check if a valid Return URL exists and is local (security practice)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return Redirect("/");
        }
        private bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
        {
            _logger.LogDebug("-->TimeBetween");

            // convert datetime to a TimeSpan
            TimeSpan now = datetime.TimeOfDay;

            // see if start comes before end
            if (start < end)
            {
                return start <= now && now <= end;
            }

            // start is after end, so do the inverse comparison
            return !(end < now && now < start);
        }
        private async Task<bool> CheckTimeRestrictionforUser(int userId)
        {
            _logger.LogDebug("-->CheckTimeRestrictionforUser");

            if (userId == 0)
            {
                _logger.LogError("Invalid Input Parameter");
                return false;
            }

            var timeBasedAccessList = await _unitOfWork.TimeBasedAccess.EnumActiveTimeBasedAccess();
            if (timeBasedAccessList == null)
            {
                _logger.LogError("EnumActiveTimeBasedAccess failed, not found");
                return false;
            }

            var userInDb = await _unitOfWork.Users.GetUserByIdWithRoleAsync(userId);
            if (userInDb == null)
            {
                _logger.LogError("GetUserByIdWithRoleAsync failed, not found");
                return false;
            }

            foreach (var item in timeBasedAccessList)
            {
                string[] roles = item.ApplicableRoles.Split(',');

                if (!roles.Contains(userInDb.Role.Name))
                    continue;

                _logger.LogInformation("START TIME: {0}", item.StartTime);
                _logger.LogInformation("END TIME: {0}", item.EndTime);
                _logger.LogInformation("START DATE: {0}", item.StartDate);
                _logger.LogInformation("END DATE: {0}", item.EndDate);

                TimeSpan start = item.StartTime.Value.ToTimeSpan();
                TimeSpan end = item.EndTime.Value.ToTimeSpan();

                // CASE 1: Permanent restriction (EndDate is null => applies every day by time)
                if (!item.EndDate.HasValue)
                {
                    var isTrue = TimeBetween(DateTime.Now, start, end);
                    if (isTrue)
                        return true; // login restricted
                }
                else
                {
                    // CASE 2: Short-term restriction (Date + Time both should match)
                    DateOnly StartDate = (DateOnly)item.StartDate;
                    DateOnly EndDate = (DateOnly)item.EndDate;

                    DateTime today = DateTime.Now.Date;
                    DateOnly todayDate = DateOnly.FromDateTime(today);

                    // Check if current date is within range
                    if (todayDate >= StartDate && todayDate <= EndDate)
                    {
                        var isTrue = TimeBetween(DateTime.Now, start, end);
                        if (isTrue)
                            return true; // login restricted
                    }
                }
            }

            _logger.LogInformation("CheckTimeRestrictionforUser Response:{0}", false);
            _logger.LogDebug("<--CheckTimeRestrictionforUser");
            return false;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AuthenticatUser([FromBody] AuthenticateUserViewModel Model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userDb = await _unitOfWork.Users.GetUserbyEmailAsync(Model.UserEmail);


            if (userDb != null && userDb.Status == StatusConstants.SUSPENDED)
            {
                var loginDetail = await _unitOfWork.UserLoginDetail.GetUserLoginDetailAsync(userDb.Id.ToString());

                if (loginDetail != null && loginDetail.BadLoginTime.HasValue)
                {
                    var timeDiff = DateTime.UtcNow - loginDetail.BadLoginTime.Value;

                    if (timeDiff.TotalHours < 24) 
                        {
                        return Ok(new
                        {
                            Success = false,
                            Message = $"Account is suspended. Try again after {Math.Ceiling(24 - timeDiff.TotalHours)} hours"
                        });
                    }


                    userDb.Status = StatusConstants.ACTIVE;

                    loginDetail.BadLoginTime = null;
                    loginDetail.DeniedCount = 0;

                    var authData = await _unitOfWork.UserAuthData
                        .GetUserAuthDataAsync(userDb.Uuid, AuthNSchemeConstants.PASSWORD);

                    if (authData != null)
                    {
                        authData.FailedLoginAttempts = 0;
                        _unitOfWork.UserAuthData.Update(authData);
                    }

                    _unitOfWork.UserLoginDetail.Update(loginDetail);
                    _unitOfWork.Users.Update(userDb);
                    await _unitOfWork.SaveAsync();

                    _logger.LogInformation("User auto-activated after 24 hours: {Email}", Model.UserEmail);
                }
                else
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = "Account is suspended"
                    });
                }
            }


            var verifyPasswordResponse = await _userConsoleService
                .VerifyUserPassword(Model.UserEmail, Model.AuthenticationData);

            if (!verifyPasswordResponse.Success)
            {
                if (userDb != null)
                {
                    var authData = await _unitOfWork.UserAuthData
                        .GetUserAuthDataAsync(userDb.Uuid, AuthNSchemeConstants.PASSWORD);

                    if (authData != null)
                    {
                        authData.FailedLoginAttempts = (authData.FailedLoginAttempts ?? 0) + 1;

                        if (authData.FailedLoginAttempts >= 3)
                        {
                            userDb.Status = StatusConstants.SUSPENDED;

                            var loginDetail = await _unitOfWork.UserLoginDetail
                                .GetUserLoginDetailAsync(userDb.Id.ToString());

                            if (loginDetail == null)
                            {
                                loginDetail = new UserLoginDetail
                                {
                                    UserId = userDb.Uuid,
                                    BadLoginTime = DateTime.UtcNow,
                                    DeniedCount = 0
                                };

                                await _unitOfWork.UserLoginDetail.AddAsync(loginDetail);
                            }
                            else
                            {
                                loginDetail.BadLoginTime = DateTime.UtcNow;
                                _unitOfWork.UserLoginDetail.Update(loginDetail);
                            }

                            _unitOfWork.Users.Update(userDb);

                            _logger.LogWarning("User suspended after 3 failed attempts: {Email}", Model.UserEmail);
                        }

                        _unitOfWork.UserAuthData.Update(authData);
                        try
                        {
                            await _unitOfWork.SaveAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "DB Save failed");
                            throw; 
                        }

                        int remainingAttempts = 3 - (authData.FailedLoginAttempts ?? 0);

                        return Ok(new
                        {
                            Success = false,
                            Message = authData.FailedLoginAttempts >= 3
                                ? "Account is suspended"
                                : $"Invalid credentials"
                        });
                    }
                }

                return Ok(new
                {
                    Success = false,
                    Message = "Invalid credentials"
                });
            }


            var userInDb = (UserTable)verifyPasswordResponse.Resource;

            if (userInDb == null)
            {
                _logger.LogError("Login Callback : get user details failed");
                return NotFound();
            }


            var successAuthData = await _unitOfWork.UserAuthData
                .GetUserAuthDataAsync(userInDb.Uuid, AuthNSchemeConstants.PASSWORD);

            if (successAuthData != null)
            {
                successAuthData.FailedLoginAttempts = 0;
                _unitOfWork.UserAuthData.Update(successAuthData);
            }

            var successLoginDetail = await _unitOfWork.UserLoginDetail
                .GetUserLoginDetailAsync(userDb.Id.ToString());

            if (successLoginDetail != null)
            {
                successLoginDetail.BadLoginTime = null;
                successLoginDetail.DeniedCount = 0;
                _unitOfWork.UserLoginDetail.Update(successLoginDetail);
            }

            await _unitOfWork.SaveAsync();


            var istrue = await CheckTimeRestrictionforUser(userInDb.Id);
            if (istrue)
            {
                return Ok(new
                {
                    Success = false,
                    Message = Constants.TimeRestrictionApplied
                });
            }


            var user = new UserSessionObj();

            if (userInDb == null)
            {
                _logger.LogError("Login Callback : get user details failed");
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get user info");
                return NotFound();
            }

            user.Uuid = userInDb.Uuid;
            user.fullname = userInDb.FullName;
            user.mailId = userInDb.MailId;
            user.sub = (int)userInDb.Id;
            user.mobileNo = userInDb.MobileNo;
            var previousLoginTime = userInDb.LastLoginTime;
            _logger.LogInformation("Login Callback  : get user details successfully");

            var roleInDb = await _roleActivityService.GetRoleAsync((int)userInDb.RoleId);

            if (roleInDb == null)
            {
                _logger.LogError("Login Callback : get user role details failed");
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get user role info");
                return NotFound();
            }
            _logger.LogInformation("Login Callback  : get user role details successfully");
            var rolesList = GetAccessibleModuleList(roleInDb.Id, roleInDb.RoleActivities).Result;
            var isUserChecker = IsCheckerRole(roleInDb.Id, roleInDb.RoleActivities).Result;

            //var kycAdminRoleId = _configuration.GetValue<string>("KycRoleId");
            //var isKycAdmin = false;
            //if (!string.IsNullOrEmpty(kycAdminRoleId))
            //{
            //    if (userInDb.RoleId.ToString() == kycAdminRoleId)
            //    {
            //        rolesList.Add(new Claim(ClaimTypes.Role, "KYC Admin"));
            //        isKycAdmin = true;
            //    }
            //}
            var updateLoginResponse = await _userConsoleService.UpdateLastLoginTimeAsync(userInDb.Id);

            if (!updateLoginResponse.Success)
            {
                return Ok(new
                {
                    Success = false,
                    Message = updateLoginResponse.Message
                });
            }
            var adminRoleId = _configuration.GetValue<string>("AdminRoleId");
            if (!string.IsNullOrEmpty(adminRoleId))
            {
                if (userInDb.RoleId.ToString() == adminRoleId)
                {
                    rolesList.Add(new Claim(ClaimTypes.Role, "Digital ID Admin"));
                    rolesList.Add(new Claim(ClaimTypes.Role, "Admin"));
                }
            }

            var checker = "false";

            var adminCheckerRoleId = _configuration.GetValue<string>("AdminCheckerRoleId");
            if (!string.IsNullOrEmpty(adminCheckerRoleId))
            {
                if (userInDb.RoleId.ToString() == adminCheckerRoleId)
                {
                    rolesList.Add(new Claim(ClaimTypes.Role, "Digital ID Admin Checker"));
                    rolesList.Add(new Claim(ClaimTypes.Role, "Admin Checker"));
                    checker = "true";
                }
            }



            //var commercialAdminRoleId = _configuration.GetValue<string>("CommercialAdminRoleId");
            //var isCommercialAdmin = false;
            //if (!string.IsNullOrEmpty(commercialAdminRoleId))
            //{
            //    if (userInDb.RoleId.ToString() == commercialAdminRoleId)
            //    {
            //        isCommercialAdmin = true;
            //        rolesList.Add(new Claim(ClaimTypes.Role, "Commercial Admin"));
            //        rolesList.Add(new Claim(ClaimTypes.Role, "Finance Admin"));
            //    }
            //}


            //var commercialAdminCheckerRoleId = _configuration.GetValue<string>("CommercialAdminCheckerRoleId");
            //var isCommercialCheckerAdmin = false;
            //if (!string.IsNullOrEmpty(commercialAdminCheckerRoleId))
            //{
            //    if (userInDb.RoleId.ToString() == commercialAdminCheckerRoleId)
            //    {
            //        isCommercialCheckerAdmin = true;
            //        checker = "true";
            //        rolesList.Add(new Claim(ClaimTypes.Role, "Commercial Admin Checker"));
            //        rolesList.Add(new Claim(ClaimTypes.Role, "Finance Admin Checker"));
            //    }
            //}

            JObject TokenResponse = openIDHelper.GetAccessToken().Result;
            if (TokenResponse.ContainsKey("error") && TokenResponse.ContainsKey("error_description"))
            {
                ViewBag.error = TokenResponse["error"].ToString();
                ViewBag.error_description = TokenResponse["error_description"].ToString();
                return View("Errorp");
            }

            var accessToken = TokenResponse["access_token"].ToString();
            if (string.IsNullOrEmpty(accessToken))
            {
                ViewBag.error = "Invalid code";
                ViewBag.error_description = "The code value is empty string or null";
                return View("Errorp");
            }



            var identity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, user.fullname),
                new Claim("Access_Token", accessToken),
                new Claim(ClaimTypes.NameIdentifier, user.Uuid),
               new Claim("LastLoginTime",
previousLoginTime.HasValue
    ? previousLoginTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
    : "First Login"),
                new Claim(ClaimTypes.Email,user.mailId),
                new Claim("UserRoleID",userInDb.RoleId.ToString()),
                new Claim(ClaimTypes.UserData,user.sub.ToString()),
                new Claim("IsUserChecker",isUserChecker.ToString()),
                new Claim("UserStatus",userInDb.Status.ToString()),
                new Claim("IsChecker",checker)
            }, CookieAuthenticationDefaults.AuthenticationScheme);

            if (rolesList.Count > 0)
                identity.AddClaims(rolesList);

            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties();
            properties.IsPersistent = true;
            properties.AllowRefresh = false;

            var configInDB = await _configurationService.GetConfigurationAsync<adminportal_config>("AdminPortal_SSOConfig");
            if (configInDB == null)
            {
                SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.FAILURE.ToString(), "Fail to get AdminPortal SSO configuration");
                return NotFound();
            }
            int SessionTimeOut = configInDB.session_timeout;
            if (0 == SessionTimeOut)
                SessionTimeOut = 30;

            properties.ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(SessionTimeOut));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

            _logger.LogInformation("Login Callback  :  Login successfully");
            SendAdminLog(ModuleNameConstants.DigitalAuthentication, ServiceNameConstants.Login, "Login callback", LogMessageType.SUCCESS.ToString(), "User login successfully", null, user.fullname);

            var Res = new
            {
                Success = true,
                Message = "Authentication Successful"
            };

            return Ok(Res);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Authenticate()
        {
            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogInformation("Authenticate : User has invalid session redirect to login");
                return RedirectToAction("Index", "Login");
            }

            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData)?.Value;

            var userInDb = await _userConsoleService.GetUserAsync(int.Parse(userId));

            //var kycAdminRoleId = _configuration.GetValue<string>("KycRoleId");
            //var isKycAdmin = false;
            //if (!string.IsNullOrEmpty(kycAdminRoleId))
            //{
            //    if (userInDb.RoleId.ToString() == kycAdminRoleId)
            //    {
            //        isKycAdmin = true;
            //    }
            //}

            var adminRoleId = _configuration.GetValue<string>("AdminRoleId");
            //var isAdmin = false;
            if (!string.IsNullOrEmpty(adminRoleId))
            {
                if (userInDb.RoleId.ToString() == adminRoleId)
                {
                    //isAdmin = true;
                }
            }

            //var checker = "false";

            var adminCheckerRoleId = _configuration.GetValue<string>("AdminCheckerRoleId");
            //var isAdminChecker = false;
            if (!string.IsNullOrEmpty(adminCheckerRoleId))
            {
                if (userInDb.RoleId.ToString() == adminCheckerRoleId)
                {
                    //isAdminChecker = true;
                    //checker = "true";
                }
            }

            //var commercialAdminRoleId = _configuration.GetValue<string>("CommercialAdminRoleId");
            //var isCommercialAdmin = false;
            //if (!string.IsNullOrEmpty(commercialAdminRoleId))
            //{
            //    if (userInDb.RoleId.ToString() == commercialAdminRoleId)
            //    {
            //        isCommercialAdmin = true;
            //    }
            //}


            //var commercialAdminCheckerRoleId = _configuration.GetValue<string>("CommercialAdminCheckerRoleId");
            //var isCommercialCheckerAdmin = false;
            //if (!string.IsNullOrEmpty(commercialAdminCheckerRoleId))
            //{
            //    if (userInDb.RoleId.ToString() == commercialAdminCheckerRoleId)
            //    {
            //        isCommercialCheckerAdmin = true;
            //        checker = "true";
            //    }
            //}

            if (userInDb.Status == "NEW")
            {
                return RedirectToAction("ChangePassword", "UserDashboard", new { id = userInDb.Id, isFirstLogin = true });
            }
            else if (userInDb.Status == "CHANGE_PASSWORD")
            {
                return RedirectToAction("SetSecurityQuestion", "UserDashboard");
            }
            else if (userInDb.Status == "SET_FIDO2")
            {
                return RedirectToAction("RegisterFido2", "Registration", new { id = userInDb.Id });
            }
            //else if (isKycAdmin)
            //{
            //    return RedirectToAction("Index", "KycAdminDashboard");
            //}
            //else if (isCommercialAdmin)
            //{
            //    return RedirectToAction("Index", "CommercialisationDashboard");
            //}
            //else if (isCommercialCheckerAdmin)
            //{
            //    return RedirectToAction("Index", "CommercialisationDashboard");
            //}
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
    }
}
