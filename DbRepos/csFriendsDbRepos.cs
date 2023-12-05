using Configuration;
using Models;
using Models.DTO;
using DbModels;
using DbContext;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Reflection.Metadata;

//DbRepos namespace is a layer to abstract the detailed plumming of
//retrieveing and modifying and data in the database using EFC.

//DbRepos implements database CRUD functionality using the DbContext
namespace DbRepos;

public class csFriendsDbRepos
{
    private ILogger<csFriendsDbRepos> _logger = null;

    #region used before csLoginService is implemented
    private string _dblogin = "sysadmin";
    //private string _dblogin = "gstusr";
    //private string _dblogin = "usr";
    //private string _dblogin = "supusr";
    #endregion


    #region only for layer verification
    private Guid _guid = Guid.NewGuid();
    private string _instanceHello = null;

    static public string Hello { get; } = $"Hello from namespace {nameof(DbRepos)}, class {nameof(csFriendsDbRepos)}";
    public string InstanceHello => _instanceHello;
    #endregion


    #region contructors
    public csFriendsDbRepos()
    {
        _instanceHello = $"Hello from class {this.GetType()} with instance Guid {_guid}.";
    }
    public csFriendsDbRepos(ILogger<csFriendsDbRepos> logger):this()
    {
        _logger = logger;
        _logger.LogInformation(_instanceHello);
    }
    #endregion


    #region Admin repo methods

    #region InfoAsync() implementation using EFC and database views
    public async Task<gstusrInfoAllDto> InfoAsync()
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            var _info = new gstusrInfoAllDto();
            _info.Db = await db.vwInfoDb.FirstAsync();
            _info.Friends = await db.vwInfoFriends.ToListAsync();
            _info.Pets = await db.vwInfoPets.ToListAsync();
            _info.Quotes = await db.vwInfoQuotes.ToListAsync();

