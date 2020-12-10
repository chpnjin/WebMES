--[導覽列生成查詢]
--指定權限群組
DECLARE @groupId NVARCHAR(300);
SET @groupId = '%admin%';　--admin|engineer|foreman|operator

--取得該權限群組所有可訪問頁面
SELECT A.pageId,B.mainNavId
INTO #canAccessPage
FROM r_pageGroup A
LEFT JOIN navSubItem B ON B.pageId = A.pageId
WHERE groupId LIKE @groupId
;

--SELECT * FROM #canAccessPage;

--權限群組可操作頁面查詢(第一層)
(
	--無子項目直接連結
	SELECT A.sortIdx,A.id,A.title,B.pageFileloc
	FROM navBar A
	INNER JOIN page B ON B.id = A.pageId
	WHERE 
	B.id IN
	(
		SELECT pageId FROM #canAccessPage
	)
	UNION
	--有子項目大標題
	SELECT A.sortIdx,A.id,A.title,null AS pageFileloc
	FROM navBar A
	WHERE A.haveSubItem = 1
	AND id IN
	(
		SELECT DISTINCT mainNavId FROM #canAccessPage
	)
) ORDER BY sortIdx
;

--權限群組可操作頁面(第二層)
SELECT A.mainNavId,A.sortIdx,A.title,B.pageFileloc
FROM navSubItem A
INNER JOIN page B ON B.id = A.pageId
WHERE 
B.id IN
(
	SELECT pageId FROM #canAccessPage
)
ORDER BY A.mainNavId,A.sortIdx
;

DROP TABLE #canAccessPage;