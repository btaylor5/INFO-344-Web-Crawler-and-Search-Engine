<?php
/**
 * User: btaylor5
 * Date: 4/8/15
 * Time: 2:37 PM
 */

class Book {
    private $name;
    private $price;
    
    function  __construct($name, $price){
        $this->name = $name;
        $this->price = $price;
    }

    public function getName() {
        return $this->name;
    }

    public function getPrice() {
        return $this->price;
    }

    public static function GetDefaultBooks() {
        return array(
            new Book('Moby Dick', 15),
            new Book('Lord of the Rings', 20),
            new Book('Ghost in the wires', 33),
            new Book('Harry Potter', 10),
            new Book('Cat in the Hat', 13)
        );
    }


    public function __toString() {
        return 'Title: ' . $this->name . ', Price: ' . $this->price;
    }
}