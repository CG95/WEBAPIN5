-- Create the APIN5DB database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'APIN5DB')
BEGIN
    CREATE DATABASE APIN5DB;
END
GO

-- Switch to the new database
USE APIN5DB;
GO

-- Create PermissionType table
CREATE TABLE PermissionTypes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Description NVARCHAR(255) NOT NULL
);
GO

-- Create Permission table
CREATE TABLE Permissions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    EmployeeForename NVARCHAR(255) NOT NULL,
    EmployeeSurname NVARCHAR(255) NOT NULL,
    PermissionTypeId INT NOT NULL,
    PermissionDate DATETIME NOT NULL,
    CONSTRAINT FK_Permissions_PermissionType FOREIGN KEY (PermissionTypeId)
        REFERENCES PermissionTypes (Id)
        ON DELETE CASCADE
);
GO

-- Insert sample data into PermissionTypes table
INSERT INTO PermissionTypes (Description) VALUES ('Vacation');
INSERT INTO PermissionTypes (Description) VALUES ('Sick Leave');
INSERT INTO PermissionTypes (Description) VALUES ('Personal Day');
GO

-- Insert sample data into Permission table
INSERT INTO Permissions (EmployeeForename, EmployeeSurname, PermissionTypeId, PermissionDate)
VALUES ('John', 'Doe', 1, '2024-09-01');

INSERT INTO Permissions (EmployeeForename, EmployeeSurname, PermissionTypeId, PermissionDate)
VALUES ('Jane', 'Smith', 2, '2024-09-15');

INSERT INTO Permissions (EmployeeForename, EmployeeSurname, PermissionTypeId, PermissionDate)
VALUES ('Michael', 'Johnson', 3, '2024-09-22');
GO

