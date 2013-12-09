/*
*TITLE: 	GO DATA SORTING SERVER APPLICATION
*AUTOHR: 	THOMAS TRASK
*STARTDATE: 12-5-2012
*
*PURPOSE: 	TO GENERATE A LIST OF RANDOM NUMBERS AND
*			SORT THEM USING MULTITHREADING.
*
*PROCESS:	A GO SERVER THAT RUNS ON THE CLUSTER NODES
*			TO FASCILITATE SORTING A LIST OF NUMBERS
*
*EXECUTION:	There are two ways to run the applicaiton:
*				Using the Go Runtime
*					go run server.go
*				Compile and Run
*					go build server.go
*					./server
*
*INPUT PARAMETERS:
*			-p:		The port to use when initializing the server
 */
package main

import (
	"flag"
	"fmt"
	"io"
	"log"
	"net"
	"net/http"
	"net/rpc"
	"os"
	"strings"
	"utility"
)

var portFlag *string = flag.String("port", ":1602", "Port Number")

func main() {

	flag.Parse()

	fmt.Println("Processing using flags: ", flag.Args())

	arith := new(utility.Util)
	rpc.Register(arith)

	rpc.HandleHTTP()

	var pf string = *portFlag

	if !strings.HasPrefix(pf, ":") {
		pf = ":" + pf
	}

	fmt.Println("Starting Server on port ", pf)
	l, e := net.Listen("tcp", pf)
	if e != nil {
		log.Fatal("listen error:", e)
	}
	http.Serve(l, nil)

}

//closes the connection.
func closeConnection(conn io.ReadWriteCloser) {

	fmt.Println("Closing Connection")
	conn.Close()

}

//checks to see if tehre was an error starting the server.
//if there is, the applicaiton closes.
func checkError(err error) {
	if err != nil {
		fmt.Println("Fatal error ", err.Error(), "  Application CLosing.")
		os.Exit(1)
	}
}
