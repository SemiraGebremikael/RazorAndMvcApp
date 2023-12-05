USE goodfriendsefc;
GO

--create a schema for guest users, i.e. not logged in
CREATE SCHEMA gstusr;
GO

--create a schema for logged in user
CREATE SCHEMA usr;
GO

--create a schema for logged in super user
--should already be created in lesson 05, when we did table annotations
--CREATE SCHEMA supusr;
--GO


/* used to cleanup if needed
DROP SCHEMA IF EXISTS gstusr;
DROP SCHEMA IF EXISTS usr;
DROP SCHEMA IF EXISTS supusr;
GO
*/