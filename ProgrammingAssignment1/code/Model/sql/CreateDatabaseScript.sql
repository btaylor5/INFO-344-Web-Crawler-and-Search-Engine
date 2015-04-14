CREATE TABLE nbaStats(
	PlayerID int not null auto_increment,
    PlayerName varchar(255) not null,
    PlayerPosition varchar(255),
    TeamAbbName varchar(255),
    TeamID int DEFAULT 0,
    GP int DEFAULT 0,
    MIN int DEFAULT 0,
    FGM int DEFAULT 0,
    FGA int DEFAULT 0,
    FGP int DEFAULT 0,
    TPM int DEFAULT 0,
    TPA int DEFAULT 0,
    TPP int DEFAULT 0,
    FTM int DEFAULT 0,
    FTA int DEFAULT 0,
    FTP int DEFAULT 0,
    RBO int DEFAULT 0,
    RBD int DEFAULT 0,
    RBT int DEFAULT 0,
    AST int DEFAULT 0,
    `TO` int DEFAULT 0,
    STL int DEFAULT 0,
    BLK int DEFAULT 0,
    PF int DEFAULT 0,
    PPG int DEFAULT 0,
    TR int DEFAULT 0,
    PRIMARY KEY (PlayerID)
);

DROP TABLE nbaStats;

TRUNCATE TABLE nbaStats;

LOAD DATA LOCAL INFILE '/home/btaylor5/Desktop/2012-2013.nba.stats.csv' INTO TABLE nbaStats FIELDS TERMINATED BY ',' ENCLOSED BY '' LINES TERMINATED BY '\n' IGNORE 9 LINES;

SELECT * FROM nbaStats;