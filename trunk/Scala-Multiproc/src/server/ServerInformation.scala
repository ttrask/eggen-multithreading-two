/*
*TITLE: 	Data Server informaiton. 
*AUTOHR: 	THOMAS TRASK
*STARTDATE: 12-5-2012
*
*PURPOSE: 	Scala class used to store server information
* 			during processing.
*   
 */

package server

import scala.actors.remote._

class ServerInformation {
  var SortingComplete: Boolean = false;
  var GenerationComplete: Boolean = false;
  var startIndex:Int = 0;
  var endIndex: Int = 0;
  val clientID = System.currentTimeMillis;
  var SendData: Boolean = false;
  var node: Node = null;
  var sym: Symbol = null;
  var ip: String = null;
  var port: Int = 0;
  var isAlive = false;
  var threadCount: Int = 0;
  var localDSSize: Int = 0;
}