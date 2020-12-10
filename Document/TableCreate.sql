--[Master Table]

--使用者基本資料設定
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

--權限群組設定
CREATE TABLE permissionGroup(
	groupId NVARCHAR(50) PRIMARY KEY,
	groupDesc NVARCHAR(100),
	isEnable BIT
);

--頁面設定
CREATE TABLE page(
	id INT PRIMARY KEY,
	title NVARCHAR(50),
	pageType NVARCHAR(10),
	pageFileloc NVARCHAR(200),
	isEnable BIT
);

--頁面導覽列
CREATE TABLE navBar(
	id INT PRIMARY KEY,
	title NVARCHAR(50),
	sortIdx INT,
	haveSubItem BIT,
	pageId INT NULL --FK:page.id
);

--頁面導覽列-子項目
CREATE TABLE navSubItem(
	id INT PRIMARY KEY,
	title NVARCHAR(50),
	mainNavId INT,
	sortIdx INT,
	pageId INT
);

--使用者與權限群組關聯設定
CREATE TABLE r_userGroup(
	userId NVARCHAR(50), --FK:users.id
	groupId NVARCHAR(50) --FK:permissionGroup.id
);

--各頁面對應可操作權限群組關聯設定
CREATE TABLE r_pageGroup(
	pageId INT,   --FK:page.Id
	groupId NVARCHAR(300) --FK:permissionGroup.groupId
);

--[Transation Table]

--登入金鑰紀錄
CREATE TABLE userLoginKey(
	loginkey NVARCHAR(30),
	userId NVARCHAR(50),
	expireTime DATETIME,
	loginTime DATETIME,
	logoutTime DATETIME,
);

--錯誤紀錄
CREATE TABLE errorLog(
	errMessage NVARCHAR(1000),
	funcName NVARCHAR(100),
	errTime DATETIME,
);

CREATE INDEX idx_userLoginKey_userID ON userLoginKey (userId);

--[Init Data]

--建立初始帳號
INSERT INTO users VALUES
('admin','123456','系統管理員','','admin',GETDATE(),'admin',GETDATE(),1),
('jim','jim','喬','','admin',GETDATE(),'admin',GETDATE(),1),
('rex','rex','雷克斯','','admin',GETDATE(),'admin',GETDATE(),1),
('op','op','作業員','','admin',GETDATE(),'admin',GETDATE(),1)
;

--建立權限群組
INSERT INTO permissionGroup VALUES
('admin','系統管理員',1),
('engineer','工程師',1),
('foreman','領班',1),
('operator','作業員',1)
;

--建立頁面
INSERT INTO page VALUES
(1,'首頁','Manual','Page/home.html',1),
(2,'帳號設定','Auto','Page/userDef.html',1),
(3,'料件設定','Auto','Page/materialDef.html',1),
(4,'倉庫設定','Auto','Page/warehouseDef.html',1),
(5,'站別設定','Auto','Page/processDef.html',1),
(6,'製程設定','Auto','Page/routing.html',1),
(7,'生產線設定','Auto','Page/productionLineDef.html',1),
(8,'入庫作業','Manual','Page/inbound.html',1),
(9,'工單管理','Manual','Page/woMaintain.html',1),
(10,'過站','Manual','Page/pass.html',1),
(11,'庫存查詢','Manual','Page/inventoryQuery.html',1),
(12,'工單查詢','Manual','Page/woQuery.html',1),
(13,'說明','Manual','Page/help.html',1)
;

--建立導覽列
INSERT INTO navBar VALUES
(1,'首頁',1,0,1),
(2,'倉儲作業',2,1,null),
(3,'製程作業',3,1,null),
(4,'查詢',4,1,null),
(5,'設定',5,1,null),
(6,'說明',6,0,13)
;
--導覽列子項目
INSERT INTO navSubItem VALUES
(1,'入庫',2,1,8),
(2,'工單管理',3,1,9),
(3,'過站',3,2,10),
(4,'庫存',4,1,11),
(5,'工單',4,2,12),
(6,'帳號',5,1,2),
(7,'料件',5,2,3),
(8,'倉庫',5,3,4),
(9,'站別',5,4,5),
(10,'製程',5,5,6),
(11,'生產線',5,6,7)
;

--設定使用者所屬權限群組
INSERT INTO r_userGroup VALUES
('admin','admin'),
('jim','engineer'),
('rex','foreman'),
('op','operator')
;

--設定各頁面允許訪問的權限群組
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