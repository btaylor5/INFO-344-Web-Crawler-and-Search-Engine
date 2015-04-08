<?php
/**
 * User: btaylor5
 * Date: 4/8/15
 * Time: 2:37 PM
 */

include_once('Book.php');

$defaultBooks = Book::GetDefaultBooks();

for ($i = 0; $i < sizeof($defaultBooks); $i++) {
    echo 'Title: ' . $defaultBooks[$i]->getName() . ' [$' . $defaultBooks[$i]->getPrice() . ']<br />';
}