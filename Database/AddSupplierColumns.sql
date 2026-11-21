-- Sam's Liquor Shop: contact, address and status columns used by Manager/Suppliers.aspx
-- Run once against the GroupWst15 database (SSMS / Azure Data Studio).
-- Safe to re-run: a column is only added when the table has no column for that detail yet
-- (e.g. if Supplier_Phone already exists, Supplier_PhoneNumber is NOT added). Existing data is not touched.

-- Contact details
IF COL_LENGTH('dbo.Supplier', 'Supplier_ContactPerson') IS NULL AND COL_LENGTH('dbo.Supplier', 'Supplier_ContactName') IS NULL
   AND COL_LENGTH('dbo.Supplier', 'Supplier_Contact') IS NULL
    ALTER TABLE dbo.Supplier ADD Supplier_ContactPerson VARCHAR(100) NULL;

IF COL_LENGTH('dbo.Supplier', 'Supplier_PhoneNumber') IS NULL AND COL_LENGTH('dbo.Supplier', 'Supplier_Phone') IS NULL
   AND COL_LENGTH('dbo.Supplier', 'Supplier_ContactNumber') IS NULL AND COL_LENGTH('dbo.Supplier', 'Supplier_TelephoneNumber') IS NULL
   AND COL_LENGTH('dbo.Supplier', 'Supplier_Telephone') IS NULL AND COL_LENGTH('dbo.Supplier', 'Supplier_CellNumber') IS NULL
   AND COL_LENGTH('dbo.Supplier', 'Supplier_Tel') IS NULL
    ALTER TABLE dbo.Supplier ADD Supplier_PhoneNumber VARCHAR(15) NULL;

IF COL_LENGTH('dbo.Supplier', 'Supplier_EmailAddress') IS NULL AND COL_LENGTH('dbo.Supplier', 'Supplier_Email') IS NULL
    ALTER TABLE dbo.Supplier ADD Supplier_EmailAddress VARCHAR(100) NULL;

-- Address (same fields as the customer address on the Register page)
IF COL_LENGTH('dbo.Supplier', 'Supplier_UnitNumber')   IS NULL ALTER TABLE dbo.Supplier ADD Supplier_UnitNumber   VARCHAR(20)  NULL;
IF COL_LENGTH('dbo.Supplier', 'Supplier_UnitName')     IS NULL ALTER TABLE dbo.Supplier ADD Supplier_UnitName     VARCHAR(100) NULL;
IF COL_LENGTH('dbo.Supplier', 'Supplier_StreetNumber') IS NULL ALTER TABLE dbo.Supplier ADD Supplier_StreetNumber VARCHAR(20)  NULL;
IF COL_LENGTH('dbo.Supplier', 'Supplier_StreetName')   IS NULL ALTER TABLE dbo.Supplier ADD Supplier_StreetName   VARCHAR(100) NULL;
IF COL_LENGTH('dbo.Supplier', 'Supplier_Suburb')       IS NULL ALTER TABLE dbo.Supplier ADD Supplier_Suburb       VARCHAR(100) NULL;
IF COL_LENGTH('dbo.Supplier', 'Supplier_City')         IS NULL ALTER TABLE dbo.Supplier ADD Supplier_City         VARCHAR(100) NULL;

IF COL_LENGTH('dbo.Supplier', 'Supplier_PostalCode') IS NULL AND COL_LENGTH('dbo.Supplier', 'Supplier_PostCode') IS NULL
   AND COL_LENGTH('dbo.Supplier', 'Supplier_ZipCode') IS NULL
    ALTER TABLE dbo.Supplier ADD Supplier_PostalCode VARCHAR(4) NULL;

IF COL_LENGTH('dbo.Supplier', 'Supplier_Province')     IS NULL ALTER TABLE dbo.Supplier ADD Supplier_Province     VARCHAR(30)  NULL;

-- Status (Active / Inactive). Existing suppliers start as Active.
IF COL_LENGTH('dbo.Supplier', 'Supplier_Status') IS NULL
    ALTER TABLE dbo.Supplier ADD Supplier_Status VARCHAR(10) NOT NULL CONSTRAINT DF_Supplier_Status DEFAULT ('Active');
GO

-- Check the result
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Supplier'
ORDER BY ORDINAL_POSITION;
