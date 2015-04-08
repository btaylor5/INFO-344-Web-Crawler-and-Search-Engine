<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/1/15
 * Time: 12:03 PM
 */
error_reporting(E_ALL);
class Player {

    var $FirstName;
    var $LastName;

//TODO cut out commas

    function __construct($name){
        $name_array = explode(' ', $name);
        $this->FirstName = $name_array[0];
        $this->LastName = $name_array[1];
//        echo $this->__toString();
    }

    public function __toString(){
        return $this->FirstName . ' ' . $this->LastName;
    }

    function lookUpPlayer($DB_Connection){
        $sql = "
            SELECT *
            FROM nbaStats
            WHERE PlayerName
            LIKE ?
            OR PlayerName
            LIKE ?
            ";

        $stmt = $DB_Connection->getConnection()->prepare($sql);
        $stmt->execute(array( '%' . $this->FirstName . '%',  '%' . $this->LastName . '%'));
        while ($row = $stmt->fetch(PDO::FETCH_ASSOC)) {
            echo $row['PlayerName'] . "</br>";
        }
    }

}