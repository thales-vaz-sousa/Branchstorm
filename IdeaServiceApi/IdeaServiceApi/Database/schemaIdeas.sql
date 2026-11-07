CREATE TABLE IF NOT EXISTS Links (
	Code TEXT PRIMARY KEY,
	FatherCode TEXT,
	Type TEXT NOT NULL,
	Title TEXT NOT NULL,
	Description TEXT,
	Status TEXT NOT NULL,
	Validation TEXT NOT NULL,
	AutorCode TEXT,
	TeamCode TEXT,
	Reason TEXT,
	CreationDate TEXT NOT NULL,
	FOREIGN KEY (FatherCode) REFERENCES Links (Code)
);

CREATE TABLE IF NOT EXISTS LinkHistory (
	Code TEXT PRIMARY KEY,
	LinkCode TEXT NOT NULL,
	Event TEXT NOT NULL,
	CreationDate TEXT NOT NULL,
	FOREIGN KEY (LinkCode) REFERENCES Links (Code)
);
