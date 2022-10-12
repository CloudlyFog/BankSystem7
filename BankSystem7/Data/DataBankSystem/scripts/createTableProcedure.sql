CREATE PROCEDURE createTableCredits
AS

-- Create a new table called '[Credits]' in schema '[dbo]'
-- Drop the table if it already exists
IF OBJECT_ID('[dbo].[Credits]', 'U') IS NOT NULL
DROP TABLE [dbo].[Credits]
GO
-- Create the table in the specified schema
CREATE TABLE [dbo].[Credits]
(
    [ID] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, -- Primary Key column
    [BankID] UNIQUEIDENTIFIER NOT NULL,
    [UserBankAccountID] UNIQUEIDENTIFIER NOT NULL,
    [CreditAmount] DECIMAL NOT NULL
    -- Specify more columns here
);
GO

EXEC createTableCredits