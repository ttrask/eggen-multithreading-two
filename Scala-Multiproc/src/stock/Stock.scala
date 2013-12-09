/*
*TITLE: 	Stock SCALA Sorting Application
*AUTOHR: 	THOMAS TRASK
*STARTDATE: 12-5-2012
*
*PURPOSE: 	TO GENERATE A LIST OF RANDOM NUMBERS AND
*			SORT THEM USING THE BUILT-IN Scala UTILITIES.  THIS APPLICATION WAS
*			DEVELOPED AS A "CONTROL" BENCHMARK TO DETERMINE IF USING MULTITHREADING
*			/CLUSTER COMPUTING HAS ANY REAL PERFORMANCE BENEFIT.
*
*PROCESS:	THIS PROGRAM CONSISTS OF 3 PARTS: GENERATION, SORT AND
*			VERIFICATION & OUTPUT.
*			GENERATION:	THE APPLICATION GENERETATES A SET OF RANDOM NUMBERS TO SORT.
*			SORT:		THE APPLICATION THEN SORTS THIS DATA SET USING
						THE "scala.util.Sorting.quickSort" FUNCTION.
*			VERIFICATION & OUTPUT: THE SORTED DATA SET IS VERIFIED AS WORKING.
*					ONCE VERIFIED, ALL IMPORTANT PARAMETERS ARE WRITTEN OUT TO
*					AN OUTPUT FILE.
*
*EXECUTION:	Once the scala application set ahs been compiled,
* 			The stock application can be executed usign the following command:
*    			scala stock.Stock -n <int> -o <somefile.ext>
*			
*
*INPUT PARAMETERS:
*			-n:		Number of Ints to generate and sort
*			-o:		The name of the output file to write the results to.				
 */

package stock

import optional.ParseParms

object Stock extends scala.App {

  override def main(args: Array[String]): Unit =
    {

      var size: Int = 100;
      var outputFileName = "scala.stock.csv"

      implicit def string2Int(s: String): Int = augmentString(s).toInt

      val helpString = "[-n size -o outputFile.csv]"
      val pp = new ParseParms(helpString)
      pp.parm("-n", size.toString())

      val result = pp.validate(args.toList)
      
      println(if (result._1) result._3 else result._2)

      val n: Int = string2Int(result._3("-n"))
      if (n > 0) {
        size = n;
      }

      println(if (result._1) result._3 else result._2)

      System.out.println("Generating Numbers Now!");

      val testList: Array[Int] = math.NumberGenerator.GenerateRandomNumberList(size, Int.MaxValue)

      System.out.println("Done Generating List!");
      System.out.println("Staritng Sort Now!");
      var startSortTime: java.util.Date = new (java.util.Date)

      //SORTS DATA
      val results = scala.util.Sorting.quickSort(testList)
      
      
      var endSortTime: java.util.Date = new (java.util.Date)
      var sortTime = endSortTime.getTime() - startSortTime.getTime();
      System.out.println("Done Sorting in:" + (sortTime).toString());

      System.out.println("Writing to output file:" + outputFileName);

      val fw = new java.io.FileWriter(outputFileName, true)
      try {
        var df: java.text.DateFormat = new java.text.SimpleDateFormat("MM/dd/yyyy hh:mm:ss")
        fw.append(df.format(new java.util.Date()) + "," + size + "," + sortTime+"\r\n")
      } finally fw.close()

    }
}