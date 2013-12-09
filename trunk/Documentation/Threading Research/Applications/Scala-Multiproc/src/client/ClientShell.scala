/*
*TITLE: 	SCALA SORT CLIENT
*AUTOHR: 	THOMAS TRASK
*STARTDATE: 12-5-2012
*
*PURPOSE: 	TO GENERATE A LIST OF RANDOM NUMBERS AND
*			HAVE A SET OF SERVERS SORT AND RECOMBINE THE LIST.
*
*PROCESS:	THIS PROGRAM CONSISTS OF 4 PARTS: SETUP, SORT, COMBINATION, AND
*			VERIFICATION & OUTPUT.
*			SETUP:	A SET OF GO SORT SERVERS ARE STARTED ON LOCAL/REMOTE
*					MACHINES.  THE SET OF SERVERS ARE LISTED IN THE SERVER LIST,
*					CURRENTLY 'clients.txt'.  WHEN THE SORT CLIENT IS RUN,
*					IT GENERATES A LIST OF N RANDON NUMBERS.
*			SORT:	IT THEN SPLITS THE LIST INTO PARTS AND SENDS EACH PART
*					TO EACH SERVER	FOR PROCESSING.  IF ALL GOES WELL, EACH
*					SERVER WILL RETURN A SORTED VERSION OF SAID DATA.
*			COMBINATIONON: ONCE ALL OF THE SETS OF DATA ARE	RETURNED, THEY ARE
*					COMBINED INTO ONE DATA SET.
*			VERIFICATION & OUTPUT: THE SORTED DATA SET IS VERIFIED AS WORKING.
*					ONCE VERIFIED, ALL IMPORTANT PARAMETERS ARE WRITTEN OUT TO
*					AN OUTPUT FILE IN THE FORM:
						<number of servers used>,<data set size>, <sortTime>,<combine time>
*
*EXECUTION:	ONCE COMPILED, THE APPLICATION CAN BE EXECUTED USING THE FOLLOWING COMMAND
* 				scala 
*				
*
*INPUT PARAMETERS:
*			-clf:	Input Server list.
*			-n:		Number of Ints to generate and sort
*			-p:		Protocol to Use
*			-s:		Number of servers to use for operation. If this is set <=0 || > # of servers found,
*					this is set to the # of servers found.  If it's 0<x<#servers, the first x #servers
*					will be used for processing. This is to streamline testing, as one server list can
*					be used for all testing, instead of having different lists for different #s of servers.
 */


package client

import scala.collection.parallel._
import scala.collection._
import scala.actors._
import scala.actors.Futures._
import scala.actors.Actor._
import scala.actors.remote._
import java.util.Random
import java.util.logging._
import scala.io._
import server._;
import java.io.File
import optional._
import scala.actors.remote.RemoteActor._
import scala.language.postfixOps;
import utility._;
import rpc._;
import rpc.ReturnThreadCount;
import rpc.GenerateNumbers
import rpc.ReturnValues


object ClientShell extends scala.App {

