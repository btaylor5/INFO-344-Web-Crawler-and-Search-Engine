<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/8/15
 * Time: 4:55 PM
 */
include_once('Player.php');

class PlayerStack {

    protected $stack;

    function __constructor() {;
        $this->stack = array();
    }

    function push($value) {
        $this->stack[] = $value;
    }

    function __toString() {
        $json = '';
        foreach ($this->stack as $player) {
            $json = $json . $player->__toString();
        }
        return $json;
    }

    function pop() {
        array_pop($this->stack);
    }

    function size() {
        return sizeof($this->stack);
    }

    function asArray() {
        return $this->stack;
    }
}