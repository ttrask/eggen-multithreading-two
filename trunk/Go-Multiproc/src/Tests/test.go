
package main

import (
	"fmt"
	"time"
//	"utility"
	"flag"
	"os"
)

//converts gotime to ms.  Don't change.
var goConversion int64 = 1;

var n = 100000000;
var outputFile string = "Out.Go.Tests.csv";
var m []int;
var outputFlag *string = flag.String("o", outputFile, "Output File.")
var intFlag *int = flag.Int("n", 100000000, "Number of Ints to generate and sort.")

func main(){
	fmt.Println("Starting Application")
	

	flag.Parse()

	n = *intFlag;
	
	fmt.Println("Generating ", n , " ints");
	
	
	var start = time.Now();
	
	m = make([]int, n)
	
	for i := 0; i < len(m)-1; i++ {
		m[i] = i;
	}
	
	var genTime = time.Since(start);
	
	fmt.Println("Generated ", n , " ints in ", (genTime));
	
	fmt.Println("Calling No Input Function ", n , " times.");
	start = time.Now();
	
	for i := 0; i < len(m)-1; i++ {
		NoInput();
	}
	
	var noCallTime = time.Since(start);
	fmt.Println("Called No Input Function ", n , " times in ", (noCallTime));
	
	
	fmt.Println("Calling Input Function ", n , " times.");
	start = time.Now();
	
	for i := 0; i < len(m)-1; i++ {
		WithInput(m);
	}
	
	var callTime = time.Since(start);
	fmt.Println("Called Input Function ", n , " times in ", (callTime));
	
	
	writeOutput(fmt.Sprintf("%d,%d,%d,%d\n", n, genTime.Nanoseconds()/goConversion, noCallTime.Nanoseconds()/goConversion, callTime.Nanoseconds()/goConversion), outputFile)
	
	return;
}

func NoInput(){
	
}


func WithInput(arr []int){
	
}

//appends input string to input file.
//if the file does not exist, the funciton creates it.
func writeOutput(out string, file string) {

	if _, err := os.Stat(file); os.IsNotExist(err) {
		os.Create(file)
	}

	f, err := os.OpenFile(file, os.O_APPEND|os.O_WRONLY, 0600)
	if err != nil {
		panic(err)
	}

	defer f.Close()

	if _, err = f.WriteString(out); err != nil {
		panic(err)
	}

}

