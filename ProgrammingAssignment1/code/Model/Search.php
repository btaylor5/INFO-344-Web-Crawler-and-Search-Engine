<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/1/15
 * Time: 12:03 PM
 */
error_reporting(E_ALL);
include_once('PlayerStack.php');
class Search {

    private $name_array;
    private $toString;

//TODO cut out commas

    function __construct($name){
        $name = trim($name);
        $this->name_array = explode(' ', $name);
        $this->toString = $name;
    }

    public function __toString(){
        return $this->toString;
    }

    function lookUpPlayer($closest_matches, $DB_Connection){
        $results = array();
        foreach($this->name_array as $segment) {
            $sql = "
            SELECT *
            FROM nbaStats
            WHERE replace(replace(PlayerName, '.', ''), '-', '')
            LIKE ?
            ";

            $stmt = $DB_Connection->getConnection()->prepare($sql);
            $stmt->execute(array( '%' . $segment . '%'));
            $results = array_merge($results, $stmt->fetchAll());
            foreach ($results as $player) {
                if (! in_array($player , $closest_matches)) {
                    array_push($closest_matches, $player);
                }
            }
        }
        return $results;
    }

    function searchLevenshtein($DB_Connection) {
        $closest_matches = array();
        $results = $this->lookUpPlayer($closest_matches, $DB_Connection);
        if(sizeof($results) == 0) {
            $stmt = $DB_Connection->getConnection()->prepare("SELECT * FROM nbaStats");
            $stmt->execute();
            $results = $stmt->fetchAll();
        }
        // no shortest distance found, yet
        $shortest = -1;

//        $closest = '';
        // loop through words to find the closest
        foreach ($results as $result) {

            // calculate the distance between the input word,
            // and the current word
            $name = $result['PlayerName']; //So We Don't have to modify original version when replacing punctuation
            $name = str_replace(array (',', '.', ';', ':', '&', '!', '?', '-'), '',  $name);
            //calculates levenshtein distance ignoring common punctuation
            $levenshtein_length = levenshtein($this->__toString(), $name);
            // check for an exact match
            if ($levenshtein_length == 0) {

                // closest word is this one (exact match)
                array_push($closest_matches, $result);
                // break out of the loop; we've found an exact match
                break;
            }

            // if this distance is less than the next found shortest
            // distance, OR if a next shortest word has not yet been found
            if ($levenshtein_length <= $shortest || $shortest < 0) {
                // set the closest match, and shortest distance
                if(!in_array($result['PlayerName'], $closest_matches)) {
                    array_push($closest_matches, $result);
                }
                $shortest = $levenshtein_length;
            }
        }

        $playerStack = new PlayerStack();
        while(sizeof($closest_matches) > 0) {
            $player = new Player(array_pop($closest_matches));
            echo $player->__toString() . "<br />";
            $playerStack->push($player);
        }
        return $playerStack;
    }

}