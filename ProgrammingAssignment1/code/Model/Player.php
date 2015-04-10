<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/9/15
 * Time: 11:09 AM
 */

class Player {

    private $PlayerPhoto;
    private $arrayRepresentation;
    private $FirstName;
    private $LastName;

    /**
     * Player constructor.
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
        $name = explode(' ', $array[1]);
        $this->FirstName = $name[0];
        $this->LastName = $name[1];
    }

    // sets the players PlayerPhoto
    public function setPhoto($url) {
        $this->PlayerPhoto = $this->findImage();
    }

    public function getPhoto() {
        return $this->PlayerPhoto;
    }

    // return the url of this players image
//    http://i.cdn.turner.com/nba/nba/.element/img/2.0/sect/statscube/players/large/chris_copeland.png
    public function findImageURL() {
        $url = 'http://i.cdn.turner.com/nba/nba/.element/img/2.0/sect/statscube/players/large/';
        $url = $url . $this->FirstName . '_' . $this->LastName . '.png';
        return $url;
    }

    public function arrayRepresentation() {
        return $this->arrayRepresentation;
    }






}