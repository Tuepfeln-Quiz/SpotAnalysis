SET IDENTITY_INSERT dbo.Users ON
INSERT INTO dbo.Users (UserName, UserID, PasswordHash) VALUES
    ('Lehrer1', 1, '$abc'),
    ('Lehrer2', 2, '$def'),
    ('Schueler1', 3, '$ghi'),
    ('Schueler2', 4, '$jkl');
SET IDENTITY_INSERT dbo.Users OFF 

SET IDENTITY_INSERT dbo.Roles ON
INSERT INTO dbo.Roles (RoleID, Title) VALUES
    (1, 'teacher'),
    (2, 'student');
SET IDENTITY_INSERT dbo.Roles OFF

INSERT INTO RoleUser (RoleID, UserID) VALUES
    (1, 1);