<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/1/15
 * Time: 12:03 PM
 *
 * Search includes the necessary methods to search for players
 */
include_once('PlayerStack.php');
class Search {

    private $name_array;
    private $name;


    // takes in the search query and prepares it for use
    function __construct($name)
    {
        $name = trim($name);
        $this->name_array = explode(' ', $name);
        $this->name = $name;
    }

    // returns the Name being searched
    public function __toString()
    {
        return $this->name;
    }

    // returns whether a player is already present in an array
    function in_player_array($player, $array)
    {
        $id = $player[0];
        foreach ($array as $other_player)
        {
            if($other_player[0] === $id)
            {
                return TRUE;
            }
        }
        return FALSE;
    }



    function exact_match($name, $DB_Connection) {
        $name = str_replace(array('.', '-', ',', '!', '?', ), '', $name);
        if (strlen($name) > 0) {
            $sql = "
                SELECT *
                FROM nbaStats
                WHERE replace(replace(replace(PlayerName, '.', ''), '-', ''), ',', '')
                = ?
                ";
            $stmt = $DB_Connection->getConnection()->prepare($sql);
            $stmt->execute(array($name));
            $result = $stmt->fetchAll();
            return $result[0];
        }
        return array();
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
    function lookUpPlayer($closest_matches, $DB_Connection)
    {
        $results = array();
        foreach($this->name_array as $segment)
        {
            if(strlen($segment) >= 2) // prevents adding every name with a letter to the results list
            {
                $sql = "
                SELECT *
                FROM nbaStats
                WHERE replace(replace(replace(PlayerName, '.', ''), '-', ''), ',', '')
                LIKE ?
                ";

                $stmt = $DB_Connection->getConnection()->prepare($sql);
                $stmt->execute(array('%' . $segment . '%'));
                $results = array_merge($results, $stmt->fetchAll());
                for ($i = 0; $i < sizeof($results); $i++)
                {
                    if (!$this->in_player_array($results[$i], $closest_matches))
                    {
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
    function searchLevenshtein($DB_Connection)
    {
        $closest_matches = array();

        //Assume user got one name right
        $closest_matches = $this->lookUpPlayer($closest_matches, $DB_Connection);
        if(sizeof($closest_matches) == 0)
        {
            // user typed in something with no results
            // Brute force to return some possible results
            $stmt = $DB_Connection->getConnection()->prepare("SELECT * FROM nbaStats WHERE PlayerName SOUNDS LIKE ?");
            $stmt->execute(array ('%' . $this->name . '%'));
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