-- Sam's Liquor Shop: stores each customer's wishlist (used by Customer/Wishlist.aspx and the Shop hearts)
-- Run once against the GroupWst15 database. Safe to re-run.

IF OBJECT_ID('dbo.Wishlist', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Wishlist
    (
        Wishlist_ID        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Wishlist PRIMARY KEY,
        Customer_ID        INT               NOT NULL CONSTRAINT FK_Wishlist_Customer REFERENCES dbo.Customer (Customer_ID),
        Product_ID         INT               NOT NULL CONSTRAINT FK_Wishlist_Product  REFERENCES dbo.Product  (Product_ID),
        Wishlist_DateAdded DATETIME          NOT NULL CONSTRAINT DF_Wishlist_DateAdded DEFAULT (GETDATE()),
        CONSTRAINT UQ_Wishlist_Customer_Product UNIQUE (Customer_ID, Product_ID)
    );
END
GO

SELECT * FROM dbo.Wishlist;
