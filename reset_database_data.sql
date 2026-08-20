-- ================================================
-- RESET DATABASE DATA - Xóa toàn bộ dữ liệu, giữ nguyên cấu trúc bảng
-- Chạy script này để đặt lại dữ liệu cho lần khởi chạy đầu tiên
-- ================================================
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Disable all foreign key constraints and triggers
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
EXEC sp_MSforeachtable 'ALTER TABLE ? DISABLE TRIGGER ALL';
GO

-- Xóa dữ liệu từ tất cả các bảng - tự động xóa mọi bảng có dữ liệu
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql = @sql + N'DELETE FROM [' + s.name + N'].[' + t.name + N']; '
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE t.is_ms_shipped = 0
  AND t.name NOT IN ('__EFMigrationsHistory');
EXEC sp_executesql @sql;
GO

-- Enable back foreign key constraints and triggers
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
EXEC sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL';
GO

PRINT 'Database data reset completed. All data cleared, tables remain intact.';
GO