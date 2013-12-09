/*
*TITLE: 	SCALA NUMBER GENERATOR
*AUTOHR: 	THOMAS TRASK
*STARTDATE: 12-5-2012
*
*PURPOSE: 	To Supplement the Scala stock application by Generating random numbers. 
*
*/

package math

import utility._;
import java.util._;
import scala.actors.Actor;
import scala.actors.Actor._;
import scala.actors.remote._
import scala.actors.remote.RemoteActor._

object NumberGenerator {

  var StartTime: Date = new Date();
  var EndTime: Date = new Date();
  var Duration: Double = 0;
  val _numbers = null;

  //GENERATES A LIST OF RANDOM NUMBERS WITH A REQUESTED SIZE.
  def GenerateRandomNumberList(requestedSize: Int, range: Int): Array[Int] = {

    val maxIndividualArraySize = scala.math.pow(2, 26);

    def arraySize: Int = {

      if (requestedSize < maxIndividualArraySize)
        requestedSize;
      else
        requestedSize / 2 + 1;
    }

    val count: Int = {
      if (requestedSize > maxIndividualArraySize) {
        2;
      } else {
        1;
      }
    }

    def arrayRange: Double = range;

    val list = Array.ofDim[Int](arraySize, count);

    for (i <- 0 to count) {
      StartTime = new Date();

      def co: Array[Int] = Array.fill(arraySize)(new Random(System.currentTimeMillis()).nextInt());

      EndTime = new Date();

      Duration = EndTime.getTime() - StartTime.getTime();

      println("Thread Finished in " + Duration + "ms");

      list(i) = co;
    }

    if (count > 1) {
      list(0) ++ list(1);
    } else
      list(0);
  }

}