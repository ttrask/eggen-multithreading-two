/*
*TITLE: 	GO SORT CLIENT
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
*EXECUTION:	There are two ways to run the applicaiton:
*				Using the Go Runtime
*					go run client.go
*				Compile and Run
*					go build client.go
*					./client
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

package main

import (
	"flag"
	"fmt"
	"io/ioutil"
	"log"
	"math/rand"
	"net"
	"net/rpc"
	"os"
	"runtime"
	"strings"
	"time"
	"utility"
)

var _threads int = runtime.NumCPU()
var _ints int
var outputFile string = "out.Go.Client.csv"

//"Server List File" - File used to load list of servers for processing
var clientList *string = flag.String("clf", "clients.txt", "Input Server List File")

//Protocol Flag - Protocols supported - TCP, UNIX (sort of)
var protocolFlag *string = flag.String("p", "tcp", "Protocol.  Available Protocols: tcp, unix")
var outputFlag *string = flag.String("o", outputFile, "Output File.")
var intFlag *int = flag.Int("n", 10000000, "Number of Ints to generate and sort.")

//
var countFlag *int = flag.Int("s", 0, "Number of servers to use for Operation.  Use 0 for all available.")

func main() {

	flag.Parse()

	_ints = *intFlag

	fmt.Println("Starting Application")

	fmt.Println(fmt.Sprintf("Processing using Parameters: -n %d -s %d -clf %s", *intFlag, *countFlag, *clientList))

	runtime.GOMAXPROCS(_threads)

	rand.Seed(time.Now().UTC().UnixNano())

	//generates lists

	var m []int = make([]int, _ints)

	fmt.Println("Generating List")

	utility.GenerateList(&m)

	/*******opens clients based on list*********/

	clients := ReadClientList(*clientList)

	clientSize := len(clients)

	if clientSize == 0 {
		fmt.Println("PANIC: NO DATA SERVERS LOADED.\nUsed Server File: ", *clientList)
		os.Exit(2)
	}

	fmt.Println("Loaded ", clientSize, " data servers from "+*clientList+".")

	if *countFlag > len(clients) || *countFlag == 0 {
		*countFlag = len(clients)
	}

	if clientSize != *countFlag {
		clientSize = *countFlag
		fmt.Println("Using", clientSize, " data servers, as per cli.")
	}


	var activeClientCount int = 0;
	var activeClientList []string = make([]string, clientSize);
	
	for	_, val := range clients {

		var err error

		_, err = rpc.DialHTTP("tcp", val)

		if err != nil {
			fmt.Println("Client Not Available: ", err)
		}else{
			fmt.Println("Client Available at " + val);
			activeClientList[activeClientCount] = val;
			activeClientCount++;
		}
	}

	if(activeClientCount==0){
		fmt.Println("No active Clients found.");
		fmt.Println("Exiting Application.");
		os.Exit(1);
	}
	list := make([][]int, activeClientCount)
	
	clients = activeClientList[:activeClientCount];
	
	l2 := make([][]int, len(list))

	startTime := time.Now()
	s := time.Now()
	fmt.Println("Start Time:", startTime)

	chans := make([]chan []int, clientSize)

	for i, _ := range chans {
		chans[i] = make(chan []int)
	}

	for i := 0; i < *countFlag; i++ {

		if len(strings.TrimSpace(clients[i])) > 0 {
			c := chans[i]
			go SortClient(&m, &l2[i], &list[i], clients[i], i, clientSize, c)
		} else {
			close(chans[i])
		}
	}

	allDone := false
	ok := make([]bool, clientSize)

	for allDone == false {
		allDone = true

		for i, val := range chans {
			data, isNotDone := <-val
			ok[i] = isNotDone
			if ok[i] == true {
				allDone = false
				l2[i] = make([]int, len(data))
				for k, v3 := range data {
					l2[i][k] = v3
				}
			}
		}
	}

	sortTime := time.Since(startTime)
	fmt.Println("List Split and Sorted in ", time.Since(startTime))

	startTime = time.Now()

	fmt.Println("Combining sorted lists")

	listLength := 0

	for i, _ := range l2 {
		listLength += len(l2[i])
	}
	fmt.Println(listLength)

	m = utility.CombineSortedLists(l2)
	fmt.Println("List Combined in ", time.Since(startTime))
	combineTime := time.Since(startTime)

	startTime = time.Now()
	fmt.Println("Verifying List of size", len(m), "is sorted.")

	//verifies list is sorted.
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

	fmt.Println("Total Time:", time.Since(s))
	fmt.Println("Writing output to ", outputFile)
	writeOutput(fmt.Sprintf("%d,%d,%d,%d\n", clientSize, _ints, sortTime.Nanoseconds(), combineTime.Nanoseconds()), outputFile)
}

