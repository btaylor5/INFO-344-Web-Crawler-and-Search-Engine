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


    /**
     * $closest_matches = an array that holds possible results already,
     * will be included in the returned array
     *
     * To be used to find NBAPlayers.
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
        $results = $this->lookUpPlayer($closest_matches, $DB_Connection);
        if(sizeof($results) == 0) {
            // user typed in something with no results
            // Brute force to return some possible results
            $stmt = $DB_Connection->getConnection()->prepare("SELECT * FROM nbaStats");
            $stmt->execute();
            $results = $stmt->fetchAll();
        }

        $shortest = -1; //Don't know shortest yet

        // loop through words to find the closest match to the search
        foreach ($results as $result) {

            //remove punctuation from search
            $name = str_replace(array (',', '.', ';', ':', '&', '!', '?', '-'), '', $result['PlayerName']);

            //calculates levenshtein distance
            //notice, for best results you need to remove punctuation first
            $levenshtein_length = levenshtein($this->__toString(), $name);

            //exact match with search
            if ($levenshtein_length == 0) {
                array_push($closest_matches, $result);

                break;
            }

            if ($levenshtein_length <= $shortest || $shortest < 0) {
                if(!in_array($result['PlayerName'], $closest_matches)) {
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