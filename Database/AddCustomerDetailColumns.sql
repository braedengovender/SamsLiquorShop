-- Sam's Liquor Shop: adds the extra customer detail columns used by Customer/Account.aspx
-- Run once against the GroupWst15 database (SSMS / Azure Data Studio).
-- Safe to re-run: a column is only added if it does not already exist.

IF COL_LENGTH('dbo.Customer', 'Customer_IDNumber')     IS NULL ALTER TABLE dbo.Customer ADD Customer_IDNumber     VARCHAR(13)  NULL;
IF COL_LENGTH('dbo.Customer', 'Customer_PhoneNumber')  IS NULL ALTER TABLE dbo.Customer ADD Customer_PhoneNumber  VARCHAR(15)  NULL;
IF COL_LENGTH('dbo.Customer', 'Customer_UnitNumber')   IS NULL ALTER TABLE dbo.Customer ADD Customer_UnitNumber   VARCHAR(20)  NULL;
IF COL_LENGTH('dbo.Customer', 'Customer_UnitName')     IS NULL ALTER TABLE dbo.Customer ADD Customer_UnitName     VARCHAR(100) NULL;
IF COL_LENGTH('dbo.Customer', 'Customer_StreetNumber') IS NULL ALTER TABLE dbo.Customer ADD Customer_StreetNumber VARCHAR(20)  NULL;
IF COL_LENGTH('dbo.Customer', 'Customer_StreetName')   IS NULL ALTER TABLE dbo.Customer ADD Customer_StreetName   VARCHAR(100) NULL;
IF COL_LENGTH('dbo.Customer', 'Customer_Suburb')       IS NULL ALTER TABLE dbo.Customer ADD Customer_Suburb       VARCHAR(100) NULL;
IF COL_LENGTH('dbo.Customer', 'Customer_City')         IS NULL ALTER TABLE dbo.Customer ADD Customer_City         VARCHAR(100) NULL;
IF COL_LENGTH('dbo.Customer', 'Customer_PostalCode')   IS NULL ALTER TABLE dbo.Customer ADD Customer_PostalCode   VARCHAR(4)   NULL;
IF COL_LENGTH('dbo.Customer', 'Customer_Province')     IS NULL ALTER TABLE dbo.Customer ADD Customer_Province     VARCHAR(30)  NULL;
GO

-- Check the result
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customer'
ORDER BY ORDINAL_POSITION;
