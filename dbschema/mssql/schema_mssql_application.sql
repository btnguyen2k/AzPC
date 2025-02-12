DROP TABLE IF EXISTS [azpc_apps];
CREATE TABLE [azpc_apps] (
    [app_id] nvarchar(64) NOT NULL,
    [display_name] nvarchar(128) NOT NULL,
    [public_key_pem] nvarchar(max) NULL,
    [created_at] datetimeoffset NOT NULL DEFAULT GETUTCDATE(),
    [updated_at] datetimeoffset NOT NULL DEFAULT GETUTCDATE(),
    [concurrency_stamp] nvarchar(48) NULL,
    CONSTRAINT [PK_azpc_apps] PRIMARY KEY ([app_id])
);
