<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/6/15
 * Time: 1:39 PM
 */
$config = array(
    'host' => 'mysql:host=uwinfo344.chunkaiw.com;port=3306;dbname=info344mysqlpdo',
    'username' => 'info344mysqlpdo',
    'password' => 'chrispaul'
);


try {
    $connection = new PDO($config['host'], $config['username'], $config['password']);
    $connection->setAttribute( PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION );
    $query = $connection->prepare("SELECT * FROM Books");
    $query->execute(array());


    while ($row = $query->fetch(PDO::FETCH_ASSOC))
    {
        echo $row['name'] . "</br>";
    }

} catch ( PDOException $e ) {
    echo "Error Connecting with the Database";
    file_put_contents( 'DB_Error_Log.txt', $e->getMessage() . "\n", FILE_APPEND);
}

