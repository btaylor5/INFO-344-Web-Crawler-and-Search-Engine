<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/9/15
 * Time: 7:46 PM
 */
header("Content-Type: application/json", true);
include_once('../Model/PlayerStack.php');
error_reporting(E_ALL);
include_once('../Model/DBAccess.php');
include_once('../Model/Search.php');
include_once('../Model/Player.php');


$DB_Connection = new DBAccess();
if(isset($_REQUEST['name'])) {
    $player = new Search($_REQUEST['name']);
    $playerStack = $player->searchLevenshtein($DB_Connection);
    $players = $playerStack->asArray();
    $results = array();
    foreach($players as $player) {
        $results[] = $player->arrayRepresentation();
    }
    echo json_encode($results);

} else {
    echo "{ERROR}";
}