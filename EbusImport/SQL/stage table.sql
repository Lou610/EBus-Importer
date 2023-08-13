
/****** Object:  Table [dbo].[Stage]    Script Date: 2015/08/21 8:36:04 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Stage](
	[id_Stage] [int] NOT NULL,
	[id_Journey] [int] NOT NULL,
	[id_Duty] [int] NOT NULL,
	[id_Module] [int] NOT NULL,
	[int2_StageID] [smallint] NULL,
	[dat_StageDate] [datetime] NULL,
	[dat_StageTime] [datetime] NULL,
	[dat_RecordMod] [datetime] NULL,
	[id_BatchNo] [int] NULL,
 CONSTRAINT [PK_Stage] PRIMARY KEY CLUSTERED 
(
	[id_Stage] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Stage]  WITH CHECK ADD  CONSTRAINT [FK_Stage_Journey] FOREIGN KEY([id_Journey])
REFERENCES [dbo].[Journey] ([id_Journey])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Stage] CHECK CONSTRAINT [FK_Stage_Journey]
GO


