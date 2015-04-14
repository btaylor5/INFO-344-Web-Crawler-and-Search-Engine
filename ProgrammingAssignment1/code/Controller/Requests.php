<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/9/15
 * Time: 7:46 PM
 *
 * Listens for requests and returns JSON representation of the query results
 *
 */
header("Content-Type: application/json", true);
include_once('../Model/PlayerStack.php');
include_once('../Model/DBAccess.php');
include_once('../Model/Search.php');
include_once('../Model/Player.php');


$DB_Connection = new DBAccess();
if(isset($_REQUEST['name'])) {
    $player = new Search($_REQUEST['name']);
    $playerStack = $player->searchLevenshtein($DB_Connection);
    $players = $playerStack->asArray();
    $results = array();
    if (sizeof($players) > 0) {
        foreach($players as $player) {
            $results[] = $player->arrayRepresentation();
        }
        echo json_encode(array_reverse($results));
    } else {
        echo "No Results, Try Refining your search!";
    }
} else {
    echo "{ERROR}";
}