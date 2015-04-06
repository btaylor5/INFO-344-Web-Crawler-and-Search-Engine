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
$DB_Connection = new DBAccess();

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
            <form id="search-form" name="player-search">
                <input type="text" id="search-box" autocomplete="off">
                <input type="submit" value="Search!">
            </form>
        </div>

        <div id="results">
            <div class="player">

            </div>
        </div>
    </body>
</html>
