USE [EbusImporter]
GO

/****** Object:  Table [dbo].[Module]    Script Date: 2015/08/19 3:12:53 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Module](
	[id_Module] [int] IDENTITY(1,1) NOT NULL,
	[str_LocationCode] [char](2) NULL,
	[int4_ModuleID] [int] NULL,
	[int4_SignOnID] [int] NULL,
	[int4_OnReaderID] [int] NULL,
	[dat_SignOnDate] [datetime] NULL,
	[dat_SignOnTime] [datetime] NULL,
	[int4_OffReaderID] [int] NULL,
	[dat_SignOffDate] [datetime] NULL,
	[dat_SignOffTime] [datetime] NULL,
	[dat_TrafficDate] [datetime] NULL,
	[dat_ModuleOutDate] [datetime] NULL,
	[dat_ModuleOutTime] [datetime] NULL,
	[int4_HdrModuleRevenue] [int] NULL,
	[int4_HdrModuleTickets] [int] NULL,
	[int4_HdrModulePasses] [int] NULL,
	[int4_ModuleRevenue] [int] NULL,
	[int4_ModuleTickets] [int] NULL,
	[int4_ModulePasses] [int] NULL,
	[int4_ModuleNonRevenue] [int] NULL,
	[int4_ModuleTransfer] [int] NULL,
	[dat_ImportStamp] [datetime] NULL,
	[dat_RecordMod] [datetime] NULL,
	[int4_ImportModuleKey] [int] NULL,
	[id_BatchNo] [int] NULL,
	[byt_IeType] [tinyint] NULL,
	[byt_ModuleType] [int] NULL,
 CONSTRAINT [PK_Module] PRIMARY KEY CLUSTERED 
(
	[id_Module] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