            return _info;
        }
    }
    #endregion

    #region InfoAsync() implementation using EFC
    /*
    public async Task<gstusrInfoAllDto> InfoAsync()
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            var _info = new gstusrInfoAllDto
            {
                Db = new gstusrInfoDbDto
                {
                    #region full seeding
                    nrSeededFriends = await db.Friends.Where(f => f.Seeded).CountAsync(),
                    nrUnseededFriends = await db.Friends.Where(f => !f.Seeded).CountAsync(),
                    nrFriendsWithAddress = await db.Friends.Where(f => f.AddressId != null).CountAsync(),

                    nrSeededAddresses = await db.Addresses.Where(f => f.Seeded).CountAsync(),
                    nrUnseededAddresses = await db.Addresses.Where(f => !f.Seeded).CountAsync(),

                    nrSeededPets = await db.Pets.Where(f => f.Seeded).CountAsync(),
                    nrUnseededPets = await db.Pets.Where(f => !f.Seeded).CountAsync(),
                    #endregion

                    nrSeededQuotes = await db.Quotes.Where(f => f.Seeded).CountAsync(),
                    nrUnseededQuotes = await db.Quotes.Where(f => !f.Seeded).CountAsync(),
                }
            };

            return _info;
        }
    }
    */
    #endregion

    public async Task<adminInfoDbDto> SeedAsync(loginUserSessionDto usr, int nrOfItems)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            var _seeder = new csSeedGenerator();

            var qcount = await db.Quotes.CountAsync(q => q.Seeded);
            if (qcount == 0)
            {
                //Start by generating the quotes table
                var _quotes = _seeder.AllQuotes.Select(q => new csQuoteDbM(q)).ToList();
                foreach (var q in _quotes)
                {
                    db.Quotes.Add(q);
                }

                //ExploreChangeTracker(db);
                await db.SaveChangesAsync();
            }

            #region full seeding
            //Now _seededquotes is always the content of the Quotes table
            var _seededquotes = await db.Quotes.ToListAsync();

            //Generate friends and addresses
            var _friends = _seeder.ToList<csFriendDbM>(nrOfItems);

            var _existingaddresses = await db.Addresses.ToListAsync();
            var _addresses = _seeder.UniqueItemToList<csAddressDbM>(nrOfItems, _existingaddresses);

            for (int c = 0; c < nrOfItems; c++)
            {
                //Assign addresses. Friends could live on the same address
                _friends[c].AddressDbM = (_seeder.Bool) ? _seeder.FromList(_addresses) : null;

                //Create between 0 and 3 pets
                var _pets = new List<csPetDbM>();
                for (int i = 0; i < _seeder.Next(0, 4); i++)
                {
                    //A Pet can only be owned by one friend
                    _pets.Add(new csPetDbM().Seed(_seeder));
                }
                _friends[c].PetsDbM = _pets;


                //Create between 0 and 5 quotes
                var _favoriteQuotes = _seeder.UniqueItemPickFromList(_seeder.Next(0, 6), _seededquotes);
                _friends[c].QuotesDbM = _favoriteQuotes;

            }

            //Add the seeded items to EFC, ChangeTracker will now pick it up
            foreach (var f in _friends)
            {
                db.Friends.Add(f);
            }
            #endregion


            var _info = new adminInfoDbDto();

            #region full seed
            _info.nrSeededFriends = db.ChangeTracker.Entries().Count(entry => (entry.Entity is csFriendDbM) && entry.State == EntityState.Added);
            _info.nrSeededAddresses = db.ChangeTracker.Entries().Count(entry => (entry.Entity is csAddressDbM) && entry.State == EntityState.Added);
            _info.nrSeededPets = db.ChangeTracker.Entries().Count(entry => (entry.Entity is csPetDbM) && entry.State == EntityState.Added);
            #endregion

            _info.nrSeededQuotes = db.ChangeTracker.Entries().Count(entry => (entry.Entity is csQuoteDbM)
               && entry.State == EntityState.Added);

            #region full seed
            await db.SaveChangesAsync();
            #endregion

            return _info;
        }
    }


    #region RemoveSeedAsync() implementation using EFC and database stored procedure

    //Implementation using stored procedure that includes everything
    //i.e. return value, input and output parameters, and a resultset from the stored procedure
    public async Task<adminInfoDbDto> RemoveSeedAsync(loginUserSessionDto usr, bool seeded)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            var parameters = new List<SqlParameter>();

            var _retvalue = new SqlParameter("retval", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var _seeded = new SqlParameter("seeded", seeded);
            var _nrF = new SqlParameter("nrF", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var _nrA = new SqlParameter("nrA", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var _nrP = new SqlParameter("nrP", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var _nrQ = new SqlParameter("nrQ", SqlDbType.Int) { Direction = ParameterDirection.Output };

            parameters.Add(_retvalue);
            parameters.Add(_seeded);
            parameters.Add(_nrF);
            parameters.Add(_nrA);
            parameters.Add(_nrP);
            parameters.Add(_nrQ);

            //there is no FromSqlRawAsync to I make one here
            var _query = await Task.Run(() =>
                db.vwInfoDb.FromSqlRaw($"EXEC @retval = supusr.spDeleteAll @seeded," +
                    $"@nrF OUTPUT, @nrA OUTPUT, " +
                    $"@nrP OUTPUT, @nrQ OUTPUT", parameters.ToArray()).AsEnumerable());


            //Execute the query and get the sp result set.
            //ALthough, I am not using this result set, but it shows how to get it
            gstusrInfoDbDto result_set = _query.FirstOrDefault();
            //gstusrInfoDbDto result_set = _query.ToList()[0];  //alternative to show you get List

            //Check the return code
            int _retcode = (int)_retvalue.Value;
            if (_retcode != 0) throw new Exception("supusr.spDeleteAll return code error");

            var _info = new adminInfoDbDto();
            if (seeded)
            {
                _info.nrSeededFriends = (int)_nrF.Value;
                _info.nrSeededAddresses = (int)_nrA.Value;
                _info.nrSeededPets = (int)_nrP.Value;
                _info.nrSeededQuotes = (int)_nrQ.Value;
            }
            else
            {
                //Explore the changeTrackerNr of items to be deleted
                _info.nrUnseededFriends = (int)_nrF.Value;
                _info.nrUnseededAddresses = (int)_nrA.Value;
                _info.nrUnseededPets = (int)_nrP.Value;
                _info.nrUnseededQuotes = (int)_nrQ.Value;
            }

            return _info;
        }
    }
    #endregion


    #region RemoveSeedAsync() implementation using EFC ChangeTracker
    /*
    public async Task<adminInfoDbDto> RemoveSeedAsync(loginUserSessionDto usr, bool seeded)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            #region full seeding
            db.Friends.RemoveRange(db.Friends.Where(f => f.Seeded == seeded));

            //db.Pets.RemoveRange(db.Pets.Where(f => f.Seeded == seeded)); //not needed when cascade delete

            db.Addresses.RemoveRange(db.Addresses.Where(f => f.Seeded == seeded));
            #endregion

            db.Quotes.RemoveRange(db.Quotes.Where(f => f.Seeded == seeded));

            //ExploreChangeTracker(db);

            var _info = new adminInfoDbDto();
            if (seeded)
            {
                //Explore the changeTrackerNr of items to be deleted

                #region full seeding
                _info.nrSeededFriends = db.ChangeTracker.Entries().Count(entry => (entry.Entity is csFriendDbM) && entry.State == EntityState.Deleted);
                _info.nrSeededAddresses = db.ChangeTracker.Entries().Count(entry => (entry.Entity is csAddressDbM) && entry.State == EntityState.Deleted);
                _info.nrSeededPets = db.ChangeTracker.Entries().Count(entry => (entry.Entity is csPetDbM) && entry.State == EntityState.Deleted);
                #endregion

                _info.nrSeededQuotes = db.ChangeTracker.Entries().Count(entry => (entry.Entity is csQuoteDbM) && entry.State == EntityState.Deleted);
            }
            else
            {
                //Explore the changeTrackerNr of items to be deleted
                #region full seeding
                _info.nrUnseededFriends = db.ChangeTracker.Entries().Count(entry => (entry.Entity is csFriendDbM) && entry.State == EntityState.Deleted);
                _info.nrUnseededAddresses = db.ChangeTracker.Entries().Count(entry => (entry.Entity is csAddressDbM) && entry.State == EntityState.Deleted);
                _info.nrUnseededPets = db.ChangeTracker.Entries().Count(entry => (entry.Entity is csPetDbM) && entry.State == EntityState.Deleted);
                #endregion

                _info.nrUnseededQuotes = db.ChangeTracker.Entries().Count(entry => (entry.Entity is csQuoteDbM) && entry.State == EntityState.Deleted);
            }

            //do the actual deletion
            await db.SaveChangesAsync();
            return _info;
        }
    }
    */
    #endregion

    #region exploring the ChangeTracker
    private static void ExploreChangeTracker(csMainDbContext db)
    {
        foreach (var e in db.ChangeTracker.Entries())
        {
            if (e.Entity is csQuote q)
            {
                Console.WriteLine(e.State);
                Console.WriteLine(q.QuoteId);
            }
        }
    }
    #endregion

    #endregion


    #region Friends repo methods
    public async Task<IFriend> ReadFriendAsync(loginUserSessionDto usr, Guid id, bool flat)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            if (!flat)
            {
                //make sure the model is fully populated, try without include.
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _query = db.Friends.AsNoTracking().Include(i => i.AddressDbM).Include(i => i.PetsDbM)
                    .Include(i => i.QuotesDbM)
                    .Where(i => i.FriendId == id);

                return await _query.FirstOrDefaultAsync<IFriend>();
            }
            else
            {
                //Not fully populated, compare the SQL Statements generated
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _query = db.Friends.AsNoTracking()
                    .Where(i => i.FriendId == id);

                return await _query.FirstOrDefaultAsync<IFriend>();
            }
        }
    }

    public async Task<List<IFriend>> ReadFriendsAsync(loginUserSessionDto usr, bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            filter ??= "";
            if (!flat)
            {
                //make sure the model is fully populated, try without include.
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _query = db.Friends.AsNoTracking().Include(i => i.AddressDbM).Include(i => i.PetsDbM)
                    .Include(i => i.QuotesDbM);

                return await _query

                    //Adding filter functionality
                    .Where(i => i.Seeded == seeded
                            && (i.FirstName.ToLower().Contains(filter) ||
                                i.LastName.ToLower().Contains(filter)))

                    //Adding paging
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)

                    .ToListAsync<IFriend>();
            }
            else
            {
                //Not fully populated, compare the SQL Statements generated
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _query = db.Friends.AsNoTracking();

                return await _query

                    //Adding filter functionality
                    .Where(i => i.Seeded == seeded
                            && (i.FirstName.ToLower().Contains(filter) ||
                                i.LastName.ToLower().Contains(filter)))

                    //Adding paging
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)

                    .ToListAsync<IFriend>();
            }
        }
    }

    public async Task<IFriend> DeleteFriendAsync(loginUserSessionDto usr, Guid id)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            //Find the instance with matching id
            var _query1 = db.Friends
                .Where(i => i.FriendId == id);
            var _item = await _query1.FirstOrDefaultAsync<csFriendDbM>();

            //If the item does not exists
            if (_item == null) throw new ArgumentException($"Item {id} is not existing");

            //delete in the database model
            db.Friends.Remove(_item);

            //write to database in a UoW
            await db.SaveChangesAsync();
            return _item;
        }
    }

    public async Task<IFriend> UpdateFriendAsync(loginUserSessionDto usr, csFriendCUdto itemDto)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            //Find the instance with matching id and read the navigation properties.
            var _query1 = db.Friends
                .Where(i => i.FriendId == itemDto.FriendId);
            var _item = await _query1.Include(i => i.AddressDbM)
                .Include(i => i.PetsDbM)
                .Include(i => i.QuotesDbM).FirstOrDefaultAsync<csFriendDbM>();

            //If the item does not exists
            if (_item == null) throw new ArgumentException($"Item {itemDto.FriendId} is not existing");

            //transfer any changes from DTO to database objects
            //Update individual properties
            _item.UpdateFromDTO(itemDto);

            //Update navigation properties
            await csFriendCUdto_to_csFriendDbM(db, itemDto, _item);

            //write to database model
            db.Friends.Update(_item);

            //write to database in a UoW
            await db.SaveChangesAsync();
            return _item;
        }
    }

    public async Task<IFriend> CreateFriendAsync(loginUserSessionDto usr, csFriendCUdto itemDto)
    {
        if (itemDto.FriendId != null)
            throw new ArgumentException($"{nameof(itemDto.FriendId)} must be null when creating a new object");

        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            //transfer any changes from DTO to database objects
            //Update individual properties Friend
            var _item = new csFriendDbM(itemDto);

            //Update navigation properties
            await csFriendCUdto_to_csFriendDbM(db, itemDto, _item);

            //write to database model
            db.Friends.Add(_item);

            //write to database in a UoW
            await db.SaveChangesAsync();
            return _item;
        }
    }

    //from all id's in _itemDtoSrc finds the corresponding object in the database and assigns it to _itemDst
    //Error is thrown if no object is found correspodning to an id.
    private static async Task csFriendCUdto_to_csFriendDbM(csMainDbContext db, csFriendCUdto _itemDtoSrc, csFriendDbM _itemDst)
    {
        //update AddressDbM from itemDto.AddressId
        _itemDst.AddressDbM = (_itemDtoSrc.AddressId != null) ? await db.Addresses.FirstOrDefaultAsync(
            a => (a.AddressId == _itemDtoSrc.AddressId)) : null;

        //update PetsDbM from itemDto.PetsId list
        List<csPetDbM> _pets = null;
        if (_itemDtoSrc.PetsId != null)
        {
            _pets = new List<csPetDbM>();
            foreach (var id in _itemDtoSrc.PetsId)
            {
                var p = await db.Pets.FirstOrDefaultAsync(i => i.PetId == id);
                if (p == null)
                    throw new ArgumentException($"Item id {id} not existing");

                //potentially changing owner of a pet
                p.FriendId = _itemDst.FriendId;
                _pets.Add(p);
            }
        }
        _itemDst.PetsDbM = _pets;

        //update QuotesDbM from itemDto.QuotesId
        List<csQuoteDbM> _quotes = null;
        if (_itemDtoSrc.QuotesId != null)
        {
            _quotes = new List<csQuoteDbM>();
            foreach (var id in _itemDtoSrc.QuotesId)
            {
                _quotes.Add(db.Quotes.First(i => i.QuoteId == id));
            }
        }
        _itemDst.QuotesDbM = _quotes;
    }

    #endregion


    #region Addresses repo methods
    public async Task<IAddress> ReadAddressAsync(loginUserSessionDto usr, Guid id, bool flat)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            if (!flat)
            {
                //make sure the model is fully populated, try without include.
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _query = db.Addresses.AsNoTracking().Include(i => i.FriendsDbM).ThenInclude(i => i.PetsDbM)
                    .Where(i => i.AddressId == id);

                return await _query.FirstOrDefaultAsync<IAddress>();
            }
            else
            {
                //Not fully populated, compare the SQL Statements generated
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _query = db.Addresses.AsNoTracking()
                    .Where(i => i.AddressId == id);

                return await _query.FirstOrDefaultAsync<IAddress>();
            }
        }
    }

    public async Task<List<IAddress>> ReadAddressesAsync(loginUserSessionDto usr, bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            filter ??= "";
            if (!flat)
            {
                //make sure the model is fully populated, try without include.
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _addresses = db.Addresses.AsNoTracking().Include(f => f.FriendsDbM);

                return await _addresses

                    //Adding filter functionality
                    .Where(i => i.Seeded == seeded
                            && (i.StreetAddress.ToLower().Contains(filter) ||
                                i.City.ToLower().Contains(filter) ||
                                i.Country.ToLower().Contains(filter)))

                    //Adding paging
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)

                    .ToListAsync<IAddress>();
            }
            else
            {
                //Not fully populated, compare the SQL Statements generated
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _addresses = db.Addresses.AsNoTracking();

                return await _addresses

                    //Adding filter functionality
                    .Where(i => i.Seeded == seeded
                            && (i.StreetAddress.ToLower().Contains(filter) ||
                                i.City.ToLower().Contains(filter) ||
                                i.Country.ToLower().Contains(filter)))

                    //Adding paging
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)

                    .ToListAsync<IAddress>();
            }
        }
    }

    public async Task<IAddress> DeleteAddressAsync(loginUserSessionDto usr, Guid id)
    {
        //Notice cascade delete of firends living on the address and their pets
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            var _query1 = db.Addresses
                .Where(i => i.AddressId == id);

            var _item = await _query1.FirstOrDefaultAsync<csAddressDbM>();

            //If the item does not exists
            if (_item == null) throw new ArgumentException($"Item {id} is not existing");

            //delete in the database model
            db.Addresses.Remove(_item);

            //write to database in a UoW
            await db.SaveChangesAsync();
            return _item;
        }
    }

    public async Task<IAddress> UpdateAddressAsync(loginUserSessionDto usr, csAddressCUdto itemDto)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            var _query1 = db.Addresses
                .Where(i => i.AddressId == itemDto.AddressId);
            var _item = await _query1.FirstOrDefaultAsync<csAddressDbM>();

            //If the item does not exists
            if (_item == null) throw new ArgumentException($"Item {itemDto.AddressId} is not existing");

            //I cannot have duplicates in the Addresses table, so check that
            var _query2 = db.Addresses
             .Where(i => ((i.StreetAddress == itemDto.StreetAddress) &&
                (i.ZipCode == itemDto.ZipCode) &&
                (i.City == itemDto.City) &&
                (i.Country == itemDto.Country)));
            var _existingItem = await _query2.FirstOrDefaultAsync<csAddressDbM>();
            if (_existingItem != null && _existingItem.AddressId != itemDto.AddressId)
                throw new ArgumentException($"Item already exist with id {_existingItem.AddressId}");

            //transfer any changes from DTO to database objects
            //Update individual properties 
            _item.UpdateFromDTO(itemDto);

            //write to database model
            db.Addresses.Update(_item);

            //write to database in a UoW
            await db.SaveChangesAsync();
            return _item;
        }
    }

    public async Task<IAddress> CreateAddressAsync(loginUserSessionDto usr, csAddressCUdto itemDto)
    {
        if (itemDto.AddressId != null)
            throw new ArgumentException($"{nameof(itemDto.AddressId)} must be null when creating a new object");

        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            //I cannot have duplicates in the Addresses table, so check that
            var _query2 = db.Addresses
              .Where(i => ((i.StreetAddress == itemDto.StreetAddress) &&
                 (i.ZipCode == itemDto.ZipCode) &&
                 (i.City == itemDto.City) &&
                 (i.Country == itemDto.Country)));
            var _existingItem = await _query2.FirstOrDefaultAsync<csAddressDbM>();
            if (_existingItem != null)
                throw new ArgumentException($"Item already exist with id {_existingItem.AddressId}");

            //transfer any changes from DTO to database objects
            //Update individual properties 
            var _item = new csAddressDbM(itemDto);

            //write to database model
            db.Addresses.Add(_item);

            //write to database in a UoW
            await db.SaveChangesAsync();
            return _item;
        }
    }
    #endregion


    #region Quotes repo methods
    public async Task<IQuote> ReadQuoteAsync(loginUserSessionDto usr, Guid id, bool flat)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            if (!flat)
            {
                //make sure the model is fully populated, try without include.
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _query = db.Quotes.AsNoTracking().Include(i => i.FriendsDbM).ThenInclude(i => i.PetsDbM)
                    .Where(i => i.QuoteId == id);

                return await _query.FirstOrDefaultAsync<IQuote>();
            }
            else
            {
                //Not fully populated, compare the SQL Statements generated
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _query = db.Quotes.AsNoTracking()
                    .Where(i => i.QuoteId == id);

                return await _query.FirstOrDefaultAsync<IQuote>();
            }
        }
    }

    public async Task<List<IQuote>> ReadQuotesAsync(loginUserSessionDto usr, bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            filter ??= "";
            if (!flat)
            {
                //make sure the model is fully populated, try without include.
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _quotes = db.Quotes.AsNoTracking().Include(f => f.FriendsDbM);

                return await _quotes

                    //Adding filter functionality
                    .Where(i => i.Seeded == seeded
                            && (i.Quote.ToLower().Contains(filter) ||
                                i.Author.ToLower().Contains(filter)))

                    //Adding paging
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)

                   .ToListAsync<IQuote>();
            }
            else
            {
                //Not fully populated, compare the SQL Statements generated
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _quotes = db.Quotes.AsNoTracking();

                return await _quotes

                    //Adding filter functionality
                    .Where(i => i.Seeded == seeded
                            && (i.Quote.ToLower().Contains(filter) ||
                                i.Author.ToLower().Contains(filter)))

                    //Adding paging
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)

                    .ToListAsync<IQuote>();
            }
        }
    }

    public async Task<IQuote> DeleteQuoteAsync(loginUserSessionDto usr, Guid id)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            var _query1 = db.Quotes
                .Where(i => i.QuoteId == id);

            var _item = await _query1.FirstOrDefaultAsync<csQuoteDbM>();

            //If the item does not exists
            if (_item == null) throw new ArgumentException($"Item {id} is not existing");

            //delete in the database model
            db.Quotes.Remove(_item);

            //write to database in a UoW
            await db.SaveChangesAsync();
            return _item;
        }
    }

    public async Task<IQuote> UpdateQuoteAsync(loginUserSessionDto usr, csQuoteCUdto itemDto)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            var _query1 = db.Quotes
                .Where(i => i.QuoteId == itemDto.QuoteId);
            var _item = await _query1.FirstOrDefaultAsync<csQuoteDbM>();

            //If the item does not exists
            if (_item == null) throw new ArgumentException($"Item {itemDto.QuoteId} is not existing");

            //Avoid duplicates in the Quotes table, so check that
            var _query2 = db.Quotes
              .Where(i => ((i.Author == itemDto.Author) &&
                (i.Quote == itemDto.Quote)));
            var _existingItem = await _query2.FirstOrDefaultAsync<csQuoteDbM>();
            if (_existingItem != null && _existingItem.QuoteId != itemDto.QuoteId)
                throw new ArgumentException($"Item already exist with id {_existingItem.QuoteId}");


            //transfer any changes from DTO to database objects
            //Update individual properties 
            _item.UpdateFromDTO(itemDto);

            //write to database model
            db.Quotes.Update(_item);

            //write to database in a UoW
            await db.SaveChangesAsync();
            return _item;
        }
    }

    public async Task<IQuote> CreateQuoteAsync(loginUserSessionDto usr, csQuoteCUdto itemDto)
    {
        if (itemDto.QuoteId != null)
            throw new ArgumentException($"{nameof(itemDto.QuoteId)} must be null when creating a new object");

        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            //Avoid duplicates in the Quotes table, so check that
            var _query2 = db.Quotes
              .Where(i => ((i.Author == itemDto.Author) &&
                (i.Quote == itemDto.Quote)));
            var _existingItem = await _query2.FirstOrDefaultAsync<csQuoteDbM>();
            if (_existingItem != null)
                throw new ArgumentException($"Item already exist with id {_existingItem.QuoteId}");

            //transfer any changes from DTO to database objects
            //Update individual properties 
            var _item = new csQuoteDbM(itemDto);

            //write to database model
            db.Quotes.Add(_item);

            //write to database in a UoW
            await db.SaveChangesAsync();
            return _item;
        }
    }
    #endregion


    #region Pets repo methods
    public async Task<IPet> ReadPetAsync(loginUserSessionDto usr, Guid id, bool flat)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            if (!flat)
            {
                //make sure the model is fully populated, try without include.
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _query = db.Pets.AsNoTracking().Include(i => i.FriendDbM).ThenInclude(i => i.AddressDbM)
                    .Where(i => i.PetId == id);

                return await _query.FirstOrDefaultAsync<IPet>();
            }
            else
            {
                //Not fully populated, compare the SQL Statements generated
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _query = db.Pets.AsNoTracking()
                    .Where(i => i.PetId == id);

                return await _query.FirstOrDefaultAsync<IPet>();
            }
        }
    }

    public async Task<List<IPet>> ReadPetsAsync(loginUserSessionDto usr, bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            filter ??= "";
            if (!flat)
            {
                //make sure the model is fully populated, try without include.
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _pets = db.Pets.AsNoTracking().Include(f => f.FriendDbM);

                return await _pets

                    //Adding filter functionality
                    .Where(i => i.Seeded == seeded
                            && (i.Name.ToLower().Contains(filter) ||
                                i.strMood.ToLower().Contains(filter) ||
                                i.strKind.ToLower().Contains(filter)))

                    //Adding paging
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)

                    .ToListAsync<IPet>();
            }
            else
            {
                //Not fully populated, compare the SQL Statements generated
                //remove tracking for all read operations for performance and to avoid recursion/circular access
                var _pets = db.Pets.AsNoTracking();

                return await _pets

                    //Adding filter functionality
                    .Where(i => i.Seeded == seeded
                            && (i.Name.ToLower().Contains(filter) ||
                                i.strMood.ToLower().Contains(filter) ||
                                i.strKind.ToLower().Contains(filter)))

                    //Adding paging
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)

                    .ToListAsync<IPet>();
            }
        }
    }

    public async Task<IPet> DeletePetAsync(loginUserSessionDto usr, Guid id)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            var _query1 = db.Pets
                .Where(i => i.PetId == id);

            var _item = await _query1.FirstOrDefaultAsync<csPetDbM>();

            //If the item does not exists
            if (_item == null) throw new ArgumentException($"Item {id} is not existing");

            //delete in the database model
            db.Pets.Remove(_item);

            //write to database in a UoW
            await db.SaveChangesAsync();
            return _item;
        }
    }

    public async Task<IPet> UpdatePetAsync(loginUserSessionDto usr, csPetCUdto itemDto)
    {
        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            var _query1 = db.Pets
                .Where(i => i.PetId == itemDto.PetId);
            var _item = await _query1.Include(i => i.FriendDbM).FirstOrDefaultAsync<csPetDbM>();

            //If the item does not exists
            if (_item == null) throw new ArgumentException($"Item {itemDto.PetId} is not existing");

            //transfer any changes from DTO to database objects
            //Update individual properties 
            _item.UpdateFromDTO(itemDto);

            //Update navigation properties
            await csPetCUdto_to_csPetDbM(db, itemDto, _item);

            //write to database model
            db.Pets.Update(_item);

            //write to database in a UoW
            await db.SaveChangesAsync();
            return _item;
        }
    }

    public async Task<IPet> CreatePetAsync(loginUserSessionDto usr, csPetCUdto itemDto)
    {
        if (itemDto.PetId != null)
            throw new ArgumentException($"{nameof(itemDto.PetId)} must be null when creating a new object");

        using (var db = csMainDbContext.DbContext(_dblogin))
        {
            //transfer any changes from DTO to database objects
            //Update individual properties
            var _item = new csPetDbM(itemDto);

            //Update navigation properties
            await csPetCUdto_to_csPetDbM(db, itemDto, _item);

            //write to database model
            db.Pets.Add(_item);

            //write to database in a UoW
            await db.SaveChangesAsync();
            return _item;
        }
    }

    private static async Task csPetCUdto_to_csPetDbM(csMainDbContext db, csPetCUdto itemDtoSrc, csPetDbM _itemDst)
    {
        //update owner, i.e. navigation property FriendDbM
        var owner = await db.Friends.FirstOrDefaultAsync(
            a => (a.FriendId == itemDtoSrc.FriendId));

        if (owner == null)
            throw new ArgumentException($"Item id {itemDtoSrc.FriendId} not existing");

        _itemDst.FriendDbM = owner;
    }
    #endregion
}
