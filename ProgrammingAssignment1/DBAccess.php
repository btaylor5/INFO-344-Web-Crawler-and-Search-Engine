<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/4/15
 * Time: 8:43 PM
 */

class DBAccess {

    private $connection;
    private $config;


    function __construct() {
        $config = include_once('config.php');
        try {
            $connection = new PDO($config['host'], $config['username'], $config['password']);
            $connection->setAttribute( PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION );
            $connection->prepare( "SELECT * FROM Users" );
        } catch ( PDOException $e ) {
            echo "I'm sorry there is a problem with your operation..";
            file_put_contents( 'dbErrors.txt', $e->getMessage(), FILE_APPEND );
        }
    }

}