using System;

using Configuration;
using Models;
using DbModels;
using DbContext;
using DbRepos;
using Services;
using Microsoft.Extensions.Logging;
using Models.DTO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;

//Service namespace is an abstraction of using services without detailed knowledge
//of how the service is implemented.
//Service is used by the application layer using interfaces. Thus, the actual
//implementation of a service can be application dependent without changing code
//at application
namespace Services;

//csLoginService class will be used for user logins of the Console or WebApi
public class csLoginService : ILoginService
{
    static public string Hello { get; } = $"Hello from namespace {nameof(Services)}, class {nameof(csLoginService)}";

    private csLoginDbRepos _repo = null;
    private ILogger<csLoginService> _logger = null;

    #region constructors
    public csLoginService(csLoginDbRepos repo, ILogger<csLoginService> logger)
    {
        _repo = repo;
        _logger = logger;
    }
    #endregion

    public Task<usrInfoDto> SeedAsync(int nrOfUsers, int nrOfSuperUsers) => _repo.SeedAsync(nrOfUsers, nrOfSuperUsers);

    public async Task<loginUserSessionDto> LoginUserAsync(loginCredentialsDto usrCreds)
           => throw new NotImplementedException();

}

