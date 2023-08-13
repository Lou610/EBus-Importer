
/****** Object:  Table [dbo].[Journey]    Script Date: 2015/08/19 3:08:37 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Journey](
	[id_Journey] [int] NOT NULL Identity,
	[id_Duty] [int] NOT NULL,
	[id_Module] [int] NOT NULL,
	[str_RouteID] [char](4) NULL,
	[int2_JourneyID] [smallint] NULL,
	[int2_Direction] [smallint] NULL,
	[dat_JourneyStartDate] [datetime] NULL,
	[dat_JourneyStartTime] [datetime] NULL,
	[dat_JourneyStopDate] [datetime] NULL,
	[dat_JourneyStopTime] [datetime] NULL,
	[dat_TrafficDate] [datetime] NULL,
	[int4_Distance] [int] NULL,
	[int4_Traveled] [int] NULL,
	[int4_JourneyRevenue] [int] NULL,
	[int4_JourneyTickets] [int] NULL,
	[int4_JourneyPasses] [int] NULL,
	[int4_JourneyNonRevenue] [int] NULL,
	[int4_JourneyTransfer] [int] NULL,
	[dat_RecordMod] [datetime] NULL,
	[id_BatchNo] [int] NULL,
	[byt_IeType] [tinyint] NULL,
	[dat_JourneyMoveTime] [datetime] NULL,
	[dat_JourneyArrivalTime] [datetime] NULL,
	[int4_GPSDistance] [int] NULL,
 CONSTRAINT [PK_Journey] PRIMARY KEY CLUSTERED 
(
	[id_Journey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[Journey]  WITH CHECK ADD  CONSTRAINT [FK_Journey_Duty] FOREIGN KEY([id_Duty])
REFERENCES [dbo].[Duty] ([id_Duty])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Journey] CHECK CONSTRAINT [FK_Journey_Duty]
GO


