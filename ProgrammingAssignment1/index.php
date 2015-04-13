<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/1/15
 * Time: 11:07 AM
 */

include_once('./code/View/header.php');
// TODO: REMOVE BEFORE FINALIZED
include_once('./code/View/Search_UI.php');
include_once('./code/View/Results_UI.php');
include_once('./code/View/footer.php');
//
//
include_once('./code/Model/PlayerStack.php');
error_reporting(E_ALL);
include_once('./code/Model/DBAccess.php');
include_once('./code/Model/Search.php');
include_once('./code/Model/Player.php');

function in_player_array($player, $array) {
    $id = $player[0];
    foreach ($array as $other_player) {
        echo "player 1: $other_player[0] <-> player 2: $id <br />";
        if($other_player[0] === $id) {
            echo "FOUND!";
            return TRUE;
        }
    }
    return FALSE;
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
            if (!$this->in_player_array($player , $closest_matches)) {
                array_push($closest_matches, $player);
            }
        }
    }
    return $closest_matches;
}

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

$DB_Connection = new DBAccess();
if(isset($_REQUEST['name'])) {


    $closest_matches = searchLevenshtein($DB_Connection);

    foreach($closest_matches as $player) {
        if(!in_player_array($player, $results2)) {
            echo 'add to array <br />';
        } else {
            echo 'no thanks <br />';
        }
    }



//    $player = new Search($_REQUEST['name']);
//    $playerStack = $player->searchLevenshtein($DB_Connection);
//    $players = $playerStack->asArray();
//    $results = array();
//    foreach($players as $player) {
//        echo $player->__toString();
//        $results[] = $player->arrayRepresentation();
//    }

} else {
    echo "{ERROR}";
}