--[Master Table]

--�ϥΪ̰򥻸�Ƴ]�w
CREATE TABLE users(
	id NVARCHAR(50) PRIMARY KEY,
	password VARCHAR(50),
	name NVARCHAR(100),
	email NVARCHAR(100),
	createUser NVARCHAR(50),
	createTime DATETIME,
	updateUser NVARCHAR(50),
	updateTime DATETIME,
	isEnable BIT
);

--�v���s�ճ]�w
CREATE TABLE permissionGroup(
	groupId NVARCHAR(50) PRIMARY KEY,
	groupDesc NVARCHAR(100),
	isEnable BIT
);

--�����]�w
CREATE TABLE page(
	id INT PRIMARY KEY,
	title NVARCHAR(50),
	pageType NVARCHAR(10),
	pageFileloc NVARCHAR(200),
	isEnable BIT
);

--���������C
CREATE TABLE navBar(
	id INT PRIMARY KEY,
	title NVARCHAR(50),
	sortIdx INT,
	haveSubItem BIT,
	pageId INT NULL --FK:page.id
);

--���������C-�l����
CREATE TABLE navSubItem(
	id INT PRIMARY KEY,
	title NVARCHAR(50),
	mainNavId INT,
	sortIdx INT,
	pageId INT
);

--�ϥΪ̻P�v���s�����p�]�w
CREATE TABLE r_userGroup(
	userId NVARCHAR(50), --FK:users.id
	groupId NVARCHAR(50) --FK:permissionGroup.id
);

--�U���������i�ާ@�v���s�����p�]�w
CREATE TABLE r_pageGroup(
	pageId INT,   --FK:page.Id
	groupId NVARCHAR(300) --FK:permissionGroup.groupId
);

--[Transation Table]

--�n�J���_����
CREATE TABLE userLoginKey(
	loginkey NVARCHAR(30),
	userId NVARCHAR(50),
	expireTime DATETIME,
	loginTime DATETIME,
	logoutTime DATETIME,
);

--���~����
CREATE TABLE errorLog(
	errMessage NVARCHAR(1000),
	funcName NVARCHAR(100),
	errTime DATETIME,
);

CREATE INDEX idx_userLoginKey_userID ON userLoginKey (userId);

--[Init Data]

--�إߪ�l�b��
INSERT INTO users VALUES
('admin','123456','�t�κ޲z��','','admin',GETDATE(),'admin',GETDATE(),1),
('jim','jim','��','','admin',GETDATE(),'admin',GETDATE(),1),
('rex','rex','�p�J��','','admin',GETDATE(),'admin',GETDATE(),1),
('op','op','�@�~��','','admin',GETDATE(),'admin',GETDATE(),1)
;

--�إ��v���s��
INSERT INTO permissionGroup VALUES
('admin','�t�κ޲z��',1),
('engineer','�u�{�v',1),
('foreman','��Z',1),
('operator','�@�~��',1)
;

--�إ߭���
INSERT INTO page VALUES
(1,'����','Manual','Page/home.html',1),
(2,'�b���]�w','Auto','Page/userDef.html',1),
(3,'�ƥ�]�w','Auto','Page/materialDef.html',1),
(4,'�ܮw�]�w','Auto','Page/warehouseDef.html',1),
(5,'���O�]�w','Auto','Page/processDef.html',1),
(6,'�s�{�]�w','Auto','Page/routing.html',1),
(7,'�Ͳ��u�]�w','Auto','Page/productionLineDef.html',1),
(8,'�J�w�@�~','Manual','Page/inbound.html',1),
(9,'�u��޲z','Manual','Page/woMaintain.html',1),
(10,'�L��','Manual','Page/pass.html',1),
(11,'�w�s�d��','Manual','Page/inventoryQuery.html',1),
(12,'�u��d��','Manual','Page/woQuery.html',1),
(13,'����','Manual','Page/help.html',1)
;

--�إ߾����C
INSERT INTO navBar VALUES
(1,'����',1,0,1),
(2,'���x�@�~',2,1,null),
(3,'�s�{�@�~',3,1,null),
(4,'�d��',4,1,null),
(5,'�]�w',5,1,null),
(6,'����',6,0,13)
;
--�����C�l����
INSERT INTO navSubItem VALUES
(1,'�J�w',2,1,8),
(2,'�u��޲z',3,1,9),
(3,'�L��',3,2,10),
(4,'�w�s',4,1,11),
(5,'�u��',4,2,12),
(6,'�b��',5,1,2),
(7,'�ƥ�',5,2,3),
(8,'�ܮw',5,3,4),
(9,'���O',5,4,5),
(10,'�s�{',5,5,6),
(11,'�Ͳ��u',5,6,7)
;

--�]�w�ϥΪ̩����v���s��
INSERT INTO r_userGroup VALUES
('admin','admin'),
('jim','engineer'),
('rex','foreman'),
('op','operator')
;

--�]�w�U�������\�X�ݪ��v���s��
INSERT INTO r_pageGroup VALUES
(1,'admin|engineer|foreman|operator'),
(2,'admin'),
(3,'admin|engineer'),
(4,'admin'),
(5,'admin|engineer'),
(6,'admin|engineer'),
(7,'admin|engineer'),
(8,'admin|foreman|operator'),
(9,'admin|engineer|foreman'),
(10,'admin|foreman|operator'),
(11,'admin|engineer|foreman|operator'),
(12,'admin|engineer|foreman|operator'),
(13,'admin|engineer|foreman|operator')
;