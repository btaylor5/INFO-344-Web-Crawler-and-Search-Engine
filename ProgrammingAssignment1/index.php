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
//include_once('./code/Model/PlayerStack.php');
//error_reporting(E_ALL);
//include_once('./code/Model/DBAccess.php');
//include_once('./code/Model/Search.php');
//include_once('./code/Model/Player.php');
//$DB_Connection = new DBAccess();
//if(isset($_REQUEST['name'])) {
//    $player = new Search($_REQUEST['name']);
//    $playerStack = $player->searchLevenshtein($DB_Connection);
//    $players = $playerStack->asArray();
//    $results = array();
//    foreach($players as $player) {
//        $results[] = $player->arrayRepresentation();
//    }
//    echo json_encode($results);
//
//} else {
//    echo "{ERROR}";
//}