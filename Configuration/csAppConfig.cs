using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

//Configuration namespace is base layer
//used for all classed needed to configure the application and it's components
namespace Configuration;

//csAppConfig will contain all configuration in our example
//but it could be segmented into several classes, each with its own
//configuration responisibily
public sealed class csAppConfig
{
    static public string Hello { get; } = $"Hello from namespace {nameof(Configuration)}, class {nameof(csAppConfig)}";

    //use the right appsettings file depending on Debug or Release Build
#if DEBUG
    public const string Appsettingfile = "appsettings.Development.json";
#else
        public const string Appsettingfile = "appsettings.json";
#endif


    #region Singleton design pattern
    private static readonly object instanceLock = new();

    private static csAppConfig _instance = null;
    private static IConfigurationRoot _configuration = null;
    #endregion

    //All the DB Connections in the appsetting file
    private static DbSetDetail _dbSetActive = new DbSetDetail();
    private static List<DbSetDetail> _dbSets = new List<DbSetDetail>();
    private static PasswordSaltDetails _passwordSaltDetails = new PasswordSaltDetails();
    private static JwtConfig _jwtConfig = new JwtConfig();

    private csAppConfig()
    {
        //Lets get the credentials access Azure KV and set them as Environment variables
        //During Development this will come from User Secrets,
        //After Deployment it will come from appsettings.json

        string s = Directory.GetCurrentDirectory();

        //Create final ConfigurationRoot which includes also AzureKeyVault
        var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile(Appsettingfile, optional: true, reloadOnChange: true)
                            .AddUserSecrets("a85b06ae-bc23-4c06-8a1b-252f0ce4c782", reloadOnChange: true);

        _configuration = builder.Build();

        //get DbSet details
        _configuration.Bind("DbSets", _dbSets);  //Need the NuGet package Microsoft.Extensions.Configuration.Binder

        //Set the active db set and fill in location and server into Login Details
        var i = int.Parse(_configuration["DbSetActiveIdx"]);
        _dbSetActive = _dbSets[i];
        _dbSetActive.DbLogins.ForEach(i =>
        {
            i.DbLocation = _dbSetActive.DbLocation;
            i.DbServer = _dbSetActive.DbServer;
        });
        
        //get user password details
        _configuration.Bind("PasswordSaltDetails", _passwordSaltDetails);

        //get jwt configurations
        _configuration.Bind("JwtConfig", _jwtConfig);

    }

    public static IConfigurationRoot ConfigurationRoot
    {
        get
        {
            lock (instanceLock)
            {
                if (_instance == null)
                {
                    _instance = new csAppConfig();
                }
                return _configuration;
            }
        }
    }

    public static DbSetDetail DbSetActive
    {
        get
        {
            lock (instanceLock)
            {
                if (_instance == null)
                {
                    _instance = new csAppConfig();
                }
                return _dbSetActive;
            }
        }
    }
    public static DbLoginDetail DbLoginDetails (string DbUserLogin)
    {
        if (string.IsNullOrEmpty(DbUserLogin) || string.IsNullOrWhiteSpace(DbUserLogin))
            throw new ArgumentNullException();

        lock (instanceLock)
        {
            if (_instance == null)
            {
                _instance = new csAppConfig();
            }

            var conn = _dbSetActive.DbLogins.First(m => m.DbUserLogin.Trim().ToLower() == DbUserLogin.Trim().ToLower());
            if (conn == null)
                throw new ArgumentException("Database connection not found");

            return conn;
        }
    }
    public static string SecretMessage => ConfigurationRoot["SecretMessage"];

    public static PasswordSaltDetails PasswordSalt
    {
        get
        {
            lock (instanceLock)
            {
                if (_instance == null)
                {
                    _instance = new csAppConfig();
                }
                return _passwordSaltDetails;
            }
        }
    }

    public static JwtConfig JwtConfig
    {
        get
        {
            lock (instanceLock)
            {
                if (_instance == null)
                {
                    _instance = new csAppConfig();
                }
                return _jwtConfig;
            }
        }
    }
}

public class DbSetDetail
{
    public string DbLocation { get; set; }
    public string DbServer { get; set; }

    public List<DbLoginDetail> DbLogins { get; set; }
}

public class DbLoginDetail
{
    //set after reading in the active DbSet
    
    public string DbLocation { get; set; } = null;
    public string DbServer { get; set; } = null;

    public string DbUserLogin { get; set; }
    public string DbConnection { get; set; }
    public string DbConnectionString => csAppConfig.ConfigurationRoot.GetConnectionString(DbConnection);
}


public class PasswordSaltDetails
{
    public string Salt { get; set; }
    public int Iterations { get; set; }
}

public class JwtConfig
{
    public int LifeTimeMinutes { get; set; }

    public bool ValidateIssuerSigningKey { get; set; }
    public string IssuerSigningKey { get; set; }

    public bool ValidateIssuer { get; set; } = true;
    public string ValidIssuer { get; set; }

    public bool ValidateAudience { get; set; } = true;
    public string ValidAudience { get; set; }

    public bool RequireExpirationTime { get; set; }
    public bool ValidateLifetime { get; set; } = true;
}


