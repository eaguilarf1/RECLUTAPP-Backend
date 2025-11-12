using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddUserPasswordAndIsActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET NOCOUNT ON;

                -- Crear IsActive si no existe (WITH VALUES para rellenar filas existentes)
                IF COL_LENGTH('dbo.Users','IsActive') IS NULL
                BEGIN
                  EXEC(N'ALTER TABLE [dbo].[Users] ADD [IsActive] bit NOT NULL CONSTRAINT [DF_Users_IsActive] DEFAULT(1) WITH VALUES;');
                END;

                -- Asegurar que todos queden activos (dinámico para evitar comprobación anticipada)
                IF COL_LENGTH('dbo.Users','IsActive') IS NOT NULL
                BEGIN
                  EXEC(N'UPDATE [dbo].[Users] SET [IsActive] = 1 WHERE [IsActive] = 0;');
                END;

                -- Crear PasswordHash si no existe
                IF COL_LENGTH('dbo.Users','PasswordHash') IS NULL
                BEGIN
                  EXEC(N'ALTER TABLE [dbo].[Users] ADD [PasswordHash] nvarchar(200) NOT NULL CONSTRAINT [DF_Users_PasswordHash] DEFAULT(N'''');');
                END;

                -- Normalizar y asegurar NOT NULL
                IF COL_LENGTH('dbo.Users','PasswordHash') IS NOT NULL
                BEGIN
                  EXEC(N'UPDATE [dbo].[Users] SET [PasswordHash] = ISNULL([PasswordHash], N'''');');
                  EXEC(N'ALTER TABLE [dbo].[Users] ALTER COLUMN [PasswordHash] nvarchar(200) NOT NULL;');
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET NOCOUNT ON;

                -- Eliminar PasswordHash (quitando default primero si existe)
                IF COL_LENGTH('dbo.Users','PasswordHash') IS NOT NULL
                BEGIN
                  DECLARE @dfPwd nvarchar(128), @sql nvarchar(max);
                  SELECT @dfPwd = d.name
                    FROM sys.default_constraints d
                    JOIN sys.columns c ON c.default_object_id = d.object_id
                    JOIN sys.tables t ON t.object_id = c.object_id
                    JOIN sys.schemas s ON s.schema_id = t.schema_id
                   WHERE t.name = 'Users' AND s.name = 'dbo' AND c.name = 'PasswordHash';
                  IF @dfPwd IS NOT NULL
                  BEGIN
                    SET @sql = N'ALTER TABLE [dbo].[Users] DROP CONSTRAINT [' + @dfPwd + N']';
                    EXEC(@sql);
                  END
                  EXEC(N'ALTER TABLE [dbo].[Users] DROP COLUMN [PasswordHash];');
                END;

                -- Eliminar IsActive (quitando default primero si existe)
                IF COL_LENGTH('dbo.Users','IsActive') IS NOT NULL
                BEGIN
                  DECLARE @dfIsActive nvarchar(128), @sql2 nvarchar(max);
                  SELECT @dfIsActive = d.name
                    FROM sys.default_constraints d
                    JOIN sys.columns c ON c.default_object_id = d.object_id
                    JOIN sys.tables t ON t.object_id = c.object_id
                    JOIN sys.schemas s ON s.schema_id = t.schema_id
                   WHERE t.name = 'Users' AND s.name = 'dbo' AND c.name = 'IsActive';
                  IF @dfIsActive IS NOT NULL
                  BEGIN
                    SET @sql2 = N'ALTER TABLE [dbo].[Users] DROP CONSTRAINT [' + @dfIsActive + N']';
                    EXEC(@sql2);
                  END
                  EXEC(N'ALTER TABLE [dbo].[Users] DROP COLUMN [IsActive];');
                END;
            ");
        }
    }
}
