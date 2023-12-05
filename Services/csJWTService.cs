using System;

using Configuration;
using Models;
using DbModels;
using DbContext;
using DbRepos;
using Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

//Service namespace is an abstraction of using services without detailed knowledge
//of how the service is implemented.
//Service is used by the application layer using interfaces. Thus, the actual
//implementation of a service can be application dependent without changing code
//at application
namespace Services
{
    //csJWTService class will be used for JSON WebToken authorization of the WebApi
    public static class csJWTService
    {
        static public string Hello { get; } = $"Hello from namespace {nameof(Services)}, class {nameof(csJWTService)}";

        #region Create a JWT Token

        //Create a list of claims to encrypt into the JWT token
        public static IEnumerable<Claim> CreateClaims(loginUserSessionDto _usrSession, out Guid TokenId)
        {
            TokenId = Guid.NewGuid();

            IEnumerable<Claim> claims = new Claim[] {
                //used to carry the loginUserSessionDto in the token
                new Claim("UserId", _usrSession.UserId.ToString()),
                new Claim("UserRole", _usrSession.UserRole),
                new Claim("UserName", _usrSession.UserName),

                //used by Microsoft.AspNetCore.Authentication and used in the HTTP request pipeline
                new Claim(ClaimTypes.Role, _usrSession.UserRole),
                new Claim(ClaimTypes.NameIdentifier, TokenId.ToString()),
                new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddMinutes(csAppConfig.JwtConfig.LifeTimeMinutes).ToString("MMM ddd dd yyyy HH:mm:ss tt"))
            };
            return claims;
        }

        //Create the JWT token
        public static JwtUserToken CreateJwtUserToken(loginUserSessionDto _usrSession)
        {
            if (_usrSession == null) throw new ArgumentException($"{nameof(_usrSession)} cannot be null");

            var _jwtSettings = csAppConfig.JwtConfig;

            var _userToken = new JwtUserToken();
            Guid tokenId = Guid.Empty;

            //get the key from user-secrets and set token expiration time
            var key = System.Text.Encoding.ASCII.GetBytes(_jwtSettings.IssuerSigningKey);
            DateTime expireTime = DateTime.UtcNow.AddMinutes(_jwtSettings.LifeTimeMinutes);

            //generate the token, including my own defined claims, expiration time, signing credentials
            var JWToken = new JwtSecurityToken(issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: CreateClaims(_usrSession, out tokenId),
                notBefore: new DateTimeOffset(DateTime.UtcNow).DateTime,
                expires: new DateTimeOffset(expireTime).DateTime,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));

            //generate a JWT user token with some unencrypted information as well
            _userToken.TokenId = tokenId;
            _userToken.EncryptedToken = new JwtSecurityTokenHandler().WriteToken(JWToken);
            _userToken.ExpireTime = expireTime;
            _userToken.UserRole = _usrSession.UserRole;
            _userToken.UserName = _usrSession.UserName;
            _userToken.UserId = _usrSession.UserId.Value;

            return _userToken;
        }
        #endregion

        #region Decode the the encrypted JWT token and exract the claims
        public static loginUserSessionDto DecodeToken(string _encryptedtoken)
        {
            if (_encryptedtoken == null) return null;

            var _decodedToken = new JwtSecurityTokenHandler().ReadJwtToken(_encryptedtoken);

            var _usr = new loginUserSessionDto();
            foreach (var claim in _decodedToken.Claims)
            {
                switch (claim.Type)
                {
                    case "UserId":
                        _usr.UserId = Guid.Parse(claim.Value);
                        break;
                    case "UserName":
                        _usr.UserName = claim.Value;
                        break;
                    case "UserRole":
                        _usr.UserRole = claim.Value;
                        break;
                }
            }
            return _usr;
        }
        #endregion

        #region Register the JWT Token Service to the HTTP Request pipeline
        public static void AddJwtTokenService(this IServiceCollection Services)
        {
            var _jwtSettings = csAppConfig.JwtConfig;

            //Here we tell ASP.NET Core that we are using JWT Authentication
            Services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })

                //Here we tell ASP.NET Core that it will be JWT Bearer token based
                .AddJwtBearer(options => {

                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    //Here we tell ASP.NET Core how to validate the JWT in the HTTP request pipeline
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSettings.IssuerSigningKey)),
                        ValidateIssuer = _jwtSettings.ValidateIssuer,
                        ValidIssuer = _jwtSettings.ValidIssuer,
                        ValidateAudience = _jwtSettings.ValidateAudience,
                        ValidAudience = _jwtSettings.ValidAudience,
                        RequireExpirationTime = _jwtSettings.RequireExpirationTime,
                        ValidateLifetime = _jwtSettings.RequireExpirationTime,
                        ClockSkew = TimeSpan.FromDays(1),
                    };
                });
        }
        #endregion
    }
}

