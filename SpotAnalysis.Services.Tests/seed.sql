SET IDENTITY_INSERT dbo.Users ON
INSERT INTO dbo.Users (UserName, UserID, PasswordHash) VALUES
    ('Lehrer1', 1, '$abc'),
    ('Lehrer2', 2, '$def'),
    ('Schueler1', 3, '$ghi'),
    ('Schueler2', 4, '$jkl');
SET IDENTITY_INSERT dbo.Users OFF 