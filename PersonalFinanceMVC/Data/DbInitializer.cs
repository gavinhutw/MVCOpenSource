using Microsoft.EntityFrameworkCore;

namespace PersonalFinanceMVC.Data;

/// <summary>
/// 對已存在的資料庫補建缺少的資料表。
/// EnsureCreated() 只在資料庫不存在時才建立所有資料表；
/// 若資料庫已存在（含舊有資料表），新增的 Model 對應資料表不會被建立，
/// 此類別以 IF NOT EXISTS 條件 DDL 解決這個問題。
/// </summary>
public static class DbInitializer
{
    public static void EnsureTablesExist(AppDbContext db)
    {
        // ── AppUsers ──────────────────────────────────────────────
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (
                SELECT 1 FROM sys.objects
                WHERE object_id = OBJECT_ID(N'[dbo].[AppUsers]') AND type = N'U'
            )
            BEGIN
                CREATE TABLE [dbo].[AppUsers] (
                    [Id]                INT            IDENTITY(1,1) NOT NULL,
                    [Username]          NVARCHAR(50)   NOT NULL,
                    [PasswordHash]      NVARCHAR(MAX)  NOT NULL,
                    [DisplayName]       NVARCHAR(100)  NOT NULL,
                    [Email]             NVARCHAR(200)  NULL,
                    [IsAdmin]           BIT            NOT NULL DEFAULT 0,
                    [IsActive]          BIT            NOT NULL DEFAULT 1,
                    [PasswordChangedAt] DATETIME2      NOT NULL DEFAULT GETDATE(),
                    [CreatedAt]         DATETIME2      NOT NULL DEFAULT GETDATE(),
                    [LastLoginAt]       DATETIME2      NULL,
                    CONSTRAINT [PK_AppUsers] PRIMARY KEY ([Id])
                );

                CREATE UNIQUE INDEX [IX_AppUsers_Username]
                    ON [dbo].[AppUsers] ([Username]);
            END
        ");

        // ── AdvancePayment ────────────────────────────────────────
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (
                SELECT 1 FROM sys.objects
                WHERE object_id = OBJECT_ID(N'[dbo].[AdvancePayments]') AND type = N'U'
            )
            BEGIN
                CREATE TABLE [dbo].[AdvancePayments] (
                    [SerialNo]      INT           IDENTITY(1,1) NOT NULL,
                    [Categories_Id] INT           NOT NULL,
                    [Name]          NVARCHAR(100) NOT NULL,
                    [MonthCount]    INT           NOT NULL DEFAULT 1,
                    [Amount]        INT           NOT NULL DEFAULT 0,
                    CONSTRAINT [PK_AdvancePayments] PRIMARY KEY ([SerialNo]),
                    CONSTRAINT [FK_AdvancePayment_Categories]
                        FOREIGN KEY ([Categories_Id])
                        REFERENCES [dbo].[Categories] ([Id])
                        ON DELETE NO ACTION
                );
            END
        ");

        // ── Categories.Budget 欄位（對已存在的資料表補欄位）────────
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (
                SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND name = N'Budget'
            )
            BEGIN
                ALTER TABLE [dbo].[Categories]
                    ADD [Budget] INT NOT NULL DEFAULT 0;
            END
        ");

        // ── LoginLogs ─────────────────────────────────────────────
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (
                SELECT 1 FROM sys.objects
                WHERE object_id = OBJECT_ID(N'[dbo].[LoginLogs]') AND type = N'U'
            )
            BEGIN
                CREATE TABLE [dbo].[LoginLogs] (
                    [Id]         INT            IDENTITY(1,1) NOT NULL,
                    [Username]   NVARCHAR(50)   NOT NULL,
                    [IpAddress]  NVARCHAR(100)  NOT NULL,
                    [LoginAt]    DATETIME2      NOT NULL DEFAULT GETDATE(),
                    [IsSuccess]  BIT            NOT NULL DEFAULT 0,
                    [FailReason] NVARCHAR(200)  NULL,
                    [UserId]     INT            NULL,
                    CONSTRAINT [PK_LoginLogs] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_LoginLogs_AppUsers_UserId]
                        FOREIGN KEY ([UserId])
                        REFERENCES [dbo].[AppUsers] ([Id])
                        ON DELETE SET NULL
                );

                CREATE INDEX [IX_LoginLogs_UserId]
                    ON [dbo].[LoginLogs] ([UserId]);
            END
        ");
    }
}
