

package Tests

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

object Test extends scala.App {

  
  def n:Int = 100000000;
    
    var ds: Array[Int] = new Array[Int](n);
    
  override def main(args: Array[String]): Unit = {

    ds = new Array[Int](n);
    
    println("Starting Application")
    
    println("Generating List of " + n + " ints")
    
    var startTime = new java.util.Date;
    
    for(i<-0 to n-1){
      var t:Int = n;
      ds(i) = t;
    }
    
    
    
    var endTime = new java.util.Date;
    
    println("Generating List of " + n + " ints in " + (endTime.getTime() - startTime.getTime()));
    
    println("Calling 1 param Function")
    startTime = new java.util.Date;
    var i=0;
    while(i<n){
      
      funcCallNoInput();
    }
    endTime = new java.util.Date;
    println("Called 1 param Function " + n + " times in " + (endTime.getTime() - startTime.getTime()));
    
     println("Calling Array param Function")
    startTime = new java.util.Date;
    for(i<-0 to n-1){
      funcCallArrInput(ds);
    }
    endTime = new java.util.Date;
    println("Called Array param Function " + n + " times in " + (endTime.getTime() - startTime.getTime()));
    
    
    
    println("Application Ending");
    System.exit(1);

  }
  
  def funcCallNoInput(){
    var s,t:Int = 0;
    
  	while(s < 10){
    	 t += ds(s)
    	 s+=1;
    }    
  }
  
  def funcCallArrInput(arr:Array[Int]){
	   var s,t:Int = 0;
    
     while(s < 10){
    	 t += arr(s)
    	 s+=1;
    }
  }
  
 
}