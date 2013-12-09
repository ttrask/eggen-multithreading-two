/*
*TITLE: 	SMP SCALA SORTING APPLICATION
*AUTOHR: 	THOMAS TRASK
*STARTDATE: 12-5-2012
*
*PURPOSE: 	TO GENERATE A LIST OF RANDOM NUMBERS AND
*			SORT THEM USING MULTITHREADING.
*
*PROCESS:	THIS PROGRAM CONSISTS OF 4 PARTS: SETUP, SORT, COMBINATION, AND
*			VERIFICATION & OUTPUT.
*			SETUP:	THE APPLICATION GENERETATES A SET OF RANDOM NUMBERS TO SORT.
*			SORT:	IT THEN SPLITS THE LIST INTO (N) PARTS AND SENDS EACH TO
*					ITS OWN PROCESSING THREAD FOR PROCESSING.  IF ALL GOES WELL, 
*					EACH THREAD WILL RETURN A SORTED VERSION OF SAID DATA.
*			COMBINATIONON: ONCE ALL OF THE SETS OF DATA ARE	RETURNED, THEY ARE
*					COMBINED INTO ONE DATA SET.
*			VERIFICATION & OUTPUT: THE SORTED DATA SET IS VERIFIED AS WORKING.
*					ONCE VERIFIED, ALL IMPORTANT PARAMETERS ARE WRITTEN OUT TO
*					AN OUTPUT FILE in the form:
*							<Time run>,<number of threads used>,<data set size>, <sortTime>,<combine time>
*
*EXECUTION:	Once the scala application set has been compiled, you can run the MC with the following code:
*				scala multicore.Multicore -n <int> -t <int> -o <somefile.ext>
*
*INPUT PARAMETERS:
*			-n:		Number of Ints to generate and sort
*			-t:		Number of threads to use.  Defaults to the total # of CPUS on the system.
*			-o:		The name of the output file to write the results to.				
 */

package multicore

import scala.collection.parallel._
import scala.collection._
import scala.actors._
import scala.actors.Futures._
import scala.actors.Actor._
import scala.actors.remote._
import java.util.Random
import java.util.Timer
import scala.io._
import optional._
import server._
import java.io.File
import scala.actors.remote.RemoteActor._
import utility._
import rpc._
import rpc.ReturnThreadCount
import rpc.GenerateNumbers
import rpc.ReturnValues
import java.util.Date

object Multicore extends scala.App {

