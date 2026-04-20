INSERT INTO "Users" ("UserName", "UserID", "PasswordHash", "Roles") VALUES
    ('Admin1', '30f7b573-eafe-46ae-a1f0-671e928f7f1d', '$argon2id$v=19$m=4096,t=4,p=4$MzBmN2I1NzMtZWFmZS00NmFlLWExZjAtNjcxZTkyOGY3ZjFk$LWy66md5V6iNzeDNmo2e4Ztd9XwnEkPUu2iH3i98OZk', ARRAY[0]),
    ('Lehrer1', '9c9c2138-f945-41fa-823e-f3bd286c0fa1', '$argon2id$v=19$m=4096,t=4,p=4$OWM5YzIxMzgtZjk0NS00MWZhLTgyM2UtZjNiZDI4NmMwZmEx$yIGUKh8n1+NddBGL4BbTZq1sxVhZlWh1p4DlYPccmWM', ARRAY[1]),
    ('Lehrer2', '48bb93c8-214f-47f0-910f-9056b19de94a', '$argon2id$v=19$m=4096,t=4,p=4$NDhiYjkzYzgtMjE0Zi00N2YwLTkxMGYtOTA1NmIxOWRlOTRh$cnenEgTM0DfaIHBN6JMFMFAWQ00p3tu4m75Ob4wgFTk', ARRAY[1]),
    ('Schueler1', '2195c82c-0a67-4938-9c88-20c089276da5', '$argon2id$v=19$m=4096,t=4,p=4$MjE5NWM4MmMtMGE2Ny00OTM4LTljODgtMjBjMDg5Mjc2ZGE1$dpiG9escRjHIzOTYTM0hQOBnSWb0mQzHOenq8IoUQ7Q', ARRAY[2]),
    ('Schueler2', 'f01c1e4f-c5e0-4f77-a3b3-f59f8b837553', '$argon2id$v=19$m=4096,t=4,p=4$ZjAxYzFlNGYtYzVlMC00Zjc3LWEzYjMtZjU5ZjhiODM3NTUz$ui1HyahZiikGT1Rd8XIeu8JeSJNaw42QHoM5a++sgO8', ARRAY[2])
ON CONFLICT ("UserID") DO NOTHING;

INSERT INTO "Chemicals" ("ChemicalID", "Type", "Name", "Formula", "Color") VALUES
    (1, 0,'Silber(I)nitrat', 'AGNo3', 'Orange'),
    (2, 0, 'Kalium', 'K', 'Rainbow')
ON CONFLICT ("ChemicalID") DO NOTHING;

INSERT INTO "Observations" ("ObservationID", "Description") VALUES
    (1, 'Some Observation')
ON CONFLICT ("ObservationID") DO NOTHING;

INSERT INTO "Reactions" ("ReactionID", "Chemical1ID", "Chemical2ID", "RelevantProduct", "Formula", "ObservationID") VALUES
    (1, 1, 2, 'Cucumber', 'It Reacts', 1)
ON CONFLICT ("ReactionID") DO NOTHING;
