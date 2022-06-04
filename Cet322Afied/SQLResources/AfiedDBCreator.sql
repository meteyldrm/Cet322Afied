create database AfiedDB_322

use AfiedDB_322

create table TblUser(
    userID int primary key identity (1,1),
    userName nvarchar(63) not null,
    userPhone varchar(15) not null,
    userEmail varchar(127) not null,
    userPasswordHash char(256) not null
)

create table TblCustomerUser(
    customerID int primary key,
    customerAddress nvarchar(127) not null
)

create table TblManagerUser(
    managerID int primary key,
    managerAuthorizationLevel int not null --0: admin, 1: manager

)

create table TblProductCategory(
    categoryID int primary key identity (1,1),
    categoryName nvarchar(63) not null
)

create table TblProduct(
    productID int primary key identity (1,1),
    productName nvarchar(63) not null,
    productCategory int not null,
    productPrice decimal not null,
	productMeasurementUnit nvarchar(15) default 'count'
)

create table TblOrder(
    orderID bigint primary key identity (1,1),
    orderCustomerID int not null,
    orderDate datetime
)

create table TblProductOrder(
    orderID bigint not null,
    productID int not null,
    quantity decimal not null,
    price decimal not null, --Price can change over time
    constraint PK_ProductOrder primary key (orderID, productID) --Many to many cart linking
)

--AlterCustomerUser
alter table TblCustomerUser
add constraint FK_CustomerUser_ID foreign key (customerID) references TblUser(userID)

--AlterManagerUser
alter table TblManagerUser
add constraint FK_ManagerUser_ID foreign key (managerID) references TblUser(userID)

--AlterProduct
alter table TblProduct
add constraint FK_Product_Category foreign key (productCategory) references TblProductCategory (categoryID)

--AlterOrder
alter table TblOrder
add constraint FK_Order_CustomerID foreign key (orderCustomerID) references TblCustomerUser (customerID)

--AlterProductOrder
alter table TblProductOrder
add constraint FK_ProductOrder_Order foreign key (orderID) references TblOrder (orderID)

alter table TblProductOrder
add constraint FK_ProductOrder_Product foreign key (productID) references TblProduct (productID)

