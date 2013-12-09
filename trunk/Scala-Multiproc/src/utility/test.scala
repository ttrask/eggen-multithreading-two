package utility;

import java.util._
import scala.actors.Actor._
import scala.actors.remote._
import scala.actors.remote.RemoteActor._
import math._;


object test extends App {

  val maxSize = 12
  
  val size: Int = scala.math.pow(2, maxSize).asInstanceOf[Int];
  
  
  System.out.println("Generating Numbers Now!");
  
  val testList: Array[Int] = math.NumberGenerator.GenerateRandomNumberList(size, size)
  
  System.out.println("Done!");
  
//  val testArr: Array[Short] = {
//    
//    System.out.println("Starting generation of array with size 2^" + maxSize + " numbers");
//
//    def test: Array[Short] = new Array(scala.Math.pow(2, maxSize).asInstanceOf[Int]);
//
//    val top = scala.Math.pow(2, 31).asInstanceOf[Int];
//
//    for (x <- 1 to top) {
//      test(x) = 1;
//      
//      System.out.println(x);
//
//    }
//
//    System.out.println("Generated array of 2^" + maxSize + " values");
//    test;
//  }
  
  
  
  

}