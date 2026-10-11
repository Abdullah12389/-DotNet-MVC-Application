use AttendanceDB;
-- CREATE TABLE [dbo].[Students] (
--     [StudentId]            INT           IDENTITY (1, 1) NOT NULL,
--     [StudentName]          NVARCHAR (100) NOT NULL,
--     [RegistrationNumber]   NVARCHAR (30) NOT NULL,
--     [Department]           NVARCHAR (50) NOT NULL,
--     [AttendancePercentage] DECIMAL (5,2)  NOT NULL,
--     CONSTRAINT [PK_Students] PRIMARY KEY CLUSTERED ([StudentId] ASC)
-- );
-- INSERT INTO Students (StudentName,RegistrationNumber,Department,AttendancePercentage) VALUES 
-- ('student2','1789','Software Engineeing',95.78)
select * from students;
-- Delete From Students where StudentId = 4;

