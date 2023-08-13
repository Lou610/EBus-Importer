USE [EbusImporter]
GO

/****** Object:  Table [dbo].[Waybill]    Script Date: 2015/08/21 3:28:58 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Waybill](
	[ModuleID] [int] NOT NULL,
	[dat_Start] [datetime] NULL,
	[dat_End] [datetime] NULL,
	[int4_Operator] [int] NULL,
	[str8_BusID] [varchar](8) NULL,
	[str6_EtmID] [varchar](6) NULL,
	[int4_EtmGrandTotal] [int] NULL,
	[int4_Revenue] [int] NULL,
	[dat_Match] [smalldatetime] NULL,
	[dat_Actual] [smalldatetime] NULL,
	[Imported_Operator] [int] NULL,
	[Imported_BusID] [varchar](8) NULL,
	[Imported_ETMID] [varchar](6) NULL,
	[Imported_GT] [int] NULL,
	[Imported_Revenue] [int] NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


