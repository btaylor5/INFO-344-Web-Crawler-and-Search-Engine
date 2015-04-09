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

    function __toString() {
        return $this->stack[0] . '';
    }

    function push($value) {
        $this->stack[] = $value;
    }

    function pop() {
        array_pop($this->stack);
    }

    private function getStack() {
        return $this->stack;
    }

    function size() {
        return sizeof($this->stack);
    }
}