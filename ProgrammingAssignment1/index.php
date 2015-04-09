<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/1/15
 * Time: 11:07 AM
 */
// TODO: REMOVE BEFORE FINALIZED
error_reporting(E_ALL);
include_once('DBAccess.php');
include_once('Player.php');
$DB_Connection = new DBAccess();
if(isset($_REQUEST['name'])) {
    $player = new Player($_REQUEST['name']);
    $player->searchLevenshtein($DB_Connection);
} else {
    echo "Search";
//    $DB_Connection->getAll();
}
?>
<html>
    <head>

    </head>

    <body>
        <div>
            <h1>
                NBA Player Search
            </h1>
        </div>
        <div>
            <form id="search-form" action="./" method="GET">
                <input type="text" name="name" id="search-box" autocomplete="off">
                <input type="submit" value="Search!">
            </form>
        </div>

        <div id="results">
            <div class="player">

            </div>
        </div>
    </body>
</html>
