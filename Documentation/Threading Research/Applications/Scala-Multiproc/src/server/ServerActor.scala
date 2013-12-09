/*
*TITLE: 	SCALA DATA SORTING SERVER 
*AUTOHR: 	THOMAS TRASK
*STARTDATE: 12-5-2012
*
*PURPOSE: 	SCALA UTITLIY SET TO FASCILITATE THE SORTING
* 			AND GENERATION OF LISTS OF NUMBERS. 
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
import rpc.GenerateNumbers;
import rpc.IsAlive;
import rpc.RequestThreadCount;
import rpc.ReturnThreadCount;
import rpc.ReturnValues;
import rpc.ThreadDone;
import rpc.Kill;
import rpc.SortData;
import rpc.StartGeneration;

class ServerActor(clientName: Symbol, portNumber: Int, threadCount: Int, args: Array[String]) extends Actor {
  //gets fired when the 'ClientActor.start is executed.

  def act() {

    //configured to run on this port
    alive(portNumber);
    println("Client %s is alive on port %d".format(clientName.toString(), portNumber));

    //registers the client in the rmi table for contact FROM THE OUTSIIIIIIDE *cue cheesy sound effect*
    register(clientName, self);

    
    var arraySize: Int = 0;

    var completedThreads: Int = 0;

    var actorThreads: Array[Thread] = new Array(threadCount);

    val StartTime = new Date();

    //loops indefinitely
    loop {
      react {

        case IsAlive() =>
          println("Host communication established.");
          sender ! ReturnThreadCount(threadCount)

        case SortData(a: Array[Int]) =>
          println("Recieving request to Sort Dataset of size: " + a.size);

          {
            def swap(i: Int, j: Int) {
              val t = a(i); a(i) = a(j); a(j) = t
            }

            def sort1(l: Int, r: Int) {
              val pivot = a((l + r) / 2)
              var i = l
              var j = r
              while (i <= j) {
                while (a(i) < pivot) i += 1
                while (a(j) > pivot) j -= 1
                if (i <= j) {
                  swap(i, j)
                  i += 1
                  j -= 1
                }
              }
              if (l < j) sort1(l, j)
              if (j < r) sort1(i, r)
            }

            if (a.length > 0)
              sort1(0, a.length - 1)

            sender ! ReturnValues(a)
          }

        case GenerateNumbers(totalArraySize: Int, range: Int) =>

          println("Recieved command to generate numbers on Client %s".format(clientName));

          val r = 0 to range;

          var a: Array[Int] = Array[Int]();

          def getRndInt(range: Range): Int = {
            val rnd = new scala.util.Random

            rnd.nextInt(range.length)
          }

          a = Array.fill(totalArraySize)(scala.util.Random.nextInt(range))

          println("Returning array of size " + a.size);

          sender ! ReturnValues(a)

          a = new Array[Int](0);

        case Kill =>
          Thread.sleep(1000);
          println(println("Killing Client"));
          exit('kill);
      }
    }

  }
}