USE MASTER
GO
CREATE DATABASE MyAlgebraDatabase
GO
USE MyAlgebraDatabase
GO
CREATE TABLE Courses
(
	Id INT IDENTITY PRIMARY KEY NOT NULL,
	Name VARCHAR(50) NOT NULL,
	Description NTEXT NOT NULL,
	Date DATE NOT NULL,
	CurrentAttendants INT NOT NULL,
	MaxAttendants INT NOT NULL
)
GO
CREATE TABLE Preorders
(
	Id INT IDENTITY PRIMARY KEY NOT NULL,
	FirstName VARCHAR(50) NOT NULL,
	LastName VARCHAR(50) NOT NULL,
	Address NVARCHAR(150) NOT NULL,
	Email NVARCHAR(250) NOT NULL UNIQUE,
	Phone NVARCHAR(20) NOT NULL,
	Date DATE NOT NULL,
	CourseId INT NOT NULL FOREIGN KEY REFERENCES Courses(Id),
	Status VARCHAR(20)
)
GO
CREATE TABLE Employees
(
	Id INT IDENTITY PRIMARY KEY NOT NULL,
	Username NVARCHAR(50) NOT NULL UNIQUE,
	HashedPassword NVARCHAR(200) NOT NULL,
	Salt NVARCHAR(100) NOT NULL,
	Token NVARCHAR(250) NOT NULL,
)
GO
INSERT INTO Employees VALUES ('Admin', 'eb7440337546ea99d274d2f8d29a61fc', '123pdo', '008c5926ca861023c1d2a36653fd88e2')
GO