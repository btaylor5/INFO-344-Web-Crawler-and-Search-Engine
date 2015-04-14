<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/9/15
 * Time: 11:09 AM
 *
 * A Player class that stores basic stats, information, and will auto retrieve player image links
 *
 */

class Player {

    private $arrayRepresentation;
    private $name;
    private $id;

    /**
     * Player constructor. Pass in an array with the following information and it will
     * find a link to the players image
     *
     * @arg $array( 'PlayerID',
     *              'PlayerName',
     *              'PlayerPosition',
     *              'TeamAbbName',
     *              'TeamID',
     *              'GP',
     *              'MIN',
     *              'FGA',
     *              'TPM',
     *              'TPA',
     *              'TPP',
     *              'FTM',
     *              'FTA',
     *              'FTP',
     *              'RBO',
     *              'RBD',
     *              'RBT',
     *              'AST',
     *              'TO',
     *              'STL',
     *              'BLK',
     *              'PF',
     *              'PPG',
     *              'TR'
     *            )
     **/
    public function __construct($array)
    {
        $this->arrayRepresentation = $array;
        $this->id = $array[0];
        $this->name = $array[1];
        $this->findImageURL();
    }

    // sets the players PlayerPhoto
    public function setPhoto($url)
    {
        $this->arrayRepresentation['ImageURL'] = $url;
    }

    // Sets the players image url
    public function findImageURL()
    {
        $baseURL = 'http://www.nba.com/media/playerfile/';
        $fileName = str_replace(' ', '_', strtolower($this->name));
        $url = $baseURL . $fileName . '.jpg';
        $this->setPhoto($url);
    }

    // returns the Player as an array
    // allows users to take advantage of built in array functions
    public function arrayRepresentation()
    {
        return $this->arrayRepresentation;
    }

    // returns the players name
    public function __toString()
    {
        return $this->name;
    }

    // returns the players ID
    public function getID() {
        return $this->id;
    }






}