  override def main(args: Array[String]): Unit = {

    println("Starting Application")

    implicit def string2Int(s: String): Int = augmentString(s).toInt

    var startTime: java.util.Date = new (java.util.Date)
    var endTime: java.util.Date = new (java.util.Date)
    var sortTime, combinationTime, totalTime: Long = 0
    var outputFileName = "out.scala.Multicore.csv"
    var datasetSize: Int = 100;
    var threadCount: Int = 4;

    val helpString = "[-n 1000 -t 4 -o out.csv] "
    val pp = new ParseParms(helpString)
    pp.parm("-n", datasetSize.toString)
      .parm("-t", threadCount.toString)
      .parm("-o", outputFileName)

    val result = pp.validate(args.toList)

    println(if (result._1) result._3 else result._2)

    val n: Int = string2Int(result._3("-n"))
    if (n > 0) {
      datasetSize = n;
    }
    val s: Int = string2Int(result._3("-t"))
    if (s > 0) {
      threadCount = s;
    }

    val o: String = result._3("-o")
    if (o.length() > 0) {
      outputFileName = o
    }

    val threads: Array[Thread] = new Array[Thread](threadCount);
    var ds: Array[Int] = Array();

    var results = List[Future[utility.Thread]]();

    println("Generating List: " + datasetSize)

    def remainder: Int = {
      if (datasetSize > threadCount)
        datasetSize % threadCount
      else
        threadCount % datasetSize
    }

    println("remainder:" + remainder)

    //use our cpu muscle to generate a set of numbers.
    //Splits the requested dataset into a smaller set of requests
    //and fields the requests out to the available CPUs.
    //This Generator Function doesn't actually fire until
    //it's called down teh line by "def generator". 

    val generatorTasks = for (i <- 0 to threads.size - 1) yield future {

      var thread = threads(i)

      var generationsCompleted: Int = 0;

      var localDS: Array[Int] = Array();

      var localClient: ServerInformation = new ServerInformation;
      var returnData: Boolean = false;

      def localDSSize: Int = {
        if (remainder != 0) {
          if (i < remainder) {
            (datasetSize / threadCount) + 1;
          } else {
            datasetSize / threadCount;
          }
        } else {
          datasetSize / threadCount
        }
      }

      thread = new Thread(localDSSize, Int.MaxValue, 1)

      thread.start

      thread ! GenerateNumbers(localDSSize, Int.MaxValue)

      /* Generates the list of data to be sorted.
    	 * This data must be combined later to fully simulate 
    	 * The list already existing when it's sorted.
    	 */

      while (returnData == false) {
        receive {
          //once the remote actor says that they're alive, trigger the generation.
          case ReturnValues(list: Array[Int]) =>
            localDS = list
            returnData = true
        }
      }

      //return the local dataset
      localDS;

    }

    //calls the number generators to get the dataset.
    def generator = awaitAll(1000000, generatorTasks: _*)

    /*
       * Combines all returned datasets into one for processing.
       */
    generator foreach (result => {
      (result: @unchecked) match {
        case Some(localDS: Array[Int]) =>
          //          println("Adding local dataset of size [" + localDS.size + "] to dataset.");

          ds = ds ++ localDS
        case None =>
          println("No return value from Client");
      }
    });

    if (datasetSize < 20) {
      ds.foreach(println)
    }

    println("Total Dataset Size: " + ds.size);
    println("Requested Dataset Size: " + datasetSize);
    println("List of " + datasetSize + " generated.")
    if (ds.size != datasetSize) {
      //   println("WARNING: Size of dataset generated does not match requested dataset size.")
    }

    println("Starting Dataset Sort now")
    startTime = new java.util.Date

    //uses the server list to sort the data that was just generated and combined.
    //as with the generator, this isn't called until the "def sorter" below.
    val sortTasks = for (i <- 0 to threads.size - 1) yield future {

      var returnLocalDS = false;
      var thread = threads(i)

      var localDS: Array[Int] = Array();

      var start, end: Int = 0;

      start = {
        if (remainder > 0) {
          if (i <= remainder) {
            (datasetSize / threadCount * i) + i
          } else {
            (datasetSize / threadCount * i) + (remainder + 1)
          }
        } else {
          (datasetSize / threadCount * i)
        }
      }

      end = {
        if (remainder > 0) {
          if (i < remainder) {
            (datasetSize / threadCount * (i + 1)) + i
          } else {
            (datasetSize / threadCount * (i + 1)) + remainder
          }
        } else {
          (datasetSize / threadCount * (i + 1))
        }
      }

      println(start + " " + end + " " + (end - start + 1))

      thread = new Thread(end - start, Int.MaxValue, 1)

      //      println("Attempting to sort data:" + start + "-" + end)
      thread.start
      thread ! SortData(ds.slice(start, end));

      while (!returnLocalDS) {
        receive {
          case ReturnValues(list: Array[Int]) => {
            //            println("localds: " + list.size)
            localDS = list;
            returnLocalDS = true;
          }
        }
      }

      localDS;
    }

    def sorter = awaitAll(1000000, sortTasks: _*)

    var sortedListSet: Array[Array[Int]] = new Array[Array[Int]](sorter.size)

    var i: Int = 0
    sorter foreach (result => {
      (result: @unchecked) match {
        case Some(localDS: Array[Int]) => {
          //          println("Adding sorted dataset of size [" + localDS.size + "] to dataset.");
          sortedListSet(i) = localDS.clone();
          i += 1;
        }
        case None =>
          println("No return value from Client");
      }
    });

    //    for (k <- 0 to sorter.size - 1) {
    //      printf(sortedListSet(k).size.toString() + " ")
    //    }

    endTime = new java.util.Date
    sortTime = endTime.getTime() - startTime.getTime()
    println("Data Sorted in: " + sortTime)

    startTime = new java.util.Date
    println("Starting Recombination of " + sortedListSet.size + " lists Now.");

    ds = Combinelists(sortedListSet)

    endTime = new java.util.Date
    combinationTime = endTime.getTime() - startTime.getTime()

    println("Time to recombine: " + combinationTime);

    println("Verifying Sorted List of size " + ds.size)
    var isSorted = true;

    for (i <- 0 to ds.size - 2) {
      if (ds(i) > ds(i + 1)) {
        isSorted = false;
      }
    }

    if (isSorted == false) {
      println("PANIC: LIST FAILED SORT VERIFICATION.")
      System.exit(0)
    } else {
      println("List verification successful.")
    }

    println("Writing info to output file")

    val fw = new java.io.FileWriter(outputFileName, true)
    try {
      var df: java.text.DateFormat = new java.text.SimpleDateFormat("MM/dd/yyyy hh:mm:ss")
      fw.append(df.format(new java.util.Date()) + "," + threadCount + "," + datasetSize + "," + sortTime + "," + combinationTime + "," + (totalTime + combinationTime) + "\r\n")
    } finally fw.close()

    println("Application Ending");
    System.exit(1);

  }
  
  //Combines a set of arrays into one array.
  def Combinelists(sortedListSet: Array[Array[Int]]): Array[Int] = {

    val ts: Int = {
      if (sortedListSet.size < 10)
        10
      else
        sortedListSet.size;
    }

    System.setProperty("actors.corePoolSize", ts.toString)

    var thread: Thread = new Thread(0, Int.MaxValue, 1)

    //      threads(i) = thread

    thread.start

    thread ! CombineData(sortedListSet)

    var returnData = false;
    var localDS: Array[Int] = Array();

    while (returnData == false) {
      receive {
        //once the remote actor says that they're alive, trigger the generation.
        case ReturnValues(list: Array[Int]) =>
          returnData = true
          localDS = list
      }
    }

    localDS

  }
}