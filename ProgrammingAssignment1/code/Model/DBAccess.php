<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/4/15
 * Time: 8:43 PM
 */

class DBAccess {

    private $connection;

    function __construct() {
        $config = include_once('config.php');
        try {
            $this->connection = new PDO($config['DBProvider'] . ':' . 'host=' . $config['host'] . ';' . 'port=' . $config['port'] . ';' . 'dbname=' . $config['dbname'], $config['username'], $config['password']);
            $this->connection->setAttribute( PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION );
        } catch ( PDOException $e ) {
            echo "Error Connecting with the Database";
            echo $e->getMessage();
            file_put_contents( './DB_Error_Log.txt', $e->getMessage() . "\n", FILE_APPEND);
        }
    }

    function getConnection() {
        return $this->connection;
    }

    function getAll() {
        $query = $this->connection->prepare("SELECT * FROM nbaStats");
        $query->execute();

        $this->printAllOfColumn($query, 'PlayerName');

    }

    function getPlayer($name) {
        $query = $this->connection->prepare("SELECT * FROM nbaStats WHERE PlayerName = ?");
        $query->execute(array ($name));
        $this->printAllOfColumn($query, 'PlayerName');
    }

    static function printAllOfColumn($query, $column) {
        while ($row = $query->fetch(PDO::FETCH_ASSOC)) {
            echo $row[$column] . "</br>";
        }
    }

}