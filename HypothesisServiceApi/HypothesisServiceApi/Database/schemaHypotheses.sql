CREATE TABLE IF NOT EXISTS Hypotheses (
	Code TEXT PRIMARY KEY,
	LinkCode TEXT NOT NULL,
	Description TEXT NOT NULL,
	SuccessCriteria TEXT NOT NULL,
	Result TEXT,
	CreationDate TEXT NOT NULL
);
