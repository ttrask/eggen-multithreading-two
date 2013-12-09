/*
*TITLE: 	SCALA DATA SORTING SERVER APPLICATION
*AUTOHR: 	THOMAS TRASK
*STARTDATE: 12-5-2012
*
*PURPOSE: 	SCALA SERVER USED TO GENERATE A LIST OF RANDOM NUMBERS AND
*			SORT THEM USING MULTITHREADING.
*
*EXECUTION:	There are two ways to run the applicaiton:
*
*INPUT PARAMETERS:
* 			-name:  THe name that the server should run under.
*    				The Scala client will need this to communicate
*        			with the server correctly. 
*			-p:		The port to use when initializing the server
 */

package server
import java.util._
import java.io._
import scala.actors.Actor
import scala.io._
import scala.actors._
import scala.actors.Actor._
import scala.actors.remote._
import scala.actors.remote.RemoteActor._
import scala.collection.immutable.List
import utility._;
import optional._;
import rpc.ReturnValues
import rpc.ReturnThreadCount;
import rpc.StartGeneration;
import rpc.IsAlive;
import rpc.GenerateNumbers
import rpc.SortData;
import rpc.ThreadDone

object ServerShell extends scala.App {

  override def main(args: Array[String]): Unit = {

    implicit def string2Int(s: String): Int = augmentString(s).toInt
    var threads: String = Runtime.getRuntime().availableProcessors().toString();
    var port: Int = 9000;

    val helpString = "-name myClientName [-t threadNumber -p portNumber] "
    val pp = new ParseParms(helpString)
    pp.parm("-name", "Client1", true)
      .parm("-t", threads)
      .parm("-p", port.toString())

    val result = pp.validate(args.toList)

    println(if (result._1) result._3 else result._2)

    //adds whichever name is sent in as the Client remoteactor name
    val name: String = result._3("-name")

    //set threadcount to use, if the system defualt isn't enough.
    threads = result._3("-t")
    val p: Int = string2Int(result._3("-p"))
    if (p > 0) {
      port = p;
    }
    val clientNames: List[Symbol] = List(Symbol(name));

    val maxArraySize: String = (Int.MaxValue / 2).toString;

    RemoteActor.classLoader = getClass().getClassLoader();

    try {

      println("Starting Server using " + threads + " cores.")

      var threadCount: Int = {
        threads
      }

      def individualArraySize(arraySize: Int): Int = {
        arraySize
      };

      clientNames.foreach { b =>
        val client: ServerActor = new ServerActor(b, port, threadCount, null);
        client.start();
        port = (port + 1);
      }

      while (1 == 1) {
        Thread.sleep(100);
      }

    } catch {
      case e: Exception =>
        println("Exception: " + e.toString());
    }
  }
}



