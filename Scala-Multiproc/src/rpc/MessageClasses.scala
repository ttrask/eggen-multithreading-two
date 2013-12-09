/*
*TITLE: 	Scala Sorting Message Classes
*AUTOHR: 	THOMAS TRASK
*STARTDATE: 12-5-2012
*
*PURPOSE: 	SCALA MESSENGER CLASSES TO HELP FASCILITATE COMMUNICATION BETWEEN
* 			SCALA HOSTS AND REMOTE ACTORS.
*/

package rpc;

import java.util._;
import java.io._;
import scala.actors._

//Sent to the host actor to tell them that the client thread is done.
//used by the SCALA Multicore process.
case object ThreadDone extends Serializable;

//sends teh kill signal to the client process.
case object Kill extends Serializable;

//sent to the scala SERVER  to see if it's alive.
case class IsAlive();

//request sent from client to server, telling it to
//generate dataset with size and range.
case class GenerateNumbers(arraySize: Int, range: Int);

//combines array set into one array.
case class CombineData(s: Array[Array[Int]]);

//sorts the data given to it.
case class SortData(list: Array[Int]);

//returns the dataset that was originally sent to it. 
case class ReturnValues(list: Array[Int]);

//requests the thread count of the remote actor. 
//Supposed to be used to load-balance servers
//with disparate thread counts.
case class RequestThreadCount();

//returns the thread count of the remote actor.
case class ReturnThreadCount(count: Int);

case class StartGeneration(size: Long);

