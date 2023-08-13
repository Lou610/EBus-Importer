
/****** Object:  Table [dbo].[Trans]    Script Date: 2015/08/21 8:36:46 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Trans](
	[id_Trans] [int] NOT NULL,
	[id_Stage] [int] NOT NULL,
	[id_Journey] [int] NOT NULL,
	[id_Duty] [int] NOT NULL,
	[id_Module] [int] NOT NULL,
	[str_LocationCode] [char](2) NULL,
	[int2_BoardingStageID] [smallint] NULL,
	[int2_AlightingStageID] [smallint] NULL,
	[int2_Class] [smallint] NULL,
	[int4_Revenue] [int] NULL,
	[int4_NonRevenue] [int] NULL,
	[int2_TicketCount] [smallint] NULL,
	[int2_PassCount] [smallint] NULL,
	[int2_Transfers] [smallint] NULL,
	[dat_TransDate] [datetime] NULL,
	[dat_TransTime] [datetime] NULL,
	[str_SerialNumber] [varchar](10) NULL,
	[int4_RevenueBal] [int] NULL,
	[int4_TripBal] [int] NULL,
	[int2_AnnulCount] [int] NULL,
	[int4_AnnulCash] [int] NULL,
	[id_SCTrans] [int] NULL,
 CONSTRAINT [PK_Trans] PRIMARY KEY CLUSTERED 
(
	[id_Trans] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[Trans]  WITH CHECK ADD  CONSTRAINT [FK_Trans_Stage] FOREIGN KEY([id_Stage])
REFERENCES [dbo].[Stage] ([id_Stage])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Trans] CHECK CONSTRAINT [FK_Trans_Stage]
GO


