package main
/*
*TITLE: 	Stock GO Sorting Application
*AUTOHR: 	THOMAS TRASK
*STARTDATE: 12-5-2012
*
*PURPOSE: 	TO GENERATE A LIST OF RANDOM NUMBERS AND
*			SORT THEM USING THE BUILT-IN GO UTILITIES.  THIS APPLICATION WAS
*			DEVELOPED AS A "CONTROL" BENCHMARK TO DETERMINE IF USING MULTITHREADING
*			/CLUSTER COMPUTING HAS ANY REAL PERFORMANCE BENEFIT.
*
*PROCESS:	THIS PROGRAM CONSISTS OF 2 PARTS: GENERATION, SORT AND
*			VERIFICATION & OUTPUT.
*			GENERATION:	THE APPLICATION GENERETATES A SET OF RANDOM NUMBERS TO SORT.
*			SORT:		THE APPLICATION THEN SORTS THIS DATA SET USING
						THE "sort" GO package.
*			VERIFICATION & OUTPUT: THE SORTED DATA SET IS VERIFIED AS WORKING.
*					ONCE VERIFIED, ALL IMPORTANT PARAMETERS ARE WRITTEN OUT TO
*					AN OUTPUT FILE.
*
*EXECUTION:	There are two ways to run the applicaiton:
*				Using the Go Runtime
*					go run stock.go
*				Compile and Run
*					go build stock.go
*					./stock
*
*INPUT PARAMETERS:
*			-n:		Number of Ints to generate and sort
*			-o:		The name of the output file to write the results to.				
 */
import (
	"flag"
	"fmt"
	"math/rand"
	"os"
	"runtime"
	"sort"
	"time"
	"utility"
)

var _ints int = 100000000

var _threads int = runtime.NumCPU()
var intsFlag *int = flag.Int("n", _ints, "Number of Ints to parse")
var outputFlag *string = flag.String("o", "out.go.stock.csv", "Output File.")

type IntList []int

func main() {

	flag.Parse()

	_ints = *intsFlag

	fmt.Println("Starting Application")

	fmt.Println(fmt.Sprintf("Processing using Parameters: -n %d -o %s", *intsFlag, *outputFlag))

	rand.Seed(time.Now().UTC().UnixNano())

	var m []int = make([]int, _ints)

	fmt.Println("Generating List")

	utility.GenerateList(&m)

	startTime := time.Now()

	fmt.Println("Sorting List")

	sort.Ints(m)

	endTime := time.Since(startTime)
	fmt.Println("Total Elapsed Time:", time.Since(startTime))
	fmt.Println("All Lists Done Sorting")

	fmt.Println("Verifying List is sorted.")

	isSorted := true

	for i := 0; i < len(m)-1; i++ {
		if m[i] > m[i+1] {
			isSorted = false
			i = len(m)
		}
	}

	if isSorted == true {
		fmt.Println("List verified as sorted in", time.Since(startTime))
	} else {
		fmt.Println("****LIST FIALED VERIFICATION. LIST IS NOT SORTED.*********")
	}

	fmt.Println("Writing output")
	writeOutput(fmt.Sprintf("%d,%d\r\n", _ints, endTime), *outputFlag)
}

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
