<?php
/**
 * Created by IntelliJ IDEA.
 * User: btaylor5
 * Date: 4/9/15
 * Time: 11:09 AM
 */

class Player {
//    protected $PlayerID;
//    protected $PlayerName;
//    private $PlayerPosition;
//    private $TeamAbbName;
//    private $TeamID;
//    private $GP;
//    private $MIN;
//    private $FGM;
//    private $FGA;
//    private $FGP;
//    private $TPM;
//    private $TPA;
//    private $TPP;
//    private $FTM;
//    private $FTA;
//    private $FTP;
//    private $RBO;
//    private $RBD;
//    private $RBT;
//    private $AST;
//    private $TO;
//    private $STL;
//    private $BLK;
//    private $PF;
//    private $PPG;
//    private $TR;
//    private $PlayerPhoto;
    private $arrayRepresentation;

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
//        $this->PlayerID = $array[0];
//        $this->PlayerName = $array[1];
//        $this->PlayerPosition = $array[2];
//        $this->TeamAbbName = $array[3];
//        $this->TeamID = $array[4];
//        $this->GP = $array[5];
//        $this->MIN = $array[6];
//        $this->FGM = $array[7];
//        $this->FGA = $array[8];
//        $this->FGP = $array[9];
//        $this->TPM = $array[10];
//        $this->TPA = $array[11];
//        $this->TPP = $array[12];
//        $this->FTM = $array[13];
//        $this->FTA = $array[14];
//        $this->FTP = $array[15];
//        $this->RBO = $array[16];
//        $this->RBD = $array[17];
//        $this->RBT = $array[18];
//        $this->AST = $array[19];
//        $this->TO = $array[20];
//        $this->STL = $array[21];
//        $this->BLK = $array[22];
//        $this->PF = $array[23];
//        $this->PPG = $array[24];
//        $this->TR = $array[25];
        $this->arrayRepresentation = $array;
    }

//    public function __toString() {
//        return $this->PlayerName;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getTR()
//    {
//        return $this->TR;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getPlayerID()
//    {
//        return $this->PlayerID;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getPlayerName()
//    {
//        return $this->PlayerName;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getPlayerPosition()
//    {
//        return $this->PlayerPosition;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getTeamAbbName()
//    {
//        return $this->TeamAbbName;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getTeamID()
//    {
//        return $this->TeamID;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getGP()
//    {
//        return $this->GP;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getMIN()
//    {
//        return $this->MIN;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getFGM()
//    {
//        return $this->FGM;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getFGA()
//    {
//        return $this->FGA;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getFGP()
//    {
//        return $this->FGP;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getTPM()
//    {
//        return $this->TPM;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getTPA()
//    {
//        return $this->TPA;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getTPP()
//    {
//        return $this->TPP;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getFTM()
//    {
//        return $this->FTM;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getFTA()
//    {
//        return $this->FTA;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getFTP()
//    {
//        return $this->FTP;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getRBO()
//    {
//        return $this->RBO;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getRBD()
//    {
//        return $this->RBD;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getRBT()
//    {
//        return $this->RBT;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getAST()
//    {
//        return $this->AST;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getTO()
//    {
//        return $this->TO;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getSTL()
//    {
//        return $this->STL;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getBLK()
//    {
//        return $this->BLK;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getPF()
//    {
//        return $this->PF;
//    }
//
//    /**
//     * @return mixed
//     */
//    public function getPPG()
//    {
//        return $this->PPG;
//    }
//
//    public function asJSON() {
//
//    }
//
//    public function setPhoto($image) {
//        $this->PlayerPhoto = $image;
//    }
//
//    public function findImage() {
//
//    }

    public function arrayRepresentation() {
        return $this->arrayRepresentation;
    }




}