//Threaded, Asynchronous approach to sorting list.
//for the given rpc client, send a portion of the given list list1
//to said client to sort and return.
func SortClient(m1 *[]int, l1 *[]int, list1 *[]int, val string, i int, clientSize int, c chan []int) {

	l2 := *l1
	m := *m1
	//	list := *list1

	defer close(c)

	var err error

	checkError(err)

	client := new(rpc.Client)

	if *protocolFlag == "tcp" {
		//		fmt.Println("Attempting connection to tcp:", val)
		client, err = rpc.DialHTTP("tcp", val)
	} else {
		var unixAddr *net.UnixAddr
		unixAddr, err = net.ResolveUnixAddr("unix", val)
		fmt.Println("Attempting connection to socket:", val)
		fmt.Println(unixAddr)
		//			client, err = rpc.Dial("unix", unixAddr.String())
		c2, err := net.Dial("unix", unixAddr.String())
		checkError(err)

		fmt.Println("Successfully Connected to Unix Port", c2)
	}

	if err != nil {
		log.Fatal("dialing:", err)
	}

	//compensates for remainders when the
	//len(m)%(# of clients) != 0
	remainder := len(m) % clientSize
	start, end := 0, 0

	if remainder > 0 && i < remainder {
		start, end = (len(m)/(clientSize))*i+i, (len(m)/(clientSize))*(i+1)+i
	} else {
		start, end = (len(m)/(clientSize))*i, (len(m)/(clientSize))*(i+1)-1
		start, end = start+remainder, end+remainder
	}

	fmt.Println("Sorting List", start, end)

	l2 = make([]int, end-start+1)
	for j := start; j < end; j++ {
		l2[j-start] = m[j]
	}

	s := ""

	err = client.Call("Util.SetThreads", 4, &s)
	fmt.Println(s)

	r := new(utility.Reply)
	err = client.Call("Util.Sort", l2, &r)

	fmt.Println("Client Returned ", len(r.Data))
	//
	//	list = make([]int, len(r.Data))
	//
	for j, val := range r.Data {
		//?		list[j] = val
		l2[j] = val
	}

	if err != nil {
		log.Fatal("util error error:", err)
	}

	client.Close()

	c <- l2

}

//loads in the list of clients availabe to teh servicer.
func ReadClientList(clientList string) []string {

	fmt.Println("Loading ClientList from <", clientList, ">")

	content, err := ioutil.ReadFile(clientList)
	if err != nil {
		//Do something
	}

	clients := strings.Split(string(content), "\n")

	list := make([]string, len(clients))

	if len(clients) > 0 {
		i2 := 0
		for i, val := range clients {
			val = utility.TrimSuffix(val, "\n")
			val = utility.TrimSuffix(val, "\r")
			v2 := strings.TrimSpace(val)

			if len(v2) > 0 {
//				fmt.Println("Client List[", i, "] - ", v2)
				i2++
				list[i] = v2
			}
		}

		list2 := make([]string, i2)

		i2 = 0

		for _, val := range list {
			if len(val) > 0 {
				list2[i2] = val
				i2++
			}
		}

		return list2
	} else {

		//else return nothing
		fmt.Println("No Clients in ", clientList)
		return nil
	}
	return nil
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

//checks if error is populated.
//if it does, the application prints the
//error and exits.
func checkError(err error) {
	if err != nil {
		fmt.Fprintf(os.Stderr, "Fatal error: %s", err.Error())
		os.Exit(1)
	}
}
