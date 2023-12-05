USE goodfriendsefc;
GO

--create a view that gives overview of the database content
CREATE OR ALTER VIEW gstusr.vwInfoDb AS
    SELECT (SELECT COUNT(*) FROM supusr.Friends WHERE Seeded = 1) as nrSeededFriends, 
        (SELECT COUNT(*) FROM supusr.Friends WHERE Seeded = 0) as nrUnseededFriends,
        (SELECT COUNT(*) FROM supusr.Friends WHERE AddressId IS NOT NULL) as nrFriendsWithAddress,
        (SELECT COUNT(*) FROM supusr.Addresses WHERE Seeded = 1) as nrSeededAddresses, 
        (SELECT COUNT(*) FROM supusr.Addresses WHERE Seeded = 0) as nrUnseededAddresses,
        (SELECT COUNT(*) FROM supusr.Pets WHERE Seeded = 1) as nrSeededPets, 
        (SELECT COUNT(*) FROM supusr.Pets WHERE Seeded = 0) as nrUnseededPets,
        (SELECT COUNT(*) FROM supusr.Quotes WHERE Seeded = 1) as nrSeededQuotes, 
        (SELECT COUNT(*) FROM supusr.Quotes WHERE Seeded = 0) as nrUnseededQuotes;

GO

CREATE OR ALTER VIEW gstusr.vwInfoFriends AS
    SELECT a.Country, a.City, COUNT(*) as NrFriends  FROM supusr.Friends f
    INNER JOIN supusr.Addresses a ON f.AddressId = a.AddressId
    GROUP BY a.Country, a.City WITH ROLLUP;
GO

CREATE OR ALTER VIEW gstusr.vwInfoPets AS
    SELECT a.Country, a.City, COUNT(p.PetId) as NrPets FROM supusr.Friends f
    INNER JOIN supusr.Addresses a ON f.AddressId = a.AddressId
    INNER JOIN supusr.Pets p ON p.FriendId = f.FriendId
    GROUP BY a.Country, a.City WITH ROLLUP;
GO

CREATE OR ALTER VIEW gstusr.vwInfoQuotes AS
    SELECT Author, COUNT(Quote) as NrQuotes FROM supusr.Quotes 
    GROUp BY Author;
GO

/* used to test run the views
SELECT * FROM gstusr.vwInfoDb;
SELECT * FROM gstusr.vwInfoFriends;
SELECT * FROM gstusr.vwInfoPets;
SELECT * FROM gstusr.vwInfoQuotes;
*/

/* used to cleanup if needed
DROP VIEW IF EXISTS [gstusr].[vwInfoDb]
DROP VIEW IF EXISTS [gstusr].[vwInfoFriends]
DROP VIEW IF EXISTS [gstusr].[vwInfoPets]
DROP VIEW IF EXISTS [gstusr].[vwInfoQuotes]
GO
*/