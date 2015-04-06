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

    function __construct($FirstName, $LastName){
        $this->FirstName = $FirstName;
        $this->LastName = $LastName;
    }

    public function __toString(){
        return $this->FirstName . ' ' . $this->LastName;
    }

}