  override def main(args: Array[String]): Unit = {

    //default values for the size, output file name, server list name and server count.
    val timeoutSeconds = 5;
    var datasetSize: Int = 10000
    var outputFileName: String = "out.scala.server.csv"
    var serverCount: Int = 0;
    var serverListFileName: String = "clients.txt"
    implicit def string2Int(s: String): Int = augmentString(s).toInt

    val helpString = "[-n 1000 -s 4 -o out.csv -clf clients.txt] "
    val pp = new ParseParms(helpString)
    pp.parm("-n", datasetSize.toString)
      .parm("-s", serverCount.toString)
      .parm("-o", outputFileName)
      .parm("-clf", serverListFileName)

    val result = pp.validate(args.toList)

    //parsers input parameters.
    println(if (result._1) result._3 else result._2)

    val n: Int = string2Int(result._3("-n"))
    if (n > 0) {
      datasetSize = n;
    }
    val s: Int = string2Int(result._3("-s"))
    if (s > 0) {
      serverCount = s;
    }

    val o: String = result._3("-o")
    if (o.length() > 0) {
      outputFileName = o
    }
    val clf: String = result._3("-clf")
    if (clf.length() > 0) {
      serverListFileName = clf
    }

    try {

      var ds: Array[Int] = Array();
      val maxIndividualArraySize: Long = Long.MaxValue;

      def f(i: Int) = i;

      //generates the server list from the server list file.
      var ServerList: Array[ServerInformation] = {

        var list: List[ServerInformation] = List[ServerInformation]();

        var fi: File = new File(serverListFileName);

        if (!fi.exists()) {
          println("Server File: " + serverListFileName + " does not exist")
          exit();
        }

        for (line <- Source.fromFile(new File(serverListFileName)).getLines) {

          val split: Array[String] = line.split(" ");

          val server: ServerInformation = new ServerInformation();

          if(split.size>=3){
          server.sym = Symbol(split(0));
          server.ip = split(1);
          server port = f(split(2));
          list ::= server;
          }
          else{
        	  println("Line is in incorrect format:" + line+" - Correct Format is <server name> <ip> <port>");
          }
        }

        list.toArray;

      }

      println("Server List Size: " + ServerList.size)

      if (ServerList.size == 0) {
        println("No Servers Loaded - Exiting")
        System.exit(2)
      }

      println("Using Server List of size: " + ServerList.size)

      RemoteActor.classLoader = getClass().getClassLoader();

      var results = List[Future[ServerInformation]]();

      if (serverCount == 0) serverCount = ServerList.size;

      //pings server list to see if we have enough servers
      //all servers that don't respond within 1 second are removed
      //from the server list.
      ServerList = {

        var list: List[ServerInformation] = List[ServerInformation]();

        for (i <- 0 to ServerList.size - 1) {

          if (list.size < serverCount) {
            var server = ServerList(i);
            server.node = new Node(server.ip, server.port);
            val remoteActor = select(server.node, server.sym);

            remoteActor ! IsAlive();
            var returnData = false;

            while (returnData == false) {
              receiveWithin(timeoutSeconds * 1000) {
                case ReturnThreadCount(threadCount: Int) =>
                  {
                    println("Registered Server " + server.sym + " on " + server.ip + ":" + server.port);

                    server.isAlive = true;
                    server.threadCount = threadCount;
                    list ::= server;

                    println(server.sym + ": Server is alive with Thread count=" + threadCount);
                    returnData = true;
                  }

                case TIMEOUT =>
                  println("Server " + server.sym + " at " + server.ip + ":" + server.port + " did not respond in a reasonable time and will not be used.");
                  returnData = true;
              }

            }
          }
        }

        list.toArray;
      }

      //if we have enough servers, continue.
      if (serverCount > 0 && serverCount < ServerList.size) {
        ServerList = ServerList.slice(0, serverCount)
      } else {
        serverCount = ServerList.size
      }

      if (ServerList.size == 0) {
        println("No Servers were available to fulfill your request.  Please verify that they're running on their respective IPs.");
        println("Exiting Applicatin.")
        System.exit(2);
      }
      
      
      //use our server muscle to generate a set of numbers.
      //Splits the requested dataset into a smaller set of requests
      //and fields the requests out to the available servers.
      //This Generator Function doesn't actually fire until
      //it's called down teh line by "def generator". 
      val generatorTasks = for (i <- 0 to ServerList.size - 1) yield future {

        var server: ServerInformation = ServerList(i);

        //prepends the client ip:port to all print statements
        def cPrint(msg: String) = {
          println(server.ip + ":" + server.port + ": " + msg)
        }

        var generationsCompleted: Int = 0;

        var localDS: Array[Int] = Array();

        var localClient: ServerInformation = new ServerInformation;
        var returnData: Boolean = false;

        def localDSSize: Int = {
          if (server == ServerList.head) {
            //  cPrint("Found ClientList.head");
            datasetSize / ServerList.size + datasetSize % ServerList.size;
          } else
            datasetSize / ServerList.size
        }

        server.localDSSize = localDSSize;

        var count: Int = (localDSSize / maxIndividualArraySize).toInt + 1;

        server.node = new Node(server.ip, server.port);
        val remoteActor = select(server.node, server.sym);

        val size: Long = {

          if (localDSSize > maxIndividualArraySize) {

            localDSSize / count;

          } else
            localDSSize;
        }

        remoteActor ! IsAlive()

        /* Generates the list of data to be sorted.
    	 * This data must be combined later to fully simulate 
    	 * The list already existing when it's sorted.
    	 */
        while (returnData == false) {
          receiveWithin(timeoutSeconds * 1000) {
            //once the remote actor says that they're alive, trigger the generation.
            case ReturnThreadCount(threadCount: Int) =>
              {
                server.isAlive = true;
                server.threadCount = threadCount;
                var totalSize: Long = localDSSize;

                remoteActor ! GenerateNumbers(
                  {
                    (size + totalSize % count).toInt
                  },
                  scala.math.pow(2, 20).toInt);

              }

            case ReturnValues(list: Array[Int]) =>
              {
                localDS = localDS ++ list;
                //keeps track of the number of generations completed
                //until we're doine.
                generationsCompleted = generationsCompleted + 1;

                if (generationsCompleted < count)
                  remoteActor ! GenerateNumbers(
                    {
                      (size).toInt
                    },
                    scala.math.pow(2, 20).toInt);
                else {

                  Thread.sleep(1000);
                  
                  def sum(xs: List[Array[Int]]): Int = {

                    def inner(xs: List[Array[Int]], accum: Int): Int = {
                      (xs: @unchecked) match {
                        case x :: tail => inner(tail, accum + x.size)
                        case Nil => accum
                      }
                    }
                    inner(xs, 0)
                  }

                  server.GenerationComplete = true;

                  returnData = true;
                }
              }
            case TIMEOUT => {
              println("Remote actor " + server.ip + ":" + server.port + " did not respond within " + timeoutSeconds + " seconds.  Please check to make sure that it's online.");
              exit()
            }
            case other => {

              println(this + " has received unexpected message " + other);
            }

          }
        }

        //return the local dataset
        localDS;

      };

      //Calls teh generoator tasks defined above and waits for all of them
      //to return their datasets.
      def generator = awaitAll(1000000, generatorTasks: _*)

      
      //Combines all returned datasets into one for processing.
      generator foreach (result => {
        (result: @unchecked) match {
          case Some(localDS: Array[Int]) =>
            ds = ds ++ localDS
          case None =>
            println("No return value from Client");
        }
      });

      println("Generating Dataset Of Size: " + ds.size);

      if (ds.size != datasetSize) {
        //        println("WARNING: Size of dataset generated does not match requested dataset size.")
      }

      var startTime: java.util.Date = new (java.util.Date)
      var endTime: java.util.Date = new (java.util.Date)

      println("Sorting Dataset");

      var curIndex: Int = 0;

      ServerList.head.endIndex = ServerList.head.localDSSize;

      for (i <- 1 to ServerList.size - 1) {
        var client: ServerInformation = ServerList(i);

        client.startIndex = ServerList(i - 1).endIndex;
        client.endIndex = client.startIndex + client.localDSSize.toInt;
      }

      //uses the server list to sort the data that was just generated and combined.
      //as with the generator, this isn't called until the "def sorter" below.
      val sortTasks = for (i <- 0 to ServerList.size - 1) yield future {

        var returnLocalDS = false;
        var client: ServerInformation = ServerList(i);

        val remoteActor = select(client.node, client.sym);

        var localDS: Array[Int] = Array();

        //println("Sending Dataset " + client.startIndex + "-" + client.endIndex + " to " + client.ip)

        remoteActor ! SortData(ds.slice(client.startIndex, client.endIndex));

        while (!returnLocalDS) {
          receive {
            case ReturnValues(list: Array[Int]) => {
              localDS = list;
              returnLocalDS = true;
            }
          }
        }

        localDS;
      }
      
      //calls the sort routines and waits for all of them to return
      //their data.
      def sorter = awaitAll(1000000, sortTasks: _*)

      //ds = Array();

      var sortedListSet: Array[Array[Int]] = new Array[Array[Int]](sorter.size)

      var i: Int = 0
      sorter foreach (result => {
        (result: @unchecked) match {
          case Some(localDS: Array[Int]) => {
            println("Adding sorted dataset of size [" + localDS.size + "] to dataset.");
            //            println(localDS.size + " " + localDs.si)
            sortedListSet(i) = localDS.clone();
            i += 1;
          }
          case None =>
            println("No return value from Client");
        }
      });

      endTime = new java.util.Date;
      var sortTime = endTime.getTime() - startTime.getTime()

      println("Time to Sort:" + (sortTime))

      val sortedDataVerified = false;
      //      println(sortedListSet.size)
      //      sortedListSet foreach (array => {
      //        array.foreach(x => {
      //          print(x + " ")
      //        })
      //        println();
      //      })

      //Recombines data for dataset.
      startTime = new java.util.Date
      println("Starting Recombination Now.");

      ds = Combinelists(sortedListSet)

      endTime = new java.util.Date
      var combinationTime = endTime.getTime() - startTime.getTime()

      println("Time to recombine: " + (combinationTime));

      println("Verifying Sorted List.")
      var isSorted = true;

      for (i <- 0 to ds.size - 2) {
        if (ds(i) > ds(i + 1)) {
          isSorted = false;
        }
      }

      if (isSorted == false) {
        println("LIST FAILED SORT VERIFICATION.")
      } else {
        println("List verification successful.")
      }

      println("Writing info to output file")

      val fw = new java.io.FileWriter(outputFileName, true)
      try {
        var df: java.text.DateFormat = new java.text.SimpleDateFormat("MM/dd/yyyy hh:mm:ss")
        fw.append(df.format(new java.util.Date()) + "," + ServerList.size + "," + datasetSize + "," + sortTime + "," + combinationTime + "\r\n")
      } finally fw.close()

      println("--------------------------------")
      println("Sort Time: " + sortTime+"ms")
      println("Combination Time: " + combinationTime + "ms")
      println("Tomtal Time: " + (sortTime+combinationTime)+"ms");
      println("--------------------------------")
      println("Application Ending");
      System.exit(1);
    }
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

  //removes an element from an array.  Pretty slow but it looks pretty.
  def remove(num: Int, list: Array[Int]) = list diff Array(num)

}