USE [EbusImporter]
GO

/****** Object:  Table [dbo].[Duty]    Script Date: 2015/08/19 3:12:35 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Duty](
	[id_Duty] [int] IDENTITY(1,1) NOT NULL,
	[id_Module] [int] NOT NULL,
	[int4_DutyID] [int] NOT NULL,
	[int4_OperatorID] [int] NULL,
	[str_ETMID] [char](6) NULL,
	[int4_GTValue] [int] NULL,
	[int4_NextTicketNumber] [int] NULL,
	[int4_DutySeqNum] [int] NULL,
	[dat_DutyStartDate] [datetime] NULL,
	[dat_DutyStartTime] [datetime] NULL,
	[dat_DutyStopDate] [datetime] NULL,
	[dat_DutyStopTime] [datetime] NULL,
	[dat_TrafficDate] [datetime] NULL,
	[str_BusID] [char](8) NULL,
	[int4_DutyRevenue] [int] NULL,
	[int4_DutyTickets] [int] NULL,
	[int4_DutyPasses] [int] NULL,
	[int4_DutyNonRevenue] [int] NULL,
	[int4_DutyTransfer] [int] NULL,
	[str_FirstRouteID] [char](4) NULL,
	[int2_FirstJourneyID] [smallint] NULL,
	[dat_RecordMod] [datetime] NULL,
	[id_BatchNo] [int] NULL,
	[byt_IeType] [tinyint] NULL,
	[str_EpromVersion] [varchar](8) NULL,
	[str_OperatorVersion] [varchar](8) NULL,
	[str_SpecialVersion] [varchar](8) NULL,
	[int4_DutyAnnulCash] [int] NULL,
	[int4_DutyAnnulCount] [int] NULL,
 CONSTRAINT [PK_Duty] PRIMARY KEY CLUSTERED 
(
	[id_Duty] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[Duty]  WITH CHECK ADD  CONSTRAINT [FK_Duty_Module] FOREIGN KEY([id_Module])
REFERENCES [dbo].[Module] ([id_Module])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Duty] CHECK CONSTRAINT [FK_Duty_Module]
GO


