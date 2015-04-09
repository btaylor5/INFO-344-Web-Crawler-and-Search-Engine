<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/1/15
 * Time: 11:07 AM
 */
// TODO: REMOVE BEFORE FINALIZED
error_reporting(E_ALL);
include_once('./code/Model/DBAccess.php');
include_once('./code/Model/Search.php');
include_once('./code/Model/Player.php');

$DB_Connection = new DBAccess();
if(isset($_REQUEST['name'])) {
    $player = new Search($_REQUEST['name']);
    $search = $player->searchLevenshtein($DB_Connection);
} else {
    echo "Search";
//    $DB_Connection->getAll();
}

include_once('./code/View/footer.html');

include_once('./code/View/Search_UI.php');

include_once('./code/View/Results_UI.php');

include_once('./code/View/footer.html');