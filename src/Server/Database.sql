﻿CREATE TABLE [dbo].[Results](
	[DeviceID] [nvarchar](50) NOT NULL,
	[PuzzleID] [nvarchar](50) NOT NULL,
	[Score] [int] NOT NULL,
	[Seconds] [int] NOT NULL,
	[IsWin] [bit] NOT NULL,
	[Rank] [int] NULL,
	[TimeStamp] [datetime] NOT NULL,
	[Results] [nvarchar](max) NOT NULL
) ON [PRIMARY]
GO

CREATE CLUSTERED INDEX [IX_Results_PuzzleID] ON [dbo].[Results]
(
	[PuzzleID] ASC,
	[IsWin] DESC,
	[Score] ASC,
	[Seconds] ASC
)
GO

CREATE NONCLUSTERED INDEX [IX_Results_DeviceID_TimeStamp] ON [dbo].[Results]
(
	[DeviceID] ASC,
	[TimeStamp] ASC
)
GO




CREATE TABLE [dbo].[Puzzles](
	[PuzzleID] [nvarchar](50) NOT NULL PRIMARY KEY NONCLUSTERED,
	[PlayDate] [datetime] NOT NULL,
	[Phrase] [nvarchar](256) NOT NULL,
	[UserCount] [int] NULL
) ON [PRIMARY]
GO
CREATE CLUSTERED INDEX [IX_Results_PuzzleID] ON [dbo].[Puzzles]
(
	[PlayDate] ASC
)
GO
