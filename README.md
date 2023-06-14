# SqlServerBulkCopy

Sample of batch registration of data to a table in SQLServer with BulkCopy


```
USE [TestDB]
GO

/****** Object:  Table [dbo].[BulkTable]    Script Date: 2023/06/14 20:36:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BulkTable](
	[ID] [int] NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Flag] [bit] NOT NULL,
 CONSTRAINT [PK_BulkTable] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BulkTable] ADD  CONSTRAINT [DF_BulkTable_Flag]  DEFAULT ((0)) FOR [Flag]
GO
```

