INSERT INTO [cdata].[dbo].[Verses]
           ([Id]
           ,[Composition_Id]
           ,[Index]
           ,[Text]
           ,[Sequence]
           ,[Audit_Author_Id]
           ,[Audit_CreateDate]
           ,[Audit_ModifyDate]
           ,[Audit_CollaboratorIndex])
     VALUES
           (newid()
           ,CONVERT(uniqueidentifier, '7002B016-CAEB-403C-A60A-F81086814A88')
           ,5
           ,'0,one;0.5,for;1,the;1.5,mon;2,two;2,*;8,*;8.5,*;'
           ,400
           ,'1'
           ,GETDATE()
           ,GETDATE()
           ,0)
GO


