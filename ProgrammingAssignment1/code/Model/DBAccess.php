<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/4/15
 * Time: 8:43 PM
 *
 * In charge of managing the creation of PDO objects.
 * NOTE: Needs config.php filled out otherwise, connection problems can be found in 'DB_Error_Log.txt'
 */

class DBAccess
{

    private $connection;

    function __construct()
    {
        $config = include_once('config.php');
        try
        {
            $this->connection = new PDO($config['DBProvider'] . ':' . 'host=' . $config['host'] . ';' . 'port=' . $config['port'] . ';' . 'dbname=' . $config['dbname'], $config['username'], $config['password']);
            $this->connection->setAttribute( PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION );
        } catch ( PDOException $e )
        {
            echo "Error Connecting with the Database";
            echo $e->getMessage();
            file_put_contents( './DB_Error_Log.txt', $e->getMessage() . "\n", FILE_APPEND);
        }
    }

    // returns the PDO object to be queried on
    function getConnection()
    {
        return $this->connection;
    }
}