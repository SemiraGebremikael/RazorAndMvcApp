using System;
using Models.DTO;

namespace Services
{
	public interface ILoginService
	{
        public Task<usrInfoDto> SeedAsync(int nrOfUsers, int nrOfSuperUsers);
        public Task<loginUserSessionDto> LoginUserAsync(loginCredentialsDto usrCreds);
    }
}

