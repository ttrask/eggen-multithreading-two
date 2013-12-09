/*
*TITLE: 	SMP GO SORTING APPLICATION
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
*							<number of threads used>,<data set size>, <sortTime>,<combine time>
*
*EXECUTION:	There are two ways to run the applicaiton:
*				Using the Go Runtime
*					go run mc.go -n <int> -t <int> -o <somefile.ext>
*				Compile and Run
*					go build mc.go
*					./mc -n <int> -t <int> -o <somefile.ext>
*
*INPUT PARAMETERS:
*			-n:		Number of Ints to generate and sort
*			-t:		Number of threads to use.  Defaults to the total # of CPUS on the system.
*			-o:		The name of the output file to write the results to.				
 */

package main

import (
	"flag"
	"fmt"
	"math/rand"
	"runtime"
	"os"
	"time"
	"utility"
)

//default number of ints to generate.
var _ints int = 100000000

//number of threads to use.  Defaults to the total 
//avaliable # of cpus on the machine.
var _threads int = runtime.NumCPU()
var threadFlag *int = flag.Int("t", 0, "Number of _threads to use")
var intsFlag *int = flag.Int("n", _ints, "Number of Ints to parse")
var outputFlag *string = flag.String("o", "out.go.multicore.csv", "Output File.")

type IntList []int

func main() {
	
	flag.Parse()

	if *threadFlag > 0 {
		_threads = *threadFlag
	}

	_ints = *intsFlag
	
	fmt.Println("Starting Application")

	//Tells GO to use the number of threads supplied.
	runtime.GOMAXPROCS(_threads)

	fmt.Println(fmt.Sprintf("Processing using Parameters: -n %d -t %d -o %s", *intsFlag, _threads, *outputFlag))

	//seeds random number generator
	rand.Seed(time.Now().UTC().UnixNano())

	var m []int = make([]int, _ints)

	fmt.Println("Generating List")

	//Step 1: Generate List 
	utility.GenerateList(&m)

	startTime := time.Now()

	fmt.Println("Sorting List")

	u := new(utility.Util)

	s := ""

	//sets threads for each utility method.  When the goroutines are called,
	//they'll need this to know what to use.
	u.SetThreads(_threads, &s)

	r := new(utility.Reply)
	
	//sorts the list and sends back a reply with the sort time and combine time
	u.Sort(m, r)

	//copies the sorted data back into the original list.
	for i, val := range r.Data {
		m[i] = val
	}

	fmt.Println("Total Elapsed Time:", time.Since(startTime))
	fmt.Println("All Lists Done Sorting")

	startTime = time.Now()
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


	writeOutput(fmt.Sprintf("%d,%d,%d,%d\n", _threads, _ints, r.SortTime.Nanoseconds(), r.CombineTime.Nanoseconds()), *outputFlag)

}


//Writes output to the specified output file.
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
