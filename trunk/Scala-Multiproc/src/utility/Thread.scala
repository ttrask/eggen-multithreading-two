/*
*TITLE: 	Scala Thread Actor class.
*AUTOHR: 	THOMAS TRASK
*STARTDATE: 12-5-2012
*
*PURPOSE: 	This where all of the Actor communication magic happens.
* 			The act() method starts a "listener" that will accept any of
*    		the Scala Message classes.  The Client and server both instantiate
*      		an actor and use them to communicate with each other.      		
*
 */

package utility

import java.util._;
import scala.collection
import scala.actors.Actor;
import scala.actors.Futures._;
import scala.actors.Actor._;
import scala.collection.mutable._;
import rpc._;

class Thread(totalArraySize: Int, range: Long, arrayCount: Int) extends Actor {

  var Duration: Double = 0;
  val _numbers = null;

  def thread: Int = 9;

  def act() {
    self.react {

      //generates a set of numbers
      //and sends it back to the requester via
      //the ReturnValues message class
      case GenerateNumbers(totalArraySize: Int, range: Int) =>
        //        println("Thread Started!")
        val returnvals = GenerateRandomNumbers(totalArraySize, range);
        //        println("Thread Complete")
        sender ! ReturnValues(returnvals);
        sender ! ThreadDone;

      //sorts the input data and returns
      //the data via the ReturnValues message class. 
      case SortData(a: Array[Int]) =>
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

        //combines a list of lists of data into one list and
        //returns it via teh ReturnValues message class.
      case CombineData(sortedListSet: Array[Array[Int]]) => {
        var i: Int = 0;
        val maxArraySet = 4

           var begintime: java.util.Date = new (java.util.Date)
           
           
        val ds3: Array[Array[Int]] = {

          if (sortedListSet.size > maxArraySet && sortedListSet.size % maxArraySet == 0) {

            val setSize: Int = sortedListSet.size / maxArraySet;
            val rem = sortedListSet.size % maxArraySet

            var ds2: Array[Array[Int]] = new Array[Array[Int]](maxArraySet)

            //            println("Set Size:" + sortedListSet.size + "=" + setSize + ", " + rem)

            var start, finish: Int = 0

            val combineTasks = for (j <- 0 to maxArraySet - 1) yield future {
        		begintime = new (java.util.Date);
              var t: Thread = new Thread(0, 0, 0)
              if (j == 0) {
                start = j * setSize

              } else {
                start = j * setSize + rem
              }
              finish = (j + 1) * setSize + rem

              //              println("combining lists " + start + "-" + finish);
              
              t.start()
              
              t ! CombineData(sortedListSet.slice(start, finish))
              var returnData = false;
              var localDS: Array[Int] = Array();

              while (returnData == false) {
                self.receive {
                  //once the remote actor says that they're alive, trigger the generation.
                  case ReturnValues(list: Array[Int]) =>
                    returnData = true
                      	var endTime: java.util.Date = new (java.util.Date)
                    var localCombineTime = endTime.getTime() - begintime.getTime(); 
                  	println("ds: " + list.length + " ct: " + localCombineTime);
                
                    localDS = list
                 	
                }
              }
              localDS
            }

            def combiner = awaitAll(1000000, combineTasks: _*)
            
            combiner foreach (result => {
              (result: @unchecked) match {
                case Some(localDS: Array[Int]) => {
                  //                  println("Adding sorted dataset of size [" + localDS.size + "] to dataset[" + i + "] of size " + ds2.size + ".");
                  ds2(i) = localDS.clone();
                  i += 1;
                }
                case None =>
                  println("No return value from Client");
              }
            });
            ds2
          } else {
            sortedListSet
          }
        }

        i = 0
        var currentIndex: Array[Int] = new Array[Int](ds3.size);
        var lowman = Int.MaxValue
        var v = 0; var lowmanIndex = -1;
        var lowmanHead: Int = 0;

        var datasetSize: Int = {
          var j: Int = 0;
          ds3.foreach(set => {
            //            println(set.length.toString())
            j += set.length
          })
          j
        }

        //        println("Combining list[" + ds3.size + "] of size:" + datasetSize)

        var ds: Array[Int] = new Array(datasetSize);

        var v2: Int = 0; var size: Int = 0;

        for (i <- 0 to datasetSize - 1) {
          lowman = Int.MaxValue
          v = 0; lowmanIndex = -1;

          for (j <- 0 to ds3.length - 1) {

            size = ds3(j).length;
            if (currentIndex(j) <= (size - 1)) {
              v2 = ds3(j)(currentIndex(j))
              if (size > 0 && lowman > v2) {

                lowman = v2
                lowmanIndex = j
              }
            }
          }

          if (lowmanIndex > -1) {
            ds(i) = lowman;

            if (ds3.size > 1) {
              currentIndex(lowmanIndex) = currentIndex(lowmanIndex) + 1;

            }
          } else {
            println("PANIC: Ran out of data at:" + i)
            System.exit(0)
          }
        }

        sender ! ReturnValues(ds.toArray)

      }
    }

  }

  var SuppressMessage: Boolean = false;

  def threadNumber: String = {
    return "(" + thread + ") ";
  }

  def println(s: String) {
    if (!SuppressMessage)
      System.out.println(s);
  }

  def GenerateRandomNumbers(size: Int, range: Int): Array[Int] = {

    val arraySize: Int = size;

    //    println("Thread started");
    val startTime = new java.util.Date()
    val co = Array.fill(size)(scala.util.Random.nextInt(range))
    if (size < 20) {
      for (x: Int <- 0 to co.length - 1) {
        //        printf("%d ", co(x))
      }
    }
    val endTime = new java.util.Date()

    Duration = endTime.getTime() - startTime.getTime();

    //    println("Thread Finished in " + Duration + "ms");
    co;
  }
}