DROP TABLE Users
CREATE TABLE Users
(
	ID UNIQUEIDENTIFIER,
	Name VARCHAR(40),
	Email VARCHAR(100),
	Password VARCHAR(100),
	Authenticated BIT,
	Access BIT,
	BankAccountID UNIQUEIDENTIFIER,
	BankID UNIQUEIDENTIFIER,
	BankAccountAmount DECIMAL
);
INSERT INTO Users(ID, Name, Email, Password, Authenticated, Access, BankAccountID, BankID, BankAccountAmount) VALUES 
('A08AB3E5-E3EC-47CD-84EF-C0EB75045A70', 'Admin','maximkirichenk0.06@gmail.com','erwkAsDWjzRNIZZrUawEwyd5z4r5ZGCbkTorVKuuhIw=', 1, 1, '216fbfbb-07a7-434e-9eff-fbeb1bd4e087', 'bed62930-9356-477a-bed5-b84d59336122', 1000)

DROP TABLE BankAccounts
CREATE TABLE BankAccounts
(
	ID UNIQUEIDENTIFIER,
	UserBankAccountID UNIQUEIDENTIFIER,
	BankID UNIQUEIDENTIFIER,
	BankAccountAmount DECIMAL
)
INSERT INTO BankAccounts(ID, UserBankAccountID, BankID, BankAccountAmount) VALUES
('216fbfbb-07a7-434e-9eff-fbeb1bd4e087', 'A08AB3E5-E3EC-47CD-84EF-C0EB75045A70', 'bed62930-9356-477a-bed5-b84d59336122', 1000)

DROP TABLE Banks
CREATE TABLE Banks
(
	ID UNIQUEIDENTIFIER,
	BankID UNIQUEIDENTIFIER,
	BankName VARCHAR(40),
	AccountAmount DECIMAL
)
INSERT INTO Banks(ID, BankID, BankName, AccountAmount) VALUES
('ae59b1df-089d-4823-a32a-41a44f878b4b', 'bed62930-9356-477a-bed5-b84d59336122', 'Tinkoff', 234523450),
('c2c4fc26-e503-4d48-8a24-ad9233e0e603', 'e4c18139-f2c8-4a4b-a8b8-cf0d230b37fa', 'SberBank', 1043200000),
('335ba509-2994-4068-9a50-f703490891ba', 'b56c8051-6eee-4441-a7de-7cb4789de362', 'PochtaBank', 100650000)

DROP TABLE Credits
CREATE TABLE Credits
(
	ID UNIQUEIDENTIFIER,
	BankID UNIQUEIDENTIFIER,
	UserBankAccountID UNIQUEIDENTIFIER,
	CreditAmount DECIMAL
)
INSERT INTO Credits(ID, BankID, UserBankAccountID, CreditAmount) VALUES
('ea94be7c-6598-43f8-8e8d-97c5fa90f29f', 'bed62930-9356-477a-bed5-b84d59336122', 'A08AB3E5-E3EC-47CD-84EF-C0EB75045A70', 1000)

DROP TABLE Operations
CREATE TABLE Operations
(
	ID UNIQUEIDENTIFIER,
	BankID UNIQUEIDENTIFIER,
	ReceiverID UNIQUEIDENTIFIER,
	SenderID UNIQUEIDENTIFIER,
	TransferAmount DECIMAL,
	OperationStatus INT,
	OperationKind INT
)
INSERT INTO Operations(ID, BankID, ReceiverID, SenderID, TransferAmount, OperationStatus, OperationKind) VALUES
('ae734776-9cb6-464e-9adf-638a04db8e0f', 'bed62930-9356-477a-bed5-b84d59336122', 'A08AB3E5-E3EC-47CD-84EF-C0EB75045A70', 'bed62930-9356-477a-bed5-b84d59336122', 120, 200, 1)

DROP TABLE Cards
CREATE TABLE Cards
(
	ID UNIQUEIDENTIFIER,
	BankId UNIQUEIDENTIFIER,
	BankAccountID UNIQUEIDENTIFIER,
	Amount DECIMAL,
	Expiration DATETIME,
	CardKind INT,
	CVV NVARCHAR(3),
	Age INT CHECK(Age > 14 OR Age < 99) 
)
INSERT INTO Cards(ID, BankID, BankAccountID, Amount, Expiration, CardKind, CVV, Age) VALUES
('e0c100db-4262-408a-a43a-c29763bb7147', 'bed62930-9356-477a-bed5-b84d59336122', '216fbfbb-07a7-434e-9eff-fbeb1bd4e087', 100, '07/12/2026', 0, '542', 16),
('0f9b577c-1de0-4931-8490-7cf5702e17b9', 'bed62930-9356-477a-bed5-b84d59336122', '216fbfbb-07a7-434e-9eff-fbeb1bd4e087', 200, '07/12/2026', 1, '753', 16),
('0be9a4e7-b6d3-422c-b38c-bb7235e54feb', 'bed62930-9356-477a-bed5-b84d59336122', '216fbfbb-07a7-434e-9eff-fbeb1bd4e087', 700, '07/12/2026', 2, '553', 16)