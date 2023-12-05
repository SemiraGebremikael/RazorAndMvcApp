use goodfriendsefc;
GO

CREATE OR ALTER PROC supusr.spDeleteAll
    @Seeded BIT = 1,

    @nrFriendsAffected INT OUTPUT,
    @nrAddressesAffected INT OUTPUT,
    @nrPetsAffected INT OUTPUT,
    @nrQuotesAffected INT OUTPUT
    
    AS

    SET NOCOUNT ON;

    SELECT  @nrFriendsAffected = COUNT(*) FROM supusr.Friends WHERE Seeded = @Seeded;
    SELECT  @nrAddressesAffected = COUNT(*) FROM supusr.Addresses WHERE Seeded = @Seeded;
    SELECT  @nrPetsAffected = COUNT(*) FROM supusr.Pets WHERE Seeded = @Seeded;
    SELECT  @nrQuotesAffected = COUNT(*) FROM supusr.Quotes WHERE Seeded = @Seeded;

    DELETE FROM supusr.Friends WHERE Seeded = @Seeded;
    DELETE FROM supusr.Addresses WHERE Seeded = @Seeded;
    DELETE FROM supusr.Pets WHERE Seeded = @Seeded;
    DELETE FROM supusr.Quotes WHERE Seeded = @Seeded;

    SELECT * FROM gstusr.vwInfoDb;

    --throw our own error
    --;THROW 999999, 'my own supusr.spDeleteAll Error directly from SQL Server', 1

    --show return code usage
    RETURN 0;  --indicating success
    --RETURN 1;  --indicating your own error code, in this case 1
GO


/* testing the sp
DECLARE @retVal  INT 
DECLARE @nrF  INT 
DECLARE @nrA  INT 
DECLARE @nrP  INT 
DECLARE @nrQ  INT 
EXEC  @retVal = supusr.spDeleteAll 1, @nrF OUTPUT, @nrA OUTPUT, @nrP  OUTPUT, @nrQ OUTPUT

PRINT @retVal
PRINT @nrF
PRINT @nrA
PRINT @nrP
PRINT @nrQ
*/

/* to cleanup if needed
DROP PROCEDURE IF EXISTS [supusr].[spDeleteAll]
*/