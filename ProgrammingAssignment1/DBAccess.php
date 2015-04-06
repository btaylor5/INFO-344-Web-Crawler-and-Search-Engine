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
            $this->connection = new PDO($config['host'], $config['username'], $config['password']);
            $this->connection->setAttribute( PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION );
            $this->connection->prepare( "SELECT * FROM Users" );
        } catch ( PDOException $e ) {
            echo "Error Connecting with the Database";
            echo phpinfo();
            file_put_contents( 'DB_Error_Log.txt', $e->getMessage() . "\n", FILE_APPEND);
        }
    }
}