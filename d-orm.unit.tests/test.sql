
use master
GO

-- KILL ALL PROCESS THAT MIGHT BE USING THE DB
DECLARE @testDB nvarchar(128) = 'test_db';
DECLARE @SQL varchar(max);

SELECT @SQL =  COALESCE(@SQL,'') + 'Kill ' + Convert(varchar, SPId) + ';'
FROM MASTER..SysProcesses
WHERE DBId = DB_ID(@testDB) AND SPId <> @@SPId;

EXECUTE(@SQL);

-- Another way to check fir DB existence
-- IF (NOT EXISTS (SELECT * FROM master.dbo.sysdatabases WHERE [name] = @testDB))

-- DROP DB IF EXIST
IF DB_ID('test_db') IS NOT NULL
BEGIN
	DROP DATABASE test_db;
END
	
-- CREATE DB
CREATE DATABASE test_db

GO

USE test_db

GO

-- DROP TABLE IF EXISTS
IF OBJECT_ID('person', 'U') IS NOT NULL
BEGIN
	DROP TABLE person
END

-- CREATE TABLE
CREATE TABLE person (
	[id] [int] IDENTITY(1,1) NOT NULL,
	[fname] [nvarchar](50) NULL,
	[lname] [nvarchar](50) NULL,
	[gender] [nvarchar](1) NULL,
	[age] [int] NULL,

	CONSTRAINT [PK_person] PRIMARY KEY CLUSTERED ( [id] ASC )
)

GO

INSERT person ( [fname], [lname], [gender], [age]) VALUES ( N'Paul', N'Jacobs', N'M', 3)
INSERT person ( [fname], [lname], [gender], [age]) VALUES ( N'John', N'Doe', N'F', 44)
INSERT person ( [fname], [lname], [gender], [age]) VALUES ( N'Lynda', N'Jacobs', N'F', 65)
INSERT person ( [fname], [lname], [gender], [age]) VALUES ( N'Paula', N'Adams', N'F', 16)
INSERT person ( [fname], [lname], [gender], [age]) VALUES ( N'Jenna', N'Pink', N'F', 59)
INSERT person ( [fname], [lname], [gender], [age]) VALUES ( N'Mama', N'Dook', N'F', 12)
INSERT person ( [fname], [lname], [gender], [age]) VALUES ( N'John', N'Obo', N'M', 9)
INSERT person ( [fname], [lname], [gender], [age]) VALUES ( N'Gerald', N'Mandingo', N'M', 34)
INSERT person ( [fname], [lname], [gender], [age]) VALUES ( N'Jeff', N'Kilode', N'M', 18)

GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sir Yes Sir
-- Create date: 6/23/2016
-- Description:	Sample stored procedure for testing
-- =============================================
CREATE PROCEDURE select_all 
	-- Add the parameters for the stored procedure here
	@param1 nvarchar(255) = 'hello'
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT TOP 1000 * FROM [test_db].[dbo].[person]
END
GO
