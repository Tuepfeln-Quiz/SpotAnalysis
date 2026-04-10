INSERT INTO dbo.Users (UserName, UserID, PasswordHash) VALUES
    ('Lehrer1', '9c9c2138-f945-41fa-823e-f3bd286c0fa1', '$abc'),
    ('Lehrer2', '48bb93c8-214f-47f0-910f-9056b19de94a', '$def'),
    ('Schueler1', '2195c82c-0a67-4938-9c88-20c089276da5', '$ghi'),
    ('Schueler2', 'f01c1e4f-c5e0-4f77-a3b3-f59f8b837553', '$jkl');

SET IDENTITY_INSERT dbo.Roles ON
INSERT INTO dbo.Roles (RoleID, Title) VALUES
    (1, 'teacher'),
    (2, 'student');
SET IDENTITY_INSERT dbo.Roles OFF

INSERT INTO RoleUser (RoleID, UserID) VALUES
    (1, '9c9c2138-f945-41fa-823e-f3bd286c0fa1');