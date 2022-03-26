CREATE TABLE [dbo].[Results](
	[DeviceID] [nvarchar](50) NOT NULL,
	[PuzzleID] [nvarchar](50) NOT NULL,
	[Score] [int] NOT NULL,
	[Rank] [int] NULL,
	[TimeStamp] [datetime] NOT NULL,
	[IsWin] [bit] NOT NULL,
	[Results] [nvarchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [IX_Results_PuzzleID] ON [dbo].[Results]
(
	[PuzzleID] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_Results_DeviceID_PuzzleID] ON [dbo].[Results]
(
	[DeviceID] ASC,
	[PuzzleID] ASC
)
GO

