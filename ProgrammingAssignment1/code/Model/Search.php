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
    private $name;

//TODO cut out commas

    function __construct($name){
        $name = trim($name);
        //TODO MAke Regex of at least 2 chars (DJ)
//        $this->name_array = preg_split("/^[a-zA-Z]{2,}$/", $name);
        $this->name_array = explode(' ', $name);
        $this->toString = $name;
    }

    public function __toString(){
        return $this->toString;
    }


    function in_player_array($player, $array) {
        $id = $player[0];
        foreach ($array as $other_player) {
            if($other_player[0] === $id) {
                return TRUE;
            }
        }
        return FALSE;
    }

    /**
     * $closest_matches = an array that holds possible results already,
     * will be included in the returned array
     *
     * To be used tPlayero find NBAPlayers.
     * Assumes DBName is nbaStats and follows the structure of this CSV file
     * (http://uwinfo344.chunkaiw.com/files/2012-2013.nba.stats.csv)
     *
     * Will Return a PlayerStack full of Player Objects
     *
     *
     * @param $closest_matches
     * @param $DB_Connection
     * @return array
     */

    function lookUpPlayer($closest_matches, $DB_Connection){
        $results = array();
        foreach($this->name_array as $segment) {
            if(strlen($segment) >= 2) {
                $sql = "
                SELECT *
                FROM nbaStats
                WHERE replace(replace(PlayerName, '.', ''), '-', '')
                LIKE ?
                ";

                $stmt = $DB_Connection->getConnection()->prepare($sql);
                $stmt->execute(array('%' . $segment . '%'));
                $results = array_merge($results, $stmt->fetchAll());
                for ($i = 0; $i < sizeof($results); $i++) {
                    if (!$this->in_player_array($results[$i], $closest_matches)) {
                        $closest_matches[] = $results[$i];
                    }
                }
            }
        }
        return $closest_matches;
    }


    /**
     * To be used to find NBAPlayers.
     * Assumes DBName is nbaStats and follows the structure of this CSV file
     * (http://uwinfo344.chunkaiw.com/files/2012-2013.nba.stats.csv)
     *
     * Will Return a PlayerStack full of Player Objects
     *
     * @param $DB_Connection,
     * @return PlayerStack
     */
    function searchLevenshtein($DB_Connection) {
        $closest_matches = array();

        //Assume user got one name right
        $closest_matches = $this->lookUpPlayer($closest_matches, $DB_Connection);
        if(sizeof($closest_matches) == 0) {
            // user typed in something with no results
            // Brute force to return some possible results
            $stmt = $DB_Connection->getConnection()->prepare("SELECT * FROM nbaStats WHERE PlayerName SOUNDS LIKE ?");
            $stmt->execute(array ('%' . $this->toString . '%'));
            $closest_matches = $stmt->fetchAll();
        }

        $shortest = -1; //Don't know shortest yet

        // loop through words to find the closest match to the search
        foreach ($closest_matches as $result) {

            //remove punctuation from search
            $name = str_replace(array (',', '.', ';', ':', '&', '!', '?', '-'), '', $result['PlayerName']);

            //calculates levenshtein distance
            //notice, for best results you need to remove punctuation first
            $levenshtein_length = levenshtein($this->__toString(), $name);

            //exact match with search
            if ($levenshtein_length == 0) {
                if(!$this->in_player_array($result, $closest_matches)) {
                    //Add to search results
                    array_push($closest_matches, $result);
                }
                break;
            }

            if ($levenshtein_length <= $shortest || $shortest < 0) {
                if(!$this->in_player_array($result, $closest_matches)) {
                    //Add to search results
                    array_push($closest_matches, $result);
                }
                $shortest = $levenshtein_length;
            }
        }

        // Convert to Players
        $playerStack = new PlayerStack();
        while(sizeof($closest_matches) > 0) {
            $player = new Player(array_pop($closest_matches));
            $playerStack->push($player);
        }
        return $playerStack;
    }

}