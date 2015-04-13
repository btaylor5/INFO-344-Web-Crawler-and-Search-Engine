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
    private $name;
    private $id;

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
        $this->id = $array[0];
        $this->name = $array[1];
        $this->findImageURL();
    }

    // sets the players PlayerPhoto
    public function setPhoto($url) {
        $this->arrayRepresentation['ImageURL'] = $url;
    }

    public function getPhoto() {
        return $this->PlayerPhoto;
    }

    // return the url of this players image
//    http://i.cdn.turner.com/nba/nba/.element/img/2.0/sect/statscube/players/large/chris_copeland.png
    public function findImageURL() {
        $baseURL = 'http://i.cdn.turner.com/nba/nba/.element/img/2.0/sect/statscube/players/large/';
        $fileName = str_replace(' ', '_', $this->name);
        $url = $baseURL . $fileName . '.png';

        $curl = curl_init($url);
        curl_setopt($curl,  CURLOPT_RETURNTRANSFER, TRUE);
        curl_exec($curl);

        /* Check for 404 (file not found). */
        $httpCode = curl_getinfo($curl, CURLINFO_HTTP_CODE);
        if($httpCode == 404) {
            $this->arrayRepresentation['ImageURL'] = 'src/generic-avatar-390x390.png';
        } else {
            $this->arrayRepresentation['ImageURL'] = $url;
        }
        curl_close($curl);
    }

    public function arrayRepresentation() {
        return $this->arrayRepresentation;
    }

    public function __toString() {
        return $this->name;
    }

    public function getID() {
        return $this->id;
    }






}